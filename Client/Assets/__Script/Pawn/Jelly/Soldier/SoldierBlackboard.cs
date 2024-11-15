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
            public float comboAttackRateBoostAfterCounterAttack;
            public float comboAttackRateStep;
            public float counterAttackRateStep;
        }

        public Selection selection = new();

    }
}