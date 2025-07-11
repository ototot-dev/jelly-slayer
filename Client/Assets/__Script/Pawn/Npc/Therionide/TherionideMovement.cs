using System;
using UniRx;
using UnityEngine;

namespace Game
{
    public class TherionideMovement : PawnMovementEx
    {
        public void StartJump(float jumpHeight)
        {
            __jumpTimeStamp = Time.time;
            __jumpImpulseTimeStamp = -1f;
            __brain.BB.body.isJumping.Value = true;
            __brain.AnimCtrler.mainAnimator.SetTrigger("OnJump");
            __brain.AnimCtrler.mainAnimator.SetBool("IsJumping", true);

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

        public void StartFalling()
        {
            __brain.AnimCtrler.mainAnimator.SetBool("IsFalling", true);
            __brain.BB.body.isJumping.Value = false;
        }

        public void FinishFalling()
        {
            __landingTimeStamp = Time.time;
            __brain.AnimCtrler.mainAnimator.SetBool("IsFalling", false);
        }

        float __jumpTimeStamp;
        float __jumpImpulseTimeStamp;
        float __landingTimeStamp;
        TherionideBrain __brain;

        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            __brain = GetComponent<TherionideBrain>();
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
                        StartFalling();
                }
                else if (__brain.BB.IsFalling)
                {
                    __ecmMovement.velocity += Time.fixedDeltaTime * gravity;
                    __ecmMovement.Move(Time.fixedDeltaTime);

                    if (__ecmMovement.isGrounded)
                        FinishFalling();
                }
                else
                {
                    base.OnFixedUpdateHandler();
                }
        }
    }
}