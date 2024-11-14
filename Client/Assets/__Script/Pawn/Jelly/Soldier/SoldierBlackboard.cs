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
            public float comboAttackRate;
            public float comboAttackRateStep;
            public float comboAttackRateBoostAfterCounterAttack;
            public float counterAttackRate;
            public float counterAttackRateStep;
        }

        public Selection selection = new();

    }
}