

//###########################################
//This file is auto-generated. Do not modify!
//###########################################

#region License
// // --------------------------------------------------------------------------------------------------------------------
// // <summary>
// //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// //   PrefEditorStyle.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------
#endregion
// Because of the auto generation there are some redundant type casts.
// ReSharper disable ArrangeObjectCreationWhenTypeEvident
// ReSharper disable RedundantCast
// ReSharper disable RedundantUsingDirective
// ReSharper disable InconsistentNaming
using JetBrains.Annotations;
using FinalFactory;
using FinalFactory.Mathematics;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
#if UNITY_MATHEMATICS
using Unity.Mathematics;
#endif
namespace FinalFactory
{
    // ReSharper disable once InconsistentNaming
    public static partial class Check 
    {
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void InRange(byte min, byte max, byte value, string message = "")
        {
            if (value > max || value < min)
            {
                throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be greater than {min} and smaller than {max}.", message));
            }
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void InsideRange(byte min, byte max, byte value, string message = "")
        {
            if (value >= max || value <= min)
            {
                throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be at least {min} and can not exeed {max}.", message));
            }
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsSmallerThan(byte value, byte min, string message = "")
        {
            if (value < min) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be smaller than {min}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsGreaterThan(byte value, byte max, string message = "")
        {
            if (value > max) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be greater than {max}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsSmallerOrEqualThan(byte value, byte min, string message = "")
        {
            if (value >= min) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be smaller or equal than {min}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsGreaterOrEqualThan(byte value, byte min, string message = "")
        {
            if (value >= min) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be greater or equal than {min}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsEqual(byte value, byte expected, string message = "")
        {
            if (value == expected) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} is not equal with {expected}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsNotEqual(byte value, byte notExpected, string message = "")
        {
            if (value != notExpected) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} is equal with {notExpected}.", message));
        }
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void InRange(sbyte min, sbyte max, sbyte value, string message = "")
        {
            if (value > max || value < min)
            {
                throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be greater than {min} and smaller than {max}.", message));
            }
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void InsideRange(sbyte min, sbyte max, sbyte value, string message = "")
        {
            if (value >= max || value <= min)
            {
                throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be at least {min} and can not exeed {max}.", message));
            }
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsSmallerThan(sbyte value, sbyte min, string message = "")
        {
            if (value < min) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be smaller than {min}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsGreaterThan(sbyte value, sbyte max, string message = "")
        {
            if (value > max) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be greater than {max}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsSmallerOrEqualThan(sbyte value, sbyte min, string message = "")
        {
            if (value >= min) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be smaller or equal than {min}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsGreaterOrEqualThan(sbyte value, sbyte min, string message = "")
        {
            if (value >= min) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be greater or equal than {min}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsEqual(sbyte value, sbyte expected, string message = "")
        {
            if (value == expected) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} is not equal with {expected}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsNotEqual(sbyte value, sbyte notExpected, string message = "")
        {
            if (value != notExpected) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} is equal with {notExpected}.", message));
        }
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void InRange(short min, short max, short value, string message = "")
        {
            if (value > max || value < min)
            {
                throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be greater than {min} and smaller than {max}.", message));
            }
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void InsideRange(short min, short max, short value, string message = "")
        {
            if (value >= max || value <= min)
            {
                throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be at least {min} and can not exeed {max}.", message));
            }
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsSmallerThan(short value, short min, string message = "")
        {
            if (value < min) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be smaller than {min}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsGreaterThan(short value, short max, string message = "")
        {
            if (value > max) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be greater than {max}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsSmallerOrEqualThan(short value, short min, string message = "")
        {
            if (value >= min) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be smaller or equal than {min}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsGreaterOrEqualThan(short value, short min, string message = "")
        {
            if (value >= min) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be greater or equal than {min}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsEqual(short value, short expected, string message = "")
        {
            if (value == expected) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} is not equal with {expected}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsNotEqual(short value, short notExpected, string message = "")
        {
            if (value != notExpected) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} is equal with {notExpected}.", message));
        }
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void InRange(ushort min, ushort max, ushort value, string message = "")
        {
            if (value > max || value < min)
            {
                throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be greater than {min} and smaller than {max}.", message));
            }
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void InsideRange(ushort min, ushort max, ushort value, string message = "")
        {
            if (value >= max || value <= min)
            {
                throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be at least {min} and can not exeed {max}.", message));
            }
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsSmallerThan(ushort value, ushort min, string message = "")
        {
            if (value < min) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be smaller than {min}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsGreaterThan(ushort value, ushort max, string message = "")
        {
            if (value > max) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be greater than {max}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsSmallerOrEqualThan(ushort value, ushort min, string message = "")
        {
            if (value >= min) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be smaller or equal than {min}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsGreaterOrEqualThan(ushort value, ushort min, string message = "")
        {
            if (value >= min) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be greater or equal than {min}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsEqual(ushort value, ushort expected, string message = "")
        {
            if (value == expected) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} is not equal with {expected}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsNotEqual(ushort value, ushort notExpected, string message = "")
        {
            if (value != notExpected) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} is equal with {notExpected}.", message));
        }
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void InRange(int min, int max, int value, string message = "")
        {
            if (value > max || value < min)
            {
                throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be greater than {min} and smaller than {max}.", message));
            }
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void InsideRange(int min, int max, int value, string message = "")
        {
            if (value >= max || value <= min)
            {
                throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be at least {min} and can not exeed {max}.", message));
            }
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsSmallerThan(int value, int min, string message = "")
        {
            if (value < min) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be smaller than {min}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsGreaterThan(int value, int max, string message = "")
        {
            if (value > max) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be greater than {max}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsSmallerOrEqualThan(int value, int min, string message = "")
        {
            if (value >= min) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be smaller or equal than {min}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsGreaterOrEqualThan(int value, int min, string message = "")
        {
            if (value >= min) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be greater or equal than {min}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsEqual(int value, int expected, string message = "")
        {
            if (value == expected) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} is not equal with {expected}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsNotEqual(int value, int notExpected, string message = "")
        {
            if (value != notExpected) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} is equal with {notExpected}.", message));
        }
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void InRange(uint min, uint max, uint value, string message = "")
        {
            if (value > max || value < min)
            {
                throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be greater than {min} and smaller than {max}.", message));
            }
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void InsideRange(uint min, uint max, uint value, string message = "")
        {
            if (value >= max || value <= min)
            {
                throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be at least {min} and can not exeed {max}.", message));
            }
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsSmallerThan(uint value, uint min, string message = "")
        {
            if (value < min) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be smaller than {min}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsGreaterThan(uint value, uint max, string message = "")
        {
            if (value > max) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be greater than {max}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsSmallerOrEqualThan(uint value, uint min, string message = "")
        {
            if (value >= min) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be smaller or equal than {min}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsGreaterOrEqualThan(uint value, uint min, string message = "")
        {
            if (value >= min) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be greater or equal than {min}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsEqual(uint value, uint expected, string message = "")
        {
            if (value == expected) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} is not equal with {expected}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsNotEqual(uint value, uint notExpected, string message = "")
        {
            if (value != notExpected) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} is equal with {notExpected}.", message));
        }
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void InRange(long min, long max, long value, string message = "")
        {
            if (value > max || value < min)
            {
                throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be greater than {min} and smaller than {max}.", message));
            }
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void InsideRange(long min, long max, long value, string message = "")
        {
            if (value >= max || value <= min)
            {
                throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be at least {min} and can not exeed {max}.", message));
            }
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsSmallerThan(long value, long min, string message = "")
        {
            if (value < min) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be smaller than {min}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsGreaterThan(long value, long max, string message = "")
        {
            if (value > max) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be greater than {max}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsSmallerOrEqualThan(long value, long min, string message = "")
        {
            if (value >= min) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be smaller or equal than {min}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsGreaterOrEqualThan(long value, long min, string message = "")
        {
            if (value >= min) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be greater or equal than {min}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsEqual(long value, long expected, string message = "")
        {
            if (value == expected) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} is not equal with {expected}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsNotEqual(long value, long notExpected, string message = "")
        {
            if (value != notExpected) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} is equal with {notExpected}.", message));
        }
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void InRange(ulong min, ulong max, ulong value, string message = "")
        {
            if (value > max || value < min)
            {
                throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be greater than {min} and smaller than {max}.", message));
            }
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void InsideRange(ulong min, ulong max, ulong value, string message = "")
        {
            if (value >= max || value <= min)
            {
                throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be at least {min} and can not exeed {max}.", message));
            }
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsSmallerThan(ulong value, ulong min, string message = "")
        {
            if (value < min) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be smaller than {min}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsGreaterThan(ulong value, ulong max, string message = "")
        {
            if (value > max) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be greater than {max}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsSmallerOrEqualThan(ulong value, ulong min, string message = "")
        {
            if (value >= min) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be smaller or equal than {min}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsGreaterOrEqualThan(ulong value, ulong min, string message = "")
        {
            if (value >= min) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be greater or equal than {min}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsEqual(ulong value, ulong expected, string message = "")
        {
            if (value == expected) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} is not equal with {expected}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsNotEqual(ulong value, ulong notExpected, string message = "")
        {
            if (value != notExpected) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} is equal with {notExpected}.", message));
        }
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void InRange(double min, double max, double value, string message = "")
        {
            if (value > max || value < min)
            {
                throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be greater than {min} and smaller than {max}.", message));
            }
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void InsideRange(double min, double max, double value, string message = "")
        {
            if (value >= max || value <= min)
            {
                throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be at least {min} and can not exeed {max}.", message));
            }
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsSmallerThan(double value, double min, string message = "")
        {
            if (value < min) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be smaller than {min}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsGreaterThan(double value, double max, string message = "")
        {
            if (value > max) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be greater than {max}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsSmallerOrEqualThan(double value, double min, string message = "")
        {
            if (value >= min) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be smaller or equal than {min}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsGreaterOrEqualThan(double value, double min, string message = "")
        {
            if (value >= min) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be greater or equal than {min}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsEqual(double value, double expected, string message = "")
        {
            if (value.Equal(expected)) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} is not equal with {expected}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsNotEqual(double value, double notExpected, string message = "")
        {
            if (!value.Equal(notExpected)) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} is equal with {notExpected}.", message));
        }
        
        /// <summary>
        /// Checks if the value is not NaN.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static void IsNotNaN(this double value, string message = "")        
        {
            if (!double.IsNaN(value)) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must not be NaN.", message));
        }
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void InRange(float min, float max, float value, string message = "")
        {
            if (value > max || value < min)
            {
                throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be greater than {min} and smaller than {max}.", message));
            }
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void InsideRange(float min, float max, float value, string message = "")
        {
            if (value >= max || value <= min)
            {
                throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be at least {min} and can not exeed {max}.", message));
            }
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsSmallerThan(float value, float min, string message = "")
        {
            if (value < min) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be smaller than {min}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsGreaterThan(float value, float max, string message = "")
        {
            if (value > max) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be greater than {max}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsSmallerOrEqualThan(float value, float min, string message = "")
        {
            if (value >= min) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be smaller or equal than {min}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsGreaterOrEqualThan(float value, float min, string message = "")
        {
            if (value >= min) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be greater or equal than {min}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsEqual(float value, float expected, string message = "")
        {
            if (value.Equal(expected)) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} is not equal with {expected}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsNotEqual(float value, float notExpected, string message = "")
        {
            if (!value.Equal(notExpected)) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} is equal with {notExpected}.", message));
        }
        
        /// <summary>
        /// Checks if the value is not NaN.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static void IsNotNaN(this float value, string message = "")        
        {
            if (!float.IsNaN(value)) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must not be NaN.", message));
        }
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void InRange(char min, char max, char value, string message = "")
        {
            if (value > max || value < min)
            {
                throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be greater than {min} and smaller than {max}.", message));
            }
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void InsideRange(char min, char max, char value, string message = "")
        {
            if (value >= max || value <= min)
            {
                throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be at least {min} and can not exeed {max}.", message));
            }
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsSmallerThan(char value, char min, string message = "")
        {
            if (value < min) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be smaller than {min}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsGreaterThan(char value, char max, string message = "")
        {
            if (value > max) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be greater than {max}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsSmallerOrEqualThan(char value, char min, string message = "")
        {
            if (value >= min) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be smaller or equal than {min}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsGreaterOrEqualThan(char value, char min, string message = "")
        {
            if (value >= min) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} must be greater or equal than {min}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsEqual(char value, char expected, string message = "")
        {
            if (value == expected) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} is not equal with {expected}.", message));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void IsNotEqual(char value, char notExpected, string message = "")
        {
            if (value != notExpected) return;
            throw new ArgumentOutOfRangeException(TextBuilder($"Value {value} is equal with {notExpected}.", message));
        }
    }
}