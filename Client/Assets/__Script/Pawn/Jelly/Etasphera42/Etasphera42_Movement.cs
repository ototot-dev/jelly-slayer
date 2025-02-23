using System;
using UniRx;
using UnityEngine;

namespace Game
{
    public class Etasphera42_Movement : PawnMovementEx
    {
        public void StartJump(float jumpHeight)
        {
            __jumpTimeStamp = Time.time;
            __ecmMovement.velocity.y = GetVerticalImpulseOnJump(jumpHeight);
            __ecmMovement.PauseGroundConstraint();
            __isFalling = false;
            __brain.BB.action.isJumping.Value = true;
        }

        public void FinishJump()
        {
            __isFalling = false;
            __brain.BB.action.isJumping.Value = false;
        }

        bool __isFalling;
        float __jumpTimeStamp;
        Etasphera42_Brain __brain;

        public override bool CanMove()
        {
            return !__brain.ActionCtrler.CheckAddictiveActionRunning("LaserB") && base.CanMove();
        }

        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            __brain = GetComponent<Etasphera42_Brain>();
        }

        protected override void OnFixedUpdateHandler()
        {
            if (!__ecmMovement.enabled)
                return;
            
            if (__brain.BB.IsJumping)
            {
                if (__jumpTimeStamp > 0f && __ecmMovement.velocity.y < 0f)
                    __isFalling = true;

                if (__isFalling && __ecmMovement.isGrounded)
                {
                    FinishJump();
                }
                else
                {
                    __ecmMovement.velocity += Time.fixedDeltaTime * gravity;

                    if (__rootMotionPosition.sqrMagnitude > 0f)
                        ;
                        // __ecmMovement.Move(GetRootMotionVelocity(Time.fixedDeltaTime).AdjustY(__ecmMovement.velocity.y), Time.fixedDeltaTime);
                    else
                        __ecmMovement.Move(Time.fixedDeltaTime);
                }
            }
            else
            {
                base.OnFixedUpdateHandler();
            }
        }
    }
}