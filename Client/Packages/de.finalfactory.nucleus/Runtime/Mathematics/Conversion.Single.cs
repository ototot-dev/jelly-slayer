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
using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using FinalFactory.UIElements;
using System.Diagnostics.CodeAnalysis;
namespace FinalFactory.Mathematics
{
    public static partial class Conversions 
    {
            [MethodImpl(GlobalConst.ImplOptions)]
            public static float ToFloat(this string value) => float.Parse(value, CultureInfo.InvariantCulture);
        
        /// <summary>
        /// Convert to float
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static float ToFloat(this object value, in float defaultValue = default)        
        {
            return value is float v ? v : default;
        }
        
        /// <summary>
        /// Parse to float
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static float TryParseToFloat(this object value, in float defaultValue = default)        
        {
            if (float.TryParse(value?.ToString(), out var result))
            {
	            return result;
            }
            return default;
        }
            [MethodImpl(GlobalConst.ImplOptions)]
            public static bool ToBool(this float value) => value > default(float);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static bool ToBool(this float? value, float defaultValue = default) => (value ?? defaultValue) > default(float);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static byte ToByte(this float value) => (byte)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static byte ToByte(this float? value, float defaultValue = default) => (byte)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static sbyte ToSByte(this float value) => (sbyte)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static sbyte ToSByte(this float? value, float defaultValue = default) => (sbyte)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static short ToShort(this float value) => (short)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static short ToShort(this float? value, float defaultValue = default) => (short)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ushort ToUShort(this float value) => (ushort)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ushort ToUShort(this float? value, float defaultValue = default) => (ushort)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static int ToInt(this float value) => (int)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static int ToInt(this float? value, float defaultValue = default) => (int)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static uint ToUInt(this float value) => (uint)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static uint ToUInt(this float? value, float defaultValue = default) => (uint)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static long ToLong(this float value) => (long)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static long ToLong(this float? value, float defaultValue = default) => (long)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ulong ToULong(this float value) => (ulong)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ulong ToULong(this float? value, float defaultValue = default) => (ulong)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static double ToDouble(this float value) => (double)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static double ToDouble(this float? value, float defaultValue = default) => (double)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static char ToChar(this float value) => (char)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static char ToChar(this float? value, float defaultValue = default) => (char)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static double ToDouble(this string value) => double.Parse(value, CultureInfo.InvariantCulture);
        
        /// <summary>
        /// Convert to double
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static double ToDouble(this object value, in double defaultValue = default)        
        {
            return value is double v ? v : default;
        }
        
        /// <summary>
        /// Parse to double
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static double TryParseToDouble(this object value, in double defaultValue = default)        
        {
            if (double.TryParse(value?.ToString(), out var result))
            {
	            return result;
            }
            return default;
        }
            [MethodImpl(GlobalConst.ImplOptions)]
            public static bool ToBool(this double value) => value > default(double);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static bool ToBool(this double? value, double defaultValue = default) => (value ?? defaultValue) > default(double);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static byte ToByte(this double value) => (byte)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static byte ToByte(this double? value, double defaultValue = default) => (byte)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static sbyte ToSByte(this double value) => (sbyte)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static sbyte ToSByte(this double? value, double defaultValue = default) => (sbyte)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static short ToShort(this double value) => (short)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static short ToShort(this double? value, double defaultValue = default) => (short)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ushort ToUShort(this double value) => (ushort)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ushort ToUShort(this double? value, double defaultValue = default) => (ushort)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static int ToInt(this double value) => (int)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static int ToInt(this double? value, double defaultValue = default) => (int)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static uint ToUInt(this double value) => (uint)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static uint ToUInt(this double? value, double defaultValue = default) => (uint)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static long ToLong(this double value) => (long)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static long ToLong(this double? value, double defaultValue = default) => (long)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ulong ToULong(this double value) => (ulong)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ulong ToULong(this double? value, double defaultValue = default) => (ulong)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static float ToFloat(this double value) => (float)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static float ToFloat(this double? value, double defaultValue = default) => (float)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static char ToChar(this double value) => (char)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static char ToChar(this double? value, double defaultValue = default) => (char)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static short ToShort(this string value) => short.Parse(value, CultureInfo.InvariantCulture);
        
