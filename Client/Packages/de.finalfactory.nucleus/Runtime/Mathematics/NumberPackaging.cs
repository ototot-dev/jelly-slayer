#region License
// // --------------------------------------------------------------------------------------------------------------------
// // <summary>
// //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// //   NumberPackaging.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------
#endregion

using System.Runtime.CompilerServices;

namespace FinalFactory.Mathematics
{
    public static class NumberPackaging
    {
        [MethodImpl(GlobalConst.ImplOptions)]
        public static int UnpackToIntA(this long packetValue)
        {
            return (int) (packetValue >> 32);
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static int UnpackToIntB(this long packetValue)
        {
            return (int) packetValue;
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static long PackToLong(int valueA, int valueB)
        {
            return (long) valueA << 32 | valueB & 0xFFFFFFFFL;
        }
    }
}