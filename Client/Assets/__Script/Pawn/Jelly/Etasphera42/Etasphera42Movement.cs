using System;
using UniRx;
using UnityEngine;

namespace Game
{
    public class Etasphera42Movement : PawnMovementEx
    {
        public void StartJump(float jumpHeight)
        {
            __jumpTimeStamp = Time.time;
            __jumpImpulseTimeStamp = -1f;
            __brain.AnimCtrler.mainAnimator.SetTrigger("OnJump");
            __brain.AnimCtrler.mainAnimator.SetBool("IsJumping", true);
            __brain.BB.action.isJumping.Value = true;
            __brain.BB.action.isFalling.Value = false;
            __brain.BB.action.isGliding.Value = false;

            Observable.Timer(TimeSpan.FromMilliseconds(40)).Subscribe(_ =>
            {            
                if (__brain.BB.IsJumping)
                {
                    __jumpImpulseTimeStamp = Time.time;
                    __ecmMovement.velocity.y = GetVerticalImpulseOnJump(jumpHeight);
                    __ecmMovement.PauseGroundConstraint();
                }
            }).AddTo(this);
        }

        public void StartGliding()
        {
            __ecmMovement.velocity = __ecmMovement.velocity.AdjustY(0f);
            __glidingTimeStamp = Time.time;
            __brain.AnimCtrler.mainAnimator.SetBool("IsGliding", true);
            __brain.BB.action.isJumping.Value = false;
            __brain.BB.action.isGliding.Value = true;
        }

        public void StartFalling()
        {
            __brain.AnimCtrler.mainAnimator.SetBool("IsFalling", true);
            __brain.BB.action.isJumping.Value = false;
            __brain.BB.action.isGliding.Value = false;
            __brain.BB.action.isFalling.Value = true;
        }

        public void FinishFalling()
        {
            __landingTimeStamp = Time.time;
            __brain.AnimCtrler.mainAnimator.SetBool("IsFalling", false);
            __brain.BB.action.isFalling.Value = false;
        }

        float __jumpTimeStamp;
        float __jumpImpulseTimeStamp;
        float __glidingTimeStamp;
        float __landingTimeStamp;
        Etasphera42Brain __brain;

        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            __brain = GetComponent<Etasphera42Brain>();
        }

        protected override void OnFixedUpdateHandler()
        {
            if (!__ecmMovement.enabled)
                return;

            if (__brain.BB.IsJumping)
            {
                Debug.Assert(__rootMotionPosition.sqrMagnitude <= 0f);

                __ecmMovement.velocity += Time.fixedDeltaTime * gravity;
                __ecmMovement.Move(Time.fixedDeltaTime);

                if (__jumpImpulseTimeStamp > 0f && __ecmMovement.velocity.y < 0f)
                    StartGliding();
            }
            else if (__brain.BB.IsFalling)
            {
                Debug.Assert(__rootMotionPosition.sqrMagnitude <= 0f);
                
                __ecmMovement.velocity += Time.fixedDeltaTime * gravity;
                __ecmMovement.Move(Time.fixedDeltaTime);

                if (__ecmMovement.isGrounded)
                    FinishFalling();
            }
            else if (__brain.BB.IsGliding)
            {
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
                    __ecmMovement.SimpleMove(moveSpeed * moveVec, moveSpeed, moveAccel, moveBrake, 1f, 1f, Vector3.zero, false, Time.fixedDeltaTime);

                    if (__brain.BB.body.glidingDuration >= 0f && glidingElapsedTime > __brain.BB.body.glidingDuration)
                        StartFalling();
                }

                __ecmMovement.rotation *= __rootMotionRotation;
                ResetRootMotion();
            }
            else
            {
                base.OnFixedUpdateHandler();
            }
        }
    }
}