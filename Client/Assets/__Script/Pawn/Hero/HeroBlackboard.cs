using System;
using System.Linq;
using FIMSpace.FProceduralAnimation;
using MainTable;
using UniRx;
using UnityEngine;

namespace Game
{
    public class HeroBlackboard : PawnBlackboard
    {
        public bool IsJumping => action.isJumping.Value;
        public bool IsRolling => action.isRolling.Value;
        public bool IsGuarding => action.isGuarding.Value;
        public bool IsGuardBroken => action.isGuardbroken.Value;
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
            public BoolReactiveProperty isRolling = new();
            public BoolReactiveProperty isGuarding = new();
            public BoolReactiveProperty isGuardbroken = new();
            public BoolReactiveProperty isAutoGuardEnabled = new(true);
            public BoolReactiveProperty isCharging = new();
            public IntReactiveProperty chargingLevel = new();


            public BoolReactiveProperty testIsAir = new();
            public BoolReactiveProperty testHanging = new();
            public ReactiveProperty<DroneBotBrain> testDroneBotBrain = new();
        }

        public Action action = new();

        protected override void AwakeInternal()
        {
            base.AwakeInternal();

            stat.staminaRecoverSpeed = pawnData.staminaRecoverSpeed;
            stat.staminaRecoverTimeThreshold = pawnData.staminaRecoverTime;;

            pawnData_Movement = MainTable.PawnData_Movement.PawnData_MovementList.First(d => d.pawnId == common.pawnId);
            body.walkSpeed = pawnData_Movement.moveSpeed;
            body.guardSpeed = pawnData_Movement.guardSpeed;
            body.sprintSpeed = pawnData_Movement.sprintSpeed;
            // body.jumpHeight = pawnData_Movement.jumpHeight;

            action.testIsAir.Skip(1).Subscribe(v =>
            {
                GetComponent<HeroBrain>().AnimCtrler.mainAnimator.SetTrigger("OnAir");
                // GetComponent<HeroBrain>().AnimCtrler.mainAnimator.SetBool("IsAir", v);

            }).AddTo(this);

            Observable.EveryFixedUpdate().Where(_ => action.testHanging.Value && action.testDroneBotBrain.Value != null).Subscribe(_ =>
            {
                var leftDummyBone = GetComponent<HeroBrain>().AnimCtrler.ragdollAnimator.GetRagdollHandler.User_GetBoneSetupByBoneID(ERagdollBoneID.LeftHand).PhysicalDummyBone;
                var rightDummyBone = GetComponent<HeroBrain>().AnimCtrler.ragdollAnimator.GetRagdollHandler.User_GetBoneSetupByBoneID(ERagdollBoneID.RightHand).PhysicalDummyBone;
                leftDummyBone.GetComponent<Rigidbody>().MovePosition(leftDummyBone.transform.position.LerpSpeed(action.testDroneBotBrain.Value.AnimCtrler.leftHand.position, 1f, Time.fixedDeltaTime));
                rightDummyBone.GetComponent<Rigidbody>().MovePosition(rightDummyBone.transform.position.LerpSpeed(action.testDroneBotBrain.Value.AnimCtrler.rightHand.position, 1f, Time.fixedDeltaTime));
            }).AddTo(this);
        }

        public MainTable.PawnData_Movement pawnData_Movement;
    }
}