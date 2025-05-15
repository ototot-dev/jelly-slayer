// // ***********************************************************************
// // Author           : Florian Schmidt
// // Created          : .. : 
// // Website          : www.finalfactory.de
// //
// // Last Modified By : Florian Schmidt
// // Last Modified On : 17.01.2020 : 19:32
// // ***********************************************************************
// // <copyright file="Check.cs" company="Final Factory">
// //     Copyright (c) Final Factory. All rights reserved.
// // </copyright>
// // <summary></summary>
// // ***********************************************************************

using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using FinalFactory.Helpers;
using JetBrains.Annotations;

namespace FinalFactory
{
    [PublicAPI]
    public static partial class Check
    {
        private static string _illegalNull;
        
        [DebuggerStepThrough]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static string TextBuilder(string exceptionMsg, string msg)
        {
            if (string.IsNullOrWhiteSpace(msg))
            {
                return exceptionMsg;
            }

            return $"{msg} Reason: {exceptionMsg}";
        }
        
        [DebuggerStepThrough]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void InRangeList(int listCount, int value, string message = "")
        {
            if (value >= 0 && value < listCount)
            {
                return;
            }
            throw new ArgumentOutOfRangeException(TextBuilder($"Value[{value}] must be in range 0-{listCount - 1}!", message));
        }

        [DebuggerStepThrough]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void InRangeList(ICollection list, int value, string message = "") => InRangeList(list.Count, value, message);

        [DebuggerStepThrough]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsTrue(bool value, string message = "")
        {
            if (!value)
            {
                throw new ArgumentOutOfRangeException(message);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsFalse(bool value, string message = "")
        {
            if (value)
            {
                throw new ArgumentOutOfRangeException(message);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void Throw(string message = "") => throw new Exception(message);

        [DebuggerStepThrough]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void XOrNotNull(object objA, object objB, string message = "") => IsFalse(objA == null && objB == null, message);

        [AssertionMethod]
        [DebuggerStepThrough]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void Null([AssertionCondition(AssertionConditionType.IS_NULL)]object obj, string message = "") => IsTrue(obj == null, TextBuilder("Value must be null!", message));

        [AssertionMethod]
        [DebuggerStepThrough]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void NotNull([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]object obj, string message = "") => IsTrue(obj != null, TextBuilder("Value must not be null!", message));

        [DebuggerStepThrough]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void HasFlag<T>(T thisEnum, T flag, string message = "") where T : struct, Enum => IsTrue(Flags.Has(thisEnum, flag), message);

        [DebuggerStepThrough]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void NotNegative(int value, string message = "") => IsTrue(value >= 0, TextBuilder("Value must not be negative!", message));

        [DebuggerStepThrough]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void NotNullAndNullsAreRequired<T>(object value, string message = "")
        {
            if (value != null || default(T) == null)
                return;
            NotNull(null, message);
        }

        [DebuggerStepThrough]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsEqual<T>(object value, string message = "")
        {
            NotNull(value);
            if (typeof(T) != value.GetType())
            {
                throw new ArrayTypeMismatchException(TextBuilder($"Expected Type[{typeof(T).FullName}] Actual Type[{value.GetType().FullName}]!", message));
            }
        }
        
        
        [DebuggerStepThrough]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsEqual<T>(object obj, out T value, string message = "")
        {
            NotNull(obj, message);
            if (typeof(T) != obj.GetType())
            {
                throw new ArrayTypeMismatchException(TextBuilder($"Expected Type[{typeof(T).FullName}] Actual Type[{obj.GetType().FullName}]!", message));
            }
            value = obj is T o ? o : default;
        }
        
        [DebuggerStepThrough]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void Is<T>(object obj, out T value, string message = "")
        {
            NotNull(obj, message);
            value = obj is T v ? v : default;
            if (value == null)
            {
                throw new ArrayTypeMismatchException(TextBuilder($"Expected Type[{typeof(T).FullName}] Actual Type[{obj.GetType().FullName}]!", message));
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void Equal(object objA, object objB, string message = "") => IsTrue(Equals(objA, objB), TextBuilder("Objects must be equal!", message));

        [DebuggerStepThrough]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void NotEqual(object objA, object objB, string message = "") => IsTrue(!Equals(objA, objB), TextBuilder("Objects must not be equal!", message));


        [DebuggerStepThrough]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void Equal<T>(T objA, T objB, string message = "") => IsTrue(Equals(objA, objB), TextBuilder("Objects must be equal!", message));

        [DebuggerStepThrough]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void NotEqual<T>(T objA, T objB, string message = "") => IsTrue(!Equals(objA, objB), TextBuilder("Objects must not be equal!", message));

        [DebuggerStepThrough]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void If(bool condition, Action action)
        {
            if (condition)
            {
                action();
            }
        }
       
        [DebuggerStepThrough]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void NotEmpty(ICollection collection, string message = "") => IsTrue(collection.Count > 0, TextBuilder("Collection must not be empty!", message));

        [DebuggerStepThrough]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void Contains(ICollection actual, object expected, string message = "")
        {
            foreach (var o in actual)
            {
                if (o == expected)
                {
                    return;
                }
            }

            DumpList(actual, TextBuilder($"The list does not contain the expected object[{expected}]!", message));
        }
        
        [DebuggerStepThrough]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void Length(ICollection actual, int expected, string message = "")
        {
            if (actual.Count == expected)
            {
                return;
            }

            DumpList(actual, TextBuilder($"The list does not contain the expected length[{expected}]!", message));
        }

        #region List Checks

        
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void ValidIndex(int value, string message = "")
        {
            if (value != -1) return;
            throw new ArgumentOutOfRangeException(TextBuilder("Index must be valid!", message));
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void ValidIndex(IList list, int index, string message = "")
        {
            if (index >= 0 && list.Count > index)
            {
                return;
            }
            throw new ArgumentOutOfRangeException(TextBuilder($"Index[{index}] must be in range 0-{list.Count - 1}!", message));
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void ValidIndex(int index, int length, string message = "")
        {
            if (index >= 0 && index < length) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Index[{index}] must be in range 0-{length - 1}!", message));
        }

        #endregion
        
        private static void DumpList(ICollection actual, string message)
        {
            var s = new StringBuilder();

            foreach (var o in actual)
            {
                s.AppendLine(o.ToString());
            }

            Throw($"{message} \n Dump: {s}");
        }
    }
}