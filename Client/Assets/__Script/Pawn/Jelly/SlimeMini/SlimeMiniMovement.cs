using System;
using UniRx;
using UnityEngine;

namespace Game
{
    public class SlimeMiniMovement : PawnMovement
    {
        [Header("Destination")]
        public Vector3 destination;
        public float jumpHeight = 1f;
        public float minApproachDistance = 1f;
        public float LastJumpTimeStamp => __jumpTimeStamp;
        public float GetVerticalImpulseOnJump() => Mathf.Sqrt(2f * jumpHeight * gravity.magnitude);
        public float GetEstimatedJumpDuration() => Mathf.Sqrt(8f * jumpHeight / gravity.magnitude);
        public virtual float GetDefaultMinApproachDistance() => 1f;
        public bool CheckReachToDestination() => (destination - capsule.position).SqrMagnitude2D() < minApproachDistance * minApproachDistance;
        public float DistanceToDestination() => Mathf.Max(0, (destination - capsule.position).Magnitude2D() - minApproachDistance);
         public float EstimateTimeToDestination() => DistanceToDestination() / (moveSpeed > 0f ? moveSpeed : 1f);

        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            __brain = GetComponent<SlimeMiniBrain>();
            minApproachDistance = GetDefaultMinApproachDistance();
        }

        SlimeMiniBrain __brain;
        bool __isFalling;
        float __jumpTimeStamp;
        float __impulseTimeStamp;
        float __landingTimeStamp;
        float __prevCapsulePositionY;
        const float __PREJUMP_DURATION = 0.7f; //* 점프 전에 응축하는 시간

        protected override void StartInternal()
        {
            base.StartInternal();

            __brain.BB.isJumping.Where(v => v).Subscribe(_ =>
            {
                jumpHeight = __brain.BB.jumpHeight;
                __prevCapsulePositionY = capsule.position.y - 1f;
                __jumpTimeStamp = Time.time;
                __impulseTimeStamp = -1f;
                __isFalling = false;
            }).AddTo(this);
        }

        protected override void OnFixedUpdateHandler()
        {
            if (!freezeMovement)
            {
                moveSpeed = __brain.BB.jumpSpeed;

                var canJump1 = __pawnBrain.PawnBB.IsSpawnFinished && !__pawnBrain.PawnBB.IsDead && !__pawnBrain.PawnBB.IsGroggy && !__pawnBrain.PawnBB.IsDown;
                var canJump2 = __actionCtrler == null || (!__actionCtrler.CheckActionPending() && !__actionCtrler.CheckActionRunning());
                var canJump3 = __buffCtrler == null || !__buffCtrler.CheckStatus(PawnStatus.Staggered);

                if (__brain.BB.IsJumping)
                {
                    if (__impulseTimeStamp < 0f && (Time.time - __jumpTimeStamp) > __PREJUMP_DURATION)
                    {
                        __impulseTimeStamp = Time.time;
                        __ecmMovement.PauseGroundConstraint();
                        __ecmMovement.velocity += GetVerticalImpulseOnJump() * Vector3.up;
                    }

                    if (__impulseTimeStamp > 0f)
                    {
                        if (__prevCapsulePositionY > capsule.position.y && (Time.time - __jumpTimeStamp) > Time.fixedDeltaTime * 2f)
                            __isFalling = true;

                        __prevCapsulePositionY = capsule.position.y;
                        if (__isFalling && __ecmMovement.isOnGround)
                        {
                            __landingTimeStamp = Time.time;
                            __brain.BB.isJumping.Value = false;
                        }

                        moveVec = moveSpeed * (destination - capsule.position).Vector2D().normalized;
                    }
                    else
                    {
                        moveVec = Vector3.zero;
                    }
                }
                else if (canJump1 && canJump2 && canJump3)
                {
                    if (Time.time - __landingTimeStamp > 0.5f)
                    {
                        //* 회전 중에 목표값까지 회전되는 것을 대기
                        var waitRotationFinished = freezeRotation || Vector3.Angle(capsule.forward.Vector2D(), (destination - capsule.position).Vector2D()) < 5f;
                        moveVec = !waitRotationFinished && IsMovingToDestination ? (destination - capsule.position).Vector2D().normalized : Vector3.zero;
                        __brain.BB.isJumping.Value = true;
                    }
                    else
                    {
                        moveVec = Vector3.zero;
                    }
                }   
                else
                {
                    moveVec = Vector3.zero;
                }
            }

            base.OnFixedUpdateHandler();
        }

        protected override void OnUpdateHandler()
        {   
            if (!freezeRotation)
            {
                var canRotate1 = __pawnBrain.PawnBB.IsSpawnFinished && !__pawnBrain.PawnBB.IsDead && !__pawnBrain.PawnBB.IsGroggy && !__pawnBrain.PawnBB.IsDown;
                var canRotate2 = __actionCtrler == null || !__actionCtrler.CheckActionRunning();
                var canRotate3 = __brain.BB.IsJumping;

                faceVec = canRotate1 && canRotate2 && canRotate3 ? (destination - capsule.position).Vector2D().normalized : Vector3.zero;
            }

            base.OnUpdateHandler();
        }
        
        public bool IsMovingToDestination { get; private set; }
        public event Action<bool> OnMovingToDestinationStopped;

        public void ReserveDestination(Vector3 destination)
        {
            this.destination = destination;
        }

        public float SetDestination(Vector3 destination, bool rememberTargetPoint = true)
        {
            this.destination = destination;
            IsMovingToDestination = true;

            return (destination - capsule.position).Magnitude2D();
        }

        public void Stop()
        {
            minApproachDistance = GetDefaultMinApproachDistance();
            moveVec = Vector3.zero;

            if (IsMovingToDestination)
            {
                IsMovingToDestination = false;
                OnMovingToDestinationStopped?.Invoke(false);
            }
        }
        
        [Header("Gizmos")]
        public bool drawGizmosEnabled;

        void OnDrawGizmos()
        {
            if (drawGizmosEnabled)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawCube(destination, 0.2f * Vector3.one);

                if (IsMovingToDestination)
                {
                    Gizmos.DrawLine(capsule.position + capsuleCollider.height * 0.5f * Vector3.up, destination + capsuleCollider.height * 0.5f * Vector3.up);
                    Gizmos.DrawLine(destination, destination + capsuleCollider.height * 0.5f * Vector3.up);
                }
            }
        }
    }
}