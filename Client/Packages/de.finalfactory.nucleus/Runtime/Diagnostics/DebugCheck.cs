#if UNITY_EDITOR
    #define DEBUG
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace FinalFactory.Diagnostics
{
    [PublicAPI]
    public static class DebugCheck
    {

        [Conditional("DEBUG")]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void Equal(object objA, object objB, string message = "") => Check.Equal(objA, objB, message);
        
        [Conditional("DEBUG")]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void InRangeList(int listCount, int value, string message = "")
        {
            Check.InRangeList(listCount, value, message);
        }

        [Conditional("DEBUG")]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void InRangeList(ICollection list, int value, string message = "")
        {
            Check.InRangeList(list, value, message);
        }

        [Conditional("DEBUG")]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsTrue(bool value, string message = "")
        {
            Check.IsTrue(value, message);
        }

        [Conditional("DEBUG")]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsFalse(bool value, string message = "")
        {
            Check.IsFalse(value, message);
        }

        [Conditional("DEBUG")]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void Fail(string message = "")
        {
            throw new Exception(message);
        }

        [Conditional("DEBUG")]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void XOrNotNull(object objA, object objB, string message = "")
        {
            Check.XOrNotNull(objA, objB, message);
        }
        
        [AssertionMethod]
        [Conditional("DEBUG")]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void Null([AssertionCondition(AssertionConditionType.IS_NULL)]object obj, string message = "")
        {
            Check.Null(obj, message);
        }

        
        [AssertionMethod]
        [Conditional("DEBUG")]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void NotNull([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]object obj, string message = "")
        {
            Check.NotNull(obj, message);
        }
        
        [Conditional("DEBUG")]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void HasFlag<T>(T thisEnum, T flag, string message = "") where T : struct, Enum
        {
            Check.HasFlag(thisEnum, flag, message);
        }
        
        [Conditional("DEBUG")]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void NotEqual(object objA, object objB, string message = "")
        {
            Check.NotEqual(objA, objB, message);
        }

        [Conditional("DEBUG")]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void If(bool condition, Action action)
        {
            Check.If(condition, action);
        }

        #region Dictionary

        [Conditional("DEBUG")]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void Contains<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key, string message = "")
        {
            Check.IsTrue(dictionary.ContainsKey(key), $"Key {key} not found! Msg: {message}");
        }
        
        
        [Conditional("DEBUG")]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void NotContains<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key, string message = "")
        {
            Check.IsFalse(dictionary.ContainsKey(key), $"Duplicated key {key} found! Msg: {message}");
        }

        #endregion
    }
}