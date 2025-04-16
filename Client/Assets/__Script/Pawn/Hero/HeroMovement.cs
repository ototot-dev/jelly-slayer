using UnityEngine;
using FIMSpace.FProceduralAnimation;
using static FIMSpace.FProceduralAnimation.LegsAnimator;
using UniRx;
using System;
using FIMSpace;
using Cinemachine.Utility;

namespace Game
{
    [TooltipAttribute("이동, 점프, 대시 등의 처리")]
    public class HeroMovement : PawnMovement, LegsAnimator.ILegStepReceiver
    {
        public float LastJumpTimeStamp => __jumpTimeStamp;
        public float LastStartHangingTimeStamp => __startHangingTimeStamp;
        public float LastFinishHangingTimeStamp => __finishHangingTimeStamp;
        public void LegAnimatorStepEvent(LegsAnimator.Leg leg, float power, bool isRight, Vector3 position, Quaternion rotation, LegsAnimator.EStepType type) {}

        public void StartJump(float jumpHeight)
        {
            __isJumpFalling = false;
            __jumpTimeStamp = Time.time;
            __ecmMovement.velocity.y = GetVerticalImpulseOnJump(jumpHeight);
            __ecmMovement.PauseGroundConstraint();
            __brain.AnimCtrler.mainAnimator.SetTrigger("OnJump");
            __brain.AnimCtrler.mainAnimator.SetBool("IsJumping", true);
            __brain.BB.body.isJumping.Value = true;
        }

        public void FinishJump()
        {
            __brain.AnimCtrler.legAnimator.User_AddImpulse(new ImpulseExecutor(0.2f * Vector3.down, Vector3.zero, 0.2f));
            __brain.AnimCtrler.mainAnimator.SetBool("IsJumping", false);
            __brain.BB.body.isJumping.Value = false;
        }

        public void CancelJump()
        {
            __brain.AnimCtrler.mainAnimator.SetBool("IsJumping", false);
            __brain.BB.body.isJumping.Value = false;
        }

        public void PrepareHanging(DroneBotBrain hangingBrain, float duration)
        {
            ReservedHangingBrain = hangingBrain;
            //* Decision 값을 'Catch'로 직접 변경함
            ReservedHangingBrain.BB.decision.currDecision.Value = DroneBotBrain.Decisions.Catch;
            ReservedHangingBrain.Movement.prepareHangingDuration = duration;
        }

        public void StartHanging(bool smoothApproachToAttachPoint = true)
        {
            Debug.Assert(ReservedHangingBrain != null);

            __brain.BB.body.hangingBrain.Value = ReservedHangingBrain;
            ReservedHangingBrain = null;

            //* 점프가 종료되는 상황은 발생하면 안됨
            Debug.Assert(__brain.BB.IsJumping);
            __brain.BB.body.isJumping.Value = false;
            __brain.AnimCtrler.mainAnimator.SetTrigger("OnHanging");
            __brain.AnimCtrler.mainAnimator.SetBool("IsHanging", true);

            if (smoothApproachToAttachPoint)
            {
                //* HangingAttachPoint까지 남은 거리를 부드럽게 Lerp 처리함
                __hangingLerpDisposable?.Dispose();
                __hangingLerpDisposable = Observable.EveryLateUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(0.1f)))
                    .DoOnCancel(() => __hangingLerpDisposable = null)
                    .DoOnCompleted(() => __hangingLerpDisposable = null)
                    .Subscribe(_ => __ecmMovement.SetPosition(capsule.transform.position.LerpSpeed(__brain.BB.body.hangingBrain.Value.AnimCtrler.hangingAttachPoint.position, moveSpeed, Time.deltaTime))).AddTo(this);
            }

