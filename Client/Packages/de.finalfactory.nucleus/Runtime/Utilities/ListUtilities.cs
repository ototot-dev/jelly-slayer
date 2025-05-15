using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace FinalFactory.Utilities
{
    [PublicAPI]
    [DebuggerStepThrough]
    public static class ListUtilities
    {
        /// <summary>
        /// Pops the last item from the list.
        /// </summary>
        /// <param name="list"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static T Pop<T>(this IList<T> list)
        {
            if (list.Count <= 0) return default(T);

            var temp = list[^1];
            list.RemoveAt(list.Count - 1);
            return temp;
        }
        
        /// <summary>
        /// Pops the item at the specified index.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static T PopAt<T>(this IList<T> list, int index)
        {
            if (index < 0 || index >= list.Count)
            {
                return default;
            }
            var temp = list[index];
            list.RemoveAt(index);
            return temp;
        }
        
        /// <summary>
        /// Returns the last item in the list without removing it.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static T Peek<T>(this IList<T> list, int index = -1)
        {
            if (list.Count <= 0) return default(T);

            if (index < 0)
            {
                index = list.Count - 1;
            }

            return list[index];
        }
        
        /// <summary>
        /// Push a new item to the list.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="item"></param>
        /// <typeparam name="T"></typeparam>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void Push<T>(this IList<T> list, T item) => list.Add(item);
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static T GetOrDefaultAt<T>(this IList<T> list, int index, T defaultValue = default)
        {
            if (index < 0 || index >= list.Count)
            {
                return defaultValue;
            }
            return list[index];
        }
        
        /// <summary>
        /// Get or create an item at the specified index.
        /// If the index is out of bounds, the list will be resized and filled with the default value.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <param name="defaultValue"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static T GetOrCreate<T>(this IList<T> list, int index, T defaultValue = default)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            while (index >= list.Count)
            {
                list.Add(defaultValue);
            }
            return list[index];
        }
        
        /// <summary>
        /// Replaces the item at the specified index.
        /// Returns the old item.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <param name="item"></param>
        /// <typeparam name="T"></typeparam>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static T ReplaceAt<T>(this IList<T> list, T item, int index)
        {
            if (index < 0 || index >= list.Count)
            {
                return default;
            }
            var temp = list[index];
            list[index] = item;
            return temp;
        }
        
        /// <summary>
        /// Clears the list and adds the specified items.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="items"></param>
        /// <typeparam name="T"></typeparam>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void Replace<T>(this IList<T> list, IEnumerable<T> items)
        {
            list.Clear();
            list.AddRange(items);
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void ToList<TSource>(this IEnumerable<TSource> source, List<TSource> list)
        {
            list.Clear();
            list.AddRange(source);
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void ToList<TSource>(this IEnumerable<TSource> source, ref List<TSource> list)
        {
            if (list == null)
            {
                list = new List<TSource>(source);
            }
            else
            {
                list.Clear();
                list.AddRange(source);
            }
        }
    }
}