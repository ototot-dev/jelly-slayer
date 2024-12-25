using System;
using Packets;
using UniRx;
using UnityEngine;

namespace Game
{
    public class JellyMovement : PawnMovementEx
    {   
        // [Header("Jump")]
        // public BoolReactiveProperty isJumping = new();

        // public void StartJumping(float jumpHeight)
        // {
        //     __isFalling = false;
        //     __jumpTimeStamp = Time.time;
        //     __ecmMovement.velocity.y = GetVerticalImpulseOnJump(jumpHeight);
        //     __ecmMovement.PauseGroundConstraint();
        //     __panwAnimCtrler.mainAnimator.SetTrigger("OnJump");
        //     __panwAnimCtrler.mainAnimator.SetBool("IsJumping", true);

        //     isJumping.Value = true;
        // }

        // public void FinishJumping()
        // {
        //     isJumping.Value = false;
        //     __landingTimeStamp = Time.time;
        //     __panwAnimCtrler.mainAnimator.SetBool("IsJumping", false);
        //     __panwAnimCtrler.legAnimator.User_AddImpulse(new ImpulseExecutor(0.2f * Vector3.down, Vector3.zero, 0.2f));
        //     isJumping.Value = false;
        // }
         
        // bool __isFalling;
        // float __jumpTimeStamp;
        // float __landingTimeStamp;
        // float __prevCapsulePositionY;
        // JellyBrain __jellyBrain;
        // PawnAnimController __panwAnimCtrler;

        // protected override void AwakeInternal()
        // {
        //     base.AwakeInternal();
        //     __jellyBrain = GetComponent<JellyBrain>();
        //     __panwAnimCtrler = GetComponent<PawnAnimController>();
        // }

        // protected override void OnFixedUpdateHandler()
        // {
        //     if (!__ecmMovement.enabled)
        //         return;

        //     if (isJumping)
        //     {
        //         if (__prevCapsulePositionY > capsule.position.y && (Time.time - __jumpTimeStamp) > 2f * Time.fixedDeltaTime)
        //             __isFalling = true;
        //         __prevCapsulePositionY = capsule.position.y;
                
        //         if (__isFalling && __ecmMovement.isGrounded)
        //         {
        //             FinishJumping();
        //         }
        //         else
        //         {
        //             // Perlin.Noise01()

        //             __ecmMovement.velocity += Time.fixedDeltaTime * gravity;
        //             __ecmMovement.Move(Time.fixedDeltaTime);
        //         }

        //         ResetRootMotion();

        //         return;
        //     }

        //     base.OnFixedUpdateHandler();
        // }
    }
}