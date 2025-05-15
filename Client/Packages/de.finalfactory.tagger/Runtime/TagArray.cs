// #region License
// // // --------------------------------------------------------------------------------------------------------------------
// // // <summary>
// // //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// // //   TagArray.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// // //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // // </summary>
// // // --------------------------------------------------------------------------------------------------------------------
// #endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using Object = System.Object;

namespace Finalfactory.Tagger
{
    [ComVisible(true)]
    [Serializable]
    public sealed class TagArray : ICollection, ICloneable, IDisposable
    {
        private const int BitsPerInt32 = 32;

        private const int ShrinkThreshold = 256;

        private static readonly List<TagArray> TagArrays = new();

        [SerializeField] private int[] _array;

        [SerializeField] private int[] _maskArray;

        [SerializeField] private int _version;

        private int _arrayLength;

        private bool _persistent;

        [NonSerialized] private object _syncRoot;

        /// <summary>
        ///     All of the values in the tag array are set to defaultValue
        /// </summary>
        /// <param name="defaultValue"></param>
        public TagArray(bool defaultValue = false)
            : this()
        {
            var trueValue = unchecked((int)0xffffffff);
            var fillValue = defaultValue ? trueValue : 0;
            for (var i = 0; i < _arrayLength; i++)
            {
                _array[i] = fillValue;
                _maskArray[i] = trueValue;
            }

            _version = 0;
        }

        /// <summary>
        ///     Clone existing TagArray
        /// </summary>
        /// <param name="bits"></param>
        public TagArray(TagArray bits)
            : this()
        {
            if (bits == null) throw new ArgumentNullException(nameof(bits));

            Array.Copy(bits._array, _array, _arrayLength);
            Array.Copy(bits._maskArray, _maskArray, _arrayLength);

            _version = bits._version;
        }

        private TagArray()
        {
            SetSize(Length);
        }

        public static int Length { get; private set; }

        /// <summary>
        ///     If a TagArray is set to persistent mode it gets automatically resizes if a tag gets added or deleted from the
        ///     system.
        ///     A persistent TagArray needs to be disposed or the persistent mode must be disabled.
        ///     If this is not observed, the TagArray will never be cleaned up by the GC.
        /// </summary>
        public bool Persistent
        {
            get => _persistent;
            set => SetPersistentState(value);
        }

        public bool this[int index]
        {
            get => Get(index);
            set => Set(index, value);
        }

        public object Clone()
        {
            var bitArray = new TagArray(this) { _version = _version };
            return bitArray;
        }

        public int Count => Length;

        public bool IsSynchronized => false;

        public object SyncRoot
        {
            get
            {
                if (_syncRoot == null) Interlocked.CompareExchange<object>(ref _syncRoot, new Object(), null);

                return _syncRoot;
            }
        }

