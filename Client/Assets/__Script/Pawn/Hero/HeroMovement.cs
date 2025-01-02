using UnityEngine;
using FIMSpace.FProceduralAnimation;
using static FIMSpace.FProceduralAnimation.LegsAnimator;
using UniRx;
using System;

namespace Game
{
    [TooltipAttribute("이동, 점프, 대시 등의 처리")]
    public class HeroMovement : PawnMovement, LegsAnimator.ILegStepReceiver
    {
        [Header("Paramter")]
        public float ziplineOffsetY = 0.5f;
        public float LastJumpTimeStamp => __jumpTimeStamp;
        public float LastRollingTimeStamp => __rollingTimeStamp;
        public void LegAnimatorStepEvent(LegsAnimator.Leg leg, float power, bool isRight, Vector3 position, Quaternion rotation, LegsAnimator.EStepType type) {}

        public void StartJump(float jumpHeight)
        {
            __isFalling = false;
            __jumpTimeStamp = Time.time;
            __ecmMovement.velocity.y = GetVerticalImpulseOnJump(jumpHeight);
            __ecmMovement.PauseGroundConstraint();
            __brain.AnimCtrler.mainAnimator.SetTrigger("OnJump");
            __brain.AnimCtrler.mainAnimator.SetBool("IsJumping", true);
            __brain.BB.action.isJumping.Value = true;
        }

        public void FinishJump()
        {
            __landingTimeStamp = Time.time;
            __brain.AnimCtrler.mainAnimator.SetBool("IsJumping", false);
            __brain.AnimCtrler.legAnimator.User_AddImpulse(new ImpulseExecutor(0.2f * Vector3.down, Vector3.zero, 0.2f));
            __brain.BB.action.isJumping.Value = false;
        }

        public void PrepareHanging(DroneBotBrain hangingBrain)
        {
            ReservedHangingBrain = hangingBrain;
            //* Decision 값을 'Catch'로 직접 변경함
            ReservedHangingBrain.BB.decision.currDecision.Value = DroneBotBrain.Decisions.Catch;
        }

