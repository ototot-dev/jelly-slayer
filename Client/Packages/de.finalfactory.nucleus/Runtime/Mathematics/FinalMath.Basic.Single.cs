



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
using static FinalFactory.Mathematics.FinalConst;
namespace FinalFactory.Mathematics
{
    // ReSharper disable once InconsistentNaming
    public static partial class FinalMathBasic 
    {
        
        #region SECTION COMPARE float
        
        #region Even Odd
        
        /// <summary>
        /// Check if value is even.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsEven(this in float value)        
        {
            return value % 2 == 0;
        }
        
        /// <summary>
        /// Check if value is odd.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsOdd(this in float value)        
        {
            return value % 2 != 0;
        }
        
        #endregion
        
        /// <summary>
        /// Returns the value with positive sign.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static float Abs(this float value)        
        {
            return (float)Math.Abs(value);
        }

        /// <summary>
        /// Determines whether the specified value is close to the other one.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool Equal(this float value1, float value2)
        {
            return IsZero(value1 - value2);
        }

        /// <summary>
        /// Returns true if the number is normal.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsNormal (this float value)
        {
            return float.IsNormal (value);
        }

        /// <summary>
        /// Returns true if the number is subnormal.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsSubnormal (this float value)
        {
            return float.IsSubnormal (value);
        }

        /// <summary>
        /// Returns true if the number is positive infinity.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsPositiveInfinity (this float value)
        {
            return float.IsPositiveInfinity (value);
        }

        /// <summary>
        /// Returns true if the number is negative infinity.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsNegativeInfinity(this float value)
        {
            return float.IsNegativeInfinity(value);
        }

        /// <summary>
        /// Returns true if the number is below zero.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsFinite(this float value)
        {
            return float.IsFinite(value);
        }

        /// <summary>
        /// Returns true if the number is infinite.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsInfinity(this float value)
        {
            return float.IsInfinity(value);
        }

        /// <summary>
        /// Returns true if the number is below zero.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsNegative(this float value)
        {
            return float.IsNegative(value);
        }

        /// <summary>
        /// Returns true if the number is zero or above.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsPositive(this float value)
        {
            return !value.IsNegative();
        }

        /// <summary>
        /// Determines whether the specified value is close to zero (0.0).
        /// </summary>
        /// <returns><c>true</c> if the specified value is close to zero (0.0D); otherwise, <c>false</c>.</returns>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsZero(this float value)
        {
            return value.Abs() < float.Epsilon;
        }

        /// <summary>
        /// Determines whether the specified value is close to one (1.0).
        /// </summary>
        /// <returns><c>true</c> if the specified value is close to one (1.0); otherwise, <c>false</c>.</returns>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsOne(this float value)
        {
            return IsZero(value - (float)1.0d);
        }

        /// <summary>
        /// Returns true if the number is one.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsNaN(this float value)
        {
            return float.IsNaN(value);
        }
        
        #region NearEqual
        
        /// <summary>
        /// Check if value left is smaller than value right with the given epsilon.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool Smaller(this in float left, in float right, in float epsilon)        
        {
            return right - left > epsilon;
        }
        
        /// <summary>
        /// Check if value left is greater than value right with the given epsilon.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool Greater(this in float left, in float right, in float epsilon)        
        {
            return left - right > epsilon;
        }
        
        /// <summary>
        /// Check if value left is equal than value right with the given epsilon.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool Equal(this in float left, in float right, in float epsilon)        
        {
            return (left - right).Abs() <= epsilon;
        }
        
        /// <summary>
        /// Checks if a and b are almost equals, taking into account the magnitude of floating point numbers (unlike <see cref="WithinEpsilon" /> method).
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool NearEqual(this in float a, in float b)        
        {
            if (a.Equal(b)) return true;
            var aInt = BitConverter.SingleToInt32Bits(a);
            var bInt = BitConverter.SingleToInt32Bits(b);
            return aInt < 0 == bInt < 0 && Math.Abs(aInt - bInt) <= 4;
        }
        
        #endregion
        
        #endregion
        
        #region SECTION MATH float
        
        #region Advanced Math
        public static float Remap(this float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            var fromAbs  =  value - fromMin;
            var fromMaxAbs = fromMax - fromMin;      
       
            var normal = fromAbs / fromMaxAbs;
 
            var toMaxAbs = toMax - toMin;
            var toAbs = toMaxAbs * normal;
 
            var to = toAbs + toMin;
       
            return (float)to;
        }

        public static float DistanceOnGrid(float x1, float y1, float x2, float y2) {
            var dx = (x2 - x1).Abs();
            var dy = (y2 - y1).Abs();

            var min = Min(dx, dy);
            var max = Max(dx, dy);

            var diagonalSteps = min;
            var straightSteps = max - min;

            return (float)(floatSqrt2 * diagonalSteps + straightSteps);
        }

                
        #endregion
        /// <summary>
        ///     Returns the number of digits inside 'number'.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static int GetNumberOfDigits(float number)
        {
            return number == 0 ? 1 : (int) Math.Floor(Math.Log10(number.Abs()) + 1);
        }
        
        /// <summary>
        /// Clamp a value between 0 and 1
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static void Clamp01(ref float value)        
        {
            Clamp(ref value, 0f, 1f);
        }
        
        /// <summary>
        /// Clamp a value between 0 and 1
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static float Clamp01(this float value)        
        {
            Clamp01(ref value);
            return value;
        }
        
        /// <summary>
        /// Clamp the specified value.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static float Clamp(this float value, in float min, in float max)        
        {
            Clamp(ref value, min, max);
            return value;
        }
        
