
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
using FinalFactory.Observable;
namespace FinalFactory.Mathematics.Units
{
    
    #region SECTION float
    [PublicAPI, DebuggerDisplay("RangeF Value:{Value} Min:{Min} Max:{Max}")]
    // ReSharper disable once InconsistentNaming
    public struct RangeF : IEquatable<RangeF>, IComparable<RangeF>, IComparable, IFormattable, ICloneable
    {
        
        /// <summary>
        /// The size of a <see cref="RangeF" /> type, in bytes.
        /// </summary>
        public static readonly int SizeInBytes = Marshal.SizeOf(typeof(RangeF));
        
        /// <summary>
        /// A <see cref="RangeF"/> with all of its components set to zero.
        /// </summary>
        public static readonly RangeF Zero = new RangeF(0F);
        
        /// <summary>
        /// A <see cref="RangeF"/> with all of its components set to one.
        /// </summary>
        public static readonly RangeF One = new RangeF(1F);
        
        /// <summary>
        /// Field access 'Value' of this <see cref="RangeF"/>
        /// </summary>
        public float Value;
        
        /// <summary>
        /// Field access 'Min' of this <see cref="RangeF"/>
        /// </summary>
        public float Min;
        
        /// <summary>
        /// Field access 'Max' of this <see cref="RangeF"/>
        /// </summary>
        public float Max;
        
        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public RangeF(float value)        
        {
            Value = (float)value;
            Min = (float)value;
            Max = (float)value;
        }
        
        /// <summary>
        /// Constructs a new instance from an existing instance.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public RangeF(RangeF value)        
        {
            this.Value = (float)value.Value;
            this.Min = (float)value.Min;
            this.Max = (float)value.Max;
        }
        
        /// <summary>
        /// Constructs a new instance and fill all fields.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public RangeF(float value, float min, float max)        
        {
            this.Value = (float)value;
            this.Min = (float)min;
            this.Max = (float)max;
        }
        public float Span => (float)(Max - Min);
        
        public Percentage Percentage
        {
            get => new Percentage((Value - Min) / (double)Span);
            set => Value = (float)(Min + value * Span);
        }

        public void SetMinMax(float min, float max)
        {
            Min = min;
            Max = max;
            Check();
        }

        public void MoveMinWithCurrentSpan(float min)
        {
            var span = Span;
            Max = (float)(min + span);
            Min = min;
        }
        
        public void MoveMaxWithCurrentSpan(float max)
        {
            var span = Span;
            Min = (float)(max - span);
            Max = max;
        }
        
        private void Check()
        {
            if (Min > Max)
            {
                throw new InvalidOperationException("Min value can not be greater than max value");
            }

            if (Value < Min)
            {
                Value = Min;
            }
            
            if (Value > Max)
            {
                Value = Max;
            }
        }
        /// <inheritdoc />
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public override bool Equals(object obj)        
        {
            if (obj is RangeF i)
            {
	            return Equals(i);
            }
            return false;
        }
        /// <inheritdoc />
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public bool Equals(RangeF obj)        
        {
            return Value.Equals(obj.Value) && Min.Equals(obj.Min) && Max.Equals(obj.Max);
        }
        
        /// <summary>
        /// Performs an equalization with <see cref="RangeF"/>.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool operator ==(RangeF left, RangeF right) => left.Equals(right);
        
        /// <summary>
        /// Performs an not equalization with <see cref="RangeF"/>.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool operator !=(RangeF left, RangeF right) => !left.Equals(right);
        
        /// <summary>
        /// Compares this <see cref="RangeF"/> with another object.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public int CompareTo(object other)        
        {
            if (other is RangeF i)
            {
	            return CompareTo(i);
            }
            return -1;
        }
        
        /// <summary>
        /// Compares this <see cref="RangeF"/> with another <see cref="RangeF"/>.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public int CompareTo(RangeF other)        
        {
            if (!Value.Equals(other.Value))
            {
	            return Value.CompareTo(other.Value);
            }
            if (!Min.Equals(other.Min))
            {
	            return Min.CompareTo(other.Min);
            }
            return Max.CompareTo(other.Max);
        }
        
