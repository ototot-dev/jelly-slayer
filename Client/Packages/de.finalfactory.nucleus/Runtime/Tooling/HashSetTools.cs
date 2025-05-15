using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using FinalFactory.UIElements;
using JetBrains.Annotations;

namespace FinalFactory.Tooling
{
    [PublicAPI]
    [DebuggerStepThrough]
    public static class HashSetTools
    {
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                hashSet.Add(item);
            }
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void RemoveRange<T>(this HashSet<T> hashSet, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                hashSet.Remove(item);
            }
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void Replace<T>(this HashSet<T> hashSet, IEnumerable<T> items)
        {
            hashSet.Clear();
            hashSet.AddRange(items);
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void ForEach<T>(this HashSet<T> hashSet, System.Action<T> action)
        {
            foreach (var item in hashSet)
            {
                action(item);
            }
        }
    }
}