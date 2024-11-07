using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{

    /// <summary>
    /// 
    /// </summary>
    public enum HeightMapFuncTypes
    {
        Global,
        Slope,
        Bubble,
        Max,
    }

    /// <summary>
    /// 
    /// </summary>
    public static class HeightMapFunc
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public delegate float HeightFunc(Vector3 position);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="worldSize"></param>
        /// <returns></returns>
        public static float GetGlobalHeight(Vector3 position, Vector2 worldSize)
        {
            var coastalX = Mathf.Abs(position.x) - worldSize.x * 0.5f + 12;
            var coastalY = Mathf.Abs(position.z) - worldSize.y * 0.5f + 12;

            var coastalHeightX = coastalX > 0 ? -Mathf.Min(5, coastalX * 0.5f) : 0;
            var coastalHeightY = coastalY > 0 ? -Mathf.Min(5, coastalY * 0.5f) : 0;

            // float a = 0.03f, b = 3.2f, c = 0.2f, d = 0.4f;
            float a = 0.03f, b = 2f, c = 0.2f, d = 0.4f;

            var point = new Vector2(position.x, position.z);

            // return Perlin.Noise(point * a) * b + Perlin.Noise(point * c) * d + Mathf.Min(coastalHeightX, coastalHeightY);
            return Perlin.Noise(point * a) * b + Perlin.Noise(point * c) * d;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="roofWidth"></param>
        /// <param name="slope"></param>
        /// <returns></returns>
        public static float GetSlopeHeight(Vector3 position, Vector3 start, Vector3 end, float roofWidth, float slope)
        {
            var positionVec = (position - start).Vector2D();
            var lineVec = (end - start).Vector2D();

            var projVec = Vector3.Project(positionVec, lineVec);

            if (projVec.sqrMagnitude > lineVec.sqrMagnitude || projVec.sqrMagnitude > lineVec.sqrMagnitude)
                return 0;

            var slopeX = (positionVec - projVec).magnitude - roofWidth * 0.5f;

            if (slopeX <= 0)
            {
                return start.y;
            }
            else
            {
                return (Mathf.Cos(Mathf.Min(Mathf.PI, Mathf.PI * slopeX * slope / start.y)) + 1) * start.y * 0.5f;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="r"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public static float GetBubbleHeight(Vector3 position, float r, float a, float b, float c, float d = 0, float e = 0)
        {
            var x = position.x;
            var z = position.z;

            var ret = Mathf.Sqrt(Mathf.Max(0, (r * r - a * x * x - b * z * z)) * c);

            if (e != 0)
                ret += Perlin.Noise(new Vector2(x, z) * d) * e;

            return ret;
        }
    }
}