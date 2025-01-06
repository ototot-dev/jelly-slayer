using System;
using ECM2;
using UniRx;
using UnityEngine;

namespace Game
{
    public class PawnMovement : MonoBehaviour
    {
        [Header("Component")]
        public Transform capsule;
        public Rigidbody capsuleRigidBody;
        public CapsuleCollider capsuleCollider;
        
        [Header("Movement")]
        public bool freezeMovement = false;
        public bool freezeRotation = false;
        public bool rootMotionEnabled = true;
        public float moveSpeed = 1f;
        public float moveAccel = 1f;
        public float moveBrake = 0f;
        public float rotateSpeed = 720f;
        public Vector3 gravity = Physics.gravity;
        public Vector3 moveVec = Vector3.zero;
        public Vector3 faceVec = Vector3.forward;
        public Vector3 CurrVelocity => __ecmMovement.velocity;
        public bool IsOnGround => __ecmMovement.isOnGround;
        public ECM2.CharacterMovement GetCharacterMovement() => __ecmMovement;
        public float GetVerticalImpulseOnJump(float jumpHeight) => Mathf.Sqrt(2f * jumpHeight * gravity.magnitude);
        public float GetEstimatedJumpingDuration(float jumpHeight) => 4f * GetVerticalImpulseOnJump(jumpHeight);
        
        void Awake()
        {
            __pawnBrain = GetComponent<PawnBrainController>();
            __pawnActionCtrler = GetComponent<PawnActionController>();
            __pawnStatusCtrler = GetComponent<PawnStatusController>();
            __ecmMovement = capsuleCollider.GetComponent<ECM2.CharacterMovement>();

            AwakeInternal();
        }

        protected virtual void AwakeInternal() {}
        protected PawnBrainController __pawnBrain;
        protected PawnActionController __pawnActionCtrler;
        protected PawnStatusController __pawnStatusCtrler;
        protected ECM2.CharacterMovement __ecmMovement;

        void Start()
        {
            __pawnBrain.onFixedUpdate += OnFixedUpdateHandler;
            __pawnBrain.onUpdate += OnUpdateHandler;
            
            StartInternal();
        }

        protected virtual void StartInternal() {}
        
        protected virtual void OnFixedUpdateHandler()
        {
            if (!__ecmMovement.enabled)
                return;

            if (IsPushForceRunning)
            {
                __ecmMovement.Move(__pushForceVec * __pushForceMagnitude, __pushForceMagnitude);
            }
            else
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
                    __ecmMovement.SimpleMove(moveSpeed * moveVec, moveSpeed, moveAccel, moveBrake, 1f, 1f, gravity, false, Time.fixedDeltaTime);
                }

                __ecmMovement.rotation *= __rootMotionRotation;
                ResetRootMotion();
            }
        }

        protected virtual void OnUpdateHandler()
        {   
            if (!__ecmMovement.enabled)
                return;

            if (faceVec != Vector3.zero)
                __ecmMovement.RotateTowards(faceVec, rotateSpeed * Time.deltaTime);
        }

        public void Teleport(Vector3 destination, bool stickToGround = true)
        {
            if (stickToGround)
                __ecmMovement.SetPosition(TerrainManager.GetTerrainPoint(destination));
            __ecmMovement.velocity = Vector3.zero;
            __ecmMovement.ClearAccumulatedForces();
        }

        public void Freeze()
        {   
            //* RootMotion에 의해서 축첟된 velocity값을 리셋함
            __ecmMovement.velocity = Vector3.zero;
            __ecmMovement.ClearAccumulatedForces();
            __freezeMovementFoeOneFrame = true;
        }

        protected bool __freezeMovementFoeOneFrame;

        public void FaceTo(Vector3 direction)
        {
            faceVec = direction;
            __ecmMovement.SetRotation(Quaternion.LookRotation(faceVec));
        }

        public void FaceAt(Vector3 target)
        {
            faceVec = (target - capsule.position).Vector2D().normalized;
            __ecmMovement.SetRotation(Quaternion.LookRotation(faceVec));
        }

        protected Vector3 __pushForceVec;
        protected float __pushForceTimeStamp;
        protected float __pushForceDuration;
        protected float __pushForceMagnitude;
        public bool IsPushForceRunning => __pushForceMagnitude > 0 && (Time.time - __pushForceTimeStamp) <= __pushForceDuration;

        public void SetPushForce(Vector3 pushForce, float duration)
        {
            __pushForceVec = pushForce.normalized;
            __pushForceTimeStamp = Time.time;
            __pushForceDuration = duration;
            __pushForceMagnitude = pushForce.magnitude;
        }

        public void StopPushForce()
        {
            __pushForceVec = Vector3.zero;
            __pushForceDuration = 0;
            __pushForceMagnitude = 0;
        }
        
        protected Vector3 __rootMotionPosition = Vector3.zero;
        protected Quaternion __rootMotionRotation = Quaternion.identity;
        IDisposable __setMovementEnabledDisposable;

        public void AddRootMotion(Vector3 deltaPosition, Quaternion deltaRotation)
        {
            __rootMotionPosition += deltaPosition;
            __rootMotionRotation *= deltaRotation;
            // __Logger.LogF(gameObject, nameof(AddRootMotion), "-", "position", position, "__rootMotionPosition", __rootMotionPosition);
        }

        public void ResetRootMotion()
        {
            __rootMotionPosition = Vector3.zero;
            __rootMotionRotation = Quaternion.identity;
        }

        public Vector3 GetRootMotionVelocity(float deltaTime)
        {
            return __rootMotionPosition / deltaTime;
        }

        public void SetMovementEnabled(bool newValue, float delayTime = 0)
        {
            if (__setMovementEnabledDisposable != null)
            {
                __setMovementEnabledDisposable.Dispose();
                __setMovementEnabledDisposable = null;
            }

            if (delayTime > 0)
            {
                __setMovementEnabledDisposable = __setMovementEnabledDisposable = Observable.Timer(TimeSpan.FromSeconds(delayTime)).Subscribe(_ =>
                {
                    capsuleCollider.GetComponent<Rigidbody>().isKinematic = !newValue;
                    capsuleCollider.enabled = newValue;
                    __ecmMovement.enabled = newValue;
                }).AddTo(this);
            }
            else
            {
                capsuleCollider.GetComponent<Rigidbody>().isKinematic = !newValue;
                capsuleCollider.enabled = newValue;
                __ecmMovement.enabled = newValue;
            }
        }
    }
}