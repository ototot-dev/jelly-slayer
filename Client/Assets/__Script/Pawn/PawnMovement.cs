using System;
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
        public float rootMotionMultiplier = 1;
        public float moveSpeed = 1;
        public float moveAccel = 1;
        public float moveBrake = 0;
        public float rotateSpeed = 720;
        public Vector3 moveVec = Vector3.zero;
        public Vector3 faceVec = Vector3.forward;
        public Vector3 CurrVelocity => __ecmMovement.velocity;
        public Vector3 CurrGravity => __ecmMovement.gravity;
        public bool IsOnGround => __ecmMovement.isOnGround;
        
        void Awake()
        {
            __pawnBrain = GetComponent<PawnBrainController>();
            __actionCtrler = GetComponent<PawnActionController>();
            __buffCtrler = GetComponent<PawnStatusController>();
            __ecmMovement = capsuleCollider.GetComponent<ECM.Components.CharacterMovement>();
            AwakeInternal();
        }

        protected virtual void AwakeInternal() {}
        protected PawnBrainController __pawnBrain;
        protected PawnActionController __actionCtrler;
        protected PawnStatusController __buffCtrler;
        protected ECM.Components.CharacterMovement __ecmMovement;

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
                __ecmMovement.useGravity = true;
            }
            else
            {
                ConsumeRootMotion();
                ResetRootMotion();

                if (moveVec != Vector3.zero)
                {
                    if (freezeRotation)
                        __ecmMovement.Move(moveVec * moveSpeed, moveSpeed, moveAccel, moveBrake, 0.1f, 0.2f);
                    else
                        __ecmMovement.Move(moveVec * moveSpeed, moveSpeed);
                }
                else
                {
                    __ecmMovement.Move(Vector3.zero, moveSpeed, 0, moveBrake, 1, 1);
                }
            }
        }

        protected virtual void OnUpdateHandler()
        {   
            if (!__ecmMovement.enabled)
                return;

            if (faceVec != Vector3.zero)
                __ecmMovement.Rotate(faceVec, rotateSpeed);
        }

        public void Teleport(Vector3 destination)
        {
            capsule.position = TerrainManager.GetTerrainPoint(destination);
        }

        public void MoveTo(Vector3 destination)
        {
            capsuleRigidBody.MovePosition(destination);
        }

        public void FaceTo(Vector3 direction)
        {
            faceVec = direction;
            capsuleRigidBody.MoveRotation(Quaternion.LookRotation(faceVec));
        }

        public void FaceAt(Vector3 position)
        {
            faceVec = (position - capsule.position).Vector2D().normalized;
            capsuleRigidBody.MoveRotation(Quaternion.LookRotation(faceVec));
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

        public void ConsumeRootMotion()
        {
            if (!rootMotionEnabled)
                return;

            if (__rootMotionPosition != Vector3.zero && __rootMotionRotation != Quaternion.identity)
                __pawnBrain.coreColliderHelper.pawnRigidbody.Move(__pawnBrain.coreColliderHelper.transform.position + rootMotionMultiplier * __rootMotionPosition, __pawnBrain.coreColliderHelper.transform.rotation * __rootMotionRotation);
            else if (__rootMotionPosition != Vector3.zero)
                __pawnBrain.coreColliderHelper.pawnRigidbody.MovePosition(__pawnBrain.coreColliderHelper.transform.position + rootMotionMultiplier * __rootMotionPosition);
            else if (__rootMotionRotation != Quaternion.identity)
                __pawnBrain.coreColliderHelper.pawnRigidbody.MoveRotation(__pawnBrain.coreColliderHelper.transform.rotation * __rootMotionRotation);
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