// // ***********************************************************************
// // Author           : Florian Schmidt
// // Created          : 01.10.2019 : 15:17
// // Website          : www.finalfactory.de
// //
// // Last Modified By : Florian Schmidt
// // Last Modified On : 01.10.2019 : 15:32
// // ***********************************************************************
// // <copyright file="DualLinkList.cs" company="Final Factory">
// //     Copyright (c) Final Factory. All rights reserved.
// // </copyright>
// // <summary></summary>
// // ***********************************************************************

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace FinalFactory.Collections
{
    /// <summary>
    ///     DualLinkList 
    ///     Linkt Key mit Value und Value mit Key
    /// </summary>
    /// <typeparam name="TKey">Key Type</typeparam>
    /// <typeparam name="TValue">Value 1 Type</typeparam>
    [DebuggerDisplay("DualLinkList {" + nameof(_keyDic) + "}")]
    public class DualDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Dictionary<TKey, TValue> _keyDic = new();
        private readonly Dictionary<TValue, TKey> _valDic = new();

        public TValue this[TKey key]
        {
            get
            {
                if (TryGetValue(key, out var item))
                    return item;

                throw new KeyNotFoundException("key not found: " + key);
            }
            set
            {
                if (_keyDic.ContainsKey(key))
                {
                    _valDic.Remove(_keyDic[key]);
                    _keyDic.Remove(key);
                }

                _keyDic.Add(key, value);
                _valDic.Add(value, key);
            }
        }

        public TKey this[TValue valueItem]
        {
            get
            {
                if (TryGetKey(valueItem, out var item))
                    return item;

                throw new KeyNotFoundException("value not found: " + valueItem);
            }
            set
            {
                if (_valDic.ContainsKey(valueItem))
                {
                    _keyDic.Remove(_valDic[valueItem]);
                    _valDic.Remove(valueItem);
                }

                _keyDic.Add(value, valueItem);
                _valDic.Add(valueItem, value);
            }
        }

        public Dictionary<TKey, TValue>.KeyCollection Key => _keyDic.Keys;

        public Dictionary<TKey, TValue>.ValueCollection Values => _keyDic.Values;

        public int Count => _keyDic.Count;

        public bool TryGetValue(TKey key, out TValue val) => _keyDic.TryGetValue(key, out val);

        public bool TryGetKey(TValue value, out TKey key) => _valDic.TryGetValue(value, out key);

        public bool ContainsKey(TKey primaryKey) => _keyDic.ContainsKey(primaryKey);

        public bool ContainsValue(TValue primaryKey) => _valDic.ContainsKey(primaryKey);

        public void ChangeKey(TKey oldKey, TKey newKey)
        {
            if (_keyDic.Remove(oldKey, out var value))
            {
                _keyDic.Add(newKey, value);
                _valDic[value] = newKey;
            }
        }
        
        public void Remove(TKey primaryKey)
        {
            if (_keyDic.ContainsKey(primaryKey))
            {
                _valDic.Remove(_keyDic[primaryKey]);
                _keyDic.Remove(primaryKey);
            }
        }

        public void Remove(TValue value)
        {
            if (_valDic.ContainsKey(value))
            {
                _keyDic.Remove(_valDic[value]);
                _valDic.Remove(value);
            }
        }

        public void Add(TKey key, TValue val)
        {
            _keyDic.Add(key, val);
            _valDic.Add(val, key);
        }

        public void Clear()
        {
            _keyDic.Clear();
            _valDic.Clear();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (KeyValuePair<TKey, TValue> keyValuePair in _keyDic)
            {
                yield return keyValuePair;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}