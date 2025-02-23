using System;
using UniRx;
using UnityEngine;

namespace Game
{
    public class FootmanBlackboard : PawnBlackboard
    {
        public FootmanBrain.Decisions CurrDecision => decision.currDecision.Value;
        public PawnBrainController TargetBrain => decision.targetPawnHP.Value != null ? decision.targetPawnHP.Value.PawnBrain : null;
        public GameObject TargetPawn => decision.targetPawnHP.Value != null ? decision.targetPawnHP.Value.gameObject : null;
        public bool IsInCombat => decision.isInCombat.Value;
        public float MinSpacingDistance => decision.minSpacingDistance;
        public float MaxSpacingDistance => decision.maxSpacingDistance;
        public float HoldPositionWeight => decision.holdPositionWeight;
        public float MoveAroundWeight => decision.moveAroundWeight;

        [Serializable]
        public class Body
        {
            public float moveSpeed = 1;
            public float moveAccel = 1;
            public float moveBrake = 1;
            public float rotateSpeed = 360;
            public float minApproachDistance = 1;
        }

        public Body body = new();

        [Serializable]
        public class Decision
        {
            public ReactiveProperty<PawnHeartPointDispatcher> targetPawnHP = new();
            public ReactiveProperty<FootmanBrain.Decisions> currDecision = new(FootmanBrain.Decisions.None);
            public BoolReactiveProperty isInCombat = new();
            public float spacingInDistance = 1;
            public float spacingOutDistance = 1;
            public float minSpacingDistance = 1;
            public float maxSpacingDistance = 1;
            public float holdPositionWeight = 1;
            public float moveAroundWeight = 1;
            public float minApproachDistance = 1;
        }

        public Decision decision = new();
    }
}