        /// <summary>
        /// Convert to short
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static short ToShort(this object value, in short defaultValue = default)        
        {
            return value is short v ? v : default;
        }
        
        /// <summary>
        /// Parse to short
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static short TryParseToShort(this object value, in short defaultValue = default)        
        {
            if (short.TryParse(value?.ToString(), out var result))
            {
	            return result;
            }
            return default;
        }
            [MethodImpl(GlobalConst.ImplOptions)]
            public static bool ToBool(this short value) => value > default(short);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static bool ToBool(this short? value, short defaultValue = default) => (value ?? defaultValue) > default(short);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static byte ToByte(this short value) => (byte)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static byte ToByte(this short? value, short defaultValue = default) => (byte)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static sbyte ToSByte(this short value) => (sbyte)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static sbyte ToSByte(this short? value, short defaultValue = default) => (sbyte)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ushort ToUShort(this short value) => (ushort)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ushort ToUShort(this short? value, short defaultValue = default) => (ushort)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static int ToInt(this short value) => (int)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static int ToInt(this short? value, short defaultValue = default) => (int)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static uint ToUInt(this short value) => (uint)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static uint ToUInt(this short? value, short defaultValue = default) => (uint)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static long ToLong(this short value) => (long)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static long ToLong(this short? value, short defaultValue = default) => (long)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ulong ToULong(this short value) => (ulong)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ulong ToULong(this short? value, short defaultValue = default) => (ulong)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static double ToDouble(this short value) => (double)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static double ToDouble(this short? value, short defaultValue = default) => (double)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static float ToFloat(this short value) => (float)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static float ToFloat(this short? value, short defaultValue = default) => (float)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static char ToChar(this short value) => (char)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static char ToChar(this short? value, short defaultValue = default) => (char)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ushort ToUShort(this string value) => ushort.Parse(value, CultureInfo.InvariantCulture);
        
        /// <summary>
        /// Convert to ushort
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static ushort ToUShort(this object value, in ushort defaultValue = default)        
        {
            return value is ushort v ? v : default;
        }
        
        /// <summary>
        /// Parse to ushort
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static ushort TryParseToUShort(this object value, in ushort defaultValue = default)        
        {
            if (ushort.TryParse(value?.ToString(), out var result))
            {
	            return result;
            }
            return default;
        }
            [MethodImpl(GlobalConst.ImplOptions)]
            public static bool ToBool(this ushort value) => value > default(ushort);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static bool ToBool(this ushort? value, ushort defaultValue = default) => (value ?? defaultValue) > default(ushort);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static byte ToByte(this ushort value) => (byte)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static byte ToByte(this ushort? value, ushort defaultValue = default) => (byte)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static sbyte ToSByte(this ushort value) => (sbyte)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static sbyte ToSByte(this ushort? value, ushort defaultValue = default) => (sbyte)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static short ToShort(this ushort value) => (short)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static short ToShort(this ushort? value, ushort defaultValue = default) => (short)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static int ToInt(this ushort value) => (int)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static int ToInt(this ushort? value, ushort defaultValue = default) => (int)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static uint ToUInt(this ushort value) => (uint)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static uint ToUInt(this ushort? value, ushort defaultValue = default) => (uint)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static long ToLong(this ushort value) => (long)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static long ToLong(this ushort? value, ushort defaultValue = default) => (long)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ulong ToULong(this ushort value) => (ulong)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ulong ToULong(this ushort? value, ushort defaultValue = default) => (ulong)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static double ToDouble(this ushort value) => (double)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static double ToDouble(this ushort? value, ushort defaultValue = default) => (double)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static float ToFloat(this ushort value) => (float)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static float ToFloat(this ushort? value, ushort defaultValue = default) => (float)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static char ToChar(this ushort value) => (char)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static char ToChar(this ushort? value, ushort defaultValue = default) => (char)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static int ToInt(this string value) => int.Parse(value, CultureInfo.InvariantCulture);
        
        /// <summary>
        /// Convert to int
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static int ToInt(this object value, in int defaultValue = default)        
        {
            return value is int v ? v : default;
        }
        