        /// <summary>
        /// Returns a string that represents the current <see cref="RangeF"/>.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public override string ToString()        
        {
            return $"{{Value: {Value.ToString(CultureInfo.InvariantCulture)} Min: {Min.ToString(CultureInfo.InvariantCulture)} Max: {Max.ToString(CultureInfo.InvariantCulture)}}}";
        }
        
        /// <summary>
        /// Returns a string that represents the current <see cref="RangeF"/>.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public string ToString(CultureInfo info)        
        {
            return $"{{Value: {Value.ToString(info)} Min: {Min.ToString(info)} Max: {Max.ToString(info)}}}";
        }
        
        /// <summary>
        /// Returns a string that represents the current <see cref="RangeF"/>.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public string ToString(string format)        
        {
            return $"{{Value: {Value.ToString(format)} Min: {Min.ToString(format)} Max: {Max.ToString(format)}}}";
        }
        
        /// <summary>
        /// Returns a string that represents the current <see cref="RangeF"/>.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public string ToString(string format, IFormatProvider formatProvider)        
        {
            return $"{{Value: {Value.ToString(format, formatProvider)} Min: {Min.ToString(format, formatProvider)} Max: {Max.ToString(format, formatProvider)}}}";
        }
        
        /// <summary>
        /// Returns the hash code for this <see cref="RangeF"/>.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public override int GetHashCode()        
        {
            return Value.GetHashCode()* 397 ^ Min.GetHashCode()* 397 ^ Max.GetHashCode();
        }
        
        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public object Clone()        
        {
            return new RangeF(Value, Min, Max);
        }
    }
    
    #endregion
    
    #region SECTION double
    [PublicAPI, DebuggerDisplay("RangeD Value:{Value} Min:{Min} Max:{Max}")]
    // ReSharper disable once InconsistentNaming
    public struct RangeD : IEquatable<RangeD>, IComparable<RangeD>, IComparable, IFormattable, ICloneable
    {
        
        /// <summary>
        /// The size of a <see cref="RangeD" /> type, in bytes.
        /// </summary>
        public static readonly int SizeInBytes = Marshal.SizeOf(typeof(RangeD));
        
        /// <summary>
        /// A <see cref="RangeD"/> with all of its components set to zero.
        /// </summary>
        public static readonly RangeD Zero = new RangeD(0D);
        
        /// <summary>
        /// A <see cref="RangeD"/> with all of its components set to one.
        /// </summary>
        public static readonly RangeD One = new RangeD(1D);
        
        /// <summary>
        /// Field access 'Value' of this <see cref="RangeD"/>
        /// </summary>
        public double Value;
        
        /// <summary>
        /// Field access 'Min' of this <see cref="RangeD"/>
        /// </summary>
        public double Min;
        
        /// <summary>
        /// Field access 'Max' of this <see cref="RangeD"/>
        /// </summary>
        public double Max;
        
        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public RangeD(double value)        
        {
            Value = (double)value;
            Min = (double)value;
            Max = (double)value;
        }
        
        /// <summary>
        /// Constructs a new instance from an existing instance.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public RangeD(RangeD value)        
        {
            this.Value = (double)value.Value;
            this.Min = (double)value.Min;
            this.Max = (double)value.Max;
        }
        
        /// <summary>
        /// Constructs a new instance and fill all fields.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public RangeD(double value, double min, double max)        
        {
            this.Value = (double)value;
            this.Min = (double)min;
            this.Max = (double)max;
        }
        public double Span => (double)(Max - Min);
        
        public Percentage Percentage
        {
            get => new Percentage((Value - Min) / (double)Span);
            set => Value = (double)(Min + value * Span);
        }

        public void SetMinMax(double min, double max)
        {
            Min = min;
            Max = max;
            Check();
        }

        public void MoveMinWithCurrentSpan(double min)
        {
            var span = Span;
            Max = (double)(min + span);
            Min = min;
        }
        
        public void MoveMaxWithCurrentSpan(double max)
        {
            var span = Span;
            Min = (double)(max - span);
            Max = max;
        }
        
