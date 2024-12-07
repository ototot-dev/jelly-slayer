using System;
using System.Linq;
using MainTable;
using UniRx;
using UnityEngine;

namespace Game
{
    public class HeroBlackboard : PawnBlackboard
    {
        public PawnBrainController TargetBrain => action.targetPawnHP.Value != null ? action.targetPawnHP.Value.PawnBrain : null;
        public PawnColliderHelper TargetColliderHelper => action.targetPawnHP.Value != null && action.targetPawnHP.Value.PawnBrain != null ? action.targetPawnHP.Value.PawnBrain.coreColliderHelper : null;
        public GameObject TargetPawn => action.targetPawnHP.Value != null ? action.targetPawnHP.Value.gameObject : null;
        public bool IsJumping => action.isJumping.Value;
        public bool IsRolling => action.isRolling.Value;
        public bool IsGuarding => action.isGuarding.Value;
        public bool IsAutoGuardEnabled => action.isAutoGuardEnabled.Value;
        public bool IsCharging => action.isCharging.Value;

        [Serializable]
        public class Body
        {   
            public float moveSpeed = 1f;
            public float walkSpeed = 1f;
            public float sprintSpeed = 0.1f;
            public float guardSpeed = 0.1f;
            public float rollingDistance = 2f;
            public float rollingDuration = 0.2f;
            public float jumpHeight = 1f;
            public float smashingHeight = 1f;
        }

        public Body body = new();

        [Serializable]
        public class Action
        {
            public ReactiveProperty<PawnHeartPointDispatcher> targetPawnHP = new();
            public BoolReactiveProperty isJumping = new();
            public BoolReactiveProperty isRolling = new();
            public BoolReactiveProperty isGuarding = new();
            public BoolReactiveProperty isAutoGuardEnabled = new(true);
            public BoolReactiveProperty isCharging = new();
            public IntReactiveProperty chargingLevel = new();
        }

        public Action action = new();

        protected override void AwakeInternal()
        {
            base.AwakeInternal();

            stat.staminaRecoverSpeed = pawnData.staminaRecoverSpeed;
            stat.staminaRecoverTimeThreshold = pawnData.staminaRecoverTime;;

            pawnData_Movement = MainTable.PawnData_Movement.PawnData_MovementList.First(d => d.pawnId == common.pawnId);
            body.moveSpeed = pawnData_Movement.moveSpeed;
            body.guardSpeed = pawnData_Movement.guardSpeed;
            body.sprintSpeed = pawnData_Movement.sprintSpeed;
            body.jumpHeight = pawnData_Movement.jumpHeight;
        }

        public MainTable.PawnData_Movement pawnData_Movement;
    }
}