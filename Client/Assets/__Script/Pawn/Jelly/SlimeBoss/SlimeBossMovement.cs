using System;
using UniRx;
using UnityEngine;

namespace Game
{
    public class SlimeBossMovement : PawnMovement
    {
        [Header("Destination")]
        public Vector3 destination;
        public float jumpHeight = 1f;
        public float minApproachDistance = 1f;
        public float LastJumpTimeStamp => __jumpTimeStamp;
        public float GetVerticalImpulseOnJump() => Mathf.Sqrt(2 * jumpHeight * __ecmMovement.gravity.magnitude);
        public float GetEstimatedJumpDuration() => Mathf.Sqrt(8 * jumpHeight / __ecmMovement.gravity.magnitude);
        public virtual float GetDefaultMinApproachDistance() => 1f;
        public bool CheckReachToDestination() => (destination - capsule.position).SqrMagnitude2D() < minApproachDistance * minApproachDistance;
        public float DistanceToDestination() => Mathf.Max(0, (destination - capsule.position).Magnitude2D() - minApproachDistance);
         public float EstimateTimeToDestination() => DistanceToDestination() / (moveSpeed > 0f ? moveSpeed : 1f);

        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            __brain = GetComponent<SlimeBossBrain>();
            minApproachDistance = GetDefaultMinApproachDistance();
        }

