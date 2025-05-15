// // ***********************************************************************
// // Author           : Florian Schmidt
// // Created          : 30.09.2019 : 15:20
// // Website          : www.finalfactory.de
// //
// // Last Modified By : Florian Schmidt
// // Last Modified On : 01.10.2019 : 15:30
// // ***********************************************************************
// // <copyright file="ArrayHelper.cs" company="Final Factory">
// //     Copyright (c) Final Factory. All rights reserved.
// // </copyright>
// // <summary></summary>
// // ***********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using FinalFactory.Mathematics;
using FinalFactory.UIElements;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace FinalFactory.Tooling
{
    [PublicAPI]
    public static class ArrayTools
    {
        /// <summary>
        /// Add one item to an existing array.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="item"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns>Index of the new item</returns>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static int Add<T>(ref T[] array, T item)
        {
            var oldLength = array.Length;
            Array.Resize(ref array, oldLength + 1);
            array[oldLength] = item;
            return oldLength;
        }
        
        [DebuggerStepThrough]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void For<T>(this T[] list, Action<T> action, bool copy = false)
        {
            if (copy)
            {
                list = list.ToArray();
            }

            for (var i = 0; i < list.Length; i++)
            {
                action(list[i]);
            }
        }

        public static T[,] ResizeArray<T>(this T[,] original, int x, int y)
        {
            var newArray = new T[x, y];
            var oX = original.GetLength(0);
            var oY = original.GetLength(1);
            var minX = Math.Min(oX, x);
            var minY = Math.Min(oY, y);

            for (var i = 0; i < minY; ++i)
            {
                Array.Copy(original, i * oX, newArray, i * x, minX);
            }

            return newArray;
        }
        public static T[,,] ResizeArray<T>(this T[,,] original, int x, int y, int z)
        {
            var newArray = new T[x, y, z];
            var minX = Math.Min(x, original.GetLength(0));
            var minY = Math.Min(y, original.GetLength(1));
            var minZ = Math.Min(z, original.GetLength(2));
            for (var i = 0; i < minX; i++)
            {
                for (var j = 0; j < minY; j++)
                {
                    for (var k = 0; k < minZ; k++)
                    {
                        newArray[i, j, k] = original[i, j, k];
                    }
                }
            }

            return newArray;
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void ResizeArrayProcessItems<T>(ref T[] original, int newLength) where T : IDisposable, new()
        {
            ResizeArrayProcessItems(ref original, newLength, i => new T());
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void ResizeArrayProcessItems<T>(ref T[] original, int newLength, Func<int, T> createItem) where T : IDisposable
        {
            ResizeArrayProcessItems(ref original, newLength, createItem, (i, disposable) => disposable.Dispose());
        }
        
        public static void ResizeArrayProcessItems<T>(ref T[] original, int newLength, Func<int, T> createItem, Action<int, T> removedItem)
        {
            var orig = original?.Length ?? 0;
            if (orig == newLength)
            {
                return;
            }
            
            var newArray = new T[newLength];
            var min = Math.Min(newLength, orig);
            for (var i = 0; i < orig; i++)
            {
                if (i < min)
                {
                    newArray[i] = original[i];
                }
                else if (i < orig)
                {
                    removedItem(i, original[i]);
                }
            }

            for (var i = orig; i < newLength; i++)
            {
                newArray[i] = createItem(i);
            }
            original = newArray;
        }
        

        public static T[,] ResizeArrayReturnLostItems<T>(this T[,] original, int x, int y, out List<T> lostItems)
        {
            var newArray = new T[x, y];
            lostItems = null;
            var origX = original.GetLength(0);
            var origY = original.GetLength(1);

            var minX = Math.Min(x, origX);
            var minY = Math.Min(y, origY);

            for (var i = 0; i < origX; i++)
            {
                for (var j = 0; j < origY; j++)
                {
                    if (i < minX && j < minY)
                    {
                        newArray[i, j] = original[i, j];
                    }
                    else if (i < origX && j < origY)
                    {
                        (lostItems ?? (lostItems = new List<T>())).Add(original[i, j]);
                    }
                }
            }

            return newArray;
        }
        public static T[,,] ResizeArrayReturnLostItems<T>(this T[,,] original, int x, int y, int z, out IList<T> lostItems)
        {
            var newArray = new T[x, y, z];
            lostItems = null;
            var origX = original.GetLength(0);
            var origY = original.GetLength(1);
            var origZ = original.GetLength(2);

            var minX = Math.Min(x, origX);
            var minY = Math.Min(y, origY);
            var minZ = Math.Min(z, origZ);

            for (var i = 0; i < origX; i++)
            {
                for (var j = 0; j < origY; j++)
                {
                    for (var k = 0; k < origZ; k++)
                    {
                        if (i < minX && j < minY && k < minZ)
                        {
                            newArray[i, j, k] = original[i, j, k];
                        }
                        else if (i < origX && j < origY && k < origZ)
                        {
                            lostItems.Add(original[i, j, k]);
                        }
                    }
                }
            }

            return newArray;
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static T[,] Make2DArray<T>(this T[] input, int width, int height)
        {
            var output = new T[width, height];
            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    output[i, j] = input[i * height + j];
                }
            }

            return output;
        }
        
        /// <summary>
        /// Fills an array with random values
        /// </summary>
        /// <param name="array"></param>
        /// <typeparam name="T">bool, byte, sbyte, short, ushort, int, uint, long, ulong, float, double, char, string</typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static T[] FillRandom<T>(this T[] array) where T : struct
        {
            var exampleValue = array[0];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = (T)FinalRandom.Static.NextOf(exampleValue);
            }
            return array;
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static T[] Fill<T>(this T[] array) where T : new()
        {
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = new T();
            }
            return array;
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static T[] Fill<T>(this T[] array, T value)
        {
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = value;
            }
            return array;
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static T[,] Fill<T>(this T[,] src, T val)
        {
            for (var i = 0; i < src.GetLength(0); i++)
            {
                for (var j = 0; j < src.GetLength(1); j++)
                {
                    src[i, j] = val;
                }
            }

            return src;
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static T[] Merge<T>(this T[] src, T[] add)
        {
            var array1OriginalLength = src.Length;
            Array.Resize(ref src, array1OriginalLength + add.Length);
            Array.Copy(add, 0, src, array1OriginalLength, add.Length);
            return src;
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static int IndexOf<T>(this T[] array, T obj)
        {
            return Array.IndexOf(array, obj);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static int IndexOf<T>(this T[] array, Func<T, bool> func)
        {
            for (var i = 0; i < array.Length; i++)
            {
                if (func(array[i]))
                {
                    return i;
                }
            }

            return -1;
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static T[] ToArray<T>(this T[,] array)
        {
            return array.Cast<T>().ToArray();
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static T[] Instantiate<T>(int arraySize) where T : new()
        {
            var array = new T[arraySize];
            for (var i = 0; i < arraySize; i++)
            {
                array[i] = new T();
            }
            return array;
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static T[] Instantiate<T>(this T[] array) where T : new()
        {
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = new T();
            }
            return array;
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static T[,] Instantiate<T>(this T[,] array) where T : new()
        {
            for (var i = 0; i < array.GetLength(0); i++)
            {
                for (var j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] = new T();
                    
                }
            }
            return array;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="index"></param>
        /// <param name="item"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns>True if next is available.</returns>
        public static bool Iterate<T>(this T[] array, ref int index, out T item)
        {
            if (array == null)
            {
                index = 0;
                item = default;
                return false;
            }
            
            //Safety check if the index was changed externally or the array length has changed.
            if (index >= array.Length)
            {
                index = 0;
            }

            //Safety check if the index was changed externally.
            if (index < 0)
            {
                index = array.Length - 1;
            }

            item = array[index];

            index++;
            
            //If we count to the end. We loop to the first.
            if (index >= array.Length)
            {
                index = 0;
                return false;
            }

            return true;
        }
        
        public static T[] Reverse<T>(this T[] array)
        {
            Array.Reverse(array);
            return array;
        }
        
        public static float Sum(this float[] array)
        {
            var sum = 0f;
            for (var i = 0; i < array.Length; i++)
            {
                sum += array[i];
            }
            return sum;
        }
        
        public static double Sum(this double[] array)
        {
            var sum = 0d;
            for (var i = 0; i < array.Length; i++)
            {
                sum += array[i];
            }
            return sum;
        }
        
        /// <summary>
        /// Normalize the values of an array to a sum of the given value.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="totalSum"></param>
        /// <returns></returns>
        public static float[] Normalize(this float[] array, float totalSum = 1f)
        {
            var total = array.Sum();
            var factor = totalSum / total;
            for (var i = 0; i < array.Length; i++)
            {
                array[i] *= factor;
            }
            return array;
        }
        
        /// <summary>
        /// Normalize the values of an array to a sum of the given value.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="totalSum"></param>
        /// <returns></returns>
        public static double[] Normalize(this double[] array, double totalSum = 1d)
        {
            var total = array.Sum();
            var factor = totalSum / total;
            for (var i = 0; i < array.Length; i++)
            {
                array[i] *= factor;
            }
            return array;
        }

        /// <summary>
        /// Provides a safe way to access an array. If the index is out of bounds it will return the next item in the array as if it was a loop.
        /// </summary>
        /// <param name="srcCollection"></param>
        /// <param name="index"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static T Overflow<T>(this T[] srcCollection, int index) => srcCollection[CollectionHelper.OverflowIndex(index,  srcCollection.Length)];
    }
}