        /// <summary>
        /// Parse to int
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static int TryParseToInt(this object value, in int defaultValue = default)        
        {
            if (int.TryParse(value?.ToString(), out var result))
            {
	            return result;
            }
            return default;
        }
            [MethodImpl(GlobalConst.ImplOptions)]
            public static bool ToBool(this int value) => value > default(int);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static bool ToBool(this int? value, int defaultValue = default) => (value ?? defaultValue) > default(int);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static byte ToByte(this int value) => (byte)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static byte ToByte(this int? value, int defaultValue = default) => (byte)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static sbyte ToSByte(this int value) => (sbyte)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static sbyte ToSByte(this int? value, int defaultValue = default) => (sbyte)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static short ToShort(this int value) => (short)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static short ToShort(this int? value, int defaultValue = default) => (short)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ushort ToUShort(this int value) => (ushort)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ushort ToUShort(this int? value, int defaultValue = default) => (ushort)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static uint ToUInt(this int value) => (uint)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static uint ToUInt(this int? value, int defaultValue = default) => (uint)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static long ToLong(this int value) => (long)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static long ToLong(this int? value, int defaultValue = default) => (long)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ulong ToULong(this int value) => (ulong)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ulong ToULong(this int? value, int defaultValue = default) => (ulong)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static double ToDouble(this int value) => (double)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static double ToDouble(this int? value, int defaultValue = default) => (double)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static float ToFloat(this int value) => (float)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static float ToFloat(this int? value, int defaultValue = default) => (float)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static char ToChar(this int value) => (char)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static char ToChar(this int? value, int defaultValue = default) => (char)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static uint ToUInt(this string value) => uint.Parse(value, CultureInfo.InvariantCulture);
        
        /// <summary>
        /// Convert to uint
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static uint ToUInt(this object value, in uint defaultValue = default)        
        {
            return value is uint v ? v : default;
        }
        
        /// <summary>
        /// Parse to uint
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static uint TryParseToUInt(this object value, in uint defaultValue = default)        
        {
            if (uint.TryParse(value?.ToString(), out var result))
            {
	            return result;
            }
            return default;
        }
            [MethodImpl(GlobalConst.ImplOptions)]
            public static bool ToBool(this uint value) => value > default(uint);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static bool ToBool(this uint? value, uint defaultValue = default) => (value ?? defaultValue) > default(uint);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static byte ToByte(this uint value) => (byte)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static byte ToByte(this uint? value, uint defaultValue = default) => (byte)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static sbyte ToSByte(this uint value) => (sbyte)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static sbyte ToSByte(this uint? value, uint defaultValue = default) => (sbyte)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static short ToShort(this uint value) => (short)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static short ToShort(this uint? value, uint defaultValue = default) => (short)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ushort ToUShort(this uint value) => (ushort)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ushort ToUShort(this uint? value, uint defaultValue = default) => (ushort)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static int ToInt(this uint value) => (int)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static int ToInt(this uint? value, uint defaultValue = default) => (int)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static long ToLong(this uint value) => (long)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static long ToLong(this uint? value, uint defaultValue = default) => (long)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ulong ToULong(this uint value) => (ulong)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ulong ToULong(this uint? value, uint defaultValue = default) => (ulong)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static double ToDouble(this uint value) => (double)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static double ToDouble(this uint? value, uint defaultValue = default) => (double)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static float ToFloat(this uint value) => (float)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static float ToFloat(this uint? value, uint defaultValue = default) => (float)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static char ToChar(this uint value) => (char)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static char ToChar(this uint? value, uint defaultValue = default) => (char)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static long ToLong(this string value) => long.Parse(value, CultureInfo.InvariantCulture);
        
        /// <summary>
        /// Convert to long
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static long ToLong(this object value, in long defaultValue = default)        
        {
            return value is long v ? v : default;
        }
        