        SlimeBossBrain __brain;
        bool __isFalling;
        float __jumpTimeStamp;
        float __impluseTimeStamp;
        float __rollingTimeStamp;
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
                __impluseTimeStamp = -1f;
                __isFalling = false;
            }).AddTo(this);

            __brain.BB.isBumping.Where(v => v).Subscribe(_ =>
            {
                jumpHeight = __brain.BB.bumpingHeight;
                __prevCapsulePositionY = capsule.position.y - 1f;
                __jumpTimeStamp = Time.time;
                __impluseTimeStamp = -1f;
                __isFalling = false;
            }).AddTo(this);

            __brain.BB.isSmashing.Where(v => v).Subscribe(_ =>
            {
                jumpHeight = __brain.BB.smashingHeight;
                __prevCapsulePositionY = capsule.position.y - 1f;
                __jumpTimeStamp = Time.time;
                __impluseTimeStamp = -1f;
                __isFalling = false;
            }).AddTo(this);

            __brain.BB.isRolling.Where(v => v).Subscribe(_ =>
            {
                __brain.AnimCtrler.springMass.coreRigidBody.isKinematic = false;
                __brain.AnimCtrler.springMass.coreRigidBody.AddForce(__brain.BB.rollingImpluse * capsule.forward.Vector2D().normalized, ForceMode.Impulse);
                __rollingTimeStamp = Time.time;
            }).AddTo(this);
        }

        protected override void OnFixedUpdateHandler()
        {
            if (__brain.BB.IsRolling)
            {
                Debug.Assert(!__brain.AnimCtrler.springMass.coreRigidBody.isKinematic);

                //* coreRigidBody가 다이나믹 상태라면 capsule을 위치는 coreRigidBody를 따라감
                capsule.transform.position = __brain.AnimCtrler.springMass.core.position.AdjustY(0f);

                var deltaTime = Time.time - __rollingTimeStamp;
                if (deltaTime > __brain.BB.rollingDuration || __brain.AnimCtrler.springMass.coreRigidBody.linearVelocity.sqrMagnitude < 0.1f)
                {
                    __brain.AnimCtrler.springMass.coreRigidBody.isKinematic = true;
                    __brain.BB.isRolling.Value = false;
                }

                //* 'coreRigidBody'가 다이나믹 상태임으로 추가적인 이동 로직 처리는 하지 않음
                return;
            }
            else if (__brain.BB.IsBumping)
            {
                if (__impluseTimeStamp < 0f && (Time.time - __jumpTimeStamp) > __PREJUMP_DURATION)
                {
                    __impluseTimeStamp = Time.time;
                    __ecmMovement.DisableGrounding();
                    __ecmMovement.ApplyVerticalImpulse(GetVerticalImpulseOnJump());

                    //* HitBox를 Trace할 수 있도록 'PawnOverlapped'로 Layer를 변경함
                    capsule.gameObject.layer = LayerMask.NameToLayer("PawnOverlapped");
                }

                if (__impluseTimeStamp > 0f)
                {
                    if (__actionCtrler.CheckActionRunning())
                    {
                        var rootMotionVec = __brain.BB.bumpingSpeed * Time.fixedDeltaTime * capsule.forward.Vector2D().normalized;
                        if (__actionCtrler.CanRootMotion(rootMotionVec))
                            AddRootMotion(rootMotionVec, Quaternion.identity);
                    }

                    //* Bumping 이동은 RootMotion으로 처리해서 moveSpeed와 moveVec는 Zero로 셋팅함
                    moveSpeed = 0;
                    moveVec = Vector3.zero;

                    if (__prevCapsulePositionY > capsule.position.y && (Time.time - __jumpTimeStamp) > Time.fixedDeltaTime * 2f)
                    {
                        __isFalling = true;
                        __ecmMovement.EnableGroundDetection();
                    }
                    __prevCapsulePositionY = capsule.position.y;

                    if (__isFalling && __ecmMovement.isOnGround)
                    {
                        __landingTimeStamp = Time.time;
                        __impluseTimeStamp = -1f;
                        __brain.BB.isBumping.Value = false;
                        //* Capsule의 Layer를 원래대로 복구함
                        capsule.gameObject.layer = LayerMask.NameToLayer("Pawn");
                    }
                }
            }
            else if (__brain.BB.IsSmashing)
            {
                if (__impluseTimeStamp < 0f && (Time.time - __jumpTimeStamp) > __PREJUMP_DURATION)
                {
                    __impluseTimeStamp = Time.time;
                    __ecmMovement.DisableGrounding();
                    __ecmMovement.ApplyVerticalImpulse(GetVerticalImpulseOnJump());

                    //* Smashing 중에는 다른 Pawn의 Capsule Collider를 밀치지 못하도록 'PawnOverlapped'로 Layer를 변경함
                    capsule.gameObject.layer = LayerMask.NameToLayer("PawnOverlapped");
                }

                if (__impluseTimeStamp > 0f)
                {
                    //* Smashing 이동은 RootMotion으로 처리해서 moveSpeed와 moveVec는 Zero로 셋팅함
                    moveSpeed = 0f;
                    moveVec = Vector3.zero;

                    //* Target까지 접근 거리를 계산해서 필요한 경우에만 이동을 함
                    if (__brain.BB.TargetBrain.coreColliderHelper.GetApproachDistance(__brain.coreColliderHelper.transform.position) > 0f)
                        AddRootMotion(__brain.BB.smashingSpeed * Time.fixedDeltaTime * capsule.forward.Vector2D().normalized, Quaternion.identity);

                    if (__prevCapsulePositionY > capsule.position.y && (Time.time - __jumpTimeStamp) > Time.fixedDeltaTime * 2f)
                    {
                        __isFalling = true;
                        __ecmMovement.EnableGroundDetection();
                        __ecmMovement.ApplyImpulse(__brain.BB.smashingForce * Vector3.down);
                    }
                    __prevCapsulePositionY = capsule.position.y;

                    if (__isFalling && __ecmMovement.isOnGround)
                    {
                        __landingTimeStamp = Time.time;
                        __impluseTimeStamp = -1f;
                        __brain.BB.isSmashing.Value = false;
                        //* Capsule의 Layer를 원래대로 복구함
                        capsule.gameObject.layer = LayerMask.NameToLayer("Pawn");
                    }
                }
            }
            else if (!freezeMovement)
            {
                moveSpeed = __brain.BB.jumpSpeed;

                var canJump1 = __pawnBrain.PawnBB.IsSpawnFinished && !__pawnBrain.PawnBB.IsDead && !__pawnBrain.PawnBB.IsGroggy && !__pawnBrain.PawnBB.IsDown;
                var canJump2 = canJump1 && (__actionCtrler == null || (!__actionCtrler.CheckActionPending() && !__actionCtrler.CheckActionRunning()));
                var canJump3 = canJump2 && (__buffCtrler == null || !__buffCtrler.CheckStatus(PawnStatus.Staggered));

                if (__brain.BB.IsJumping)
                {
                    if (__impluseTimeStamp < 0f && (Time.time - __jumpTimeStamp) > __PREJUMP_DURATION)
                    {
                        __impluseTimeStamp = Time.time;
                        __ecmMovement.DisableGrounding();
                        __ecmMovement.ApplyVerticalImpulse(GetVerticalImpulseOnJump());
                    }

                    if (__impluseTimeStamp > 0f)
                    {
                        if (__prevCapsulePositionY > capsule.position.y && (Time.time - __jumpTimeStamp) > Time.fixedDeltaTime * 2f)
                        {
                            __isFalling = true;
                            __ecmMovement.EnableGroundDetection();
                        }

                        __prevCapsulePositionY = capsule.position.y;
                        if (__isFalling && __ecmMovement.isOnGround)
                        {
                            __landingTimeStamp = Time.time;
                            __impluseTimeStamp = -1f;
                            __brain.BB.isJumping.Value = false;
                        }

                        moveVec = moveSpeed * (destination - capsule.position).Vector2D().normalized;
                    }
                    else
                    {
                        moveVec = Vector3.zero;
                    }
                }
                else
                {
                    if (canJump3 && (Time.time - __landingTimeStamp) > 0.5f)
                        __brain.BB.isJumping.Value = true;

                    moveVec = Vector3.zero;
                }
            }

            //* Zombie한테 Local-Avoidance 적용해서 SlimeBoss에게 달라붙지 않도록 함
            foreach (var c in __pawnBrain.PawnSensorCtrler.TouchingColliders)
            {
                if (c.TryGetComponent<PawnColliderHelper>(out var collierHelper) && collierHelper.pawnBrain.PawnBB.common.pawnId == PawnId.Zombie && collierHelper.pawnBrain.PawnBB.IsSpawnFinished)
                    (collierHelper.pawnBrain as IMovable).AddRootMotion(Time.fixedDeltaTime * (c.transform.position - capsule.position).Vector2D().normalized, Quaternion.identity);
            }

            base.OnFixedUpdateHandler();
        }

        protected override void OnUpdateHandler()
        {   
            if (!freezeRotation)
            {
                var canRotate1 = __pawnBrain.PawnBB.IsSpawnFinished && !__pawnBrain.PawnBB.IsDead && !__pawnBrain.PawnBB.IsGroggy && !__pawnBrain.PawnBB.IsDown;
                var canRotate2 = canRotate1 && (__actionCtrler == null || !__actionCtrler.CheckActionRunning());
                var canRotate3 = canRotate2 && __brain.BB.IsJumping && __impluseTimeStamp > 0f;

                faceVec = canRotate3 ? (destination - capsule.position).Vector2D().normalized : Vector3.zero;
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