        private void Check()
        {
            if (Min > Max)
            {
                throw new InvalidOperationException("Min value can not be greater than max value");
            }

            if (Value < Min)
            {
                Value = Min;
            }
            
            if (Value > Max)
            {
                Value = Max;
            }
        }
        /// <inheritdoc />
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public override bool Equals(object obj)        
        {
            if (obj is RangeD i)
            {
	            return Equals(i);
            }
            return false;
        }
        /// <inheritdoc />
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public bool Equals(RangeD obj)        
        {
            return Value.Equals(obj.Value) && Min.Equals(obj.Min) && Max.Equals(obj.Max);
        }
        
        /// <summary>
        /// Performs an equalization with <see cref="RangeD"/>.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool operator ==(RangeD left, RangeD right) => left.Equals(right);
        
        /// <summary>
        /// Performs an not equalization with <see cref="RangeD"/>.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool operator !=(RangeD left, RangeD right) => !left.Equals(right);
        
        /// <summary>
        /// Compares this <see cref="RangeD"/> with another object.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public int CompareTo(object other)        
        {
            if (other is RangeD i)
            {
	            return CompareTo(i);
            }
            return -1;
        }
        
        /// <summary>
        /// Compares this <see cref="RangeD"/> with another <see cref="RangeD"/>.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public int CompareTo(RangeD other)        
        {
            if (!Value.Equals(other.Value))
            {
	            return Value.CompareTo(other.Value);
            }
            if (!Min.Equals(other.Min))
            {
	            return Min.CompareTo(other.Min);
            }
            return Max.CompareTo(other.Max);
        }
        
        /// <summary>
        /// Returns a string that represents the current <see cref="RangeD"/>.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public override string ToString()        
        {
            return $"{{Value: {Value.ToString(CultureInfo.InvariantCulture)} Min: {Min.ToString(CultureInfo.InvariantCulture)} Max: {Max.ToString(CultureInfo.InvariantCulture)}}}";
        }
        
        /// <summary>
        /// Returns a string that represents the current <see cref="RangeD"/>.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public string ToString(CultureInfo info)        
        {
            return $"{{Value: {Value.ToString(info)} Min: {Min.ToString(info)} Max: {Max.ToString(info)}}}";
        }
        
        /// <summary>
        /// Returns a string that represents the current <see cref="RangeD"/>.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public string ToString(string format)        
        {
            return $"{{Value: {Value.ToString(format)} Min: {Min.ToString(format)} Max: {Max.ToString(format)}}}";
        }
        
        /// <summary>
        /// Returns a string that represents the current <see cref="RangeD"/>.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public string ToString(string format, IFormatProvider formatProvider)        
        {
            return $"{{Value: {Value.ToString(format, formatProvider)} Min: {Min.ToString(format, formatProvider)} Max: {Max.ToString(format, formatProvider)}}}";
        }
        
        /// <summary>
        /// Returns the hash code for this <see cref="RangeD"/>.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public override int GetHashCode()        
        {
            return Value.GetHashCode()* 397 ^ Min.GetHashCode()* 397 ^ Max.GetHashCode();
        }
        
        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public object Clone()        
        {
            return new RangeD(Value, Min, Max);
        }
    }
    
    #endregion
    
    #region SECTION int
    [PublicAPI, DebuggerDisplay("Range Value:{Value} Min:{Min} Max:{Max}")]
    // ReSharper disable once InconsistentNaming
    public struct Range : IEquatable<Range>, IComparable<Range>, IComparable, IFormattable, ICloneable
    {
        
