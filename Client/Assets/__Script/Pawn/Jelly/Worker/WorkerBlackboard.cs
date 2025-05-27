using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Game
{
    public class WorkerBlackboard : NpcBlackboard
    {
        [Serializable]
        public class Body
        {
            public float walkSpeed = 1f;
            public float runSpeed = 1f;
        }

        public Body body = new();

        [Serializable]
        public class Action
        {
            public BoolReactiveProperty isGuarding = new();
            public float spacingInDistance = 1f;
            public float spacingOutDistance = 1f;
            public float minSpacingDistance = 1f;
            public float maxSpacingDistance = 1f;
            public float comboAttackRateBoostAfterCounterAttack;   //* 반격 후 콤보 1타 콤보 발생 확률 증가
            public float comboAttackRateStep;  //* Idle 상태에서 콤보 1타 발생 확률 증가
            public float counterAttackRateStep; //* 블럭 후 반격 발생 확률 증가
        }

        public Action action = new();
    }
}