        public void CopyTo(Array array, int index)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));

            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), "ArgumentOutOfRange_NeedNonNegNum");

            if (array.Rank != 1) throw new ArgumentException("Arg_RankMultiDimNotSupported");

            if (array is int[])
            {
                Array.Copy(_array, 0, array, index, _arrayLength);
            }
            else if (array is bool[])
            {
                if (array.Length - index < Length) throw new ArgumentException("Argument_InvalidOffLen");

                var b = (bool[])array;
                for (var i = 0; i < Length; i++)
                    b[index + i] = ((_array[i / BitsPerInt32] >> (i % BitsPerInt32)) & 0x00000001) != 0;
            }
            else
            {
                throw new ArgumentException("Arg_BitArrayTypeUnsupported");
            }
        }

        public IEnumerator GetEnumerator()
        {
            return new BitArrayEnumeratorSimple(this);
        }

        public void Dispose()
        {
            TagArrays.Remove(this);
        }

        public bool HasAny()
        {
            return _array.Any(t => t > 0);
        }

        /// <summary>
        ///     DO N_O_T USE THIS METHOD. IT COULD BREAK THE TAGGERSYSTEM.
        ///     If you need access, contact me. So I can figure out why
        ///     it makes sense to make this method public.
        /// </summary>
        internal static void INTERNAL_Add()
        {
            var lastIndex = Length;
            INTERNAL_SetSize(Length + 1);

            foreach (var tagArray in TagArrays)
            {
                tagArray.Set(lastIndex, false);
                tagArray.SetMask(lastIndex, true);
            }
        }

        /// <summary>
        ///     DO N_O_T USE THIS METHOD. IT COULD BREAK THE TAGGERSYSTEM.
        ///     If you need access, contact me. So I can figure out why
        ///     it makes sense to make this method public.
        /// </summary>
        internal static void INTERNAL_Clear()
        {
            TagArrays.Clear();
        }

        /// <summary>
        ///     DO N_O_T USE THIS METHOD. IT COULD BREAK THE TAGGERSYSTEM.
        ///     If you need access, contact me. So I can figure out why
        ///     it makes sense to make this method public.
        /// </summary>
        internal static void INTERNAL_RemoveAt(int index)
        {
            if (Length > 0)
            {
                foreach (var tagArray in TagArrays)
                    for (var i = index; i < Length - 1; i++)
                        tagArray.Set(i, tagArray.Get(i + 1));

                INTERNAL_SetSize(Length - 1);
            }
        }

        /// <summary>
        ///     DO N_O_T USE THIS METHOD. IT COULD BREAK THE TAGGERSYSTEM.
        ///     If you need access, contact me. So I can figure out why
        ///     it makes sense to make this method public.
        /// </summary>
        internal static void INTERNAL_SetSize(int bits)
        {
            var newints = GetArrayLength(bits, BitsPerInt32);
            foreach (var tagArray in TagArrays)
            {
                if (newints > tagArray._array.Length || newints + ShrinkThreshold < tagArray._array.Length)
                {
                    // grow or shrink (if wasting more than _ShrinkThreshold ints)
                    var newarray = new int[newints];
                    var newMaskarray = new int[newints];
                    Array.Copy(tagArray._array, newarray,
                        newints > tagArray._array.Length ? tagArray._array.Length : newints);
                    tagArray._array = newarray;
                    Array.Copy(tagArray._maskArray, newMaskarray,
                        newints > tagArray._maskArray.Length ? tagArray._maskArray.Length : newints);
                    tagArray._maskArray = newMaskarray;
                    tagArray._arrayLength = newints;
                }

                if (bits > Length)
                {
                    // clear high bit values in the last int
                    var last = GetArrayLength(Length, BitsPerInt32) - 1;
                    var newBits = Length % BitsPerInt32;
                    if (newBits > 0) tagArray._array[last] &= (1 << newBits) - 1;

                    // clear remaining int values
                    Array.Clear(tagArray._array, last + 1, newints - last - 1);
                }

                tagArray._version++;
            }

            Length = bits;
        }

        /*=========================================================================
        ** Returns a reference to the current instance ANDed with value.
        **
        ** Exceptions: ArgumentException if value == null or
        **             value.Length != this.Length.
        =========================================================================*/
        public TagArray And(TagArray value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            for (var i = 0; i < _arrayLength; i++) _array[i] &= value._array[i];

            _version++;
            return this;
        }

        /*=========================================================================
        ** Returns a reference to the current instance ANDed with value.
        **
        ** Exceptions: ArgumentException if value == null or
        **             value.Length != this.Length.
        =========================================================================*/
        public TagArray AndMask(TagArray value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            for (var i = 0; i < _arrayLength; i++) _array[i] &= value._array[i] & value._maskArray[i];

            _version++;
            return this;
        }

        public bool Contains(TagArray other)
        {
            for (var i = 0; i < Length; i++)
            {
                var arrayIndex = i / BitsPerInt32;
                var bit = 1 << (i % BitsPerInt32);
                var thisBit = _array[arrayIndex] & bit;
                var otherBit = other._array[arrayIndex] & bit;
                if (otherBit != 0)
                    if (thisBit == 0)
                        return false;
            }

            return true;
        }

        public bool Any(TagArray other)
        {
            for (var i = 0; i < Length; i++)
            {
                var arrayIndex = i / BitsPerInt32;
                var bit = 1 << (i % BitsPerInt32);
                var thisBit = _array[arrayIndex] & bit;
                var otherBit = other._array[arrayIndex] & bit;
                if (otherBit != 0)
                    if (thisBit != 0)
                        return true;
            }

            return false;
        }

        /// <summary>
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        /// G M O R
        /// 0 0 0 1
        /// 0 0 1 0
        /// 0 1 0 1
        /// 0 1 1 0
        /// 1 0 0 0
        /// 1 0 1 0
        /// 1 1 0 1
        /// 1 1 1 1
        public bool ContainsWithMask(TagArray other)
        {
            var otherMaskArray = other._maskArray;
            var otherArray = other._array;
            for (var i = 0; i < Length; i++)
            {
                var arrayIndex = i / BitsPerInt32;
                var bit = 1 << (i % BitsPerInt32);
                var thisBit = _array[arrayIndex] & bit;
                if (thisBit != 0 && (otherMaskArray[arrayIndex] & bit) == 0) return false;

                if ((otherArray[arrayIndex] & bit) != 0)
                    if (thisBit == 0)
                        return false;
            }

            return true;
        }

        /*=========================================================================
        ** Returns the bit value at position index.
        **
        ** Exceptions: ArgumentOutOfRangeException if index < 0 or
        **             index >= GetLength().
        =========================================================================*/
        public bool Get(int index)
        {
            if (index < 0 || index >= Length)
                throw new ArgumentOutOfRangeException(nameof(index), "ArgumentOutOfRange_Index");

            return (_array[index / BitsPerInt32] & (1 << (index % BitsPerInt32))) != 0;
        }

        /*=========================================================================
        ** Returns the bit value at position index.
        **
        ** Exceptions: ArgumentOutOfRangeException if index < 0 or
        **             index >= GetLength().
        =========================================================================*/
        public bool GetMask(int index)
        {
            if (index < 0 || index >= Length)
                throw new ArgumentOutOfRangeException(nameof(index), "ArgumentOutOfRange_Index");

            return (_maskArray[index / BitsPerInt32] & (1 << (index % BitsPerInt32))) != 0;
        }

        /*=========================================================================
        ** Returns the bit value at position index.
        **
        ** Exceptions: ArgumentOutOfRangeException if index < 0 or
        **             index >= GetLength().
        =========================================================================*/
        public bool GetWithMask(int index)
        {
            if (index < 0 || index >= Length)
                throw new ArgumentOutOfRangeException(nameof(index), "ArgumentOutOfRange_Index");

            var i = index / BitsPerInt32;
            var bit = 1 << (index % BitsPerInt32);
            return (_array[i] & bit) != 0 && (_maskArray[i] & bit) != 0;
        }

        public TagArray Not(TagArray value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            for (var i = 0; i < _arrayLength; i++) _array[i] &= ~value._array[i];

            _version++;
            return this;
        }

        /*=========================================================================
        ** Inverts all the bit values. On/true bit values are converted to
        ** off/false. Off/false bit values are turned on/true. The current instance
        ** is updated and returned.
        =========================================================================*/
        public TagArray Not()
        {
            for (var i = 0; i < _arrayLength; i++) _array[i] = ~_array[i];

            _version++;
            return this;
        }

        /*=========================================================================
        ** Returns a reference to the current instance ORed with value.
        **
        ** Exceptions: ArgumentException if value == null or
        **             value.Length != this.Length.
        =========================================================================*/
        public TagArray Or(TagArray value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            for (var i = 0; i < _arrayLength; i++) _array[i] |= value._array[i];

            _version++;
            return this;
        }

        /*=========================================================================
        ** Sets the bit value at position index to value.
        **
        ** Exceptions: ArgumentOutOfRangeException if index < 0 or
        **             index >= GetLength().
        =========================================================================*/
        public void Set(int index, bool value)
        {
            if (index < 0 || index >= Length)
                throw new ArgumentOutOfRangeException(nameof(index), "ArgumentOutOfRange_Index");

            if (value)
                _array[index / BitsPerInt32] |= 1 << (index % BitsPerInt32);
            else
                _array[index / BitsPerInt32] &= ~(1 << (index % BitsPerInt32));

            _version++;
        }

        /*=========================================================================
        ** Sets all the bit values to value.
        =========================================================================*/
        public void SetAll(bool value)
        {
            var fillValue = value ? unchecked((int)0xffffffff) : 0;
            for (var i = 0; i < _arrayLength; i++) _array[i] = fillValue;

            _version++;
        }

        /*=========================================================================
        ** Sets the bit value at position index to value.
        **
        ** Exceptions: ArgumentOutOfRangeException if index < 0 or
        **             index >= GetLength().
        =========================================================================*/
        public void SetMask(int index, bool value)
        {
            if (index < 0 || index >= Length)
                throw new ArgumentOutOfRangeException(nameof(index), "ArgumentOutOfRange_Index");

            if (value)
                _maskArray[index / BitsPerInt32] |= 1 << (index % BitsPerInt32);
            else
                _maskArray[index / BitsPerInt32] &= ~(1 << (index % BitsPerInt32));

            _version++;
        }

        public TagArray SetMask(TagArray value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            for (var i = 0; i < _arrayLength; i++) _maskArray[i] &= ~value._array[i];

            _version++;
            return this;
        }

        /*=========================================================================
        ** Sets all the bit values to value.
        =========================================================================*/
        public void SetMaskAll(bool value)
        {
            var fillValue = value ? unchecked((int)0xffffffff) : 0;
            for (var i = 0; i < _arrayLength; i++) _maskArray[i] = fillValue;

            _version++;
        }

        /*=========================================================================
        ** Returns a reference to the current instance XORed with value.
        **
        ** Exceptions: ArgumentException if value == null or
        **             value.Length != this.Length.
        =========================================================================*/
        public TagArray Xor(TagArray value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            for (var i = 0; i < _arrayLength; i++) _array[i] ^= value._array[i];

            _version++;
            return this;
        }

        /// <summary>
        ///     Used for conversion between different representations of bit array.
        ///     Returns (n+(div-1))/div, rearranged to avoid arithmetic overflow.
        ///     For example, in the bit to int case, the straightforward calc would
        ///     be (n+31)/32, but that would cause overflow. So instead it's
        ///     rearranged to ((n-1)/32) + 1, with special casing for 0.
        ///     Usage:
        ///     GetArrayLength(77, BitsPerInt32): returns how many ints must be
        ///     allocated to store 77 bits.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="div">
        ///     use a conversion constant, e.g. BytesPerInt32 to get
        ///     how many ints are required to store n bytes
        /// </param>
        /// <returns></returns>
        private static int GetArrayLength(int n, int div)
        {
            return n > 0 ? (n - 1) / div + 1 : 0;
        }

        private void SetSize(int bits)
        {
            _arrayLength = GetArrayLength(bits, BitsPerInt32);
            _array = new int[_arrayLength];
            _maskArray = new int[_arrayLength];
        }

        internal void SetPersistentState(bool value, bool force = false)
        {
            if ((!_persistent && value) || force)
            {
                TagArrays.Add(this);
                _persistent = true;
            }
            else if (_persistent && !value)
            {
                TagArrays.Remove(this);
                _persistent = false;
            }
        }

        public class TagArrayEqualityComparer : EqualityComparer<TagArray>
        {
            public override bool Equals(TagArray first, TagArray second)
            {
                if (first == null || second == null) return first == second;

                if (ReferenceEquals(first, second)) return true;

                for (var i = 0; i < first._array.Length; i++)
                    if (first._array[i] != second._array[i])
                        return false;

                return true;
            }

            public override int GetHashCode(TagArray obj)
            {
                var result = 17;
                if (obj == null) return result;
                foreach (var t in obj._array)
                    unchecked
                    {
                        result = result * 23 + t;
                    }

                return result;
            }
        }

        [Serializable]
        private sealed class BitArrayEnumeratorSimple : IEnumerator, ICloneable
        {
            private readonly TagArray _bitarray;

            private readonly int _version;

            private bool _currentElement;

            private int _index;

            internal BitArrayEnumeratorSimple(TagArray bitarray)
            {
                _bitarray = bitarray;
                _index = -1;
                _version = bitarray._version;
            }

            public object Clone()
            {
                return MemberwiseClone();
            }

            public object Current
            {
                get
                {
                    if (_index == -1) throw new InvalidOperationException("InvalidOperation_EnumNotStarted");
                    if (_index >= _bitarray.Count) throw new InvalidOperationException("InvalidOperation_EnumEnded");
                    return _currentElement;
                }
            }

            public bool MoveNext()
            {
                if (_version != _bitarray._version)
                    throw new InvalidOperationException("InvalidOperation_EnumFailedVersion");
                if (_index < _bitarray.Count - 1)
                {
                    _index++;
                    _currentElement = _bitarray.Get(_index);
                    return true;
                }

                _index = _bitarray.Count;

                return false;
            }

            public void Reset()
            {
                if (_version != _bitarray._version)
                    throw new InvalidOperationException("InvalidOperation_EnumFailedVersion");
                _index = -1;
            }
        }
    }
}