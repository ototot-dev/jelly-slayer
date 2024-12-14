using UnityEngine;
using FIMSpace.FProceduralAnimation;
using static FIMSpace.FProceduralAnimation.LegsAnimator;
using Unity.VisualScripting;
using UnityEditor.Rendering;

namespace Game
{
    [TooltipAttribute("이동, 점프, 대시 등의 처리")]
    public class HeroMovement : PawnMovement, LegsAnimator.ILegStepReceiver
    {
        public float LastJumpTimeStamp => __jumpTimeStamp;
        public float LastRollingTimeStamp => __rollingTimeStamp;
        public float ImpulsePowerOnJump => Mathf.Sqrt(2 * __heroBrain.BB.body.jumpHeight * Physics.gravity.magnitude);
        public float EstimatedJumpDuration => Mathf.Sqrt(8 * __heroBrain.BB.body.jumpHeight / Physics.gravity.magnitude);
        public void LegAnimatorStepEvent(LegsAnimator.Leg leg, float power, bool isRight, Vector3 position, Quaternion rotation, LegsAnimator.EStepType type) {}

        public void StartJumping()
        {
            __isFalling = false;
            __jumpTimeStamp = Time.time;
            __ecmMovement.velocity.y = ImpulsePowerOnJump;
            __ecmMovement.PauseGroundConstraint();
            __heroBrain.AnimCtrler.mainAnimator.SetTrigger("OnJump");
            __heroBrain.AnimCtrler.mainAnimator.SetBool("IsJumping", true);
            __heroBrain.BB.action.isJumping.Value = true;
        }

        public void FinishJumping()
        {
            __landingTimeStamp = Time.time;
            __heroBrain.AnimCtrler.mainAnimator.SetBool("IsJumping", false);
            __heroBrain.AnimCtrler.legAnimator.User_AddImpulse(new ImpulseExecutor(0.2f * Vector3.down, Vector3.zero, 0.2f));
            __heroBrain.BB.action.isJumping.Value = false;
        }

        public void StartRolling(float duration)
        {
            if (__heroBrain.BB.IsRolling)
                return;

            var rollingXZ = Vector3.zero;
            if (__heroBrain.Movement.moveVec == Vector3.zero)
            {
                rollingXZ = Vector3.back;
                __rollingVec = -capsule.forward.Vector2D().normalized;
            }
            else
            {
                rollingXZ = capsule.InverseTransformDirection(__heroBrain.Movement.moveVec);
                __rollingVec = __heroBrain.Movement.moveVec.Vector2D().normalized;
            }

            __rollingTimeStamp = Time.time;
            __rollingSpeed = __heroBrain.BB.body.rollingDistance / duration;
            __rollingDuration = duration;

            if (Mathf.Abs(rollingXZ.x) > Mathf.Abs(rollingXZ.z))
            {
                __heroBrain.AnimCtrler.mainAnimator.SetFloat("RollingX", rollingXZ.x > 0f ? 1f : -1f);
                __heroBrain.AnimCtrler.mainAnimator.SetFloat("RollingY", 0);
            }
            else
            {
                __heroBrain.AnimCtrler.mainAnimator.SetFloat("RollingX", 0);
                __heroBrain.AnimCtrler.mainAnimator.SetFloat("RollingY", rollingXZ.z > 0f ? 1f : -1f);
            }
            __heroBrain.AnimCtrler.mainAnimator.SetBool("IsRolling", true);
            __heroBrain.AnimCtrler.mainAnimator.SetTrigger("OnRolling");
            __heroBrain.BB.action.isRolling.Value = true;
        }

        public void FinishRolling()
        {
            if (__heroBrain.BB.IsRolling)
            {
                __heroBrain.AnimCtrler.mainAnimator.SetBool("IsRolling", false);
                __heroBrain.BB.action.isRolling.Value = false;
            }
        }

        bool __isFalling;
        float __jumpTimeStamp;
        float __landingTimeStamp;
        float __prevCapsulePositionY;
        float __rollingTimeStamp;
        float __rollingDuration;
        float __rollingSpeed;
        Vector3 __rollingVec;
        HeroBrain __heroBrain;

        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            __heroBrain = GetComponent<HeroBrain>();
        }

        protected override void OnFixedUpdateHandler()
        {
            if (!__ecmMovement.enabled)
                return;

            if (__heroBrain.BB.IsJumping)
            {
                if (__prevCapsulePositionY > capsule.position.y && Time.time - __jumpTimeStamp > 2f * Time.fixedDeltaTime)
                    __isFalling = true;
                __prevCapsulePositionY = capsule.position.y;

                if (__isFalling && __ecmMovement.isGrounded)
                {
                    FinishJumping();
                }
                else
                {
                    __ecmMovement.velocity += Time.fixedDeltaTime * gravity;
                    __ecmMovement.Move(Time.fixedDeltaTime);
                }

                ResetRootMotion();

            }
            else if (__heroBrain.BB.IsRolling)
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

            var canRotate1 = __pawnBrain.PawnBB.IsSpawnFinished && !__pawnBrain.PawnBB.IsDead && !__pawnBrain.PawnBB.IsGroggy && !__pawnBrain.PawnBB.IsDown;
            var canRotate2 = canRotate1 && !__heroBrain.BB.IsRolling && !__heroBrain.BB.IsJumping;
            var canRotate3 = canRotate2 && (!__pawnActionCtrler.CheckActionRunning() || __pawnActionCtrler.currActionContext.movementEnabled) && !__pawnStatusCtrler.CheckStatus(PawnStatus.Staggered);

            if (canRotate3)
                __ecmMovement.RotateTowards(faceVec, rotateSpeed);
        }
    }
}