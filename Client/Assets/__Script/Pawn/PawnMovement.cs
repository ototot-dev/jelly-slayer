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
        public float rootMotionMultiplier = 1f;
        public float moveSpeed = 1f;
        public float moveAccel = 1f;
        public float moveBrake = 0f;
        public float rotateSpeed = 720f;
        public Vector3 gravity = Physics.gravity;
        public Vector3 moveVec = Vector3.zero;
        public Vector3 faceVec = Vector3.forward;
        public Vector3 CurrVelocity => __ecmMovement.velocity;
        public bool IsOnGround => __ecmMovement.isOnGround;
        
        void Awake()
        {
            __pawnBrain = GetComponent<PawnBrainController>();
            __actionCtrler = GetComponent<PawnActionController>();
            __buffCtrler = GetComponent<PawnStatusController>();
            __ecmMovement = capsuleCollider.GetComponent<ECM2.CharacterMovement>();

            AwakeInternal();
        }

        protected virtual void AwakeInternal() {}
        protected PawnBrainController __pawnBrain;
        protected PawnActionController __actionCtrler;
        protected PawnStatusController __buffCtrler;
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

        public void Teleport(Vector3 destination)
        {
            __ecmMovement.SetPosition(TerrainManager.GetTerrainPoint(destination));
            __ecmMovement.velocity = Vector3.zero;
            __ecmMovement.ClearAccumulatedForces();
        }

        public void Freeze()
        {
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

        public void AddRootMotion(Vector3 position, Quaternion rotation)
        {
            __rootMotionPosition += position;
            __rootMotionRotation *= rotation;
            // __Logger.LogF(gameObject, nameof(AddRootMotion), "-", "position", position, "__rootMotionPosition", __rootMotionPosition);
        }

        public void ResetRootMotion()
        {
            __rootMotionPosition = Vector3.zero;
            __rootMotionRotation = Quaternion.identity;
        }

        public Vector3 GetRootMotionVelocity(float deltaTime)
        {
            Debug.Assert(deltaTime > 0f);
            return rootMotionMultiplier / deltaTime * __rootMotionPosition;
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