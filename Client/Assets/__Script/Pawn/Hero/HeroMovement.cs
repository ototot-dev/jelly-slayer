using UnityEngine;
using FIMSpace.FProceduralAnimation;
using static FIMSpace.FProceduralAnimation.LegsAnimator;
using Unity.VisualScripting;

namespace Game
{
    [TooltipAttribute("이동, 점프, 대시 등의 처리")]
    public class HeroMovement : PawnMovement, LegsAnimator.ILegStepReceiver
    {
        public float LastJumpTimeStamp => __jumpTimeStamp;
        public float LastRollingTimeStamp => __rollingTimeStamp;
        public float JumpVerticalImpulse => Mathf.Sqrt(2 * __brain.BB.body.jumpHeight * __ecmMovement.gravity.magnitude);
        public float EstimatedJumpDuration => Mathf.Sqrt(8 * __brain.BB.body.jumpHeight / __ecmMovement.gravity.magnitude);
        public void LegAnimatorStepEvent(LegsAnimator.Leg leg, float power, bool isRight, Vector3 position, Quaternion rotation, LegsAnimator.EStepType type) {}

        public Vector3 Gravity {
            get { return __ecmMovement.gravity; }
            set { __ecmMovement.gravity = value; }
        }

        public void StartJumping()
        {
            __isFalling = false;
            __jumpTimeStamp = Time.time;
            __ecmMovement.ApplyVerticalImpulse(JumpVerticalImpulse);
            __ecmMovement.DisableGrounding();
            __brain.AnimCtrler.mainAnimator.SetTrigger("OnJump");
            __brain.AnimCtrler.mainAnimator.SetBool("IsJumping", true);
            __brain.BB.action.isJumping.Value = true;
        }

        public void FinishJumping()
        {
            __landingTimeStamp = Time.time;
            __brain.AnimCtrler.mainAnimator.SetBool("IsJumping", false);
            __brain.AnimCtrler.legAnimator.User_AddImpulse(new ImpulseExecutor(0.2f * Vector3.down, Vector3.zero, 0.2f));
            __brain.BB.action.isJumping.Value = false;
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

            if (__brain.BB.IsJumping) // 점프
            {
                ResetRootMotion();

                if (__prevCapsulePositionY > capsule.position.y && Time.time - __jumpTimeStamp > 2f * Time.fixedDeltaTime)
                {
                    __isFalling = true;
                    __ecmMovement.EnableGroundDetection();
                }
                __prevCapsulePositionY = capsule.position.y;

                if (__isFalling && __ecmMovement.isOnGround)
                    FinishJumping();
                else
                    __ecmMovement.Move(moveVec * moveSpeed, moveSpeed);

            }
            else if (__brain.BB.IsRolling) // 대시
            {
                if (Time.time - __rollingTimeStamp <= __rollingDuration)
                {
                    var rollingSpeed = __brain.BB.body.rollingDistance / __rollingDuration;
                    AddRootMotion(rollingSpeed * Time.fixedDeltaTime * __rollingVec, Quaternion.identity);
                }
                else
                {
                    FinishRolling();
                }

                ConsumeRootMotion();
                ResetRootMotion();

                __ecmMovement.Move(Vector3.zero, 0);
            }
            else if (IsPushForceRunning)
            {
                ResetRootMotion();

                __ecmMovement.Move(__pushForceVec * __pushForceMagnitude, __pushForceMagnitude);
                __ecmMovement.useGravity = true;
            }
            else
            {
                if (!freezeMovement)
                {
                    var canMove1 = __pawnBrain.PawnBB.IsSpawnFinished && !__pawnBrain.PawnBB.IsDead && !__pawnBrain.PawnBB.IsStunned && !__pawnBrain.PawnBB.IsDown;
                    var canMove2 = canMove1 && !__actionCtrler.CheckActionRunning() && !__buffCtrler.CheckStatus(PawnStatus.Staggered);
                    moveVec = canMove2 ? moveVec : Vector3.zero;
                }

                ConsumeRootMotion();
                ResetRootMotion();

                if (moveVec != Vector3.zero)
                {
                    if (freezeRotation)
                        __ecmMovement.Move(moveVec * moveSpeed, moveSpeed, moveAccel, moveBrake, 0.1f, 0.2f);
                    else
                        __ecmMovement.Move(moveVec * moveSpeed, moveSpeed);
                }
                else
                {
                    __ecmMovement.Move(Vector3.zero, moveSpeed, 0, moveBrake, 1, 1);
                }
            }
        }

        protected override void OnUpdateHandler()
        {
            if (!__ecmMovement.enabled)
                return;

            var canRotate1 = __pawnBrain.PawnBB.IsSpawnFinished && !__pawnBrain.PawnBB.IsDead && !__pawnBrain.PawnBB.IsStunned && !__pawnBrain.PawnBB.IsDown;
            var canRotate2 = canRotate1 && !__brain.BB.IsRolling && !__brain.BB.IsJumping;
            var canRotate3 = canRotate2 && !__actionCtrler.CheckActionRunning() && !__buffCtrler.CheckStatus(PawnStatus.Staggered);

            if (canRotate3)
                __ecmMovement.Rotate(faceVec, rotateSpeed);
        }
    }
}