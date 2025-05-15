using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using FinalFactory.UIElements;
using JetBrains.Annotations;
using UnityEngine;

namespace FinalFactory.Mathematics
{
    [PublicAPI]
    public class FinalRandom
    {
        public static readonly FinalRandom Static;
        private static readonly RNGCryptoServiceProvider CryptoProvider;
        private readonly System.Random _random;
        public FinalRandom()
        {
            var buffer = new byte[4];
            CryptoProvider.GetBytes(buffer);
            var seed = BitConverter.ToInt32(buffer, 0);
            _random = new System.Random(seed);
        }

        public FinalRandom(int seed) => _random = new System.Random(seed);
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public bool NextBool() => _random.Next().ToBool();
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public byte NextByte() => (byte)_random.Next(byte.MaxValue);
        [MethodImpl(GlobalConst.ImplOptions)]
        public byte NextByte(byte maxValue) => (byte)_random.Next(maxValue);
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public byte NextByte(byte minValue, byte maxValue) => (byte)_random.Next(minValue, maxValue);
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public sbyte NextSByte() => (sbyte)_random.Next(sbyte.MaxValue);
        [MethodImpl(GlobalConst.ImplOptions)]
        public sbyte NextSByte(sbyte maxValue) => (sbyte)_random.Next(maxValue);
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public sbyte NextSByte(sbyte minValue, sbyte maxValue) => (sbyte)_random.Next(minValue, maxValue);
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public short NextShort() => (short)_random.Next(short.MaxValue);
        [MethodImpl(GlobalConst.ImplOptions)]
        public short NextShort(short maxValue) => (short)_random.Next(maxValue);
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public short NextShort(short minValue, short maxValue) => (short)_random.Next(minValue, maxValue);
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public ushort NextUShort() => (ushort)_random.Next(ushort.MaxValue);
        [MethodImpl(GlobalConst.ImplOptions)]
        public ushort NextUShort(ushort maxValue) => (ushort)_random.Next(maxValue);
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public ushort NextUShort(ushort minValue, ushort maxValue) => (ushort)_random.Next(minValue, maxValue);
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public int NextInt() => _random.Next();
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public int NextInt(int maxValue) => _random.Next(maxValue);
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public int NextInt(int minValue, int maxValue) => _random.Next(minValue, maxValue);
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public char NextChar() => (char)_random.Next(char.MaxValue);
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public char NextChar(char maxValue) => (char)_random.Next(maxValue);
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public char NextChar(char minValue, char maxValue) => (char)_random.Next(minValue, maxValue);
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public uint NextUInt() => (uint)(_random.NextDouble() * uint.MaxValue);
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public uint NextUInt(uint maxValue) => (uint)(_random.NextDouble() * maxValue);
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public uint NextUInt(uint minValue, uint maxValue) => (uint)(minValue + _random.NextDouble() * (maxValue - minValue));
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public long NextLong() => _random.Next();

        [MethodImpl(GlobalConst.ImplOptions)]
        public long NextLong(long maxValue) => (long)(_random.NextDouble() * maxValue);
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public long NextLong(long minValue, long maxValue) => (long)(minValue + _random.NextDouble() * (maxValue - minValue));
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public ulong NextULong() => (ulong)(_random.NextDouble() * ulong.MaxValue);
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public ulong NextULong(ulong maxValue) => (ulong)(_random.NextDouble() * maxValue);
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public ulong NextULong(ulong minValue, ulong maxValue) => (ulong)(minValue + _random.NextDouble() * (maxValue - minValue));
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public float NextFloat() => (float)_random.NextDouble();
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public float NextFloat(float maxValue) =>  NextFloat(0.0f, maxValue);
        
        [MethodImpl(GlobalConst.ImplOptions)] 
        public float NextFloat(float minValue, float maxValue) => (float)NextDouble(minValue, maxValue);

        [MethodImpl(GlobalConst.ImplOptions)]
        public double NextDouble() => _random.NextDouble();

        [MethodImpl(GlobalConst.ImplOptions)]
        public double NextDouble(double maxValue) => NextDouble(0.0, maxValue);
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public double NextDouble(double minValue, double maxValue) => minValue + _random.NextDouble() * (maxValue - minValue);
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public string NextString(int maxLength) => NextString(char.MinValue, char.MaxValue, 0, maxLength);
        [MethodImpl(GlobalConst.ImplOptions)]
        public string NextStringExact(int length) => NextString(char.MinValue, char.MaxValue, 0, length);

        [MethodImpl(GlobalConst.ImplOptions)]
        public string NextString(char maxValue, int maxLength) => NextString((char)0, maxValue, 0, maxLength);
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public string NextString(char minValue, char maxValue, int minLength, int maxLength)
        {
            var length = minLength == maxLength ? minLength : NextInt(minLength, maxLength);
            var sb = new StringBuilder(length);
            for (var i = 0; i < length; i++)
            {
                sb.Append(NextChar(minValue, maxValue));
            }
            return sb.ToString();
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public float Range(float min, float max) => min + NextFloat() * (max - min);
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public int Range(int min, int max) => (int)(min + NextFloat() * (max - min));

        static FinalRandom()
        {
            CryptoProvider = new RNGCryptoServiceProvider();
            Static  = new FinalRandom();
        }

        public object NextOf(object exampleValue)
        {
            return exampleValue switch
            {
                bool bo => NextBool(),
                byte by => NextByte(),
                sbyte sb => NextSByte(),
                short sh => NextShort(),
                ushort us => NextUShort(),
                int i => NextInt(),
                uint ui => NextUInt(),
                long l => NextLong(),
                ulong ul => NextULong(),
                float f => NextFloat(),
                double d => NextDouble(),
                char c => NextChar(),
                string s => NextStringExact(s.Length),
                _ => throw new ArgumentException($"{exampleValue} is not a valid type for FinalRandom.{nameof(NextOf)}")
            };
        }

        public Vector2 RandomDirection(float distance = 1f)
        {
            var angle = NextFloat(0, FinalConst.floatPi2);
            return new Vector2(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance);
        }
    }
}