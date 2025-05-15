using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using FinalFactory.UIElements;
using JetBrains.Annotations;

namespace FinalFactory.Helpers
{
    [PublicAPI]
    public static class Flags
    {
        [MethodImpl(GlobalConst.ImplOptions)]
        public static T Toggle<T>(T thisEnum, T flag) where T : struct, Enum, IConvertible, IFormattable
        {
            (int enumInt, int flagInt) = EnumHelpers.EnumAndFlagToInt(thisEnum, flag);
            return Set<T>(!Has(thisEnum, flag), enumInt, flagInt);
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static T Set<T>(this T thisEnum, T flag, bool set) where T : struct, Enum, IConvertible, IFormattable
        {
            (int enumInt, int flagInt) = EnumHelpers.EnumAndFlagToInt(thisEnum, flag);
            return Set<T>(set, enumInt, flagInt);
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void Set<T>(ref T thisEnum, T flag, bool set) where T : struct, Enum, IConvertible, IFormattable
        {
            (int enumInt, int flagInt) = EnumHelpers.EnumAndFlagToInt(thisEnum, flag);
            thisEnum = Set<T>(set, enumInt, flagInt);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static T Set<T>(bool set, int enumInt, int flagInt) where T : struct, Enum, IConvertible, IFormattable
        {
            if (set)
            {
                enumInt |= flagInt;
            }
            else
            {
                enumInt &= ~flagInt;
            }
            return (T) Convert.ChangeType(enumInt, Enum.GetUnderlyingType(typeof(T)));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool Has<T>(this T thisEnum, T flag) where T : struct, Enum, IConvertible, IFormattable
        {
            if (thisEnum.GetType() != flag.GetType())
            {
                throw new ArgumentException("Argument_EnumTypeDoesNotMatch " + flag.GetType() + " " + thisEnum.GetType());
            }

            ulong uFlag = ToUInt64(flag);
            ulong uThis = ToUInt64(thisEnum);
            return (uThis & uFlag) == uFlag;
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool Has(this Enum thisEnum, Enum flag)
        {
            if (thisEnum.GetType() != flag.GetType())
            {
                throw new ArgumentException("Argument_EnumTypeDoesNotMatch " + flag.GetType() + " " + thisEnum.GetType());
            }

            ulong uFlag = ToUInt64(flag);
            ulong uThis = ToUInt64(thisEnum);
            return (uThis & uFlag) == uFlag;
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool Any(this Enum thisEnum, Enum flags)
        {
            if (thisEnum.GetType() != flags.GetType())
            {
                throw new ArgumentException("Argument_EnumTypeDoesNotMatch " + flags.GetType() + " " + thisEnum.GetType());
            }

            ulong uFlag = ToUInt64(flags);
            ulong uThis = ToUInt64(thisEnum);
            return (uThis & uFlag) != 0;
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static T Add<T>(T thisEnum, T flag) where T : struct, Enum, IConvertible, IFormattable => Set(thisEnum, flag, true);
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void Add<T>(ref T thisEnum, T flag) where T : struct, Enum, IConvertible, IFormattable => thisEnum = Set(thisEnum, flag, true);
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static T Remove<T>(T thisEnum, T flag) where T : struct, Enum, IConvertible, IFormattable => Set(thisEnum, flag, false);
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void Remove<T>(ref T thisEnum, T flag) where T : struct, Enum, IConvertible, IFormattable => thisEnum = Set(thisEnum, flag, false);
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static ulong ToUInt64(Object value)
        {
            var typeCode = Convert.GetTypeCode(value);
            ulong result = 0;

            switch (typeCode)
            {
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    result = (UInt64)Convert.ToInt64(value, CultureInfo.InvariantCulture);
                    break;

                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    result = Convert.ToUInt64(value, CultureInfo.InvariantCulture);
                    break;
            }

            return result;
        }
    }
}