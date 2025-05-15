using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using FinalFactory.Mathematics;
using FinalFactory.UIElements;
using JetBrains.Annotations;

namespace FinalFactory
{
    [PublicAPI]
    public static class CollectionHelper
    {
        #region ICollection
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool AddIfNotNull<T>(this ICollection<T> collection, T item)
        {
            if (item is not null)
            {
                collection.Add(item);
                return true;
            }

            return false;
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool AddIfNotExists<T>(this ICollection<T> collection, T item)
        {
            if (!collection.Contains(item))
            {
                collection.Add(item);
                return true;
            }

            return false;
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void AddFor<T>(this ICollection<T> srcCollection, int count) where T : new()
        {
            for (int i = 0; i < count; i++)
            {
                srcCollection.Add(new T());
            }
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void AddRange<T>(this ICollection<T> srcCollection, IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                srcCollection.Add(item);
            }
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void AddRangeIfNotExists<T>(this ICollection<T> srcCollection, IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                if (!srcCollection.Contains(item))
                {
                    srcCollection.Add(item);
                }
            }
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void AddRangeIfNotNull<T>(this ICollection<T> srcCollection, IEnumerable<T> collection)
        {
            if (collection != null)
            {
                foreach (var item in collection)
                {
                    if (item != null)
                    {
                        srcCollection.Add(item);
                    }
                }
            }
        }

        
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static int OverflowIndex<T>(this ICollection<T> srcCollection, int index) => OverflowIndex(index,  srcCollection.Count);

        [MethodImpl(GlobalConst.ImplOptions)]
        public static int OverflowIndexNG(this ICollection srcCollection, int index) => OverflowIndex(index, srcCollection.Count);
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static int OverflowIndex(int index, int length, int start = 0)
        {
            length -= start;
            var lastValidIndex = length - 1;
            while (index > lastValidIndex)
            {
                index -= length;
            }

            while (index < start)
            {
                index += length;
            }

            return index;
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsIndexValid<T>(this ICollection<T> srcCollection, int index) => index >= 0 && srcCollection.Count > index;

        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsIndexValidNG(this ICollection srcCollection, int index) => index >= 0 && srcCollection.Count > index;
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static int NextValidIndex<T>(this ICollection<T> srcCollection, ref int index) => index = NextValidIndex(srcCollection, index);

        [MethodImpl(GlobalConst.ImplOptions)]
        public static int NextValidIndex<T>(this ICollection<T> srcCollection, int index)
        {
            index++;
            if (index >= srcCollection.Count)
            {
                index = 0;
            }
            return index;
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static int NextValidIndexNG(this ICollection srcCollection, int index)
        {
            index++;
            if (index >= srcCollection.Count)
            {
                index = 0;
            }
            return index;
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static int NextValidIndexNG(this ICollection srcCollection, ref int index) => index = NextValidIndexNG(srcCollection, index);
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static int PreviousValidIndex<T>(this ICollection<T> srcCollection, int index)
        {
            index--;
            if (index < 0)
            {
                index = srcCollection.Count - 1;
            }
            return index;
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static int PreviousValidIndex<T>(this ICollection<T> srcCollection, ref int index) => index = PreviousValidIndex(srcCollection, index);
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static int PreviousValidIndexNG(this ICollection srcCollection, int index)
        {
            index--;
            if (index < 0)
            {
                index = srcCollection.Count - 1;
            }
            return index;
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static int PreviousValidIndexNG(this ICollection srcCollection, ref int index) => index = PreviousValidIndexNG(srcCollection, index);
        
        /// <summary>
        /// Disposes every item and clears the list.
        /// </summary>
        /// <param name="srcCollection"></param>
        /// <typeparam name="T"></typeparam>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void Dispose<T>(this IList<T> srcCollection) where T : IDisposable
        {
            foreach (var disposable in srcCollection)
            {
                disposable.Dispose();
            }
            
            //check if srcCollection is a array
            if (srcCollection is T[] array)
            {
                Array.Clear(array, 0, array.Length);
            }
            else
            {
                srcCollection.Clear();
            }
        }

        public static void RemoveWhere<T>(this ICollection<T> collection, Func<T, bool> condition, bool onlyFirstMatch = false)
        {
            foreach (var obj in collection.Where(condition).ToArray())
            {
                collection.Remove(obj);
                if (onlyFirstMatch)
                {
                    return;
                }
            }
        }
        
        public static void RemoveWhere<T>(this ICollection<T> collection, IEnumerable<T> itemsToRemove, bool onlyFirstMatch = false)
        {
            foreach (var obj in itemsToRemove)
            {
                collection.Remove(obj);
                if (onlyFirstMatch)
                {
                    return;
                }
            }
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void RemoveRange<T>(this ICollection<T> srcCollection, IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                srcCollection.Remove(item);
            }
        }

        #endregion

        #region IDictionary

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> srcDictionary, IEnumerable<(TKey, TValue)> collection)
        {
            foreach (var item in collection)
            {
                srcDictionary.Add(item.Item1, item.Item2);
            }
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> srcDictionary, IEnumerable<TKey> keys, IEnumerable<TValue> values)
        {
            using var enumerator = values.GetEnumerator();
            foreach (var item in keys)
            {
                if (!enumerator.MoveNext())
                {
                    Check.Throw("Keys and Values are not equal long.");
                }
                srcDictionary.Add(item, enumerator.Current);
            }
            if (enumerator.MoveNext())
            {
                Check.Throw("Keys and Values are not equal long.");
            }
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool AddIfNotExists<TKey, TValue>(this IDictionary<TKey, TValue> srcDictionary, TKey key, TValue value)
        {
            if (!srcDictionary.ContainsKey(key))
            {
                srcDictionary.Add(key, value);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Add or updates a value in the dictionary.
        /// </summary>
        /// <param name="srcDictionary"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns>Returns true, if the value was added. Returns false, if the value was updated.</returns>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> srcDictionary, TKey key, TValue value)
        {
            if (!srcDictionary.ContainsKey(key))
            {
                srcDictionary.Add(key, value);
                return true;
            }

            srcDictionary[key] = value;
            return false;
        }

        /// <summary>
        /// Add or updates a value in the dictionary.
        /// </summary>
        /// <param name="srcDictionary"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns>Returns true, if the a value was added. Returns false, if all values have just been updated.</returns>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> srcDictionary, IDictionary<TKey, TValue> otherDictionary)
        {
            var result = false;
            foreach (KeyValuePair<TKey,TValue> keyValuePair in otherDictionary)
            {
                result |= srcDictionary.AddOrUpdate(keyValuePair.Key, keyValuePair.Value);
            }
            return result;
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void ForEach<TKey, TValue>(this IDictionary<TKey, TValue> srcDictionary, Action<TValue> action, bool copy = false)
        {
            var list = srcDictionary.Values;
            if (copy)
            {
                list = list.ToArray();
            }
            foreach (var t in list) action(t);
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> srcDictionary, TKey key)
        {
            srcDictionary.TryGetValue(key, out var value);
            return value;
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> srcDictionary, TKey key, TValue defaultValue)
        {
            return srcDictionary.TryGetValue(key, out var value) ? value : defaultValue;
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> srcDictionary, TKey key) where TValue : new()
        {
            if (srcDictionary.TryGetValue(key, out var value))
            {
                return value;
            }

            value = new TValue();
            srcDictionary.Add(key, value);
            return value;
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> srcDictionary, TKey key, Func<TValue> constructor)
        {
            if (srcDictionary.TryGetValue(key, out var value))
            {
                return value;
            }

            value = constructor();
            srcDictionary.Add(key, value);
            return value;
        }
        
        
        /// <summary>
        /// Get a value from the dictionary or create a new one if it doesn't exist.
        /// </summary>
        /// <param name="srcDictionary"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns>Returns true if the value was created</returns>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> srcDictionary, TKey key, out TValue value) where TValue : new()
        {
            if (srcDictionary.TryGetValue(key, out value))
            {
                return false;
            }

            value = new TValue();
            srcDictionary.Add(key, value);
            return true;
        }
        
        /// <summary>
        /// Get a value from the dictionary or create a new one if it doesn't exist.
        /// </summary>
        /// <param name="srcDictionary"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns>Returns true if the value was created</returns>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> srcDictionary, TKey key, out TValue value, Func<TValue> constructor)
        {
            if (srcDictionary.TryGetValue(key, out value))
            {
                return false;
            }

            value = constructor();
            srcDictionary.Add(key, value);
            return true;
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static TValue Pop<TKey, TValue>(this IDictionary<TKey, TValue> srcDictionary, TKey key)
        {
            var value = srcDictionary[key];
            srcDictionary.Remove(key);
            return value;
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        [Obsolete("Use Remove instead.")]
        public static bool TryPop<TKey, TValue>(this IDictionary<TKey, TValue> srcDictionary, TKey key, out TValue value)
        {
            return srcDictionary.Remove(key, out value);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool TryGetValueAs<TKey, TValue, T>(this IDictionary<TKey, TValue> srcDictionary, TKey key, out T value)
        {
            if (srcDictionary.TryGetValue(key, out var obj))
            {
                if (obj is T t)
                {
                    value = t;
                    return true;
                }
            }

            value = default;
            return false;
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static int GetKey(int a, int b)
        {
            return a << 16 | b;
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsNullOrEmpty(this IDictionary list) => list == null || list.Count == 0;

        [MethodImpl(GlobalConst.ImplOptions)]
        public static IEnumerable<KeyValuePair<TKey, TValue>> Where<TKey, TValue>(this IDictionary<TKey, TValue> srcDictionary, IEnumerable<TKey> keys)
        {
            var hash = keys.ToHashSet();
            foreach (var kv in srcDictionary)
            {
                if (hash.Contains(kv.Key))
                {
                    yield return kv;
                }
            }
        }
        
        //method expect for dictionary with key list
        [MethodImpl(GlobalConst.ImplOptions)]
        public static IEnumerable<KeyValuePair<TKey, TValue>> Except<TKey, TValue>(this IDictionary<TKey, TValue> srcDictionary, HashSet<TKey> keys)
        {
            return srcDictionary.Where(kv => !keys.Contains(kv.Key));
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static IEnumerable<KeyValuePair<TKey, TValue>> Except<TKey, TValue>(this IDictionary<TKey, TValue> srcDictionary, IEnumerable<TKey> keys)
        {
            var hash = keys.ToHashSet();
            return srcDictionary.Where(kv => !hash.Contains(kv.Key));
        }
        
        #endregion

        #region Stack
        
        /// <summary>
        /// Pop all items from stack and add to collection
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="collection"></param>
        /// <typeparam name="T"></typeparam>
        public static void PopAll<T>(this Stack<T> stack, ICollection<T> collection)
        {
            while (stack.Count > 0)
            {
                collection.Add(stack.Pop());
            }
        }
        
        /// <summary>
        /// Pop all items from stack and add to list
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="list"></param>
        /// <typeparam name="T"></typeparam>
        public static void PopAll<T>(this Stack<T> stack, List<T> list)
        {
            while (stack.Count > 0)
            {
                list.Add(stack.Pop());
            }
        }
        
        /// <summary>
        /// Pop all items from stack and add to array
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="array"></param>
        /// <typeparam name="T"></typeparam>
        public static void PopAll<T>(this Stack<T> stack, T[] array)
        {
            var i = array.Length - 1;
            while (stack.Count > 0)
            {
                array[i--] = stack.Pop();
            }
        }
        
        /// <summary>
        /// Pop all items from stack and add to list until count is reached
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="list"></param>
        /// <param name="count"></param>
        /// <typeparam name="T"></typeparam>
        public static void PopAll<T>(this Stack<T> stack, List<T> list, int count)
        {
            while (stack.Count > 0 && count > 0)
            {
                list.Add(stack.Pop());
                count--;
            }
        }
        
        
        /// <summary>
        /// Pop all items from stack and add to collection until count is reached
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="collection"></param>
        /// <param name="count"></param>
        /// <typeparam name="T"></typeparam>
        public static void PopAll<T>(this Stack<T> stack, ICollection<T> collection, int count)
        {
            while (stack.Count > 0 && count > 0)
            {
                collection.Add(stack.Pop());
                count--;
            }
        }
        
        /// <summary>
        /// Pop all items from stack and call action for each item
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="popAction"></param>
        /// <typeparam name="T"></typeparam>
        public static void PopAll<T>(this Stack<T> stack, Action<T> popAction)
        {
            while (stack.Count > 0)
            {
                popAction(stack.Pop());
            }
        }
        

        #endregion
    }
}