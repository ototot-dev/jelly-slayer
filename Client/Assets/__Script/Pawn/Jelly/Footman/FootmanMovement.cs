using System;
using Packets;
using UniRx;
using UnityEngine;

namespace Game
{
    public class FootmanMovement : MonoBehaviour
    {
        [Header("Component")]
        public Transform core;
        public Transform pelvisBone;
        public CapsuleCollider capsuleCollider;
        public FIMSpace.FProceduralAnimation.LegsAnimator legAnimator;
        
        [Header("Movement")]
        public float forwardSpeed = 1;
        public float forwardBrake = 0;
        public float rotateSpeed = 720;
        public Vector3 CurrVelocity => __ecmMovement.velocity;
        public Vector3 CurrGravity => Physics.gravity;
        public Vector3 ForwardVecCached { get; private set; } = Vector3.back;

        [Header("Target")]
        public Vector3 targetPoint;
        public const float DEFAULT_MIN_APPROACH_DISTANCE = 0.1f;
        public float minApproachDistance = DEFAULT_MIN_APPROACH_DISTANCE;
        public bool CheckReachToTargetPoint() => (targetPoint - core.position).SqrMagnitude2D() < minApproachDistance * minApproachDistance;
        public float DistanceToReachToTarget() => Mathf.Max(0, (targetPoint - core.position).Magnitude2D() - minApproachDistance);
        public float EstimateTimeToReachTargetPoint() => DistanceToReachToTarget() / forwardSpeed;

        void Awake()
        {
            __spiderBrain = GetComponent<SpiderBrain>();
            __ecmMovement = capsuleCollider.GetComponent<ECM2.CharacterMovement>();
        }

        SpiderBrain __spiderBrain;
        ECM2.CharacterMovement __ecmMovement;
        bool __movementStopped = true;
        public bool IsMoving => !__movementStopped;
        public event Action<bool> OnMovementStopped;

        public void Teleport(Vector3 position)
        {
            core.position = TerrainManager.GetTerrainPoint(position);
        }

        public void ReserveTargetPoint(Vector3 targetPoint)
        {
            this.targetPoint = targetPoint;
        }

        public void FaceTo(Vector3 targetPoint, float rotateSpeed = 360 * 360)
        {
            __ecmMovement.RotateTowards((targetPoint - core.position).Vector2D().normalized, rotateSpeed);
        }

        public float GoTo(Vector3 targetPoint, bool rememberTargetPoint = true)
        {
            this.targetPoint = targetPoint;
            __movementStopped = false;

            return (targetPoint - core.position).Magnitude2D();
        }

        public void Stop()
        {
            minApproachDistance = DEFAULT_MIN_APPROACH_DISTANCE;

            if (!__movementStopped)
            {
                __movementStopped = true;
                OnMovementStopped?.Invoke(false);
            }
        }

        public void DisableMovement(float delayTime = 0)
        {
            if (delayTime > 0)
            {
                Observable.Timer(TimeSpan.FromSeconds(delayTime)).Subscribe(_ =>
                {
                    capsuleCollider.enabled = false;
                    __ecmMovement.enabled = false;
                }).AddTo(this);
            }
            else
            {
                capsuleCollider.enabled = false;
                __ecmMovement.enabled = false;
            }
        }

        Vector3 __backForceVec;
        float __backForceTimeStamp;
        float __backForceDuration;
        float __backForceMagnitude;
        public bool IsBackForceRunning => __backForceMagnitude > 0 && (Time.time - __backForceTimeStamp) <= __backForceDuration;

        public void SetBackForce(Vector3 backForce, float duration)
        {
            __backForceVec = backForce.normalized;
            __backForceTimeStamp = Time.time;
            __backForceDuration = duration;
            __backForceMagnitude = backForce.magnitude;
        }

        /// <summary>
        /// 
        /// </summary>
        public void StopBackForce()
        {
            __backForceVec = Vector3.zero;
            __backForceDuration = 0;
            __backForceMagnitude = 0;
        }

        void Start()
        {
            __spiderBrain.onFixedUpdate += OnFixedUpdateHandler;
            __spiderBrain.onUpdate += OnUpdateHandler;
        }

        public void OnFixedUpdateHandler()
        {
            if (IsBackForceRunning)
            {
                __ecmMovement.Move(__backForceVec * __backForceMagnitude, __backForceMagnitude);
                // __ecmMovement.useGravity = true;
            }
            else
            {
                // 이동 가능 체크 스텝
                var canMove1 = __spiderBrain.BB.IsSpawnFinished && !__spiderBrain.BB.IsDead && !__spiderBrain.BB.IsGroggy;
                var canMove2 = IsMoving && Vector3.Angle(core.forward.Vector2D(), ForwardVecCached) < 5;
                var canMove3 = !CheckReachToTargetPoint() && !__spiderBrain.ActionCtrler.CheckActionRunning();

                if (canMove1 && canMove2 && canMove3)
                    __ecmMovement.Move(ForwardVecCached * forwardSpeed, Time.fixedDeltaTime);
                else
                    __ecmMovement.Move(Vector3.zero, Time.fixedDeltaTime);
            }

            legAnimator.transform.SetPositionAndRotation(core.position, core.rotation);
        }

        public void OnUpdateHandler()
        {
            if (__spiderBrain.BB.IsDead)
                return;
            
            ForwardVecCached = (targetPoint - core.position).Vector2D().normalized;

            var canRotate1 = __spiderBrain.BB.IsSpawnFinished && !__spiderBrain.BB.IsDead && !__spiderBrain.BB.IsGroggy && ForwardVecCached != Vector3.zero;
            var canRotate2 =  IsMoving && (Vector3.Angle(core.forward.Vector2D(), ForwardVecCached) < 5 || __ecmMovement.forwardSpeed < 0.02f);
            var canRotate3 = !__spiderBrain.ActionCtrler.CheckActionRunning();

            //* 회전
            if (canRotate1 && canRotate2 && canRotate3)
                __ecmMovement.RotateTowards(ForwardVecCached, rotateSpeed);
        }

        [Header("Gizmos")]
        public bool drawGizmosEnabled;
        
        void OnDrawGizmos()
        {
            if (drawGizmosEnabled & IsMoving)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(core.position + Vector3.up * 0.23f, targetPoint + Vector3.up * 0.23f);
                Gizmos.DrawCube(targetPoint + Vector3.up * 0.23f, Vector3.one * 0.23f);
            }
        }
    }
}