            PawnEventManager.Instance.SendPawnActionEvent(__brain, "OnHanging");
        }

        public DroneBotBrain ReservedHangingBrain { get; private set; }
        IDisposable __hangingLerpDisposable;
        float __startHangingTimeStamp;
        float __finishHangingTimeStamp;

        public void FinishHanging()
        {
            Debug.Assert(__brain.BB.body.hangingBrain.Value != null);
            
            __finishHangingTimeStamp = Time.time;
            __brain.BB.body.hangingBrain.Value.InvalidateDecision(0.1f);
            __brain.BB.body.hangingBrain.Value = null;
            __brain.AnimCtrler.mainAnimator.SetBool("IsHanging", false);
        }

        //* 외부 영향으로 강제로 Hanging 상태가 취소되는 경우
        public void CancelHanging()
        {
            FinishHanging();
            __brain.AnimCtrler.mainAnimator.SetBool("IsJumping", false);
        }

        bool __isJumpFalling;
        float __jumpTimeStamp;
        float __prevCapsulePositionY;
        HeroBrain __brain;

        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            __brain = GetComponent<HeroBrain>();
        }

        protected override void StartInternal()
        {
            base.StartInternal();

            __brain.onLateUpdate += () =>
            {
                if (!__ecmMovement.enabled)
                    return;

                if (__brain.BB.IsHanging)
                {
                    //* hangingPoint로 위치값 및 회전값을 맞춰줌
                    if (__hangingLerpDisposable == null)
                        __ecmMovement.SetPositionAndRotation(__brain.BB.body.hangingBrain.Value.AnimCtrler.hangingAttachPoint.position, __brain.BB.body.hangingBrain.Value.AnimCtrler.hangingAttachPoint.rotation);
                }
            };
        }

        protected override void OnUpdateHandler()
        {
            if (!__ecmMovement.enabled)
                return;

            if (!__brain.BB.IsHanging)
            {
                var canRotate1 = __brain.PawnBB.IsSpawnFinished && !__brain.PawnBB.IsDead && !__brain.PawnBB.IsGroggy && !__brain.AnimCtrler.CheckAnimStateRunning("OnDown");
                var canRotate2 = canRotate1 && !__brain.BB.IsRolling && !__brain.BB.IsJumping;
                var canRotate3 = canRotate2 && (!__pawnActionCtrler.CheckActionRunning() || __pawnActionCtrler.currActionContext.movementEnabled) && !__pawnStatusCtrler.CheckStatus(PawnStatus.Staggered);

                if (canRotate3)
                    __ecmMovement.RotateTowards(faceVec, rotateSpeed);
            }

            DampenRootMotion();
        }

        protected override void OnFixedUpdateHandler()
        {
            if (!__ecmMovement.enabled)
                return;

            if (__brain.BB.IsJumping)
            {
                if (__prevCapsulePositionY > capsule.position.y && Time.time - __jumpTimeStamp > 2f * Time.fixedDeltaTime)
                    __isJumpFalling = true;
                __prevCapsulePositionY = capsule.position.y;

                if (__isJumpFalling && __ecmMovement.isGrounded)
                {
                    FinishJump();
                }
                else if (CheckRootMotionZero())
                {
                    __ecmMovement.velocity += Time.fixedDeltaTime * gravity;
                    __ecmMovement.Move(Time.fixedDeltaTime);
                }
            }
            else if (__brain.BB.IsHanging)
            {
                ;
            }
            else
            {
                if (!freezeMovement)
                {
                    var canMove1 = __brain.PawnBB.IsSpawnFinished && !__brain.PawnBB.IsDead && !__brain.PawnBB.IsGroggy && !__brain.AnimCtrler.CheckAnimStateRunning("OnDown");
                    var canMove2 = canMove1 && (!__pawnActionCtrler.CheckActionRunning() || __pawnActionCtrler.currActionContext.movementEnabled) && !__pawnStatusCtrler.CheckStatus(PawnStatus.Staggered);
                    moveVec = canMove2 ? moveVec : Vector3.zero;
                }
                
                if (CheckRootMotionNonZero())
                {
                    if (!__ecmMovement.isGrounded)
                    {
                        __ecmMovement.velocity += Time.fixedDeltaTime * gravity;
                        __ecmMovement.Move(Time.fixedDeltaTime);
                    }
                }
                else
                {
                    if (__ecmMovement.isGrounded || gravity.AlmostZero())
                    {
                        __ecmMovement.SimpleMove(moveSpeed * moveVec, moveSpeed, moveAccel, moveBrake, 1f, 1f, gravity, false, Time.fixedDeltaTime);
                    }
                    else
                    {
                        __ecmMovement.velocity += Time.fixedDeltaTime * gravity;
                        __ecmMovement.Move(Time.fixedDeltaTime);
                    }
                }
            }
        }
    }
}