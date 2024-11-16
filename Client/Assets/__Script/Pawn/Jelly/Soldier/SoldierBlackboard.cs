using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Game
{
    public class SoldierBlackboard : JellyManBlackboard
    {

        [Serializable]
        public class Selection
        {
            public float comboAttackRateBoostAfterCounterAttack;   //* 반격 후 콤보 1타 콤보 발생 확률 증가
            public float comboAttackRateStep;  //* Idle 상태에서 콤보 1타 발생 확률 증가
            public float counterAttackRateStep; //* 블럭 후 반격 발생 확률 증가
        }

        public Selection selection = new();

    }
}