using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Game
{
    public class SpiderBlackboard : PawnBlackboard
    {
        public SpiderBrain.Decisions CurrDecision => decision.currDecision.Value;
        public PawnBrainController TargetBrain => decision.targetPawnHP.Value.PawnBrain;
        public GameObject TargetPawn => decision.targetPawnHP.Value.gameObject;
        public bool IsInCombat => decision.isInCombat.Value;
        // public bool IsSearchSomething => decision.currDecision.Value == SpiderBrain.Decisions.Search;
        public float HoldPositionWeight => decision.holdPositionWeight;
        public float MoveAroundWeight => decision.moveAroundWeight;
        public float PatrolWeight => decision.partrolWeight;

        /// <summary>
        /// Body 데이터 섹션
        /// </summary>
        [Serializable]
        public class Body
        {
        }

        public Body body = new();

        [Serializable]
        public class Decision
        {
            public ReactiveProperty<PawnHeartPointDispatcher> targetPawnHP = new();
            public ReactiveProperty<SpiderBrain.Decisions> currDecision = new();
            public BoolReactiveProperty isInCombat = new();
            public PawnSoundSourceGenerator.SoundSource searchReasonCached;
            public float minApproachDistance = 1;
            public float holdPositionWeight = 1;
            public float moveAroundWeight = 1;
            public float partrolWeight = 1;
            public float moveSpeed = 1;
            public float moveBrake = 1;
            public float emitPossibility = 0.1f;
        }

        public Decision decision = new();

        [Header("Test")]
        public Transform moveTarget;
    }
}