        public void StartHanging()
        {
            Debug.Assert(ReservedHangingBrain != null);

            __brain.BB.action.hangingBrain.Value = ReservedHangingBrain;
            ReservedHangingBrain = null;

            //* 점프가 종료되는 상황은 발생하면 안됨
            Debug.Assert(__brain.BB.IsJumping);
            __brain.BB.action.isJumping.Value = false;
            __brain.AnimCtrler.mainAnimator.SetTrigger("OnHanging");
            __brain.AnimCtrler.mainAnimator.SetBool("IsHanging", true);

            //* HangingAttachPoint까지 남은 거리를 부드럽게 Lerp 처리함
            __hangingLerpDisposable?.Dispose();
            __hangingLerpDisposable = Observable.EveryUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(0.1f)))
                .DoOnCancel(() => __hangingLerpDisposable = null)
                .DoOnCompleted(() => __hangingLerpDisposable = null)
                .Subscribe(_ => __ecmMovement.SetPosition(capsule.transform.position.LerpSpeed(__brain.BB.action.hangingBrain.Value.AnimCtrler.hangingPoint.position, moveSpeed, Time.deltaTime))).AddTo(this);
        }

        public DroneBotBrain ReservedHangingBrain { get; private set; }
        IDisposable __hangingLerpDisposable;

        public void FinishHanging()
        {
            Debug.Assert(__brain.BB.action.hangingBrain.Value != null);

            __brain.BB.action.hangingBrain.Value.InvalidateDecision(0.1f);
            __brain.BB.action.hangingBrain.Value = null;
            __brain.AnimCtrler.mainAnimator.SetBool("IsHanging", false);
        }

        public void StartZipRiding(Transform startPoint, Transform endPoint, float duration)
        {
            __zipRidingStartPoint = startPoint;
            __zipRidingEndPoint = endPoint;
            __zipRidingDuration = duration;
            __zipRidingTimeStamp = Time.time;
            __isZipRidingActionPending = false;
            __brain.BB.action.isZipRiding.Value = true;

            ziplineOffsetY = startPoint.transform.position.y - capsule.transform.position.y;

            //* IsJumping, IsHanging 파라메터 값은 True값으로 고정되어져 있어야함
            Debug.Assert(__brain.AnimCtrler.mainAnimator.GetBool("IsJumping"));
            Debug.Assert(__brain.AnimCtrler.mainAnimator.GetBool("IsHanging"));
        }

        public void FinishZipRiding()
        {
            __brain.BB.action.isZipRiding.Value = false;
            __brain.AnimCtrler.mainAnimator.SetBool("IsJumping", false);
            __brain.AnimCtrler.mainAnimator.SetBool("IsHanding", false);

            __brain.ActionCtrler.SetPendingAction("Leaping");
        }

        public void StartRolling(float duration)
        {
            if (__brain.BB.IsRolling)
                return;

            var rollingXZ = Vector3.zero;
            if (__brain.Movement.moveVec == Vector3.zero)
            {
                rollingXZ = Vector3.back;
                __rollingVec = -capsule.forward.Vector2D().normalized;
            }
            else
            {
                rollingXZ = capsule.InverseTransformDirection(__brain.Movement.moveVec);
                __rollingVec = __brain.Movement.moveVec.Vector2D().normalized;
            }

            __rollingTimeStamp = Time.time;
            __rollingSpeed = __brain.BB.body.rollingDistance / duration;
            __rollingDuration = duration;

            if (Mathf.Abs(rollingXZ.x) > Mathf.Abs(rollingXZ.z))
            {
                __brain.AnimCtrler.mainAnimator.SetFloat("RollingX", rollingXZ.x > 0f ? 1f : -1f);
                __brain.AnimCtrler.mainAnimator.SetFloat("RollingY", 0);
            }
            else
            {
                __brain.AnimCtrler.mainAnimator.SetFloat("RollingX", 0);
                __brain.AnimCtrler.mainAnimator.SetFloat("RollingY", rollingXZ.z > 0f ? 1f : -1f);
            }
            __brain.AnimCtrler.mainAnimator.SetBool("IsRolling", true);
            __brain.AnimCtrler.mainAnimator.SetTrigger("OnRolling");
            __brain.BB.action.isRolling.Value = true;
        }

        public void FinishRolling()
        {
            if (__brain.BB.IsRolling)
            {
                __brain.AnimCtrler.mainAnimator.SetBool("IsRolling", false);
                __brain.BB.action.isRolling.Value = false;
            }
        }

        bool __isFalling;
        float __jumpTimeStamp;
        float __landingTimeStamp;
        float __prevCapsulePositionY;
        Transform __zipRidingStartPoint;
        Transform __zipRidingEndPoint;
        float __zipRidingDuration;
        float __zipRidingTimeStamp;
        bool __isZipRidingActionPending;
        float __rollingTimeStamp;
        float __rollingDuration;
        float __rollingSpeed;
        Vector3 __rollingVec;
        HeroBrain __brain;

        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            __brain = GetComponent<HeroBrain>();
        }

        protected override void OnFixedUpdateHandler()
        {
            if (!__ecmMovement.enabled)
                return;

            if (__brain.BB.IsJumping)
            {
                if (__prevCapsulePositionY > capsule.position.y && Time.time - __jumpTimeStamp > 2f * Time.fixedDeltaTime)
                    __isFalling = true;
                __prevCapsulePositionY = capsule.position.y;

                if (__isFalling && __ecmMovement.isGrounded)
                {
                    FinishJump();
                }
                else
                {
                    if (__rootMotionPosition.sqrMagnitude > 0f)
                    {
                        var rootMotionVelocity = GetRootMotionVelocity(Time.fixedDeltaTime);
                        //* 평면 방향 성분은 없어야 함 (점프 방향으로 자연스럽게 이동하도록 강제함)
                        Debug.Assert(rootMotionVelocity.x == 0f && rootMotionVelocity.z == 0f);

                        __ecmMovement.Move(__ecmMovement.velocity.AdjustY(rootMotionVelocity.y), Time.fixedDeltaTime);
                    }
                    else
                    {
                        __ecmMovement.velocity += Time.fixedDeltaTime * gravity;
                        __ecmMovement.Move(Time.fixedDeltaTime);
                    }
                }

                ResetRootMotion();
            }
            else if (__brain.BB.IsHanging)
            {
                ;
            }
            else if (__brain.BB.IsZipRiding)
            {
                var alpha = (Time.time - __zipRidingTimeStamp) / __zipRidingDuration;
                if (alpha >= 1f || __ecmMovement.isGrounded)
                {
                    FinishZipRiding();
                }
                else
                {
                    if (!__isZipRidingActionPending && __brain.coreColliderHelper.GetDistanceBetween(__brain.BB.TargetColliderHelper) < 3f)
                    {
                        __brain.ActionCtrler.SetPendingAction("Leaping");
                        __isZipRidingActionPending = true;
                    }

                    var newPosition = Vector3.Lerp(__zipRidingStartPoint.position, __zipRidingEndPoint.position, alpha);
                    __ecmMovement.SetPosition(newPosition + ziplineOffsetY * Vector3.down);
                    __ecmMovement.SetRotation(Quaternion.LookRotation((__zipRidingEndPoint.position - __zipRidingStartPoint.position).Vector2D().normalized));
                }
            }
            else if (__brain.BB.IsRolling)
            {
                if (Time.time - __rollingTimeStamp <= __rollingDuration)
                    AddRootMotion(__rollingSpeed * Time.fixedDeltaTime * __rollingVec, Quaternion.identity);
                else
                    FinishRolling();

                if (__rootMotionPosition.sqrMagnitude > 0f)
                    __ecmMovement.Move(GetRootMotionVelocity(Time.fixedDeltaTime), Time.fixedDeltaTime);
                else
                    __ecmMovement.SimpleMove(moveSpeed * moveVec, moveSpeed, moveAccel, moveBrake, 1f, 1f, gravity, false, Time.fixedDeltaTime);

                __ecmMovement.rotation *= __rootMotionRotation;
                ResetRootMotion();
            }
            else if (IsPushForceRunning)
            {
                __ecmMovement.Move(__pushForceVec * __pushForceMagnitude, __pushForceMagnitude);
                ResetRootMotion();
            }
            else
            {
                if (!freezeMovement)
                {
                    var canMove1 = __pawnBrain.PawnBB.IsSpawnFinished && !__pawnBrain.PawnBB.IsDead && !__pawnBrain.PawnBB.IsGroggy && !__pawnBrain.PawnBB.IsDown;
                    var canMove2 = canMove1 && (!__pawnActionCtrler.CheckActionRunning() || __pawnActionCtrler.currActionContext.movementEnabled) && !__pawnStatusCtrler.CheckStatus(PawnStatus.Staggered);
                    moveVec = canMove2 ? moveVec : Vector3.zero;
                }

                if (__freezeMovementFoeOneFrame)
                {
                    __freezeMovementFoeOneFrame = false;
                    __ecmMovement.Move(Time.fixedDeltaTime);
                }
                else if (__rootMotionPosition.sqrMagnitude > 0f)
                {
                    __ecmMovement.Move(GetRootMotionVelocity(Time.fixedDeltaTime), Time.fixedDeltaTime);
                }
                else
                {
                    __ecmMovement.SimpleMove(moveSpeed * moveVec, moveSpeed, moveAccel, moveBrake, 1f, 1f, gravity, false, Time.fixedDeltaTime);
                }

                __ecmMovement.rotation *= __rootMotionRotation;
                ResetRootMotion();
            }
        }

        protected override void OnUpdateHandler()
        {
            if (!__ecmMovement.enabled)
                return;

            if (__brain.BB.IsHanging)
            {
                //* hangingPoint로 위치값 및 회전값을 맞춰줌
                if (__hangingLerpDisposable == null)
                    __ecmMovement.SetPositionAndRotation(__brain.BB.action.hangingBrain.Value.AnimCtrler.hangingPoint.position, __brain.BB.action.hangingBrain.Value.AnimCtrler.hangingPoint.rotation);
            }
            else
            {
                var canRotate1 = __pawnBrain.PawnBB.IsSpawnFinished && !__pawnBrain.PawnBB.IsDead && !__pawnBrain.PawnBB.IsGroggy && !__pawnBrain.PawnBB.IsDown;
                var canRotate2 = canRotate1 && !__brain.BB.IsRolling && !__brain.BB.IsJumping;
                var canRotate3 = canRotate2 && (!__pawnActionCtrler.CheckActionRunning() || __pawnActionCtrler.currActionContext.movementEnabled) && !__pawnStatusCtrler.CheckStatus(PawnStatus.Staggered);

                if (canRotate3)
                    __ecmMovement.RotateTowards(faceVec, rotateSpeed);
            }
        }
    }
}