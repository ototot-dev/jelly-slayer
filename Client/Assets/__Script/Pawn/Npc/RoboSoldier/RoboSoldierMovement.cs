using System;
using UniRx;
using UnityEngine;

namespace Game
{
    public class RoboSoldierMovement : PawnMovementEx
    {
        [Header("Movement")]
        public Vector3 glideVec;

        public void StartJump(float jumpHeight)
        {
            __jumpTimeStamp = Time.time;
            __jumpImpulseTimeStamp = -1f;
            __brain.AnimCtrler.mainAnimator.SetTrigger("OnJump");
            __brain.AnimCtrler.mainAnimator.SetBool("IsJumping", true);
            __brain.BB.body.isJumping.Value = true;
            __brain.BB.body.isFalling.Value = false;
            __brain.BB.body.isGliding.Value = false;

            Observable.NextFrame(FrameCountType.FixedUpdate).Subscribe(_ =>
            {
                if (__brain.BB.IsJumping)
                {
                    if (jumpHeight > 0f)
                        __ecmMovement.velocity.y = GetVerticalImpulseOnJump(jumpHeight);

                    __jumpImpulseTimeStamp = Time.time;
                    __ecmMovement.PauseGroundConstraint();
                }
            }).AddTo(this);
        }

        public void StartGliding(bool dampingVelocityY = false)
        {
            if (!dampingVelocityY)
                __ecmMovement.velocity = __ecmMovement.velocity.AdjustY(0f);
            else
            {
                __glidingDampingSpeed = Mathf.Max(10f, Mathf.Abs(__ecmMovement.velocity.y));
                __Logger.Log(gameObject, "__glidingDampingSpeed", __glidingDampingSpeed);
            }

            __glidingTimeStamp = Time.time;
            __brain.AnimCtrler.mainAnimator.SetBool("IsGliding", true);
            __brain.BB.body.isJumping.Value = false;
            __brain.BB.body.isGliding.Value = true;
        }

        public void StartFalling()
        {
            __brain.AnimCtrler.mainAnimator.SetBool("IsFalling", true);
            __brain.BB.body.isJumping.Value = false;
            __brain.BB.body.isGliding.Value = false;
            __brain.BB.body.isFalling.Value = true;
        }

        public void FinishFalling()
        {
            __landingTimeStamp = Time.time;
            __brain.AnimCtrler.mainAnimator.SetBool("IsFalling", false);
            __brain.BB.body.isFalling.Value = false;
        }

        float __jumpTimeStamp;
        float __jumpImpulseTimeStamp;
        float __glidingTimeStamp;
        float __glidingDampingSpeed;
        float __landingTimeStamp;
        RoboSoldierBrain __brain;

        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            __brain = GetComponent<RoboSoldierBrain>();
        }

        protected override void OnFixedUpdateHandler()
        {
            if (!__ecmMovement.enabled)
                return;
            if (!__brain.BB.IsSpawnFinished)
                return;

            if (__brain.BB.IsJumping)
                {
                    __ecmMovement.velocity += Time.fixedDeltaTime * gravity;
                    __ecmMovement.Move(Time.fixedDeltaTime);

                    if (__jumpImpulseTimeStamp > 0f && __ecmMovement.velocity.y < 0f)
                        StartGliding(true);
                }
                else if (__brain.BB.IsFalling)
                {
                    __ecmMovement.velocity += Time.fixedDeltaTime * gravity;
                    __ecmMovement.Move(Time.fixedDeltaTime);

                    if (__ecmMovement.isGrounded)
                        FinishFalling();
                }
                else if (__brain.BB.IsGliding)
                {
                    if (__ecmMovement.velocity.y < 0f)
                        __ecmMovement.velocity = __ecmMovement.velocity.LerpSpeedY(0f, __glidingDampingSpeed, Time.fixedDeltaTime);

                    if (CheckRootMotionZero())
                    {
                        if (!freezeMovement)
                        {
                            var canMove1 = __pawnBrain.PawnBB.IsSpawnFinished && !__pawnBrain.PawnBB.IsDead && !__pawnBrain.PawnBB.IsGroggy && !__pawnBrain.PawnBB.IsDown;
                            var canMove2 = canMove1 && !CheckReachToDestination() && (__pawnActionCtrler == null || !__pawnActionCtrler.CheckActionRunning());
                            var canMove3 = canMove2 && (__pawnStatusCtrler == null || (!__pawnStatusCtrler.CheckStatus(PawnStatus.Staggered) && !__pawnStatusCtrler.CheckStatus(PawnStatus.CanNotMove)));

                            if (IsMovingToDestination)
                                moveVec = canMove3 ? (destination - capsule.position).Vector2D().normalized : Vector3.zero;
                            else
                                moveVec = canMove3 ? moveVec : Vector3.zero;
                        }

                        var glidingElapsedTime = Time.time - __glidingTimeStamp;

                        //* 공중에 뜬 상태에서 자연스러운 업다운 모션을 생성함
                        moveVec = moveVec.AdjustY(__brain.BB.body.glidingAmplitude * Perlin.Noise(__brain.BB.body.glidingFrequency * glidingElapsedTime + __glidingTimeStamp));
                        __ecmMovement.SimpleMove(moveSpeed * (moveVec + glideVec), moveSpeed, moveAccel, moveBrake, 1f, 1f, Vector3.zero, false, Time.fixedDeltaTime);

                        if (__brain.BB.body.glidingDuration >= 0f && glidingElapsedTime > __brain.BB.body.glidingDuration)
                            StartFalling();
                    }
                }
                else
                {
                    base.OnFixedUpdateHandler();
                }
        }
    }
}