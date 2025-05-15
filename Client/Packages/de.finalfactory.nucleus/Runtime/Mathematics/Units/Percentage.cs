// // ***********************************************************************
// // Author           : Florian Schmidt
// // Created          : .. : 
// // Website          : www.finalfactory.de
// //
// // Last Modified By : Florian Schmidt
// // Last Modified On : 08.05.2020 : 09:27
// // ***********************************************************************
// // <copyright file="Percentage.cs" company="Final Factory">
// //     Copyright (c) Final Factory. All rights reserved.
// // </copyright>
// // <summary></summary>
// // ***********************************************************************

using System.Diagnostics;

namespace FinalFactory.Mathematics.Units
{
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public readonly struct Percentage
    {
        public const float Full = 1.0f;
        public const float Half = 0.5f;
        public const float Zero = 0f;
        
        public readonly float Value;
        public bool IsFull => Value.Equal(Full);
        public bool IsHalf => Value.Equal(Half);
        public bool IsZero => Value.Equal(Zero);

        public Percentage(float value)
        {
            Check.InRange(0f, Full, value);
            Value = value;
        }
        
        public Percentage(double value)
        {
            Check.InRange(0f, Full, value);
            Value = (float)value;
        }
        
        public bool Equals(Percentage other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is Percentage other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return $"Percentage: {Value:P}";
        }

        public static Percentage operator +(Percentage a, Percentage b)
        {
            return  new Percentage(a.Value + b.Value);
        }
        
        public static Percentage operator -(Percentage a, Percentage b)
        {
            return  new Percentage(a.Value - b.Value);
        }

        public static Percentage operator *(Percentage a, Percentage b)
        {
            return  new Percentage(a.Value + b.Value);
        }
        
        public static Percentage operator /(Percentage a, Percentage b)
        {
            return new Percentage(a.Value - b.Value);
        }
        
        public static implicit operator float(Percentage p) => p.Value;
    }
}