        /// <summary>
        /// The size of a <see cref="Range" /> type, in bytes.
        /// </summary>
        public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Range));
        
        /// <summary>
        /// A <see cref="Range"/> with all of its components set to zero.
        /// </summary>
        public static readonly Range Zero = new Range(0);
        
        /// <summary>
        /// A <see cref="Range"/> with all of its components set to one.
        /// </summary>
        public static readonly Range One = new Range(1);
        
        /// <summary>
        /// Field access 'Value' of this <see cref="Range"/>
        /// </summary>
        public int Value;
        
        /// <summary>
        /// Field access 'Min' of this <see cref="Range"/>
        /// </summary>
        public int Min;
        
        /// <summary>
        /// Field access 'Max' of this <see cref="Range"/>
        /// </summary>
        public int Max;
        
        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public Range(int value)        
        {
            Value = (int)value;
            Min = (int)value;
            Max = (int)value;
        }
        
        /// <summary>
        /// Constructs a new instance from an existing instance.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public Range(Range value)        
        {
            this.Value = (int)value.Value;
            this.Min = (int)value.Min;
            this.Max = (int)value.Max;
        }
        
        /// <summary>
        /// Constructs a new instance and fill all fields.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public Range(int value, int min, int max)        
        {
            this.Value = (int)value;
            this.Min = (int)min;
            this.Max = (int)max;
        }
        public int Span => (int)(Max - Min);
        
        public Percentage Percentage
        {
            get => new Percentage((Value - Min) / (double)Span);
            set => Value = (int)(Min + value * Span);
        }

        public void SetMinMax(int min, int max)
        {
            Min = min;
            Max = max;
            Check();
        }

        public void MoveMinWithCurrentSpan(int min)
        {
            var span = Span;
            Max = (int)(min + span);
            Min = min;
        }
        
        public void MoveMaxWithCurrentSpan(int max)
        {
            var span = Span;
            Min = (int)(max - span);
            Max = max;
        }
        
        private void Check()
        {
            if (Min > Max)
            {
                throw new InvalidOperationException("Min value can not be greater than max value");
            }

            if (Value < Min)
            {
                Value = Min;
            }
            
            if (Value > Max)
            {
                Value = Max;
            }
        }
        /// <inheritdoc />
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public override bool Equals(object obj)        
        {
            if (obj is Range i)
            {
	            return Equals(i);
            }
            return false;
        }
        /// <inheritdoc />
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public bool Equals(Range obj)        
        {
            return Value.Equals(obj.Value) && Min.Equals(obj.Min) && Max.Equals(obj.Max);
        }
        
        /// <summary>
        /// Performs an equalization with <see cref="Range"/>.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool operator ==(Range left, Range right) => left.Equals(right);
        
        /// <summary>
        /// Performs an not equalization with <see cref="Range"/>.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool operator !=(Range left, Range right) => !left.Equals(right);
        
        /// <summary>
        /// Compares this <see cref="Range"/> with another object.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public int CompareTo(object other)        
        {
            if (other is Range i)
            {
	            return CompareTo(i);
            }
            return -1;
        }
        
        /// <summary>
        /// Compares this <see cref="Range"/> with another <see cref="Range"/>.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public int CompareTo(Range other)        
        {
            if (!Value.Equals(other.Value))
            {
	            return Value.CompareTo(other.Value);
            }
            if (!Min.Equals(other.Min))
            {
	            return Min.CompareTo(other.Min);
            }
            return Max.CompareTo(other.Max);
        }
        
        /// <summary>
        /// Returns a string that represents the current <see cref="Range"/>.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public override string ToString()        
        {
            return $"{{Value: {Value.ToString(CultureInfo.InvariantCulture)} Min: {Min.ToString(CultureInfo.InvariantCulture)} Max: {Max.ToString(CultureInfo.InvariantCulture)}}}";
        }
        
        /// <summary>
        /// Returns a string that represents the current <see cref="Range"/>.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public string ToString(CultureInfo info)        
        {
            return $"{{Value: {Value.ToString(info)} Min: {Min.ToString(info)} Max: {Max.ToString(info)}}}";
        }
        
        /// <summary>
        /// Returns a string that represents the current <see cref="Range"/>.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public string ToString(string format)        
        {
            return $"{{Value: {Value.ToString(format)} Min: {Min.ToString(format)} Max: {Max.ToString(format)}}}";
        }
        
        /// <summary>
        /// Returns a string that represents the current <see cref="Range"/>.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public string ToString(string format, IFormatProvider formatProvider)        
        {
            return $"{{Value: {Value.ToString(format, formatProvider)} Min: {Min.ToString(format, formatProvider)} Max: {Max.ToString(format, formatProvider)}}}";
        }
        
        /// <summary>
        /// Returns the hash code for this <see cref="Range"/>.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public override int GetHashCode()        
        {
            return Value.GetHashCode()* 397 ^ Min.GetHashCode()* 397 ^ Max.GetHashCode();
        }
        
        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public object Clone()        
        {
            return new Range(Value, Min, Max);
        }
    }
    
    #endregion
}