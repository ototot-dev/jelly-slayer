using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using Unity.Linq;
using UnityEngine;

namespace Game
{

    /// <summary>
    /// 
    /// </summary>
    public static class Rand
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int Range(int min, int max)
        {
            return UnityEngine.Random.Range(min, max + 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float Range(float min, float max)
        {
            return UnityEngine.Random.Range(min, max);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>œ
        /// <returns></returns>
        public static float Range01()
        {
            return UnityEngine.Random.Range(0f, 1f);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>œ
        /// <returns></returns>
        public static float Range11()
        {
            return UnityEngine.Random.Range(-1f, 1f);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        public static int Dice(int face = 6)
        {
            return UnityEngine.Random.Range(1, face + 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        public static int Dice(out Tuple<int, int> result, int face = 6)
        {
            result = new Tuple<int, int>(UnityEngine.Random.Range(1, face + 1), UnityEngine.Random.Range(1, face + 1));
            return result.Item1 + result.Item2;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        public static int Dice(out Tuple<int, int, int> result, int face = 6)
        {
            result = new Tuple<int, int, int>(UnityEngine.Random.Range(1, face + 1), UnityEngine.Random.Range(1, face + 1), UnityEngine.Random.Range(1, face + 1));
            return result.Item1 + result.Item2 + result.Item3;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        public static int Dice(out Tuple<int, int, int, int> result, int face = 6)
        {
            result = new Tuple<int, int, int, int>(UnityEngine.Random.Range(1, face + 1), UnityEngine.Random.Range(1, face + 1), UnityEngine.Random.Range(1, face + 1), UnityEngine.Random.Range(1, face + 1));
            return result.Item1 + result.Item2 + result.Item3 + result.Item4;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        public static int Dice(out Tuple<int, int, int, int, int> result, int face = 6)
        {
            result = new Tuple<int, int, int, int, int>(UnityEngine.Random.Range(1, face + 1), UnityEngine.Random.Range(1, face + 1), UnityEngine.Random.Range(1, face + 1), UnityEngine.Random.Range(1, face + 1), UnityEngine.Random.Range(1, face + 1));
            return result.Item1 + result.Item2 + result.Item3 + result.Item4 + result.Item5;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        public static int Dice(out Tuple<int, int, int, int, int, int> result, int face = 6)
        {
            result = new Tuple<int, int, int, int, int, int>(UnityEngine.Random.Range(1, face + 1), UnityEngine.Random.Range(1, face + 1), UnityEngine.Random.Range(1, face + 1), UnityEngine.Random.Range(1, face + 1), UnityEngine.Random.Range(1, face + 1), UnityEngine.Random.Range(1, face + 1));
            return result.Item1 + result.Item2 + result.Item3 + result.Item4 + result.Item5 + result.Item6;
        }

    }

}
