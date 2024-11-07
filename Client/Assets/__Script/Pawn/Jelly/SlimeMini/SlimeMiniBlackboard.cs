using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Game
{
    public class SlimeMiniBlackboard : PawnBlackboard
    {
        public SlimeMiniBrain.Decisions CurrDecision => currDecision.Value;
        public PawnBrainController TargetBrain => targetPawnHP.Value != null ? targetPawnHP.Value.PawnBrain : null;
        public GameObject TargetPawn => targetPawnHP.Value != null ? targetPawnHP.Value.gameObject : null;
        public Transform TargetCore => TargetBrain !=  null ? TargetBrain.coreColliderHelper.transform : null;
        public bool IsInCombat => isInCombat.Value;
        public bool IsJumping => isJumping.Value;

        [Header("Body")]
        public float rotateSpeed = 360f;
        public float jumpSpeed = 1f;
        public float jumpHeight = 1f;


        [Header("Decision")]
        public ReactiveProperty<PawnHeartPointDispatcher> targetPawnHP = new();
        public ReactiveProperty<SlimeMiniBrain.Decisions> currDecision = new(SlimeMiniBrain.Decisions.None);
        public FloatReactiveProperty aggressiveLevel = new(0);
        public BoolReactiveProperty isInCombat = new();
        public BoolReactiveProperty isGuarding = new();
        public BoolReactiveProperty isJumping = new();
        public float holdPositionWeight = 1;
        public float moveAroundWeight = 1;
        public float approachDistance = 1;

        
        protected override void AwakeInternal()
        {
            base.AwakeInternal();
        }
    }
}