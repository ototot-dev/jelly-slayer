using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Game
{
    public class  DroneBotBlackboard : JellyBlackboard
    {
        [Serializable]
        public class Selection
        {
            public float fireAttackRateStep;  //* Idle 상태에서 Fire 발생 확률 증가
        }

        public Selection selection = new();
    }
}