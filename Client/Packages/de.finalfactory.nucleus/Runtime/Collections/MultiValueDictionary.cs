// // ***********************************************************************
// // Author           : Florian Schmidt
// // Created          : 01.10.2019 : 15:17
// // Website          : www.finalfactory.de
// //
// // Last Modified By : Florian Schmidt
// // Last Modified On : 01.10.2019 : 15:32
// // ***********************************************************************
// // <copyright file="MultiValueDictionary.cs" company="Final Factory">
// //     Copyright (c) Final Factory. All rights reserved.
// // </copyright>
// // <summary></summary>
// // ***********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FinalFactory.Collections
{
    /// <summary>
    ///     Multi-Value Dictionary Class
    /// </summary>
    /// <typeparam name="TK">Key Type</typeparam>
    /// <typeparam name="TV1">Value 1 Type</typeparam>
    /// <typeparam name="TV2">Value 2 Type</typeparam>
    public class MultiValueDictionary<TK, TV1, TV2>
    {
        private readonly ReaderWriterLockSlim _readerWriterLock = new ReaderWriterLockSlim();
        internal readonly Dictionary<TK, TV1> V1Dictionary = new Dictionary<TK, TV1>();
        internal readonly Dictionary<TK, TV2> V2Dictionary = new Dictionary<TK, TV2>();

        public ValueTuple<TV1, TV2> this[TK primaryKey]
        {
            get
            {
                ValueTuple<TV1, TV2> item;
                if (TryGetValue(primaryKey, out item))
                    return item;

                throw new KeyNotFoundException("key not found: " + primaryKey);
            }
        }

        public List<TV1> Values1
        {
            get
            {
                _readerWriterLock.EnterReadLock();

                try
                {
                    return V1Dictionary.Values.ToList();
                }
                finally
                {
                    _readerWriterLock.ExitReadLock();
                }
            }
        }

        public List<TV2> Values2
        {
            get
            {
                _readerWriterLock.EnterReadLock();

                try
                {
                    return V2Dictionary.Values.ToList();
                }
                finally
                {
                    _readerWriterLock.ExitReadLock();
                }
            }
        }

        public int Count
        {
            get
            {
                _readerWriterLock.EnterReadLock();

                try
                {
                    return V1Dictionary.Count;
                }
                finally
                {
                    _readerWriterLock.ExitReadLock();
                }
            }
        }

        public TV1 GetV1(TK key)
        {
            TV1 item;
            if (TryGetValue1(key, out item))
                return item;
            throw new KeyNotFoundException("key not found: " + key);
        }

        public TV2 GetV2(TK key)
        {
            TV2 item;
            if (TryGetValue2(key, out item))
                return item;
            throw new KeyNotFoundException("key not found: " + key);
        }

        public bool TryGetValue(TK primaryKey, out ValueTuple<TV1, TV2> val)
        {
            _readerWriterLock.EnterReadLock();

            try
            {
                TV1 v1;
                TV2 v2;
                bool b1 = V1Dictionary.TryGetValue(primaryKey, out v1);
                bool b2 = V2Dictionary.TryGetValue(primaryKey, out v2);
                val = new ValueTuple<TV1, TV2>(v1, v2);
                if (b1 || b2)
                {
                    return true;
                }
            }
            finally
            {
                _readerWriterLock.ExitReadLock();
            }

            return false;
        }

        public bool TryGetValue1(TK primaryKey, out TV1 val)
        {
            _readerWriterLock.EnterReadLock();

            try
            {
                return V1Dictionary.TryGetValue(primaryKey, out val);
            }
            finally
            {
                _readerWriterLock.ExitReadLock();
            }
        }

        public bool TryGetValue2(TK primaryKey, out TV2 val)
        {
            _readerWriterLock.EnterReadLock();

            try
            {
                return V2Dictionary.TryGetValue(primaryKey, out val);
            }
            finally
            {
                _readerWriterLock.ExitReadLock();
            }
        }

        public bool ContainsKey(TK primaryKey)
        {
            TV1 v1;
            TV2 v2;
            bool b1 = V1Dictionary.TryGetValue(primaryKey, out v1);
            bool b2 = V2Dictionary.TryGetValue(primaryKey, out v2);
            return b1 || b2;
        }

        public void Remove(TK primaryKey)
        {
            _readerWriterLock.EnterWriteLock();

            try
            {
                if (V1Dictionary.ContainsKey(primaryKey))
                {
                    V1Dictionary.Remove(primaryKey);
                }

                if (V2Dictionary.ContainsKey(primaryKey))
                {
                    V2Dictionary.Remove(primaryKey);
                }
            }
            finally
            {
                _readerWriterLock.ExitWriteLock();
            }
        }

        public void SetV1(TK primaryKey, TV1 val)
        {
            _readerWriterLock.EnterWriteLock();

            try
            {
                V1Dictionary[primaryKey] = val;
            }
            finally
            {
                _readerWriterLock.ExitWriteLock();
            }
        }

        public void SetV2(TK primaryKey, TV2 val)
        {
            _readerWriterLock.EnterWriteLock();

            try
            {
                V2Dictionary[primaryKey] = val;
            }
            finally
            {
                _readerWriterLock.ExitWriteLock();
            }
        }


        public void Add(TK primaryKey, TV1 val)
        {
            _readerWriterLock.EnterWriteLock();

            try
            {
                V1Dictionary.Add(primaryKey, val);
            }
            finally
            {
                _readerWriterLock.ExitWriteLock();
            }
        }

        public void Add(TK primaryKey, TV2 val)
        {
            _readerWriterLock.EnterWriteLock();

            try
            {
                V2Dictionary.Add(primaryKey, val);
            }
            finally
            {
                _readerWriterLock.ExitWriteLock();
            }
        }

        public void Add(TK primaryKey, TV1 val1, TV2 val2)
        {
            Add(primaryKey, val1);
            Add(primaryKey, val2);
        }

        public TV1[] CloneValues1()
        {
            _readerWriterLock.EnterReadLock();

            try
            {
                var values = new TV1[V1Dictionary.Values.Count];

                V1Dictionary.Values.CopyTo(values, 0);

                return values;
            }
            finally
            {
                _readerWriterLock.ExitReadLock();
            }
        }

        public TV2[] CloneValues2()
        {
            _readerWriterLock.EnterReadLock();

            try
            {
                var values = new TV2[V2Dictionary.Values.Count];

                V2Dictionary.Values.CopyTo(values, 0);

                return values;
            }
            finally
            {
                _readerWriterLock.ExitReadLock();
            }
        }

        public TK[] ClonePrimaryKeys()
        {
            _readerWriterLock.EnterReadLock();

            try
            {
                TK[] k1 = V1Dictionary.Keys.ToArray();
                TK[] k2 = V2Dictionary.Keys.ToArray();
                var list = new HashSet<TK>(k1);
                for (int i = 0; i < k2.Length; i++)
                {
                    if (!list.Contains(k2[i]))
                    {
                        list.Add(k2[i]);
                    }
                }

                return list.ToArray();
            }
            finally
            {
                _readerWriterLock.ExitReadLock();
            }
        }

        public void Clear()
        {
            _readerWriterLock.EnterWriteLock();

            try
            {
                V2Dictionary.Clear();
                V1Dictionary.Clear();
            }
            finally
            {
                _readerWriterLock.ExitWriteLock();
            }
        }

        public IEnumerator<ValueTuple<TK, TV1, TV2>> GetEnumerator()
        {
            TK[] list = ClonePrimaryKeys();
            for (int index = list.Length - 1; index >= 0; index--)
            {
                var k = list[index];
                TV1 v1;
                TV2 v2;
                V1Dictionary.TryGetValue(k, out v1);
                V2Dictionary.TryGetValue(k, out v2);
                yield return new ValueTuple<TK, TV1, TV2>(k, v1, v2);
            }
        }
    }
}