using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Game
{
    public class HeroBlackboard : PawnBlackboard
    {
        public bool IsJumping => body.isJumping.Value;
        public bool IsHanging => body.hangingBrain.Value != null;
        public bool IsRolling => body.isRolling.Value;
        public bool IsGuarding => body.isGuarding.Value;
        public bool IsGuardBroken => body.isGuardBroken.Value;
        public bool IsAutoGuardEnabled => body.isAutoGuardEnabled.Value;
        public bool IsCharging => body.isCharging.Value;

        [Serializable]
        public class Body
        {   
            public ReactiveProperty<DroneBotBrain> hangingBrain = new();
            public BoolReactiveProperty isJumping = new();
            public BoolReactiveProperty isRolling = new();
            public BoolReactiveProperty isGuarding = new();
            public BoolReactiveProperty isGuardBroken = new();
            public BoolReactiveProperty isAutoGuardEnabled = new(true);
            public BoolReactiveProperty isCharging = new();
            public IntReactiveProperty chargingLevel = new();
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
            public float guardParryRootMotionMultiplier = 1f;
        }

        public Action action = new();

        [Serializable]
        public class Graphics
        {
            public MeshRenderer forceShieldRenderer;
            public GameObject onHitFx;
            public GameObject onGuardParriedFx;
            public GameObject onBlockFx;
            public GameObject onBleedFx;
        }

        public Graphics graphics = new();

        [Serializable]
        public class Attachment
        {
            public Transform leftHandBone;
            public Transform leftElbowBone;
            public Transform leftMechHandBone;
            public Transform leftMechElbowBone;
            public Transform blockingFxAttachPoint;
            public GameObject healingPotion;
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