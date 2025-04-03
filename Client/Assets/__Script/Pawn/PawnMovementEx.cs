using System;
using Packets;
using UnityEngine;

namespace Game
{
    public class PawnMovementEx : PawnMovement
    {   
        [Header("Destination")]
        public Vector3 destination;
        public float minApproachDistance = 1f;
        public virtual float GetDefaultMinApproachDistance() => __pawnBrain != null ? Mathf.Max(0.1f, __pawnBrain.coreColliderHelper.GetCapsuleRadius() * 0.5f) : 0.1f;
        public virtual bool CanMove()
        {
                var canMove1 = __pawnBrain.PawnBB.IsSpawnFinished && !__pawnBrain.PawnBB.IsDead && !__pawnBrain.PawnBB.IsGroggy && !__pawnBrain.PawnBB.IsDown && !CheckReachToDestination();
                var canMove2 = canMove1 && (__pawnActionCtrler == null || (!__pawnActionCtrler.CheckKnockBackRunning() && (!__pawnActionCtrler.CheckActionRunning() || __pawnActionCtrler.currActionContext.movementEnabled)));
                var canMove3 = canMove2 && (__pawnStatusCtrler == null || (!__pawnStatusCtrler.CheckStatus(PawnStatus.Staggered) && !__pawnStatusCtrler.CheckStatus(PawnStatus.CanNotMove)));
                return canMove3;
        }
        public virtual bool CanRotate()
        {
            var canRotate1 = __pawnBrain.PawnBB.IsSpawnFinished && !__pawnBrain.PawnBB.IsDead && !__pawnBrain.PawnBB.IsGroggy && !__pawnBrain.PawnBB.IsDown;
            var canRotate2 = canRotate1 && (__pawnActionCtrler == null || !__pawnActionCtrler.CheckActionRunning());
            return canRotate2;
        }
        public bool CheckReachToDestination() => (destination - capsule.position).SqrMagnitude2D() < minApproachDistance * minApproachDistance;
        public float DistanceToDestination() => Mathf.Max(0, (destination - capsule.position).Magnitude2D() - minApproachDistance);
        public float EstimateTimeToDestination() => DistanceToDestination() / (moveSpeed > 0f ? moveSpeed : 1f);

        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            minApproachDistance = GetDefaultMinApproachDistance();
        }

        protected override void OnFixedUpdateHandler()
        {
            if (!freezeMovement)
            {
                if (IsMovingToDestination)
                    moveVec = CanMove() ? (destination - capsule.position).Vector2D().normalized : Vector3.zero;
                else
                    moveVec = CanMove() ? moveVec : Vector3.zero;
            }
            else
            {
                moveVec = Vector3.zero;
            }

            base.OnFixedUpdateHandler();
        }

        protected override void OnUpdateHandler()
        {   
            var isHomingRunning = __pawnActionCtrler != null && __pawnActionCtrler.currActionContext.homingRotationDisposable != null;
            if (!isHomingRunning && !freezeRotation)
            {
                var canRotate1 = __pawnBrain.PawnBB.IsSpawnFinished && !__pawnBrain.PawnBB.IsDead && !__pawnBrain.PawnBB.IsGroggy && !__pawnBrain.PawnBB.IsDown;
                var canRotate2 = canRotate1 && (__pawnActionCtrler == null || !__pawnActionCtrler.CheckActionRunning());
                
                if (IsMovingToDestination)
                    faceVec = canRotate2 ? (destination - capsule.position).Vector2D().normalized : Vector3.zero;
                else
                    faceVec = canRotate2 ? faceVec : Vector3.zero;
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