        /// <summary>
        /// Parse to long
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static long TryParseToLong(this object value, in long defaultValue = default)        
        {
            if (long.TryParse(value?.ToString(), out var result))
            {
	            return result;
            }
            return default;
        }
            [MethodImpl(GlobalConst.ImplOptions)]
            public static bool ToBool(this long value) => value > default(long);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static bool ToBool(this long? value, long defaultValue = default) => (value ?? defaultValue) > default(long);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static byte ToByte(this long value) => (byte)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static byte ToByte(this long? value, long defaultValue = default) => (byte)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static sbyte ToSByte(this long value) => (sbyte)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static sbyte ToSByte(this long? value, long defaultValue = default) => (sbyte)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static short ToShort(this long value) => (short)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static short ToShort(this long? value, long defaultValue = default) => (short)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ushort ToUShort(this long value) => (ushort)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ushort ToUShort(this long? value, long defaultValue = default) => (ushort)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static int ToInt(this long value) => (int)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static int ToInt(this long? value, long defaultValue = default) => (int)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static uint ToUInt(this long value) => (uint)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static uint ToUInt(this long? value, long defaultValue = default) => (uint)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ulong ToULong(this long value) => (ulong)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ulong ToULong(this long? value, long defaultValue = default) => (ulong)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static double ToDouble(this long value) => (double)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static double ToDouble(this long? value, long defaultValue = default) => (double)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static float ToFloat(this long value) => (float)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static float ToFloat(this long? value, long defaultValue = default) => (float)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static char ToChar(this long value) => (char)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static char ToChar(this long? value, long defaultValue = default) => (char)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ulong ToULong(this string value) => ulong.Parse(value, CultureInfo.InvariantCulture);
        
        /// <summary>
        /// Convert to ulong
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static ulong ToULong(this object value, in ulong defaultValue = default)        
        {
            return value is ulong v ? v : default;
        }
        
        /// <summary>
        /// Parse to ulong
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static ulong TryParseToULong(this object value, in ulong defaultValue = default)        
        {
            if (ulong.TryParse(value?.ToString(), out var result))
            {
	            return result;
            }
            return default;
        }
            [MethodImpl(GlobalConst.ImplOptions)]
            public static bool ToBool(this ulong value) => value > default(ulong);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static bool ToBool(this ulong? value, ulong defaultValue = default) => (value ?? defaultValue) > default(ulong);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static byte ToByte(this ulong value) => (byte)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static byte ToByte(this ulong? value, ulong defaultValue = default) => (byte)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static sbyte ToSByte(this ulong value) => (sbyte)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static sbyte ToSByte(this ulong? value, ulong defaultValue = default) => (sbyte)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static short ToShort(this ulong value) => (short)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static short ToShort(this ulong? value, ulong defaultValue = default) => (short)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ushort ToUShort(this ulong value) => (ushort)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ushort ToUShort(this ulong? value, ulong defaultValue = default) => (ushort)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static int ToInt(this ulong value) => (int)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static int ToInt(this ulong? value, ulong defaultValue = default) => (int)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static uint ToUInt(this ulong value) => (uint)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static uint ToUInt(this ulong? value, ulong defaultValue = default) => (uint)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static long ToLong(this ulong value) => (long)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static long ToLong(this ulong? value, ulong defaultValue = default) => (long)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static double ToDouble(this ulong value) => (double)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static double ToDouble(this ulong? value, ulong defaultValue = default) => (double)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static float ToFloat(this ulong value) => (float)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static float ToFloat(this ulong? value, ulong defaultValue = default) => (float)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static char ToChar(this ulong value) => (char)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static char ToChar(this ulong? value, ulong defaultValue = default) => (char)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static byte ToByte(this string value) => byte.Parse(value, CultureInfo.InvariantCulture);
        
        /// <summary>
        /// Convert to byte
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static byte ToByte(this object value, in byte defaultValue = default)        
        {
            return value is byte v ? v : default;
        }
        
        /// <summary>
        /// Parse to byte
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static byte TryParseToByte(this object value, in byte defaultValue = default)        
        {
            if (byte.TryParse(value?.ToString(), out var result))
            {
	            return result;
            }
            return default;
        }
            [MethodImpl(GlobalConst.ImplOptions)]
            public static bool ToBool(this byte value) => value > default(byte);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static bool ToBool(this byte? value, byte defaultValue = default) => (value ?? defaultValue) > default(byte);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static sbyte ToSByte(this byte value) => (sbyte)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static sbyte ToSByte(this byte? value, byte defaultValue = default) => (sbyte)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static short ToShort(this byte value) => (short)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static short ToShort(this byte? value, byte defaultValue = default) => (short)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ushort ToUShort(this byte value) => (ushort)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ushort ToUShort(this byte? value, byte defaultValue = default) => (ushort)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static int ToInt(this byte value) => (int)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static int ToInt(this byte? value, byte defaultValue = default) => (int)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static uint ToUInt(this byte value) => (uint)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static uint ToUInt(this byte? value, byte defaultValue = default) => (uint)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static long ToLong(this byte value) => (long)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static long ToLong(this byte? value, byte defaultValue = default) => (long)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ulong ToULong(this byte value) => (ulong)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ulong ToULong(this byte? value, byte defaultValue = default) => (ulong)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static double ToDouble(this byte value) => (double)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static double ToDouble(this byte? value, byte defaultValue = default) => (double)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static float ToFloat(this byte value) => (float)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static float ToFloat(this byte? value, byte defaultValue = default) => (float)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static char ToChar(this byte value) => (char)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static char ToChar(this byte? value, byte defaultValue = default) => (char)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static sbyte ToSByte(this string value) => sbyte.Parse(value, CultureInfo.InvariantCulture);
        
