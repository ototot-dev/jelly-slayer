using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Game
{
    public class HeroBlackboard : PawnBlackboard
    {
        public bool IsJumping => action.isJumping.Value;
        public bool IsHanging => action.hangingBrain.Value != null;
        public bool IsRolling => action.isRolling.Value;
        public bool IsGuarding => action.isGuarding.Value;
        public bool IsGuardBroken => action.isGuardBroken.Value;
        public bool IsAutoGuardEnabled => action.isAutoGuardEnabled.Value;
        public bool IsCharging => action.isCharging.Value;

        [Serializable]
        public class Body
        {   
            public float walkSpeed = 1f;
            public float runSpeed = 1f;
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
            public BoolReactiveProperty isJumping = new();
            public ReactiveProperty<DroneBotBrain> hangingBrain = new();
            public BoolReactiveProperty isRolling = new();
            public BoolReactiveProperty isGuarding = new();
            public BoolReactiveProperty isGuardBroken = new();
            public BoolReactiveProperty isAutoGuardEnabled = new(true);
            public BoolReactiveProperty isCharging = new();
            public IntReactiveProperty chargingLevel = new();
        }

        public Action action = new();

        [Serializable]
        public class Graphics
        {
            public MeshRenderer forceShieldRenderer;
            public GameObject onGuardParriedFx;
            public GameObject onBlockFx;
            public GameObject onBleedFx;
        }

        public Graphics graphics = new();

        [Serializable]
        public class Attachment
        {
            public Transform BlockingFxAttachPoint;
        }

        public Attachment attachment = new();

        protected override void AwakeInternal()
        {
            base.AwakeInternal();

            stat.staminaRecoverSpeed = pawnData.staminaRecoverSpeed;
            stat.staminaRecoverTimeThreshold = pawnData.staminaRecoverTime;

            pawnData_Movement = MainTable.PawnData_Movement.PawnData_MovementList.First(d => d.pawnId == common.pawnId);
            body.walkSpeed = pawnData_Movement.moveSpeed;
            body.guardSpeed = pawnData_Movement.guardSpeed;
            body.sprintSpeed = pawnData_Movement.sprintSpeed;
        }

        public MainTable.PawnData_Movement pawnData_Movement;
    }
}