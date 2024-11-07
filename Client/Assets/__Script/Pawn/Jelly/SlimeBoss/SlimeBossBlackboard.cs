using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Game
{
    public class SlimeBossBlackboard : PawnBlackboard
    {
        public SlimeBossBrain.Decisions CurrDecision => currDecision.Value;
        public PawnBrainController TargetBrain => targetPawnHP.Value != null ? targetPawnHP.Value.PawnBrain : null;
        public GameObject TargetPawn => targetPawnHP.Value != null ? targetPawnHP.Value.gameObject : null;
        public Transform TargetCore => TargetBrain !=  null ? TargetBrain.coreColliderHelper.transform : null;
        public bool IsInCombat => isInCombat.Value;
        public bool IsGuarding => isGuarding.Value;
        public bool IsJumping => isJumping.Value;
        public bool IsBumping => isBumping.Value;
        public bool IsRolling => isRolling.Value;
        public bool IsSmashing => isSmashing.Value;
        public bool IsSwelling => swellingLevel.Value > 0;
        public float HoldPositionWeight => holdPositionWeight;
        public float MoveAroundWeight => moveAroundWeight;
        public float ApproachDistance => approachDistance;

        [Header("Body")]
        public float rotateSpeed = 360f;
        public float jumpSpeed = 1f;
        public float jumpHeight = 1f;
        public float bumpingSpeed = 1f;
        public float bumpingHeight = 1f;
        public float bumpingDuration = 1f;
        public float rollingImpluse = 1f;
        public float rollingDuration = 1f;
        public float smashingSpeed = 1f;
        public float smashingHeight = 1f;
        public float smashingForce = 1f;

        [Header("Decision")]
        public ReactiveProperty<PawnHeartPointDispatcher> targetPawnHP = new();
        public ReactiveProperty<SlimeBossBrain.Decisions> currDecision = new(SlimeBossBrain.Decisions.None);
        public FloatReactiveProperty aggressiveLevel = new(0);
        public BoolReactiveProperty isInCombat = new();
        public BoolReactiveProperty isGuarding = new();
        public BoolReactiveProperty isJumping = new();
        public BoolReactiveProperty isBumping = new();
        public BoolReactiveProperty isRolling = new();
        public BoolReactiveProperty isSmashing = new();
        public IntReactiveProperty swellingLevel = new();
        public float holdPositionWeight = 1;
        public float moveAroundWeight = 1;
        public float approachDistance = 1;

        
        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            
            // var pawnData = MainTable.PawnData_Movement.PawnData_MovementList.First(d => d.pawnId == common.pawnId);
            // body.jumpSpeed = pawnData.moveSpeed;
            // body.walkSpeed = pawnData.walkSpeed;
            
            // var movement = GetComponent<PawnMovementEx>();
            // movement.moveSpeed = body.jumpSpeed;
            // movement.moveAccel = body.moveAccel;
            // movement.moveBrake = body.moveBrake;
            // movement.rotateSpeed = body.rotateSpeed;
        }
    }
}