        /// <summary>
        /// Convert to sbyte
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static sbyte ToSByte(this object value, in sbyte defaultValue = default)        
        {
            return value is sbyte v ? v : default;
        }
        
        /// <summary>
        /// Parse to sbyte
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static sbyte TryParseToSByte(this object value, in sbyte defaultValue = default)        
        {
            if (sbyte.TryParse(value?.ToString(), out var result))
            {
	            return result;
            }
            return default;
        }
            [MethodImpl(GlobalConst.ImplOptions)]
            public static bool ToBool(this sbyte value) => value > default(sbyte);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static bool ToBool(this sbyte? value, sbyte defaultValue = default) => (value ?? defaultValue) > default(sbyte);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static byte ToByte(this sbyte value) => (byte)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static byte ToByte(this sbyte? value, sbyte defaultValue = default) => (byte)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static short ToShort(this sbyte value) => (short)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static short ToShort(this sbyte? value, sbyte defaultValue = default) => (short)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ushort ToUShort(this sbyte value) => (ushort)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ushort ToUShort(this sbyte? value, sbyte defaultValue = default) => (ushort)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static int ToInt(this sbyte value) => (int)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static int ToInt(this sbyte? value, sbyte defaultValue = default) => (int)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static uint ToUInt(this sbyte value) => (uint)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static uint ToUInt(this sbyte? value, sbyte defaultValue = default) => (uint)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static long ToLong(this sbyte value) => (long)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static long ToLong(this sbyte? value, sbyte defaultValue = default) => (long)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ulong ToULong(this sbyte value) => (ulong)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ulong ToULong(this sbyte? value, sbyte defaultValue = default) => (ulong)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static double ToDouble(this sbyte value) => (double)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static double ToDouble(this sbyte? value, sbyte defaultValue = default) => (double)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static float ToFloat(this sbyte value) => (float)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static float ToFloat(this sbyte? value, sbyte defaultValue = default) => (float)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static char ToChar(this sbyte value) => (char)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static char ToChar(this sbyte? value, sbyte defaultValue = default) => (char)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static bool ToBool(this string value) => bool.Parse(value);
        
        /// <summary>
        /// Convert to bool
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool ToBool(this object value, in bool defaultValue = default)        
        {
            return value is bool v ? v : default;
        }
        
        /// <summary>
        /// Parse to bool
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool TryParseToBool(this object value, in bool defaultValue = default)        
        {
            if (bool.TryParse(value?.ToString(), out var result))
            {
	            return result;
            }
            return default;
        }
            [MethodImpl(GlobalConst.ImplOptions)]
            public static byte ToByte(this bool value) => value ? (byte)1 : default(byte);

            [MethodImpl(GlobalConst.ImplOptions)]
            public static byte ToByte(this bool? value, bool defaultValue = false) => value ?? defaultValue ? (byte)1 : default(byte);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static sbyte ToSByte(this bool value) => value ? (sbyte)1 : default(sbyte);

            [MethodImpl(GlobalConst.ImplOptions)]
            public static sbyte ToSByte(this bool? value, bool defaultValue = false) => value ?? defaultValue ? (sbyte)1 : default(sbyte);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static short ToShort(this bool value) => value ? (short)1 : default(short);

            [MethodImpl(GlobalConst.ImplOptions)]
            public static short ToShort(this bool? value, bool defaultValue = false) => value ?? defaultValue ? (short)1 : default(short);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ushort ToUShort(this bool value) => value ? (ushort)1 : default(ushort);

