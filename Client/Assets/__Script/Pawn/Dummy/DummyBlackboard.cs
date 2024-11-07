using System;
using UniRx;
using UnityEngine;

namespace Game
{
    public class DummyBlackboard : PawnBlackboard
    {
        public DummyBrain.Decisions CurrDecision => decision.currDecision.Value;
        public PawnBrainController TargetBrain => decision.targetPawnHP.Value != null ? decision.targetPawnHP.Value.PawnBrain : null;
        public GameObject TargetPawn => decision.targetPawnHP.Value != null ? decision.targetPawnHP.Value.gameObject : null;
        public bool IsInCombat => decision.isInCombat.Value;
        public float HoldPositionWeight => decision.holdPositionWeight;
        public float MoveAroundWeight => decision.moveAroundWeight;

        [Serializable]
        public class Movement
        {
            public float moveSpeed = 1;
            public float moveAccel = 1;
            public float moveBrake = 1;
            public float rotateSpeed = 360;
            public float minApproachDistance = 1;
        }

        public Movement movement = new();

        [Serializable]
        public class Decision
        {
            public ReactiveProperty<PawnHeartPointDispatcher> targetPawnHP = new();
            public ReactiveProperty<DummyBrain.Decisions> currDecision = new(DummyBrain.Decisions.None);
            public BoolReactiveProperty isInCombat = new();
            public float holdPositionWeight = 1;
            public float moveAroundWeight = 1;
            public float minApproachDistance = 1;
        }

        public Decision decision = new();
    }
}