// // ***********************************************************************
// // Author           : Florian Schmidt
// // Created          : 30.09.2019 : 15:20
// // Website          : www.finalfactory.de
// //
// // Last Modified By : Florian Schmidt
// // Last Modified On : 01.10.2019 : 15:30
// // ***********************************************************************
// // <copyright file="EnumHelpers.cs" company="Final Factory">
// //     Copyright (c) Final Factory. All rights reserved.
// // </copyright>
// // <summary></summary>
// // ***********************************************************************

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using FinalFactory.Tooling;
using FinalFactory.UIElements;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace FinalFactory.Helpers
{
    [PublicAPI]
    public static class EnumHelpers
    {
        private static readonly Type IntType = typeof(int);
        private static class EnumInfo<T> where T : Enum
        {
            public static readonly T[] Values;

            static EnumInfo()
            {
                Values = (T[])Enum.GetValues(typeof(T));
            }
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static int IndexOf<T>(T value) where T : struct, Enum, IConvertible, IFormattable
        {
            return GetValues<T>().IndexOf(value);
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static T Min<T>(params T[] values) where T : struct, Enum, IConvertible, IFormattable
        {
            return values.Select(x => x.ToInt()).Min().ToEnum<T>(); 
        }
        [MethodImpl(GlobalConst.ImplOptions)]
        public static T Max<T>(params T[] values) where T : struct, Enum, IConvertible, IFormattable
        {
            return values.Select(x => x.ToInt()).Max().ToEnum<T>(); 
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static int Count<T>() where T : struct, Enum, IConvertible, IFormattable
        {
            return EnumInfo<T>.Values.Length;    
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static T[] GetValues<T>() where T : struct, Enum, IConvertible, IFormattable
        {
            return EnumInfo<T>.Values;    
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static T[] GetValues<T>(this T thisEnum) where T : struct, Enum, IConvertible, IFormattable
        {
            return EnumInfo<T>.Values;    
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static (int enumInt, int flagInt) EnumAndFlagToInt<T>(T thisEnum, T flag) where T : struct, Enum, IConvertible, IFormattable
        {
            if (thisEnum.GetType() != flag.GetType())
            {
                throw new ArgumentException("Argument_EnumTypeDoesNotMatch " + flag.GetType() + " " + thisEnum.GetType());
            }

            var enumInt = thisEnum.ToInt();
            var flagInt = flag.ToInt();
            return (enumInt, flagInt);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static int ToInt<T>(this T thisEnum) where T : struct, Enum, IConvertible, IFormattable => (int) Convert.ChangeType(thisEnum, IntType);

        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool AnyFlags(this Enum thisEnum, Enum flags)
        {
            return Flags.Any(thisEnum, flags);
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static T ToEnum<T>(this object value) where T : struct, Enum, IConvertible, IFormattable
        {
            if (value is string txt)
            {
                return Enum.Parse<T>(txt);
            }
            return (T) value;
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static T ToEnum<T>(this string value) where T : struct, Enum, IConvertible, IFormattable => Enum.Parse<T>(value);

        [MethodImpl(GlobalConst.ImplOptions)]
        public static T GetByIndex<T>(int value) where T : struct, Enum, IConvertible, IFormattable => EnumInfo<T>.Values[value];
    }
}