        /// <summary>
        /// Clamp the specified value.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static void Clamp(ref float value, in float min, in float max)        
        {
            if (value < min)
            {
	            value = min;
            }
            else if (value > max)
            {
	            value = max;
            }
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Scale the value and add a offset.
        /// </summary>
		/// <returns>Returns the scaled value with offset</returns>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static float ScaleOffset(this float value, float scale, float offset)        
        {
            return (float)(offset + value * scale);
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the smaller value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static float Min(this in float value1, in float value2)        
        {
            return 
value1 < value2 || value1.IsNaN() ? value1 : value2;            
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the larger value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static float Max(this in float value1, in float value2)        
        {
            return 
value1 > value2 || value1.IsNaN() ? value1 : value2;            
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the smaller value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static float Min(this float value1, float value2, float value3)        
        {
    
            return value1.IsNaN() || value2.IsNaN() || value3.IsNaN() ? value1 :
                value1 < value2 && value1 < value3 ? value1 :
                value2 < value3 ? value2 : value3;
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the larger value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static float Max(this float value1, float value2, float value3)        
        {
    
            return value1.IsNaN() || value2.IsNaN() || value3.IsNaN() ? value1 :
                value1 > value2 && value1 > value3 ? value1 :
                value2 > value3 ? value2 : value3;
        }
   
            /// <summary>
            ///     <para>
            ///         Returns smallest of two or more values.
            ///     </para>
            /// </summary>
            /// <param name="a" />
            /// <param name="b" />
            /// <param name="values" />
            [MethodImpl(GlobalConst.ImplOptions)]
            public static float Min(float val1, params float[] values)
            {
                int length = values.Length;
                if (length == 0)
                    return val1;
                float num = val1;
                for (int index = 0; index < length; ++index)
                {
                    num.Min(values[index]);
                }
                return num;
            }
   
            /// <summary>
            ///     <para>
            ///         Returns largest of two or more values.
            ///     </para>
            /// </summary>
            /// <param name="a" />
            /// <param name="b" />
            /// <param name="values" />
            [MethodImpl(GlobalConst.ImplOptions)]
            public static float Max(float val1, params float[] values)
            {
                int length = values.Length;
                if (length == 0)
                    return val1;
                float num = val1;
                for (int index = 0; index < length; ++index)
                {
                    num.Max(values[index]);
                }
                return num;
            }
#if UNITY_MATHEMATICS
#endif
        /// <summary>
        ///     When using the 'Mathf.Acos' function we run into the risk of having an exception thrown if the
        ///     argument to that function resides outside the [-1, 1] range. This shouldn't happen too often,
        ///     but it is certainly possible. Imagine a scenario in which we perform a dot product between 2
        ///     normalized vectors which are perfectly aligned. Ideally, the dot product would return a result
        ///     of exactly 1.0f, but because of floating point rounding errors, the result might exceed the value
        ///     1.0f a little bit. We might get somehting like 1.000001f. If we then use the resulting value as an
        ///     argument to 'Mathf.Acos', we will be thrown an exception because the value that we specified is
        ///     outside of the valid range. This function will make sure that the specified parameter is clamped
        ///     to the correct range before 'Mathf.Acos' is called.
        /// </summary>
        /// <param name="cosine">
        ///     The value whose arc cosine must be calculated. The function will make sure that this
        ///     parameter resides inside the [-1, 1] range.
        /// </param>
        /// <returns>
        ///     The arc cosine of the specified cosine value.
        /// </returns>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static float SafeAcos(float cosine)
        {
            // Clamp the specified value and then return the arc cosine
            cosine = (float)Max((float)-1.0d, Min((float)1.0d, cosine));
            return (float) Math.Acos(cosine);
        }
        [MethodImpl(GlobalConst.ImplOptions)]
        public static float DiscardInteger(this float value)
		{
			return value - value.Truncate();
		}
        [MethodImpl(GlobalConst.ImplOptions)]
        public static float Length(float x, float y)
        {
            return (float)Math.Sqrt((double)(x * x + y * y));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static float Length(float x, float y, float z)
        {
            return (float)Math.Sqrt((double)(x * x + y * y + z * z));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static float Length(float x, float y, float z, float w)
        {
            return (float)Math.Sqrt((double)(x * x + y * y + z * z + w * w));
        }
        [MethodImpl(GlobalConst.ImplOptions)]
        public static float LengthSquared(float x, float y)
        {
            return (float)(double)(x * x + y * y);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static float LengthSquared(float x, float y, float z)
        {
            return (float)(double)(x * x + y * y + z * z);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static float LengthSquared(float x, float y, float z, float w)
        {
            return (float)(double)(x * x + y * y + z * z + w * w);
        }
#if UNITY_MATHEMATICS
        
        /// <summary>
        /// Returns the determinant of the 2x2 matrix. Performs X1 * Y2 - X2 * Y1
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static float Determinant2(in float x1, in float x2, in float y1, in float y2)        
        {
            return (float)(x1 * y2 - x2 * y1);
        }
#endif
#if UNITY_MATHEMATICS
        
        /// <summary>
        /// Returns the value of the first argument raised to the power of the second argument.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static float Pow(this in float value, in float power)        
        {
            return (float)Math.Pow(value, power);
        }
#endif
        
        /// <summary>
        /// Interpolates between two values using a linear function by a given amount.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static float Lerp(this in float from, in float to, in float amount)        
        {
            return (float)((1.0f - amount) * from + amount * to);
        }
        
        /// <summary>
        /// Interpolates between two values using a linear function by a given amount.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static float Lerp(this in float from, in float to, in double amount)        
        {
            return (float)((1.0f - amount) * from + amount * to);
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Repeats a value to ensue that the value stays in the defined range.
        /// If the value is under the offset: The range will be added from the value.
        /// If the value is over the offset + range: The range will be subtracted from the value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static float Repeat(this float value, float range, float offset = default)        
        {
            float min = offset;
            float max = (float)(offset + range);
            float result = value;
            while (result < min)
            {
                result += range;
            }
            while (result > max)
            {
                result -= range;
            }

            return result;
        }
        
        #region Truncate
        [MethodImpl(GlobalConst.ImplOptions)]
        public static float Truncate(this float value)
		{
			return (float) Math.Truncate((double)value);
		}
		        
        #endregion
        
        #region Floor
        [MethodImpl(GlobalConst.ImplOptions)]
        public static float Floor(this float value)
		{
			return (float) Math.Floor((double)value);
		}
		        
        #endregion
        
        #region Floor
        [MethodImpl(GlobalConst.ImplOptions)]
        public static int FloorToInt(this float value)
		{
			return (int) Math.Floor((double)value);
		}
		        
        #endregion
        
        #region Ceiling
        [MethodImpl(GlobalConst.ImplOptions)]
        public static float Ceiling(this float value)
		{
			return (float) Math.Ceiling((double)value);
		}
		        
        #endregion
        
        #region Ceiling
        [MethodImpl(GlobalConst.ImplOptions)]
        public static int CeilingToInt(this float value)
		{
			return (int) Math.Ceiling((double)value);
		}
		        
        #endregion
        
        #region Round
        [MethodImpl(GlobalConst.ImplOptions)]
        public static float Round(this float value)
		{
			return (float) Math.Round((double)value);
		}
		        
        #endregion
        
        #region Round
        [MethodImpl(GlobalConst.ImplOptions)]
        public static int RoundToInt(this float value)
		{
			return (int) Math.Round((double)value);
		}
		        
        #endregion
        
        #region NearEqual
        
        #endregion
        
        /// <summary>
        /// Returns the inverted value.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static float Inverted(this in float value)        
        {
            return 1f / value;
        }
        
        /// <summary>
        /// Returns the negated value.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static float Negated(this in float value)        
        {
            return (float)(value * floatMinusOne);
        }
        
        /// <summary>
        /// Checks if a value is in range.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsInRange(this in float value, in float min, in float max)        
        {
            return value >= min && value <= max;
        }
        
        /// <summary>
        /// Checks if a value is in range of 0-1.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsInRange01(this in float value)        
        {
            return value.IsInRange(0f, 1f);
        }
        
        #endregion
        
        #region SECTION COMPARE double
        
        #region Even Odd
        
        /// <summary>
        /// Check if value is even.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsEven(this in double value)        
        {
            return value % 2 == 0;
        }
        
        /// <summary>
        /// Check if value is odd.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsOdd(this in double value)        
        {
            return value % 2 != 0;
        }
        
        #endregion
        
        /// <summary>
        /// Returns the value with positive sign.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static double Abs(this double value)        
        {
            return (double)Math.Abs(value);
        }

        /// <summary>
        /// Determines whether the specified value is close to the other one.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool Equal(this double value1, double value2)
        {
            return IsZero(value1 - value2);
        }

        /// <summary>
        /// Returns true if the number is normal.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsNormal (this double value)
        {
            return double.IsNormal (value);
        }

        /// <summary>
        /// Returns true if the number is subnormal.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsSubnormal (this double value)
        {
            return double.IsSubnormal (value);
        }

        /// <summary>
        /// Returns true if the number is positive infinity.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsPositiveInfinity (this double value)
        {
            return double.IsPositiveInfinity (value);
        }

        /// <summary>
        /// Returns true if the number is negative infinity.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsNegativeInfinity(this double value)
        {
            return double.IsNegativeInfinity(value);
        }

        /// <summary>
        /// Returns true if the number is below zero.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsFinite(this double value)
        {
            return double.IsFinite(value);
        }

        /// <summary>
        /// Returns true if the number is infinite.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsInfinity(this double value)
        {
            return double.IsInfinity(value);
        }

        /// <summary>
        /// Returns true if the number is below zero.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsNegative(this double value)
        {
            return double.IsNegative(value);
        }

        /// <summary>
        /// Returns true if the number is zero or above.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsPositive(this double value)
        {
            return !value.IsNegative();
        }

        /// <summary>
        /// Determines whether the specified value is close to zero (0.0).
        /// </summary>
        /// <returns><c>true</c> if the specified value is close to zero (0.0D); otherwise, <c>false</c>.</returns>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsZero(this double value)
        {
            return value.Abs() < double.Epsilon;
        }

        /// <summary>
        /// Determines whether the specified value is close to one (1.0).
        /// </summary>
        /// <returns><c>true</c> if the specified value is close to one (1.0); otherwise, <c>false</c>.</returns>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsOne(this double value)
        {
            return IsZero(value - (double)1.0d);
        }

        /// <summary>
        /// Returns true if the number is one.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsNaN(this double value)
        {
            return double.IsNaN(value);
        }
        
        #region NearEqual
        
        /// <summary>
        /// Check if value left is smaller than value right with the given epsilon.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool Smaller(this in double left, in double right, in double epsilon)        
        {
            return right - left > epsilon;
        }
        
        /// <summary>
        /// Check if value left is greater than value right with the given epsilon.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool Greater(this in double left, in double right, in double epsilon)        
        {
            return left - right > epsilon;
        }
        
        /// <summary>
        /// Check if value left is equal than value right with the given epsilon.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool Equal(this in double left, in double right, in double epsilon)        
        {
            return (left - right).Abs() <= epsilon;
        }
        
        /// <summary>
        /// Checks if a and b are almost equals, taking into account the magnitude of floating point numbers (unlike <see cref="WithinEpsilon" /> method).
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool NearEqual(this in double a, in double b)        
        {
            if (a.Equal(b)) return true;
            var aInt = BitConverter.DoubleToInt64Bits(a);
            var bInt = BitConverter.DoubleToInt64Bits(b);
            return aInt < 0 == bInt < 0 && Math.Abs(aInt - bInt) <= 4;
        }
        
        #endregion
        
        #endregion
        
        #region SECTION MATH double
        
        #region Advanced Math
        public static double Remap(this double value, double fromMin, double fromMax, double toMin, double toMax)
        {
            var fromAbs  =  value - fromMin;
            var fromMaxAbs = fromMax - fromMin;      
       
            var normal = fromAbs / fromMaxAbs;
 
            var toMaxAbs = toMax - toMin;
            var toAbs = toMaxAbs * normal;
 
            var to = toAbs + toMin;
       
            return (double)to;
        }

        public static double DistanceOnGrid(double x1, double y1, double x2, double y2) {
            var dx = (x2 - x1).Abs();
            var dy = (y2 - y1).Abs();

            var min = Min(dx, dy);
            var max = Max(dx, dy);

            var diagonalSteps = min;
            var straightSteps = max - min;

            return (double)(doubleSqrt2 * diagonalSteps + straightSteps);
        }

                
        #endregion
        /// <summary>
        ///     Returns the number of digits inside 'number'.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static int GetNumberOfDigits(double number)
        {
            return number == 0 ? 1 : (int) Math.Floor(Math.Log10(number.Abs()) + 1);
        }
        
        /// <summary>
        /// Clamp a value between 0 and 1
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static void Clamp01(ref double value)        
        {
            Clamp(ref value, 0d, 1d);
        }
        
        /// <summary>
        /// Clamp a value between 0 and 1
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static double Clamp01(this double value)        
        {
            Clamp01(ref value);
            return value;
        }
        
        /// <summary>
        /// Clamp the specified value.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static double Clamp(this double value, in double min, in double max)        
        {
            Clamp(ref value, min, max);
            return value;
        }
        
        /// <summary>
        /// Clamp the specified value.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static void Clamp(ref double value, in double min, in double max)        
        {
            if (value < min)
            {
	            value = min;
            }
            else if (value > max)
            {
	            value = max;
            }
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Scale the value and add a offset.
        /// </summary>
		/// <returns>Returns the scaled value with offset</returns>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static double ScaleOffset(this double value, double scale, double offset)        
        {
            return (double)(offset + value * scale);
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the smaller value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static double Min(this in double value1, in double value2)        
        {
            return 
value1 < value2 || value1.IsNaN() ? value1 : value2;            
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the larger value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static double Max(this in double value1, in double value2)        
        {
            return 
value1 > value2 || value1.IsNaN() ? value1 : value2;            
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the smaller value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static double Min(this double value1, double value2, double value3)        
        {
    
            return value1.IsNaN() || value2.IsNaN() || value3.IsNaN() ? value1 :
                value1 < value2 && value1 < value3 ? value1 :
                value2 < value3 ? value2 : value3;
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the larger value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static double Max(this double value1, double value2, double value3)        
        {
    
            return value1.IsNaN() || value2.IsNaN() || value3.IsNaN() ? value1 :
                value1 > value2 && value1 > value3 ? value1 :
                value2 > value3 ? value2 : value3;
        }
   
            /// <summary>
            ///     <para>
            ///         Returns smallest of two or more values.
            ///     </para>
            /// </summary>
            /// <param name="a" />
            /// <param name="b" />
            /// <param name="values" />
            [MethodImpl(GlobalConst.ImplOptions)]
            public static double Min(double val1, params double[] values)
            {
                int length = values.Length;
                if (length == 0)
                    return val1;
                double num = val1;
                for (int index = 0; index < length; ++index)
                {
                    num.Min(values[index]);
                }
                return num;
            }
   
            /// <summary>
            ///     <para>
            ///         Returns largest of two or more values.
            ///     </para>
            /// </summary>
            /// <param name="a" />
            /// <param name="b" />
            /// <param name="values" />
            [MethodImpl(GlobalConst.ImplOptions)]
            public static double Max(double val1, params double[] values)
            {
                int length = values.Length;
                if (length == 0)
                    return val1;
                double num = val1;
                for (int index = 0; index < length; ++index)
                {
                    num.Max(values[index]);
                }
                return num;
            }
        /// <summary>
        ///     When using the 'Mathf.Acos' function we run into the risk of having an exception thrown if the
        ///     argument to that function resides outside the [-1, 1] range. This shouldn't happen too often,
        ///     but it is certainly possible. Imagine a scenario in which we perform a dot product between 2
        ///     normalized vectors which are perfectly aligned. Ideally, the dot product would return a result
        ///     of exactly 1.0f, but because of floating point rounding errors, the result might exceed the value
        ///     1.0f a little bit. We might get somehting like 1.000001f. If we then use the resulting value as an
        ///     argument to 'Mathf.Acos', we will be thrown an exception because the value that we specified is
        ///     outside of the valid range. This function will make sure that the specified parameter is clamped
        ///     to the correct range before 'Mathf.Acos' is called.
        /// </summary>
        /// <param name="cosine">
        ///     The value whose arc cosine must be calculated. The function will make sure that this
        ///     parameter resides inside the [-1, 1] range.
        /// </param>
        /// <returns>
        ///     The arc cosine of the specified cosine value.
        /// </returns>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static double SafeAcos(double cosine)
        {
            // Clamp the specified value and then return the arc cosine
            cosine = (double)Max((double)-1.0d, Min((double)1.0d, cosine));
            return (double) Math.Acos(cosine);
        }
        [MethodImpl(GlobalConst.ImplOptions)]
        public static double DiscardInteger(this double value)
		{
			return value - value.Truncate();
		}
        [MethodImpl(GlobalConst.ImplOptions)]
        public static double Length(double x, double y)
        {
            return (double)Math.Sqrt((double)(x * x + y * y));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static double Length(double x, double y, double z)
        {
            return (double)Math.Sqrt((double)(x * x + y * y + z * z));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static double Length(double x, double y, double z, double w)
        {
            return (double)Math.Sqrt((double)(x * x + y * y + z * z + w * w));
        }
        [MethodImpl(GlobalConst.ImplOptions)]
        public static double LengthSquared(double x, double y)
        {
            return (double)(double)(x * x + y * y);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static double LengthSquared(double x, double y, double z)
        {
            return (double)(double)(x * x + y * y + z * z);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static double LengthSquared(double x, double y, double z, double w)
        {
            return (double)(double)(x * x + y * y + z * z + w * w);
        }
        
        /// <summary>
        /// Returns the determinant of the 2x2 matrix. Performs X1 * Y2 - X2 * Y1
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static double Determinant2(in double x1, in double x2, in double y1, in double y2)        
        {
            return (double)(x1 * y2 - x2 * y1);
        }
        
        /// <summary>
        /// Returns the value of the first argument raised to the power of the second argument.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static double Pow(this in double value, in double power)        
        {
            return (double)Math.Pow(value, power);
        }
        
        /// <summary>
        /// Interpolates between two values using a linear function by a given amount.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static double Lerp(this in double from, in double to, in float amount)        
        {
            return (double)((1.0f - amount) * from + amount * to);
        }
        
        /// <summary>
        /// Interpolates between two values using a linear function by a given amount.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static double Lerp(this in double from, in double to, in double amount)        
        {
            return (double)((1.0f - amount) * from + amount * to);
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Repeats a value to ensue that the value stays in the defined range.
        /// If the value is under the offset: The range will be added from the value.
        /// If the value is over the offset + range: The range will be subtracted from the value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static double Repeat(this double value, double range, double offset = default)        
        {
            double min = offset;
            double max = (double)(offset + range);
            double result = value;
            while (result < min)
            {
                result += range;
            }
            while (result > max)
            {
                result -= range;
            }

            return result;
        }
        
        #region Truncate
        [MethodImpl(GlobalConst.ImplOptions)]
        public static double Truncate(this double value)
		{
			return (double) Math.Truncate((double)value);
		}
		        
        #endregion
        
        #region Floor
        [MethodImpl(GlobalConst.ImplOptions)]
        public static double Floor(this double value)
		{
			return (double) Math.Floor((double)value);
		}
		        
        #endregion
        
        #region Floor
        [MethodImpl(GlobalConst.ImplOptions)]
        public static int FloorToInt(this double value)
		{
			return (int) Math.Floor((double)value);
		}
		        
        #endregion
        
        #region Ceiling
        [MethodImpl(GlobalConst.ImplOptions)]
        public static double Ceiling(this double value)
		{
			return (double) Math.Ceiling((double)value);
		}
		        
        #endregion
        
        #region Ceiling
        [MethodImpl(GlobalConst.ImplOptions)]
        public static int CeilingToInt(this double value)
		{
			return (int) Math.Ceiling((double)value);
		}
		        
        #endregion
        
        #region Round
        [MethodImpl(GlobalConst.ImplOptions)]
        public static double Round(this double value)
		{
			return (double) Math.Round((double)value);
		}
		        
        #endregion
        
        #region Round
        [MethodImpl(GlobalConst.ImplOptions)]
        public static int RoundToInt(this double value)
		{
			return (int) Math.Round((double)value);
		}
		        
        #endregion
        
        #region NearEqual
        
        #endregion
        
        /// <summary>
        /// Returns the inverted value.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static double Inverted(this in double value)        
        {
            return 1d / value;
        }
        
        /// <summary>
        /// Returns the negated value.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static double Negated(this in double value)        
        {
            return (double)(value * doubleMinusOne);
        }
        
        /// <summary>
        /// Checks if a value is in range.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsInRange(this in double value, in double min, in double max)        
        {
            return value >= min && value <= max;
        }
        
        /// <summary>
        /// Checks if a value is in range of 0-1.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsInRange01(this in double value)        
        {
            return value.IsInRange(0d, 1d);
        }
        
        #endregion
        
        #region SECTION COMPARE short
        
        #region Even Odd
        
        /// <summary>
        /// Check if value is even.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsEven(this in short value)        
        {
            return value % 2 == 0;
        }
        
        /// <summary>
        /// Check if value is odd.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsOdd(this in short value)        
        {
            return value % 2 != 0;
        }
        
        #endregion
        
        /// <summary>
        /// Returns the value with positive sign.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static short Abs(this short value)        
        {
            return (short)Math.Abs(value);
        }

        /// <summary>
        /// Determines whether the specified value is close to the other one.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool Equal(this short value1, short value2)
        {
            return IsZero(value1 - value2);
        }

        /// <summary>
        /// Returns true if the number is below zero.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsNegative(this short value)
        {
            return value < 0;
        }

        /// <summary>
        /// Returns true if the number is zero or above.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsPositive(this short value)
        {
            return value >= 0;
        }

        /// <summary>
        /// Returns true if the number is zero.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsZero(this short value)
        {
            return value == 0;
        }

        /// <summary>
        /// Returns true if the number is one.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsOne(this short value)
        {
            return value == 1;
        }

        /// <summary>
        /// Returns true if the number is one.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsNaN(this short value)
        {
            return false;
        }
        
        #endregion
        
        #region SECTION MATH short
        
        #region Advanced Math
        public static short Remap(this short value, short fromMin, short fromMax, short toMin, short toMax)
        {
            var fromAbs  =  value - fromMin;
            var fromMaxAbs = fromMax - fromMin;      
       
            var normal = fromAbs / fromMaxAbs;
 
            var toMaxAbs = toMax - toMin;
            var toAbs = toMaxAbs * normal;
 
            var to = toAbs + toMin;
       
            return (short)to;
        }

        public static short DistanceOnGrid(short x1, short y1, short x2, short y2) {
            var dx = (x2 - x1).Abs();
            var dy = (y2 - y1).Abs();

            var min = Min(dx, dy);
            var max = Max(dx, dy);

            var diagonalSteps = min;
            var straightSteps = max - min;

            return (short)(floatSqrt2 * diagonalSteps + straightSteps);
        }

                
        #endregion
        /// <summary>
        ///     Returns the number of digits inside 'number'.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static int GetNumberOfDigits(short number)
        {
            return number == 0 ? 1 : (int) Math.Floor(Math.Log10(number.Abs()) + 1);
        }
        
        /// <summary>
        /// Clamp a value between 0 and 1
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static void Clamp01(ref short value)        
        {
            Clamp(ref value, (short)0, (short)1);
        }
        
        /// <summary>
        /// Clamp a value between 0 and 1
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static short Clamp01(this short value)        
        {
            Clamp01(ref value);
            return value;
        }
        
        /// <summary>
        /// Clamp the specified value.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static short Clamp(this short value, in short min, in short max)        
        {
            Clamp(ref value, min, max);
            return value;
        }
        
        /// <summary>
        /// Clamp the specified value.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static void Clamp(ref short value, in short min, in short max)        
        {
            if (value < min)
            {
	            value = min;
            }
            else if (value > max)
            {
	            value = max;
            }
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Scale the value and add a offset.
        /// </summary>
		/// <returns>Returns the scaled value with offset</returns>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static short ScaleOffset(this short value, short scale, short offset)        
        {
            return (short)(offset + value * scale);
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the smaller value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static short Min(this in short value1, in short value2)        
        {
            return 
value1 < value2 || value1.IsNaN() ? value1 : value2;            
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the larger value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static short Max(this in short value1, in short value2)        
        {
            return 
value1 > value2 || value1.IsNaN() ? value1 : value2;            
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the smaller value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static short Min(this short value1, short value2, short value3)        
        {
    
            return value1.IsNaN() || value2.IsNaN() || value3.IsNaN() ? value1 :
                value1 < value2 && value1 < value3 ? value1 :
                value2 < value3 ? value2 : value3;
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the larger value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static short Max(this short value1, short value2, short value3)        
        {
    
            return value1.IsNaN() || value2.IsNaN() || value3.IsNaN() ? value1 :
                value1 > value2 && value1 > value3 ? value1 :
                value2 > value3 ? value2 : value3;
        }
   
            /// <summary>
            ///     <para>
            ///         Returns smallest of two or more values.
            ///     </para>
            /// </summary>
            /// <param name="a" />
            /// <param name="b" />
            /// <param name="values" />
            [MethodImpl(GlobalConst.ImplOptions)]
            public static short Min(short val1, params short[] values)
            {
                int length = values.Length;
                if (length == 0)
                    return val1;
                short num = val1;
                for (int index = 0; index < length; ++index)
                {
                    num.Min(values[index]);
                }
                return num;
            }
   
            /// <summary>
            ///     <para>
            ///         Returns largest of two or more values.
            ///     </para>
            /// </summary>
            /// <param name="a" />
            /// <param name="b" />
            /// <param name="values" />
            [MethodImpl(GlobalConst.ImplOptions)]
            public static short Max(short val1, params short[] values)
            {
                int length = values.Length;
                if (length == 0)
                    return val1;
                short num = val1;
                for (int index = 0; index < length; ++index)
                {
                    num.Max(values[index]);
                }
                return num;
            }
        [MethodImpl(GlobalConst.ImplOptions)]
        public static short Length(short x, short y)
        {
            return (short)Math.Sqrt((double)(x * x + y * y));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static short Length(short x, short y, short z)
        {
            return (short)Math.Sqrt((double)(x * x + y * y + z * z));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static short Length(short x, short y, short z, short w)
        {
            return (short)Math.Sqrt((double)(x * x + y * y + z * z + w * w));
        }
        [MethodImpl(GlobalConst.ImplOptions)]
        public static short LengthSquared(short x, short y)
        {
            return (short)(double)(x * x + y * y);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static short LengthSquared(short x, short y, short z)
        {
            return (short)(double)(x * x + y * y + z * z);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static short LengthSquared(short x, short y, short z, short w)
        {
            return (short)(double)(x * x + y * y + z * z + w * w);
        }
        
        /// <summary>
        /// Returns the determinant of the 2x2 matrix. Performs X1 * Y2 - X2 * Y1
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static short Determinant2(in short x1, in short x2, in short y1, in short y2)        
        {
            return (short)(x1 * y2 - x2 * y1);
        }
        
        /// <summary>
        /// Returns the value of the first argument raised to the power of the second argument.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static short Pow(this in short value, in short power)        
        {
            return (short)Math.Pow(value, power);
        }
        
        /// <summary>
        /// Interpolates between two values using a linear function by a given amount.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static short Lerp(this in short from, in short to, in float amount)        
        {
            return (short)((1.0f - amount) * from + amount * to);
        }
        
        /// <summary>
        /// Interpolates between two values using a linear function by a given amount.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static short Lerp(this in short from, in short to, in double amount)        
        {
            return (short)((1.0f - amount) * from + amount * to);
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Repeats a value to ensue that the value stays in the defined range.
        /// If the value is under the offset: The range will be added from the value.
        /// If the value is over the offset + range: The range will be subtracted from the value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static short Repeat(this short value, short range, short offset = default)        
        {
            short min = offset;
            short max = (short)(offset + range);
            short result = value;
            while (result < min)
            {
                result += range;
            }
            while (result > max)
            {
                result -= range;
            }

            return result;
        }
        
        /// <summary>
        /// Returns the negated value.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static short Negated(this in short value)        
        {
            return (short)(value * shortMinusOne);
        }
        
        /// <summary>
        /// Checks if a value is in range.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsInRange(this in short value, in short min, in short max)        
        {
            return value >= min && value <= max;
        }
        
        /// <summary>
        /// Checks if a value is in range of 0-1.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsInRange01(this in short value)        
        {
            return value.IsInRange((short)0, (short)1);
        }
        
        #endregion
        
        #region SECTION COMPARE ushort
        
        #region Even Odd
        
        /// <summary>
        /// Check if value is even.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsEven(this in ushort value)        
        {
            return value % 2 == 0;
        }
        
        /// <summary>
        /// Check if value is odd.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsOdd(this in ushort value)        
        {
            return value % 2 != 0;
        }
        
        #endregion
        
        /// <summary>
        /// Returns the value with positive sign.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static ushort Abs(this ushort value)        
        {
            //This is added for compability reasons. Otherwise .. useless.
            return value;
        }

        /// <summary>
        /// Determines whether the specified value is close to the other one.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool Equal(this ushort value1, ushort value2)
        {
            return IsZero(value1 - value2);
        }

        /// <summary>
        /// Returns true if the number is below zero.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsNegative(this ushort value)
        {
            return value < 0;
        }

        /// <summary>
        /// Returns true if the number is zero or above.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsPositive(this ushort value)
        {
            return value >= 0;
        }

        /// <summary>
        /// Returns true if the number is zero.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsZero(this ushort value)
        {
            return value == 0;
        }

        /// <summary>
        /// Returns true if the number is one.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsOne(this ushort value)
        {
            return value == 1;
        }

        /// <summary>
        /// Returns true if the number is one.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsNaN(this ushort value)
        {
            return false;
        }
        
        #endregion
        
        #region SECTION MATH ushort
        
        #region Advanced Math
        public static ushort Remap(this ushort value, ushort fromMin, ushort fromMax, ushort toMin, ushort toMax)
        {
            var fromAbs  =  value - fromMin;
            var fromMaxAbs = fromMax - fromMin;      
       
            var normal = fromAbs / fromMaxAbs;
 
            var toMaxAbs = toMax - toMin;
            var toAbs = toMaxAbs * normal;
 
            var to = toAbs + toMin;
       
            return (ushort)to;
        }

        public static ushort DistanceOnGrid(ushort x1, ushort y1, ushort x2, ushort y2) {
            var dx = (x2 - x1).Abs();
            var dy = (y2 - y1).Abs();

            var min = Min(dx, dy);
            var max = Max(dx, dy);

            var diagonalSteps = min;
            var straightSteps = max - min;

            return (ushort)(floatSqrt2 * diagonalSteps + straightSteps);
        }

                
        #endregion
        /// <summary>
        ///     Returns the number of digits inside 'number'.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static int GetNumberOfDigits(ushort number)
        {
            return number == 0 ? 1 : (int) Math.Floor(Math.Log10(number.Abs()) + 1);
        }
        
        /// <summary>
        /// Clamp a value between 0 and 1
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static void Clamp01(ref ushort value)        
        {
            Clamp(ref value, (ushort)0, (ushort)1);
        }
        
        /// <summary>
        /// Clamp a value between 0 and 1
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static ushort Clamp01(this ushort value)        
        {
            Clamp01(ref value);
            return value;
        }
        
        /// <summary>
        /// Clamp the specified value.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static ushort Clamp(this ushort value, in ushort min, in ushort max)        
        {
            Clamp(ref value, min, max);
            return value;
        }
        
        /// <summary>
        /// Clamp the specified value.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static void Clamp(ref ushort value, in ushort min, in ushort max)        
        {
            if (value < min)
            {
	            value = min;
            }
            else if (value > max)
            {
	            value = max;
            }
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Scale the value and add a offset.
        /// </summary>
		/// <returns>Returns the scaled value with offset</returns>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static ushort ScaleOffset(this ushort value, ushort scale, ushort offset)        
        {
            return (ushort)(offset + value * scale);
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the smaller value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static ushort Min(this in ushort value1, in ushort value2)        
        {
            return 
value1 < value2 || value1.IsNaN() ? value1 : value2;            
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the larger value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static ushort Max(this in ushort value1, in ushort value2)        
        {
            return 
value1 > value2 || value1.IsNaN() ? value1 : value2;            
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the smaller value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static ushort Min(this ushort value1, ushort value2, ushort value3)        
        {
    
            return value1.IsNaN() || value2.IsNaN() || value3.IsNaN() ? value1 :
                value1 < value2 && value1 < value3 ? value1 :
                value2 < value3 ? value2 : value3;
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the larger value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static ushort Max(this ushort value1, ushort value2, ushort value3)        
        {
    
            return value1.IsNaN() || value2.IsNaN() || value3.IsNaN() ? value1 :
                value1 > value2 && value1 > value3 ? value1 :
                value2 > value3 ? value2 : value3;
        }
   
            /// <summary>
            ///     <para>
            ///         Returns smallest of two or more values.
            ///     </para>
            /// </summary>
            /// <param name="a" />
            /// <param name="b" />
            /// <param name="values" />
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ushort Min(ushort val1, params ushort[] values)
            {
                int length = values.Length;
                if (length == 0)
                    return val1;
                ushort num = val1;
                for (int index = 0; index < length; ++index)
                {
                    num.Min(values[index]);
                }
                return num;
            }
   
            /// <summary>
            ///     <para>
            ///         Returns largest of two or more values.
            ///     </para>
            /// </summary>
            /// <param name="a" />
            /// <param name="b" />
            /// <param name="values" />
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ushort Max(ushort val1, params ushort[] values)
            {
                int length = values.Length;
                if (length == 0)
                    return val1;
                ushort num = val1;
                for (int index = 0; index < length; ++index)
                {
                    num.Max(values[index]);
                }
                return num;
            }
        [MethodImpl(GlobalConst.ImplOptions)]
        public static ushort Length(ushort x, ushort y)
        {
            return (ushort)Math.Sqrt((double)(x * x + y * y));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static ushort Length(ushort x, ushort y, ushort z)
        {
            return (ushort)Math.Sqrt((double)(x * x + y * y + z * z));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static ushort Length(ushort x, ushort y, ushort z, ushort w)
        {
            return (ushort)Math.Sqrt((double)(x * x + y * y + z * z + w * w));
        }
        [MethodImpl(GlobalConst.ImplOptions)]
        public static ushort LengthSquared(ushort x, ushort y)
        {
            return (ushort)(double)(x * x + y * y);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static ushort LengthSquared(ushort x, ushort y, ushort z)
        {
            return (ushort)(double)(x * x + y * y + z * z);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static ushort LengthSquared(ushort x, ushort y, ushort z, ushort w)
        {
            return (ushort)(double)(x * x + y * y + z * z + w * w);
        }
        
        /// <summary>
        /// Returns the determinant of the 2x2 matrix. Performs X1 * Y2 - X2 * Y1
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static ushort Determinant2(in ushort x1, in ushort x2, in ushort y1, in ushort y2)        
        {
            return (ushort)(x1 * y2 - x2 * y1);
        }
        
        /// <summary>
        /// Returns the value of the first argument raised to the power of the second argument.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static ushort Pow(this in ushort value, in ushort power)        
        {
            return (ushort)Math.Pow(value, power);
        }
        
        /// <summary>
        /// Interpolates between two values using a linear function by a given amount.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static ushort Lerp(this in ushort from, in ushort to, in float amount)        
        {
            return (ushort)((1.0f - amount) * from + amount * to);
        }
        
        /// <summary>
        /// Interpolates between two values using a linear function by a given amount.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static ushort Lerp(this in ushort from, in ushort to, in double amount)        
        {
            return (ushort)((1.0f - amount) * from + amount * to);
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Repeats a value to ensue that the value stays in the defined range.
        /// If the value is under the offset: The range will be added from the value.
        /// If the value is over the offset + range: The range will be subtracted from the value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static ushort Repeat(this ushort value, ushort range, ushort offset = default)        
        {
            ushort min = offset;
            ushort max = (ushort)(offset + range);
            ushort result = value;
            while (result < min)
            {
                result += range;
            }
            while (result > max)
            {
                result -= range;
            }

            return result;
        }
        
        /// <summary>
        /// Checks if a value is in range.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsInRange(this in ushort value, in ushort min, in ushort max)        
        {
            return value >= min && value <= max;
        }
        
        /// <summary>
        /// Checks if a value is in range of 0-1.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsInRange01(this in ushort value)        
        {
            return value.IsInRange((ushort)0, (ushort)1);
        }
        
        #endregion
        
        #region SECTION COMPARE int
        
        #region Even Odd
        
        /// <summary>
        /// Check if value is even.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsEven(this in int value)        
        {
            return value % 2 == 0;
        }
        
        /// <summary>
        /// Check if value is odd.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsOdd(this in int value)        
        {
            return value % 2 != 0;
        }
        
        #endregion
        
        /// <summary>
        /// Returns the value with positive sign.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static int Abs(this int value)        
        {
            return (int)Math.Abs(value);
        }

        /// <summary>
        /// Determines whether the specified value is close to the other one.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool Equal(this int value1, int value2)
        {
            return IsZero(value1 - value2);
        }

        /// <summary>
        /// Returns true if the number is below zero.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsNegative(this int value)
        {
            return value < 0;
        }

        /// <summary>
        /// Returns true if the number is zero or above.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsPositive(this int value)
        {
            return value >= 0;
        }

        /// <summary>
        /// Returns true if the number is zero.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsZero(this int value)
        {
            return value == 0;
        }

        /// <summary>
        /// Returns true if the number is one.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsOne(this int value)
        {
            return value == 1;
        }

        /// <summary>
        /// Returns true if the number is one.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsNaN(this int value)
        {
            return false;
        }
        
        #endregion
        
        #region SECTION MATH int
        
        #region Advanced Math
        public static int Remap(this int value, int fromMin, int fromMax, int toMin, int toMax)
        {
            var fromAbs  =  value - fromMin;
            var fromMaxAbs = fromMax - fromMin;      
       
            var normal = fromAbs / fromMaxAbs;
 
            var toMaxAbs = toMax - toMin;
            var toAbs = toMaxAbs * normal;
 
            var to = toAbs + toMin;
       
            return (int)to;
        }

        public static int DistanceOnGrid(int x1, int y1, int x2, int y2) {
            var dx = (x2 - x1).Abs();
            var dy = (y2 - y1).Abs();

            var min = Min(dx, dy);
            var max = Max(dx, dy);

            var diagonalSteps = min;
            var straightSteps = max - min;

            return (int)(floatSqrt2 * diagonalSteps + straightSteps);
        }

                
        #endregion
        /// <summary>
        ///     Returns the number of digits inside 'number'.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static int GetNumberOfDigits(int number)
        {
            return number == 0 ? 1 : (int) Math.Floor(Math.Log10(number.Abs()) + 1);
        }
        
        /// <summary>
        /// Clamp a value between 0 and 1
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static void Clamp01(ref int value)        
        {
            Clamp(ref value, 0, 1);
        }
        
        /// <summary>
        /// Clamp a value between 0 and 1
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static int Clamp01(this int value)        
        {
            Clamp01(ref value);
            return value;
        }
        
        /// <summary>
        /// Clamp the specified value.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static int Clamp(this int value, in int min, in int max)        
        {
            Clamp(ref value, min, max);
            return value;
        }
        
        /// <summary>
        /// Clamp the specified value.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static void Clamp(ref int value, in int min, in int max)        
        {
            if (value < min)
            {
	            value = min;
            }
            else if (value > max)
            {
	            value = max;
            }
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Scale the value and add a offset.
        /// </summary>
		/// <returns>Returns the scaled value with offset</returns>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static int ScaleOffset(this int value, int scale, int offset)        
        {
            return (int)(offset + value * scale);
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the smaller value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static int Min(this in int value1, in int value2)        
        {
            return 
value1 < value2 || value1.IsNaN() ? value1 : value2;            
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the larger value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static int Max(this in int value1, in int value2)        
        {
            return 
value1 > value2 || value1.IsNaN() ? value1 : value2;            
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the smaller value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static int Min(this int value1, int value2, int value3)        
        {
    
            return value1.IsNaN() || value2.IsNaN() || value3.IsNaN() ? value1 :
                value1 < value2 && value1 < value3 ? value1 :
                value2 < value3 ? value2 : value3;
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the larger value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static int Max(this int value1, int value2, int value3)        
        {
    
            return value1.IsNaN() || value2.IsNaN() || value3.IsNaN() ? value1 :
                value1 > value2 && value1 > value3 ? value1 :
                value2 > value3 ? value2 : value3;
        }
   
            /// <summary>
            ///     <para>
            ///         Returns smallest of two or more values.
            ///     </para>
            /// </summary>
            /// <param name="a" />
            /// <param name="b" />
            /// <param name="values" />
            [MethodImpl(GlobalConst.ImplOptions)]
            public static int Min(int val1, params int[] values)
            {
                int length = values.Length;
                if (length == 0)
                    return val1;
                int num = val1;
                for (int index = 0; index < length; ++index)
                {
                    num.Min(values[index]);
                }
                return num;
            }
   
            /// <summary>
            ///     <para>
            ///         Returns largest of two or more values.
            ///     </para>
            /// </summary>
            /// <param name="a" />
            /// <param name="b" />
            /// <param name="values" />
            [MethodImpl(GlobalConst.ImplOptions)]
            public static int Max(int val1, params int[] values)
            {
                int length = values.Length;
                if (length == 0)
                    return val1;
                int num = val1;
                for (int index = 0; index < length; ++index)
                {
                    num.Max(values[index]);
                }
                return num;
            }
        [MethodImpl(GlobalConst.ImplOptions)]
        public static int Length(int x, int y)
        {
            return (int)Math.Sqrt((double)(x * x + y * y));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static int Length(int x, int y, int z)
        {
            return (int)Math.Sqrt((double)(x * x + y * y + z * z));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static int Length(int x, int y, int z, int w)
        {
            return (int)Math.Sqrt((double)(x * x + y * y + z * z + w * w));
        }
        [MethodImpl(GlobalConst.ImplOptions)]
        public static int LengthSquared(int x, int y)
        {
            return (int)(double)(x * x + y * y);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static int LengthSquared(int x, int y, int z)
        {
            return (int)(double)(x * x + y * y + z * z);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static int LengthSquared(int x, int y, int z, int w)
        {
            return (int)(double)(x * x + y * y + z * z + w * w);
        }
        
        /// <summary>
        /// Returns the determinant of the 2x2 matrix. Performs X1 * Y2 - X2 * Y1
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static int Determinant2(in int x1, in int x2, in int y1, in int y2)        
        {
            return (int)(x1 * y2 - x2 * y1);
        }
        
        /// <summary>
        /// Returns the value of the first argument raised to the power of the second argument.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static int Pow(this in int value, in int power)        
        {
            return (int)Math.Pow(value, power);
        }
        
        /// <summary>
        /// Interpolates between two values using a linear function by a given amount.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static int Lerp(this in int from, in int to, in float amount)        
        {
            return (int)((1.0f - amount) * from + amount * to);
        }
        
        /// <summary>
        /// Interpolates between two values using a linear function by a given amount.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static int Lerp(this in int from, in int to, in double amount)        
        {
            return (int)((1.0f - amount) * from + amount * to);
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Repeats a value to ensue that the value stays in the defined range.
        /// If the value is under the offset: The range will be added from the value.
        /// If the value is over the offset + range: The range will be subtracted from the value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static int Repeat(this int value, int range, int offset = default)        
        {
            int min = offset;
            int max = (int)(offset + range);
            int result = value;
            while (result < min)
            {
                result += range;
            }
            while (result > max)
            {
                result -= range;
            }

            return result;
        }
        
        /// <summary>
        /// Returns the negated value.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static int Negated(this in int value)        
        {
            return (int)(value * intMinusOne);
        }
        
        /// <summary>
        /// Checks if a value is in range.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsInRange(this in int value, in int min, in int max)        
        {
            return value >= min && value <= max;
        }
        
        /// <summary>
        /// Checks if a value is in range of 0-1.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsInRange01(this in int value)        
        {
            return value.IsInRange(0, 1);
        }
        
        #endregion
        
        #region SECTION COMPARE uint
        
        #region Even Odd
        
        /// <summary>
        /// Check if value is even.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsEven(this in uint value)        
        {
            return value % 2 == 0;
        }
        
        /// <summary>
        /// Check if value is odd.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsOdd(this in uint value)        
        {
            return value % 2 != 0;
        }
        
        #endregion
        
        /// <summary>
        /// Returns the value with positive sign.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static uint Abs(this uint value)        
        {
            //This is added for compability reasons. Otherwise .. useless.
            return value;
        }

        /// <summary>
        /// Determines whether the specified value is close to the other one.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool Equal(this uint value1, uint value2)
        {
            return IsZero(value1 - value2);
        }

        /// <summary>
        /// Returns true if the number is below zero.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsNegative(this uint value)
        {
            return value < 0;
        }

        /// <summary>
        /// Returns true if the number is zero or above.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsPositive(this uint value)
        {
            return value >= 0;
        }

        /// <summary>
        /// Returns true if the number is zero.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsZero(this uint value)
        {
            return value == 0;
        }

        /// <summary>
        /// Returns true if the number is one.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsOne(this uint value)
        {
            return value == 1;
        }

        /// <summary>
        /// Returns true if the number is one.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsNaN(this uint value)
        {
            return false;
        }
        
        #endregion
        
        #region SECTION MATH uint
        
        #region Advanced Math
        public static uint Remap(this uint value, uint fromMin, uint fromMax, uint toMin, uint toMax)
        {
            var fromAbs  =  value - fromMin;
            var fromMaxAbs = fromMax - fromMin;      
       
            var normal = fromAbs / fromMaxAbs;
 
            var toMaxAbs = toMax - toMin;
            var toAbs = toMaxAbs * normal;
 
            var to = toAbs + toMin;
       
            return (uint)to;
        }

        public static uint DistanceOnGrid(uint x1, uint y1, uint x2, uint y2) {
            var dx = (x2 - x1).Abs();
            var dy = (y2 - y1).Abs();

            var min = Min(dx, dy);
            var max = Max(dx, dy);

            var diagonalSteps = min;
            var straightSteps = max - min;

            return (uint)(floatSqrt2 * diagonalSteps + straightSteps);
        }

                
        #endregion
        /// <summary>
        ///     Returns the number of digits inside 'number'.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static int GetNumberOfDigits(uint number)
        {
            return number == 0 ? 1 : (int) Math.Floor(Math.Log10(number.Abs()) + 1);
        }
        
        /// <summary>
        /// Clamp a value between 0 and 1
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static void Clamp01(ref uint value)        
        {
            Clamp(ref value, 0u, 1u);
        }
        
        /// <summary>
        /// Clamp a value between 0 and 1
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static uint Clamp01(this uint value)        
        {
            Clamp01(ref value);
            return value;
        }
        
        /// <summary>
        /// Clamp the specified value.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static uint Clamp(this uint value, in uint min, in uint max)        
        {
            Clamp(ref value, min, max);
            return value;
        }
        
        /// <summary>
        /// Clamp the specified value.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static void Clamp(ref uint value, in uint min, in uint max)        
        {
            if (value < min)
            {
	            value = min;
            }
            else if (value > max)
            {
	            value = max;
            }
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Scale the value and add a offset.
        /// </summary>
		/// <returns>Returns the scaled value with offset</returns>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static uint ScaleOffset(this uint value, uint scale, uint offset)        
        {
            return (uint)(offset + value * scale);
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the smaller value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static uint Min(this in uint value1, in uint value2)        
        {
            return 
value1 < value2 || value1.IsNaN() ? value1 : value2;            
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the larger value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static uint Max(this in uint value1, in uint value2)        
        {
            return 
value1 > value2 || value1.IsNaN() ? value1 : value2;            
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the smaller value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static uint Min(this uint value1, uint value2, uint value3)        
        {
    
            return value1.IsNaN() || value2.IsNaN() || value3.IsNaN() ? value1 :
                value1 < value2 && value1 < value3 ? value1 :
                value2 < value3 ? value2 : value3;
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the larger value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static uint Max(this uint value1, uint value2, uint value3)        
        {
    
            return value1.IsNaN() || value2.IsNaN() || value3.IsNaN() ? value1 :
                value1 > value2 && value1 > value3 ? value1 :
                value2 > value3 ? value2 : value3;
        }
   
            /// <summary>
            ///     <para>
            ///         Returns smallest of two or more values.
            ///     </para>
            /// </summary>
            /// <param name="a" />
            /// <param name="b" />
            /// <param name="values" />
            [MethodImpl(GlobalConst.ImplOptions)]
            public static uint Min(uint val1, params uint[] values)
            {
                int length = values.Length;
                if (length == 0)
                    return val1;
                uint num = val1;
                for (int index = 0; index < length; ++index)
                {
                    num.Min(values[index]);
                }
                return num;
            }
   
            /// <summary>
            ///     <para>
            ///         Returns largest of two or more values.
            ///     </para>
            /// </summary>
            /// <param name="a" />
            /// <param name="b" />
            /// <param name="values" />
            [MethodImpl(GlobalConst.ImplOptions)]
            public static uint Max(uint val1, params uint[] values)
            {
                int length = values.Length;
                if (length == 0)
                    return val1;
                uint num = val1;
                for (int index = 0; index < length; ++index)
                {
                    num.Max(values[index]);
                }
                return num;
            }
        [MethodImpl(GlobalConst.ImplOptions)]
        public static uint Length(uint x, uint y)
        {
            return (uint)Math.Sqrt((double)(x * x + y * y));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static uint Length(uint x, uint y, uint z)
        {
            return (uint)Math.Sqrt((double)(x * x + y * y + z * z));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static uint Length(uint x, uint y, uint z, uint w)
        {
            return (uint)Math.Sqrt((double)(x * x + y * y + z * z + w * w));
        }
        [MethodImpl(GlobalConst.ImplOptions)]
        public static uint LengthSquared(uint x, uint y)
        {
            return (uint)(double)(x * x + y * y);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static uint LengthSquared(uint x, uint y, uint z)
        {
            return (uint)(double)(x * x + y * y + z * z);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static uint LengthSquared(uint x, uint y, uint z, uint w)
        {
            return (uint)(double)(x * x + y * y + z * z + w * w);
        }
        
        /// <summary>
        /// Returns the determinant of the 2x2 matrix. Performs X1 * Y2 - X2 * Y1
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static uint Determinant2(in uint x1, in uint x2, in uint y1, in uint y2)        
        {
            return (uint)(x1 * y2 - x2 * y1);
        }
        
        /// <summary>
        /// Returns the value of the first argument raised to the power of the second argument.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static uint Pow(this in uint value, in uint power)        
        {
            return (uint)Math.Pow(value, power);
        }
        
        /// <summary>
        /// Interpolates between two values using a linear function by a given amount.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static uint Lerp(this in uint from, in uint to, in float amount)        
        {
            return (uint)((1.0f - amount) * from + amount * to);
        }
        
        /// <summary>
        /// Interpolates between two values using a linear function by a given amount.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static uint Lerp(this in uint from, in uint to, in double amount)        
        {
            return (uint)((1.0f - amount) * from + amount * to);
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Repeats a value to ensue that the value stays in the defined range.
        /// If the value is under the offset: The range will be added from the value.
        /// If the value is over the offset + range: The range will be subtracted from the value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static uint Repeat(this uint value, uint range, uint offset = default)        
        {
            uint min = offset;
            uint max = (uint)(offset + range);
            uint result = value;
            while (result < min)
            {
                result += range;
            }
            while (result > max)
            {
                result -= range;
            }

            return result;
        }
        
        /// <summary>
        /// Checks if a value is in range.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsInRange(this in uint value, in uint min, in uint max)        
        {
            return value >= min && value <= max;
        }
        
        /// <summary>
        /// Checks if a value is in range of 0-1.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsInRange01(this in uint value)        
        {
            return value.IsInRange(0u, 1u);
        }
        
        #endregion
        
        #region SECTION COMPARE long
        
        #region Even Odd
        
        /// <summary>
        /// Check if value is even.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsEven(this in long value)        
        {
            return value % 2 == 0;
        }
        
        /// <summary>
        /// Check if value is odd.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsOdd(this in long value)        
        {
            return value % 2 != 0;
        }
        
        #endregion
        
        /// <summary>
        /// Returns the value with positive sign.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static long Abs(this long value)        
        {
            return (long)Math.Abs(value);
        }

        /// <summary>
        /// Determines whether the specified value is close to the other one.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool Equal(this long value1, long value2)
        {
            return IsZero(value1 - value2);
        }

        /// <summary>
        /// Returns true if the number is below zero.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsNegative(this long value)
        {
            return value < 0;
        }

        /// <summary>
        /// Returns true if the number is zero or above.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsPositive(this long value)
        {
            return value >= 0;
        }

        /// <summary>
        /// Returns true if the number is zero.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsZero(this long value)
        {
            return value == 0;
        }

        /// <summary>
        /// Returns true if the number is one.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsOne(this long value)
        {
            return value == 1;
        }

        /// <summary>
        /// Returns true if the number is one.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsNaN(this long value)
        {
            return false;
        }
        
        #endregion
        
        #region SECTION MATH long
        
        #region Advanced Math
        public static long Remap(this long value, long fromMin, long fromMax, long toMin, long toMax)
        {
            var fromAbs  =  value - fromMin;
            var fromMaxAbs = fromMax - fromMin;      
       
            var normal = fromAbs / fromMaxAbs;
 
            var toMaxAbs = toMax - toMin;
            var toAbs = toMaxAbs * normal;
 
            var to = toAbs + toMin;
       
            return (long)to;
        }

        public static long DistanceOnGrid(long x1, long y1, long x2, long y2) {
            var dx = (x2 - x1).Abs();
            var dy = (y2 - y1).Abs();

            var min = Min(dx, dy);
            var max = Max(dx, dy);

            var diagonalSteps = min;
            var straightSteps = max - min;

            return (long)(doubleSqrt2 * diagonalSteps + straightSteps);
        }

                
        #endregion
        /// <summary>
        ///     Returns the number of digits inside 'number'.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static int GetNumberOfDigits(long number)
        {
            return number == 0 ? 1 : (int) Math.Floor(Math.Log10(number.Abs()) + 1);
        }
        
        /// <summary>
        /// Clamp a value between 0 and 1
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static void Clamp01(ref long value)        
        {
            Clamp(ref value, 0L, 1L);
        }
        
        /// <summary>
        /// Clamp a value between 0 and 1
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static long Clamp01(this long value)        
        {
            Clamp01(ref value);
            return value;
        }
        
        /// <summary>
        /// Clamp the specified value.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static long Clamp(this long value, in long min, in long max)        
        {
            Clamp(ref value, min, max);
            return value;
        }
        
        /// <summary>
        /// Clamp the specified value.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static void Clamp(ref long value, in long min, in long max)        
        {
            if (value < min)
            {
	            value = min;
            }
            else if (value > max)
            {
	            value = max;
            }
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Scale the value and add a offset.
        /// </summary>
		/// <returns>Returns the scaled value with offset</returns>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static long ScaleOffset(this long value, long scale, long offset)        
        {
            return (long)(offset + value * scale);
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the smaller value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static long Min(this in long value1, in long value2)        
        {
            return 
value1 < value2 || value1.IsNaN() ? value1 : value2;            
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the larger value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static long Max(this in long value1, in long value2)        
        {
            return 
value1 > value2 || value1.IsNaN() ? value1 : value2;            
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the smaller value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static long Min(this long value1, long value2, long value3)        
        {
    
            return value1.IsNaN() || value2.IsNaN() || value3.IsNaN() ? value1 :
                value1 < value2 && value1 < value3 ? value1 :
                value2 < value3 ? value2 : value3;
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the larger value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static long Max(this long value1, long value2, long value3)        
        {
    
            return value1.IsNaN() || value2.IsNaN() || value3.IsNaN() ? value1 :
                value1 > value2 && value1 > value3 ? value1 :
                value2 > value3 ? value2 : value3;
        }
   
            /// <summary>
            ///     <para>
            ///         Returns smallest of two or more values.
            ///     </para>
            /// </summary>
            /// <param name="a" />
            /// <param name="b" />
            /// <param name="values" />
            [MethodImpl(GlobalConst.ImplOptions)]
            public static long Min(long val1, params long[] values)
            {
                int length = values.Length;
                if (length == 0)
                    return val1;
                long num = val1;
                for (int index = 0; index < length; ++index)
                {
                    num.Min(values[index]);
                }
                return num;
            }
   
            /// <summary>
            ///     <para>
            ///         Returns largest of two or more values.
            ///     </para>
            /// </summary>
            /// <param name="a" />
            /// <param name="b" />
            /// <param name="values" />
            [MethodImpl(GlobalConst.ImplOptions)]
            public static long Max(long val1, params long[] values)
            {
                int length = values.Length;
                if (length == 0)
                    return val1;
                long num = val1;
                for (int index = 0; index < length; ++index)
                {
                    num.Max(values[index]);
                }
                return num;
            }
#if UNITY_MATHEMATICS
#endif
        [MethodImpl(GlobalConst.ImplOptions)]
        public static long Length(long x, long y)
        {
            return (long)Math.Sqrt((double)(x * x + y * y));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static long Length(long x, long y, long z)
        {
            return (long)Math.Sqrt((double)(x * x + y * y + z * z));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static long Length(long x, long y, long z, long w)
        {
            return (long)Math.Sqrt((double)(x * x + y * y + z * z + w * w));
        }
        [MethodImpl(GlobalConst.ImplOptions)]
        public static long LengthSquared(long x, long y)
        {
            return (long)(double)(x * x + y * y);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static long LengthSquared(long x, long y, long z)
        {
            return (long)(double)(x * x + y * y + z * z);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static long LengthSquared(long x, long y, long z, long w)
        {
            return (long)(double)(x * x + y * y + z * z + w * w);
        }
#if UNITY_MATHEMATICS
        
        /// <summary>
        /// Returns the determinant of the 2x2 matrix. Performs X1 * Y2 - X2 * Y1
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static long Determinant2(in long x1, in long x2, in long y1, in long y2)        
        {
            return (long)(x1 * y2 - x2 * y1);
        }
#endif
#if UNITY_MATHEMATICS
        
        /// <summary>
        /// Returns the value of the first argument raised to the power of the second argument.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static long Pow(this in long value, in long power)        
        {
            return (long)Math.Pow(value, power);
        }
#endif
        
        /// <summary>
        /// Interpolates between two values using a linear function by a given amount.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static long Lerp(this in long from, in long to, in float amount)        
        {
            return (long)((1.0f - amount) * from + amount * to);
        }
        
        /// <summary>
        /// Interpolates between two values using a linear function by a given amount.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static long Lerp(this in long from, in long to, in double amount)        
        {
            return (long)((1.0f - amount) * from + amount * to);
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Repeats a value to ensue that the value stays in the defined range.
        /// If the value is under the offset: The range will be added from the value.
        /// If the value is over the offset + range: The range will be subtracted from the value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static long Repeat(this long value, long range, long offset = default)        
        {
            long min = offset;
            long max = (long)(offset + range);
            long result = value;
            while (result < min)
            {
                result += range;
            }
            while (result > max)
            {
                result -= range;
            }

            return result;
        }
        
        /// <summary>
        /// Returns the negated value.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static long Negated(this in long value)        
        {
            return (long)(value * longMinusOne);
        }
        
        /// <summary>
        /// Checks if a value is in range.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsInRange(this in long value, in long min, in long max)        
        {
            return value >= min && value <= max;
        }
        
        /// <summary>
        /// Checks if a value is in range of 0-1.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsInRange01(this in long value)        
        {
            return value.IsInRange(0L, 1L);
        }
        
        #endregion
        
        #region SECTION COMPARE ulong
        
        #region Even Odd
        
        /// <summary>
        /// Check if value is even.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsEven(this in ulong value)        
        {
            return value % 2 == 0;
        }
        
        /// <summary>
        /// Check if value is odd.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsOdd(this in ulong value)        
        {
            return value % 2 != 0;
        }
        
        #endregion
        
        /// <summary>
        /// Returns the value with positive sign.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static ulong Abs(this ulong value)        
        {
            //This is added for compability reasons. Otherwise .. useless.
            return value;
        }

        /// <summary>
        /// Determines whether the specified value is close to the other one.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool Equal(this ulong value1, ulong value2)
        {
            return IsZero(value1 - value2);
        }

        /// <summary>
        /// Returns true if the number is below zero.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsNegative(this ulong value)
        {
            return value < 0;
        }

        /// <summary>
        /// Returns true if the number is zero or above.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsPositive(this ulong value)
        {
            return value >= 0;
        }

        /// <summary>
        /// Returns true if the number is zero.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsZero(this ulong value)
        {
            return value == 0;
        }

        /// <summary>
        /// Returns true if the number is one.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsOne(this ulong value)
        {
            return value == 1;
        }

        /// <summary>
        /// Returns true if the number is one.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsNaN(this ulong value)
        {
            return false;
        }
        
        #endregion
        
        #region SECTION MATH ulong
        
        #region Advanced Math
        public static ulong Remap(this ulong value, ulong fromMin, ulong fromMax, ulong toMin, ulong toMax)
        {
            var fromAbs  =  value - fromMin;
            var fromMaxAbs = fromMax - fromMin;      
       
            var normal = fromAbs / fromMaxAbs;
 
            var toMaxAbs = toMax - toMin;
            var toAbs = toMaxAbs * normal;
 
            var to = toAbs + toMin;
       
            return (ulong)to;
        }

        public static ulong DistanceOnGrid(ulong x1, ulong y1, ulong x2, ulong y2) {
            var dx = (x2 - x1).Abs();
            var dy = (y2 - y1).Abs();

            var min = Min(dx, dy);
            var max = Max(dx, dy);

            var diagonalSteps = min;
            var straightSteps = max - min;

            return (ulong)(doubleSqrt2 * diagonalSteps + straightSteps);
        }

                
        #endregion
        /// <summary>
        ///     Returns the number of digits inside 'number'.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static int GetNumberOfDigits(ulong number)
        {
            return number == 0 ? 1 : (int) Math.Floor(Math.Log10(number.Abs()) + 1);
        }
        
        /// <summary>
        /// Clamp a value between 0 and 1
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static void Clamp01(ref ulong value)        
        {
            Clamp(ref value, 0ul, 1ul);
        }
        
        /// <summary>
        /// Clamp a value between 0 and 1
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static ulong Clamp01(this ulong value)        
        {
            Clamp01(ref value);
            return value;
        }
        
        /// <summary>
        /// Clamp the specified value.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static ulong Clamp(this ulong value, in ulong min, in ulong max)        
        {
            Clamp(ref value, min, max);
            return value;
        }
        
        /// <summary>
        /// Clamp the specified value.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static void Clamp(ref ulong value, in ulong min, in ulong max)        
        {
            if (value < min)
            {
	            value = min;
            }
            else if (value > max)
            {
	            value = max;
            }
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Scale the value and add a offset.
        /// </summary>
		/// <returns>Returns the scaled value with offset</returns>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static ulong ScaleOffset(this ulong value, ulong scale, ulong offset)        
        {
            return (ulong)(offset + value * scale);
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the smaller value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static ulong Min(this in ulong value1, in ulong value2)        
        {
            return 
value1 < value2 || value1.IsNaN() ? value1 : value2;            
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the larger value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static ulong Max(this in ulong value1, in ulong value2)        
        {
            return 
value1 > value2 || value1.IsNaN() ? value1 : value2;            
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the smaller value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static ulong Min(this ulong value1, ulong value2, ulong value3)        
        {
    
            return value1.IsNaN() || value2.IsNaN() || value3.IsNaN() ? value1 :
                value1 < value2 && value1 < value3 ? value1 :
                value2 < value3 ? value2 : value3;
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the larger value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static ulong Max(this ulong value1, ulong value2, ulong value3)        
        {
    
            return value1.IsNaN() || value2.IsNaN() || value3.IsNaN() ? value1 :
                value1 > value2 && value1 > value3 ? value1 :
                value2 > value3 ? value2 : value3;
        }
   
            /// <summary>
            ///     <para>
            ///         Returns smallest of two or more values.
            ///     </para>
            /// </summary>
            /// <param name="a" />
            /// <param name="b" />
            /// <param name="values" />
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ulong Min(ulong val1, params ulong[] values)
            {
                int length = values.Length;
                if (length == 0)
                    return val1;
                ulong num = val1;
                for (int index = 0; index < length; ++index)
                {
                    num.Min(values[index]);
                }
                return num;
            }
   
            /// <summary>
            ///     <para>
            ///         Returns largest of two or more values.
            ///     </para>
            /// </summary>
            /// <param name="a" />
            /// <param name="b" />
            /// <param name="values" />
            [MethodImpl(GlobalConst.ImplOptions)]
            public static ulong Max(ulong val1, params ulong[] values)
            {
                int length = values.Length;
                if (length == 0)
                    return val1;
                ulong num = val1;
                for (int index = 0; index < length; ++index)
                {
                    num.Max(values[index]);
                }
                return num;
            }
#if UNITY_MATHEMATICS
#endif
        [MethodImpl(GlobalConst.ImplOptions)]
        public static ulong Length(ulong x, ulong y)
        {
            return (ulong)Math.Sqrt((double)(x * x + y * y));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static ulong Length(ulong x, ulong y, ulong z)
        {
            return (ulong)Math.Sqrt((double)(x * x + y * y + z * z));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static ulong Length(ulong x, ulong y, ulong z, ulong w)
        {
            return (ulong)Math.Sqrt((double)(x * x + y * y + z * z + w * w));
        }
        [MethodImpl(GlobalConst.ImplOptions)]
        public static ulong LengthSquared(ulong x, ulong y)
        {
            return (ulong)(double)(x * x + y * y);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static ulong LengthSquared(ulong x, ulong y, ulong z)
        {
            return (ulong)(double)(x * x + y * y + z * z);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static ulong LengthSquared(ulong x, ulong y, ulong z, ulong w)
        {
            return (ulong)(double)(x * x + y * y + z * z + w * w);
        }
#if UNITY_MATHEMATICS
        
        /// <summary>
        /// Returns the determinant of the 2x2 matrix. Performs X1 * Y2 - X2 * Y1
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static ulong Determinant2(in ulong x1, in ulong x2, in ulong y1, in ulong y2)        
        {
            return (ulong)(x1 * y2 - x2 * y1);
        }
#endif
#if UNITY_MATHEMATICS
        
        /// <summary>
        /// Returns the value of the first argument raised to the power of the second argument.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static ulong Pow(this in ulong value, in ulong power)        
        {
            return (ulong)Math.Pow(value, power);
        }
#endif
        
        /// <summary>
        /// Interpolates between two values using a linear function by a given amount.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static ulong Lerp(this in ulong from, in ulong to, in float amount)        
        {
            return (ulong)((1.0f - amount) * from + amount * to);
        }
        
        /// <summary>
        /// Interpolates between two values using a linear function by a given amount.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static ulong Lerp(this in ulong from, in ulong to, in double amount)        
        {
            return (ulong)((1.0f - amount) * from + amount * to);
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Repeats a value to ensue that the value stays in the defined range.
        /// If the value is under the offset: The range will be added from the value.
        /// If the value is over the offset + range: The range will be subtracted from the value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static ulong Repeat(this ulong value, ulong range, ulong offset = default)        
        {
            ulong min = offset;
            ulong max = (ulong)(offset + range);
            ulong result = value;
            while (result < min)
            {
                result += range;
            }
            while (result > max)
            {
                result -= range;
            }

            return result;
        }
        
        /// <summary>
        /// Checks if a value is in range.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsInRange(this in ulong value, in ulong min, in ulong max)        
        {
            return value >= min && value <= max;
        }
        
        /// <summary>
        /// Checks if a value is in range of 0-1.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsInRange01(this in ulong value)        
        {
            return value.IsInRange(0ul, 1ul);
        }
        
        #endregion
        
        #region SECTION COMPARE byte
        
        #region Even Odd
        
        /// <summary>
        /// Check if value is even.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsEven(this in byte value)        
        {
            return value % 2 == 0;
        }
        
        /// <summary>
        /// Check if value is odd.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsOdd(this in byte value)        
        {
            return value % 2 != 0;
        }
        
        #endregion
        
        /// <summary>
        /// Returns the value with positive sign.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static byte Abs(this byte value)        
        {
            //This is added for compability reasons. Otherwise .. useless.
            return value;
        }

        /// <summary>
        /// Determines whether the specified value is close to the other one.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool Equal(this byte value1, byte value2)
        {
            return IsZero(value1 - value2);
        }

        /// <summary>
        /// Returns true if the number is below zero.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsNegative(this byte value)
        {
            return value < 0;
        }

        /// <summary>
        /// Returns true if the number is zero or above.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsPositive(this byte value)
        {
            return value >= 0;
        }

        /// <summary>
        /// Returns true if the number is zero.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsZero(this byte value)
        {
            return value == 0;
        }

        /// <summary>
        /// Returns true if the number is one.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsOne(this byte value)
        {
            return value == 1;
        }

        /// <summary>
        /// Returns true if the number is one.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsNaN(this byte value)
        {
            return false;
        }
        
        #endregion
        
        #region SECTION MATH byte
        
        #region Advanced Math
        public static byte Remap(this byte value, byte fromMin, byte fromMax, byte toMin, byte toMax)
        {
            var fromAbs  =  value - fromMin;
            var fromMaxAbs = fromMax - fromMin;      
       
            var normal = fromAbs / fromMaxAbs;
 
            var toMaxAbs = toMax - toMin;
            var toAbs = toMaxAbs * normal;
 
            var to = toAbs + toMin;
       
            return (byte)to;
        }

        public static byte DistanceOnGrid(byte x1, byte y1, byte x2, byte y2) {
            var dx = (x2 - x1).Abs();
            var dy = (y2 - y1).Abs();

            var min = Min(dx, dy);
            var max = Max(dx, dy);

            var diagonalSteps = min;
            var straightSteps = max - min;

            return (byte)(floatSqrt2 * diagonalSteps + straightSteps);
        }

                
        #endregion
        /// <summary>
        ///     Returns the number of digits inside 'number'.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static int GetNumberOfDigits(byte number)
        {
            return number == 0 ? 1 : (int) Math.Floor(Math.Log10(number.Abs()) + 1);
        }
        
        /// <summary>
        /// Clamp a value between 0 and 1
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static void Clamp01(ref byte value)        
        {
            Clamp(ref value, (byte)0, (byte)1);
        }
        
        /// <summary>
        /// Clamp a value between 0 and 1
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static byte Clamp01(this byte value)        
        {
            Clamp01(ref value);
            return value;
        }
        
        /// <summary>
        /// Clamp the specified value.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static byte Clamp(this byte value, in byte min, in byte max)        
        {
            Clamp(ref value, min, max);
            return value;
        }
        
        /// <summary>
        /// Clamp the specified value.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static void Clamp(ref byte value, in byte min, in byte max)        
        {
            if (value < min)
            {
	            value = min;
            }
            else if (value > max)
            {
	            value = max;
            }
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Scale the value and add a offset.
        /// </summary>
		/// <returns>Returns the scaled value with offset</returns>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static byte ScaleOffset(this byte value, byte scale, byte offset)        
        {
            return (byte)(offset + value * scale);
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the smaller value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static byte Min(this in byte value1, in byte value2)        
        {
            return 
value1 < value2 || value1.IsNaN() ? value1 : value2;            
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the larger value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static byte Max(this in byte value1, in byte value2)        
        {
            return 
value1 > value2 || value1.IsNaN() ? value1 : value2;            
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the smaller value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static byte Min(this byte value1, byte value2, byte value3)        
        {
    
            return value1.IsNaN() || value2.IsNaN() || value3.IsNaN() ? value1 :
                value1 < value2 && value1 < value3 ? value1 :
                value2 < value3 ? value2 : value3;
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the larger value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static byte Max(this byte value1, byte value2, byte value3)        
        {
    
            return value1.IsNaN() || value2.IsNaN() || value3.IsNaN() ? value1 :
                value1 > value2 && value1 > value3 ? value1 :
                value2 > value3 ? value2 : value3;
        }
   
            /// <summary>
            ///     <para>
            ///         Returns smallest of two or more values.
            ///     </para>
            /// </summary>
            /// <param name="a" />
            /// <param name="b" />
            /// <param name="values" />
            [MethodImpl(GlobalConst.ImplOptions)]
            public static byte Min(byte val1, params byte[] values)
            {
                int length = values.Length;
                if (length == 0)
                    return val1;
                byte num = val1;
                for (int index = 0; index < length; ++index)
                {
                    num.Min(values[index]);
                }
                return num;
            }
   
            /// <summary>
            ///     <para>
            ///         Returns largest of two or more values.
            ///     </para>
            /// </summary>
            /// <param name="a" />
            /// <param name="b" />
            /// <param name="values" />
            [MethodImpl(GlobalConst.ImplOptions)]
            public static byte Max(byte val1, params byte[] values)
            {
                int length = values.Length;
                if (length == 0)
                    return val1;
                byte num = val1;
                for (int index = 0; index < length; ++index)
                {
                    num.Max(values[index]);
                }
                return num;
            }
        [MethodImpl(GlobalConst.ImplOptions)]
        public static byte Length(byte x, byte y)
        {
            return (byte)Math.Sqrt((double)(x * x + y * y));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static byte Length(byte x, byte y, byte z)
        {
            return (byte)Math.Sqrt((double)(x * x + y * y + z * z));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static byte Length(byte x, byte y, byte z, byte w)
        {
            return (byte)Math.Sqrt((double)(x * x + y * y + z * z + w * w));
        }
        [MethodImpl(GlobalConst.ImplOptions)]
        public static byte LengthSquared(byte x, byte y)
        {
            return (byte)(double)(x * x + y * y);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static byte LengthSquared(byte x, byte y, byte z)
        {
            return (byte)(double)(x * x + y * y + z * z);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static byte LengthSquared(byte x, byte y, byte z, byte w)
        {
            return (byte)(double)(x * x + y * y + z * z + w * w);
        }
        
        /// <summary>
        /// Returns the determinant of the 2x2 matrix. Performs X1 * Y2 - X2 * Y1
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static byte Determinant2(in byte x1, in byte x2, in byte y1, in byte y2)        
        {
            return (byte)(x1 * y2 - x2 * y1);
        }
        
        /// <summary>
        /// Returns the value of the first argument raised to the power of the second argument.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static byte Pow(this in byte value, in byte power)        
        {
            return (byte)Math.Pow(value, power);
        }
        
        /// <summary>
        /// Interpolates between two values using a linear function by a given amount.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static byte Lerp(this in byte from, in byte to, in float amount)        
        {
            return (byte)((1.0f - amount) * from + amount * to);
        }
        
        /// <summary>
        /// Interpolates between two values using a linear function by a given amount.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static byte Lerp(this in byte from, in byte to, in double amount)        
        {
            return (byte)((1.0f - amount) * from + amount * to);
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Repeats a value to ensue that the value stays in the defined range.
        /// If the value is under the offset: The range will be added from the value.
        /// If the value is over the offset + range: The range will be subtracted from the value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static byte Repeat(this byte value, byte range, byte offset = default)        
        {
            byte min = offset;
            byte max = (byte)(offset + range);
            byte result = value;
            while (result < min)
            {
                result += range;
            }
            while (result > max)
            {
                result -= range;
            }

            return result;
        }
        
        /// <summary>
        /// Checks if a value is in range.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsInRange(this in byte value, in byte min, in byte max)        
        {
            return value >= min && value <= max;
        }
        
        /// <summary>
        /// Checks if a value is in range of 0-1.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsInRange01(this in byte value)        
        {
            return value.IsInRange((byte)0, (byte)1);
        }
        
        #endregion
        
        #region SECTION COMPARE sbyte
        
        #region Even Odd
        
        /// <summary>
        /// Check if value is even.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsEven(this in sbyte value)        
        {
            return value % 2 == 0;
        }
        
        /// <summary>
        /// Check if value is odd.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsOdd(this in sbyte value)        
        {
            return value % 2 != 0;
        }
        
        #endregion
        
        /// <summary>
        /// Returns the value with positive sign.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static sbyte Abs(this sbyte value)        
        {
            return (sbyte)Math.Abs(value);
        }

        /// <summary>
        /// Determines whether the specified value is close to the other one.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool Equal(this sbyte value1, sbyte value2)
        {
            return IsZero(value1 - value2);
        }

        /// <summary>
        /// Returns true if the number is below zero.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsNegative(this sbyte value)
        {
            return value < 0;
        }

        /// <summary>
        /// Returns true if the number is zero or above.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsPositive(this sbyte value)
        {
            return value >= 0;
        }

        /// <summary>
        /// Returns true if the number is zero.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsZero(this sbyte value)
        {
            return value == 0;
        }

        /// <summary>
        /// Returns true if the number is one.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsOne(this sbyte value)
        {
            return value == 1;
        }

        /// <summary>
        /// Returns true if the number is one.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsNaN(this sbyte value)
        {
            return false;
        }
        
        #endregion
        
        #region SECTION MATH sbyte
        
        #region Advanced Math
        public static sbyte Remap(this sbyte value, sbyte fromMin, sbyte fromMax, sbyte toMin, sbyte toMax)
        {
            var fromAbs  =  value - fromMin;
            var fromMaxAbs = fromMax - fromMin;      
       
            var normal = fromAbs / fromMaxAbs;
 
            var toMaxAbs = toMax - toMin;
            var toAbs = toMaxAbs * normal;
 
            var to = toAbs + toMin;
       
            return (sbyte)to;
        }

        public static sbyte DistanceOnGrid(sbyte x1, sbyte y1, sbyte x2, sbyte y2) {
            var dx = (x2 - x1).Abs();
            var dy = (y2 - y1).Abs();

            var min = Min(dx, dy);
            var max = Max(dx, dy);

            var diagonalSteps = min;
            var straightSteps = max - min;

            return (sbyte)(floatSqrt2 * diagonalSteps + straightSteps);
        }

                
        #endregion
        /// <summary>
        ///     Returns the number of digits inside 'number'.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static int GetNumberOfDigits(sbyte number)
        {
            return number == 0 ? 1 : (int) Math.Floor(Math.Log10(number.Abs()) + 1);
        }
        
        /// <summary>
        /// Clamp a value between 0 and 1
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static void Clamp01(ref sbyte value)        
        {
            Clamp(ref value, (sbyte)0, (sbyte)1);
        }
        
        /// <summary>
        /// Clamp a value between 0 and 1
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static sbyte Clamp01(this sbyte value)        
        {
            Clamp01(ref value);
            return value;
        }
        
        /// <summary>
        /// Clamp the specified value.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static sbyte Clamp(this sbyte value, in sbyte min, in sbyte max)        
        {
            Clamp(ref value, min, max);
            return value;
        }
        
        /// <summary>
        /// Clamp the specified value.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static void Clamp(ref sbyte value, in sbyte min, in sbyte max)        
        {
            if (value < min)
            {
	            value = min;
            }
            else if (value > max)
            {
	            value = max;
            }
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Scale the value and add a offset.
        /// </summary>
		/// <returns>Returns the scaled value with offset</returns>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static sbyte ScaleOffset(this sbyte value, sbyte scale, sbyte offset)        
        {
            return (sbyte)(offset + value * scale);
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the smaller value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static sbyte Min(this in sbyte value1, in sbyte value2)        
        {
            return 
value1 < value2 || value1.IsNaN() ? value1 : value2;            
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the larger value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static sbyte Max(this in sbyte value1, in sbyte value2)        
        {
            return 
value1 > value2 || value1.IsNaN() ? value1 : value2;            
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the smaller value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static sbyte Min(this sbyte value1, sbyte value2, sbyte value3)        
        {
    
            return value1.IsNaN() || value2.IsNaN() || value3.IsNaN() ? value1 :
                value1 < value2 && value1 < value3 ? value1 :
                value2 < value3 ? value2 : value3;
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Returns the larger value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static sbyte Max(this sbyte value1, sbyte value2, sbyte value3)        
        {
    
            return value1.IsNaN() || value2.IsNaN() || value3.IsNaN() ? value1 :
                value1 > value2 && value1 > value3 ? value1 :
                value2 > value3 ? value2 : value3;
        }
   
            /// <summary>
            ///     <para>
            ///         Returns smallest of two or more values.
            ///     </para>
            /// </summary>
            /// <param name="a" />
            /// <param name="b" />
            /// <param name="values" />
            [MethodImpl(GlobalConst.ImplOptions)]
            public static sbyte Min(sbyte val1, params sbyte[] values)
            {
                int length = values.Length;
                if (length == 0)
                    return val1;
                sbyte num = val1;
                for (int index = 0; index < length; ++index)
                {
                    num.Min(values[index]);
                }
                return num;
            }
   
            /// <summary>
            ///     <para>
            ///         Returns largest of two or more values.
            ///     </para>
            /// </summary>
            /// <param name="a" />
            /// <param name="b" />
            /// <param name="values" />
            [MethodImpl(GlobalConst.ImplOptions)]
            public static sbyte Max(sbyte val1, params sbyte[] values)
            {
                int length = values.Length;
                if (length == 0)
                    return val1;
                sbyte num = val1;
                for (int index = 0; index < length; ++index)
                {
                    num.Max(values[index]);
                }
                return num;
            }
        [MethodImpl(GlobalConst.ImplOptions)]
        public static sbyte Length(sbyte x, sbyte y)
        {
            return (sbyte)Math.Sqrt((double)(x * x + y * y));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static sbyte Length(sbyte x, sbyte y, sbyte z)
        {
            return (sbyte)Math.Sqrt((double)(x * x + y * y + z * z));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static sbyte Length(sbyte x, sbyte y, sbyte z, sbyte w)
        {
            return (sbyte)Math.Sqrt((double)(x * x + y * y + z * z + w * w));
        }
        [MethodImpl(GlobalConst.ImplOptions)]
        public static sbyte LengthSquared(sbyte x, sbyte y)
        {
            return (sbyte)(double)(x * x + y * y);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static sbyte LengthSquared(sbyte x, sbyte y, sbyte z)
        {
            return (sbyte)(double)(x * x + y * y + z * z);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static sbyte LengthSquared(sbyte x, sbyte y, sbyte z, sbyte w)
        {
            return (sbyte)(double)(x * x + y * y + z * z + w * w);
        }
        
        /// <summary>
        /// Returns the determinant of the 2x2 matrix. Performs X1 * Y2 - X2 * Y1
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static sbyte Determinant2(in sbyte x1, in sbyte x2, in sbyte y1, in sbyte y2)        
        {
            return (sbyte)(x1 * y2 - x2 * y1);
        }
        
        /// <summary>
        /// Returns the value of the first argument raised to the power of the second argument.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static sbyte Pow(this in sbyte value, in sbyte power)        
        {
            return (sbyte)Math.Pow(value, power);
        }
        
        /// <summary>
        /// Interpolates between two values using a linear function by a given amount.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static sbyte Lerp(this in sbyte from, in sbyte to, in float amount)        
        {
            return (sbyte)((1.0f - amount) * from + amount * to);
        }
        
        /// <summary>
        /// Interpolates between two values using a linear function by a given amount.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static sbyte Lerp(this in sbyte from, in sbyte to, in double amount)        
        {
            return (sbyte)((1.0f - amount) * from + amount * to);
        }
        
        /// <summary>
        /// 
        /// <summary>
        /// Repeats a value to ensue that the value stays in the defined range.
        /// If the value is under the offset: The range will be added from the value.
        /// If the value is over the offset + range: The range will be subtracted from the value.
        /// </summary>
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static sbyte Repeat(this sbyte value, sbyte range, sbyte offset = default)        
        {
            sbyte min = offset;
            sbyte max = (sbyte)(offset + range);
            sbyte result = value;
            while (result < min)
            {
                result += range;
            }
            while (result > max)
            {
                result -= range;
            }

            return result;
        }
        
        /// <summary>
        /// Returns the negated value.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static sbyte Negated(this in sbyte value)        
        {
            return (sbyte)(value * sbyteMinusOne);
        }
        
        /// <summary>
        /// Checks if a value is in range.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsInRange(this in sbyte value, in sbyte min, in sbyte max)        
        {
            return value >= min && value <= max;
        }
        
        /// <summary>
        /// Checks if a value is in range of 0-1.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static bool IsInRange01(this in sbyte value)        
        {
            return value.IsInRange((sbyte)0, (sbyte)1);
        }
        
        #endregion
    }
}