            [MethodImpl(GlobalConst.ImplOptions)]
            public static ushort ToUShort(this bool? value, bool defaultValue = false) => value ?? defaultValue ? (ushort)1 : default(ushort);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static int ToInt(this bool value) => value ? (int)1 : default(int);

            [MethodImpl(GlobalConst.ImplOptions)]
            public static int ToInt(this bool? value, bool defaultValue = false) => value ?? defaultValue ? (int)1 : default(int);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static uint ToUInt(this bool value) => value ? (uint)1U : default(uint);

            [MethodImpl(GlobalConst.ImplOptions)]
            public static uint ToUInt(this bool? value, bool defaultValue = false) => value ?? defaultValue ? (uint)1U : default(uint);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static long ToLong(this bool value) => value ? (long)1L : default(long);

            [MethodImpl(GlobalConst.ImplOptions)]
            public static long ToLong(this bool? value, bool defaultValue = false) => value ?? defaultValue ? (long)1L : default(long);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ulong ToULong(this bool value) => value ? (ulong)1UL : default(ulong);

            [MethodImpl(GlobalConst.ImplOptions)]
            public static ulong ToULong(this bool? value, bool defaultValue = false) => value ?? defaultValue ? (ulong)1UL : default(ulong);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static double ToDouble(this bool value) => value ? (double)1D : default(double);

            [MethodImpl(GlobalConst.ImplOptions)]
            public static double ToDouble(this bool? value, bool defaultValue = false) => value ?? defaultValue ? (double)1D : default(double);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static float ToFloat(this bool value) => value ? (float)1F : default(float);

            [MethodImpl(GlobalConst.ImplOptions)]
            public static float ToFloat(this bool? value, bool defaultValue = false) => value ?? defaultValue ? (float)1F : default(float);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static char ToChar(this bool value) => value ? (char)1 : default(char);

            [MethodImpl(GlobalConst.ImplOptions)]
            public static char ToChar(this bool? value, bool defaultValue = false) => value ?? defaultValue ? (char)1 : default(char);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static string ToString(this bool value) => value ? "true" : "false";

            [MethodImpl(GlobalConst.ImplOptions)]
            public static string ToString(this bool? value, bool defaultValue = false) => value ?? defaultValue ? "true" : "false";
        
        /// <summary>
        /// Convert to char
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static char ToChar(this object value, in char defaultValue = default)        
        {
            return value is char v ? v : default;
        }
        
        /// <summary>
        /// Parse to char
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static char TryParseToChar(this object value, in char defaultValue = default)        
        {
            if (char.TryParse(value?.ToString(), out var result))
            {
	            return result;
            }
            return default;
        }
            [MethodImpl(GlobalConst.ImplOptions)]
            public static bool ToBool(this char value) => value > default(char);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static bool ToBool(this char? value, char defaultValue = default) => (value ?? defaultValue) > default(char);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static byte ToByte(this char value) => (byte)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static byte ToByte(this char? value, char defaultValue = default) => (byte)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static sbyte ToSByte(this char value) => (sbyte)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static sbyte ToSByte(this char? value, char defaultValue = default) => (sbyte)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static short ToShort(this char value) => (short)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static short ToShort(this char? value, char defaultValue = default) => (short)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ushort ToUShort(this char value) => (ushort)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ushort ToUShort(this char? value, char defaultValue = default) => (ushort)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static int ToInt(this char value) => (int)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static int ToInt(this char? value, char defaultValue = default) => (int)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static uint ToUInt(this char value) => (uint)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static uint ToUInt(this char? value, char defaultValue = default) => (uint)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static long ToLong(this char value) => (long)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static long ToLong(this char? value, char defaultValue = default) => (long)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ulong ToULong(this char value) => (ulong)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ulong ToULong(this char? value, char defaultValue = default) => (ulong)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static double ToDouble(this char value) => (double)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static double ToDouble(this char? value, char defaultValue = default) => (double)(value ?? defaultValue);
            [MethodImpl(GlobalConst.ImplOptions)]
            public static float ToFloat(this char value) => (float)value;
            [MethodImpl(GlobalConst.ImplOptions)]
            public static float ToFloat(this char? value, char defaultValue = default) => (float)(value ?? defaultValue);
    }
}