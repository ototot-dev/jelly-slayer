// // ***********************************************************************
// // Author           : Florian Schmidt
// // Created          : 01.10.2019 : 15:17
// // Website          : www.finalfactory.de
// //
// // Last Modified By : Florian Schmidt
// // Last Modified On : 01.10.2019 : 15:32
// // ***********************************************************************
// // <copyright file="MultiKeyDictionary.cs" company="Final Factory">
// //     Copyright (c) Final Factory. All rights reserved.
// // </copyright>
// // <summary></summary>
// // ***********************************************************************

#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace FinalFactory.Collections
{
    /// <summary>
    ///     Multi-Key Dictionary Class
    /// </summary>
    /// <typeparam name="TK1">Primary Key Type</typeparam>
    /// <typeparam name="TK2">Sub Key Type</typeparam>
    /// <typeparam name="TK3">Sub 2 Key Type</typeparam>
    /// <typeparam name="TValue">Value Type</typeparam>
    public class MultiKeyDictionary<TK1, TK2, TK3, TValue>
    {
        internal readonly Dictionary<TK1, TValue> BaseDictionary = new Dictionary<TK1, TValue>();
        internal readonly Dictionary<TK1, TK2> PrimaryToSubkeyMapping = new Dictionary<TK1, TK2>();
        internal readonly Dictionary<TK1, TK3> PrimaryToSubkeyMapping2 = new Dictionary<TK1, TK3>();
        internal readonly Dictionary<TK2, TK1> SubDictionary = new Dictionary<TK2, TK1>();
        internal readonly Dictionary<TK3, TK1> SubDictionary2 = new Dictionary<TK3, TK1>();

        public TValue this[TK3 subKey]
        {
            get
            {
                if (TryGetValue(subKey, out var item))
                    return item;

                throw new KeyNotFoundException("sub key not found: " + subKey);
            }
        }

        public TValue this[TK2 subKey]
        {
            get
            {
                if (TryGetValue(subKey, out var item))
                    return item;

                throw new KeyNotFoundException("sub key not found: " + subKey);
            }
        }

        public TValue this[TK1 primaryKey]
        {
            get
            {
                if (TryGetValue(primaryKey, out var item))
                    return item;

                throw new KeyNotFoundException("primary key not found: " + primaryKey);
            }
        }

        public List<TValue> Values => BaseDictionary.Values.ToList();

        public int Count => BaseDictionary.Count;

        public void Associate(TK2 subKey, TK1 primaryKey)
        {
            if (!BaseDictionary.ContainsKey(primaryKey))
                throw new KeyNotFoundException(string.Format("The base dictionary does not contain the key '{0}'",
                    primaryKey));

            if (PrimaryToSubkeyMapping.TryGetValue(primaryKey, out var value)) // Remove the old mapping first
            {
                if (SubDictionary.ContainsKey(value))
                {
                    SubDictionary.Remove(value);
                }

                PrimaryToSubkeyMapping.Remove(primaryKey);
            }

            SubDictionary[subKey] = primaryKey;
            PrimaryToSubkeyMapping[primaryKey] = subKey;
        }

        public void Associate(TK3 subKey, TK1 primaryKey)
        {
            if (!BaseDictionary.ContainsKey(primaryKey))
                throw new KeyNotFoundException(string.Format("The base dictionary does not contain the key '{0}'",
                    primaryKey));

            if (PrimaryToSubkeyMapping2.TryGetValue(primaryKey, out var value)) // Remove the old mapping first
            {
                if (SubDictionary2.ContainsKey(value))
                {
                    SubDictionary2.Remove(value);
                }

                PrimaryToSubkeyMapping2.Remove(primaryKey);
            }

            SubDictionary2[subKey] = primaryKey;
            PrimaryToSubkeyMapping2[primaryKey] = subKey;
        }

        public bool TryGetValue(TK3 subKey, out TValue val)
        {
            val = default(TValue);

            if (SubDictionary2.TryGetValue(subKey, out var primaryKey))
            {
                return BaseDictionary.TryGetValue(primaryKey, out val);
            }

            return false;
        }

        public bool TryGetValue(TK2 subKey, out TValue val)
        {
            val = default(TValue);

            if (SubDictionary.TryGetValue(subKey, out var primaryKey))
            {
                return BaseDictionary.TryGetValue(primaryKey, out val);
            }

            return false;
        }

        public bool TryGetValue(TK1 primaryKey, out TValue val)
        {
            return BaseDictionary.TryGetValue(primaryKey, out val);
        }

        public bool TryGetSubKey(TK1 value, out TK3 subKey)
        {
            return PrimaryToSubkeyMapping2.TryGetValue(value, out subKey);
        }

        public bool TryGetSubKey(TK1 value, out TK2 subKey)
        {
            return PrimaryToSubkeyMapping.TryGetValue(value, out subKey);
        }

        public bool ContainsKey(TK3 subKey)
        {
            return TryGetValue(subKey, out var val);
        }

        public bool ContainsKey(TK2 subKey)
        {
            return TryGetValue(subKey, out var val);
        }

        public bool ContainsKey(TK1 primaryKey)
        {
            return TryGetValue(primaryKey, out var val);
        }

        public void Remove(TK1 primaryKey)
        {
            if (PrimaryToSubkeyMapping.TryGetValue(primaryKey, out var v1))
            {
                if (SubDictionary.ContainsKey(v1))
                {
                    SubDictionary.Remove(v1);
                }

                PrimaryToSubkeyMapping.Remove(primaryKey);
            }

            if (PrimaryToSubkeyMapping2.TryGetValue(primaryKey, out var v2))
            {
                if (SubDictionary2.ContainsKey(v2))
                {
                    SubDictionary2.Remove(v2);
                }

                PrimaryToSubkeyMapping2.Remove(primaryKey);
            }

            BaseDictionary.Remove(primaryKey);
        }

        public void Remove(TK2 subKey)
        {
            BaseDictionary.Remove(SubDictionary[subKey]);
            PrimaryToSubkeyMapping.Remove(SubDictionary[subKey]);
            SubDictionary.Remove(subKey);
        }

        public void Remove(TK3 subKey)
        {
            BaseDictionary.Remove(SubDictionary2[subKey]);
            PrimaryToSubkeyMapping.Remove(SubDictionary2[subKey]);
            SubDictionary2.Remove(subKey);
        }

        public void Add(TK1 primaryKey, TValue val)
        {
            BaseDictionary.Add(primaryKey, val);
        }

        public void Add(TK1 primaryKey, TK2 subKey, TValue val)
        {
            Add(primaryKey, val);
            Associate(subKey, primaryKey);
        }

        public void Add(TK1 primaryKey, TK2 subKey, TK3 subKey2, TValue val)
        {
            Add(primaryKey, val);
            Associate(subKey, primaryKey);
            Associate(subKey2, primaryKey);
        }

        public TValue[] CloneValues()
        {
            var values = new TValue[BaseDictionary.Values.Count];

            BaseDictionary.Values.CopyTo(values, 0);

            return values;
        }

        public TK1[] ClonePrimaryKeys()
        {
            var values = new TK1[BaseDictionary.Keys.Count];

            BaseDictionary.Keys.CopyTo(values, 0);

            return values;
        }

        public TK2[] CloneSubKeys()
        {
            var values = new TK2[SubDictionary.Keys.Count];

            SubDictionary.Keys.CopyTo(values, 0);

            return values;
        }

        public TK3[] CloneSubKeys2()
        {
            var values = new TK3[SubDictionary.Keys.Count];

            SubDictionary2.Keys.CopyTo(values, 0);

            return values;
        }

        public void Clear()
        {
            BaseDictionary.Clear();
            SubDictionary.Clear();
            PrimaryToSubkeyMapping.Clear();
        }

        public IEnumerator<ValueTuple<TK1, TK2, TK3, TValue>> GetEnumerator()
        {
            TK1[] list = ClonePrimaryKeys();
            for (int index = list.Length - 1; index >= 0; index--)
            {
                var k = list[index];
                PrimaryToSubkeyMapping.TryGetValue(k, out var k1);
                PrimaryToSubkeyMapping2.TryGetValue(k, out var k2);
                BaseDictionary.TryGetValue(k, out var v2);
                yield return new ValueTuple<TK1, TK2, TK3, TValue>(k, k1, k2, v2);
            }
        }
    }

    /// <summary>
    ///     Multi-Key Dictionary Class
    /// </summary>
    /// <typeparam name="TK1">Primary Key Type</typeparam>
    /// <typeparam name="TK2">Sub Key Type</typeparam>
    /// <typeparam name="TValue">Value Type</typeparam>
    public class MultiKeyDictionary<TK1, TK2, TValue>
    {
        internal readonly Dictionary<TK1, TValue> BaseDictionary = new Dictionary<TK1, TValue>();
        internal readonly Dictionary<TK1, TK2> PrimaryToSubkeyMapping = new Dictionary<TK1, TK2>();

        internal readonly Dictionary<TK2, TK1> SubDictionary = new Dictionary<TK2, TK1>();

        public TValue this[TK2 subKey]
        {
            get
            {
                if (TryGetValue(subKey, out var item))
                    return item;

                throw new KeyNotFoundException("sub key not found: " + subKey);
            }
        }

        public TValue this[TK1 primaryKey]
        {
            get
            {
                if (TryGetValue(primaryKey, out var item))
                    return item;

                throw new KeyNotFoundException("primary key not found: " + primaryKey);
            }
        }

        public List<TValue> Values => BaseDictionary.Values.ToList();

        public int Count => BaseDictionary.Count;

        public void Associate(TK2 subKey, TK1 primaryKey)
        {
            if (!BaseDictionary.ContainsKey(primaryKey))
                throw new KeyNotFoundException(string.Format("The base dictionary does not contain the key '{0}'",
                    primaryKey));

            if (PrimaryToSubkeyMapping.TryGetValue(primaryKey, out var value)) // Remove the old mapping first
            {
                if (SubDictionary.ContainsKey(value))
                {
                    SubDictionary.Remove(value);
                }

                PrimaryToSubkeyMapping.Remove(primaryKey);
            }

            SubDictionary[subKey] = primaryKey;
            PrimaryToSubkeyMapping[primaryKey] = subKey;
        }

        public bool TryGetValue(TK2 subKey, out TValue val)
        {
            val = default(TValue);

            if (SubDictionary.TryGetValue(subKey, out var primaryKey))
            {
                return BaseDictionary.TryGetValue(primaryKey, out val);
            }

            return false;
        }

        public bool TryGetValue(TK1 primaryKey, out TValue val)
        {
            return BaseDictionary.TryGetValue(primaryKey, out val);
        }

        public bool TryGetSubKey(TK1 value, out TK2 subKey)
        {
            return PrimaryToSubkeyMapping.TryGetValue(value, out subKey);
        }

        public bool ContainsKey(TK2 subKey)
        {
            return TryGetValue(subKey, out var val);
        }

        public bool ContainsKey(TK1 primaryKey)
        {
            return TryGetValue(primaryKey, out var val);
        }

        public void Remove(TK1 primaryKey)
        {
            if (PrimaryToSubkeyMapping.TryGetValue(primaryKey, out var value))
            {
                if (SubDictionary.ContainsKey(value))
                {
                    SubDictionary.Remove(value);
                }

                PrimaryToSubkeyMapping.Remove(primaryKey);
            }

            BaseDictionary.Remove(primaryKey);
        }

        public void Remove(TK2 subKey)
        {
            BaseDictionary.Remove(SubDictionary[subKey]);
            PrimaryToSubkeyMapping.Remove(SubDictionary[subKey]);
            SubDictionary.Remove(subKey);
        }

        public void Add(TK1 primaryKey, TValue val)
        {
            BaseDictionary.Add(primaryKey, val);
        }

        public void Add(TK1 primaryKey, TK2 subKey, TValue val)
        {
            Add(primaryKey, val);
            Associate(subKey, primaryKey);
        }

        public TValue[] CloneValues()
        {
            var values = new TValue[BaseDictionary.Values.Count];

            BaseDictionary.Values.CopyTo(values, 0);

            return values;
        }

        public TK1[] ClonePrimaryKeys()
        {
            var values = new TK1[BaseDictionary.Keys.Count];

            BaseDictionary.Keys.CopyTo(values, 0);

            return values;
        }

        public TK2[] CloneSubKeys()
        {
            var values = new TK2[SubDictionary.Keys.Count];

            SubDictionary.Keys.CopyTo(values, 0);

            return values;
        }

        public void Clear()
        {
            BaseDictionary.Clear();
            SubDictionary.Clear();
            PrimaryToSubkeyMapping.Clear();
        }

        public IEnumerator<ValueTuple<TK1, TK2, TValue>> GetEnumerator()
        {
            TK1[] list = ClonePrimaryKeys();
            for (int index = list.Length - 1; index >= 0; index--)
            {
                var k = list[index];
                PrimaryToSubkeyMapping.TryGetValue(k, out var v1);
                BaseDictionary.TryGetValue(k, out var v2);
                yield return new ValueTuple<TK1, TK2, TValue>(k, v1, v2);
            }
        }
    }
}