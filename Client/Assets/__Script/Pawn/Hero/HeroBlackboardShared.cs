using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 
    /// </summary>
    [CreateAssetMenu(fileName = "HeroBB_Shared", menuName = "Game/HeroBlackboardShared", order = 1)]
    public class HeroBlackboardShared : ScriptableObject
    {

        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public struct MinMax<T> { public T min, max; }

        /// <summary>
        /// Ranged 데이터 섹션
        /// </summary>
        [Serializable]
        public class Ranged
        {
            public MinMax<int> actionPoint = new();
            public MinMax<int> captureNum = new();
            public MinMax<float> captureTimeOut = new();
            public MinMax<int> bombNum = new();
            public MinMax<float> bombFillRatePerSecond = new();
            public MinMax<float> bombFillRatePerAction = new();
            public MinMax<float> bombFillRatePerKill = new();
            public MinMax<float> enchantBonusMultiplier = new();
            public MinMax<int> cubeNum = new();
            public MinMax<float> cubeCollectAngle = new();
            public MinMax<float> cubeCollectDistance = new();
        }

        public Ranged ranged = new Ranged();

        /// <summary>
        /// Curves 데이터 섹션
        /// </summary>
        [Serializable]
        public class Curves
        {
            /// <summary>
            /// 
            /// </summary>
            public AnimationCurve mainScaleCurve;

            [HideInInspector]
            public AnimationCurve directScaleCurve;

            [HideInInspector]
            public AnimationCurve inverseScaleCurve;

            /// <summary>
            /// 
            /// </summary>
            public AnimationCurve sensorScaleCurve;
        }

        public Curves curves = new Curves();

    }

}