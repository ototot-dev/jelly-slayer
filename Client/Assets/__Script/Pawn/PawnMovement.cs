using System;
using Cinemachine.Utility;
using FIMSpace;
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
        public Collider[] ignoredColliders;
        
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
        public bool CheckRootMotionNonZero() => __rootMotionPosition.sqrMagnitude > 0f;
        public bool CheckRootMotionZero() => __rootMotionPosition.sqrMagnitude <= 0f;
        
        void Awake()
        {
            __pawnBrain = GetComponent<PawnBrainController>();
            __pawnActionCtrler = GetComponent<PawnActionController>();
            __pawnStatusCtrler = GetComponent<PawnStatusController>();
            __ecmMovement = capsuleCollider.GetComponent<ECM2.CharacterMovement>();

            //* 자기 자신에게 속한 충돌체는 __ecmMovement와 충돌하지 않도록 등록해줌
            foreach (var c in ignoredColliders)
                __ecmMovement.IgnoreCollision(c);

            AwakeInternal();
        }

        protected virtual void AwakeInternal() {}
        protected PawnBrainController __pawnBrain;
        protected PawnActionController __pawnActionCtrler;
        protected PawnStatusController __pawnStatusCtrler;
        protected ECM2.CharacterMovement __ecmMovement;
        protected Vector3 __rootMotionPosition;
        IDisposable __movementEnabledDisposable;

        void Start()
        {
            __pawnBrain.onUpdate += OnUpdateHandler;
            __pawnBrain.onFixedUpdate += OnFixedUpdateHandler;

            StartInternal();
        }

        protected virtual void StartInternal() {}
        
        protected virtual void OnFixedUpdateHandler()
        {
            if (!__ecmMovement.enabled)
                return;

            if (CheckRootMotionNonZero())
            {
                if (!__ecmMovement.isGrounded)
                {
                    __ecmMovement.velocity += Time.fixedDeltaTime * gravity;
                    __ecmMovement.Move(Time.fixedDeltaTime);
                }
            }
            else
            {
                if (__ecmMovement.isGrounded || gravity.AlmostZero())
                {
                    __ecmMovement.SimpleMove(moveSpeed * moveVec, moveSpeed, moveAccel, moveBrake, 1f, 1f, gravity, false, Time.fixedDeltaTime);
                }
                else
                {
                    __ecmMovement.velocity += Time.fixedDeltaTime * gravity;
                    __ecmMovement.Move(Time.fixedDeltaTime);
                }
            }
        }

        protected virtual void OnUpdateHandler()
        {   
            if (!__ecmMovement.enabled)
                return;

            if (faceVec != Vector3.zero)
                __ecmMovement.RotateTowards(faceVec, rotateSpeed * Time.deltaTime);

            DampenRootMotion();
        }

        public void Teleport(Vector3 destination, bool stickToGround = true)
        {
            if (stickToGround) __ecmMovement.SetPosition(TerrainManager.GetTerrainPoint(destination));
            __ecmMovement.velocity = Vector3.zero;
            __ecmMovement.ClearAccumulatedForces();
        }

        public void FaceTo(Vector3 direction)
        {
            faceVec = direction.normalized;
            __ecmMovement.SetRotation(Quaternion.LookRotation(faceVec));

            if (!freezeRotation)
                moveVec = faceVec;
        }

        public void FaceAt(Vector3 target)
        {
            faceVec = (target - capsule.position).Vector2D().normalized;
            __ecmMovement.SetRotation(Quaternion.LookRotation(faceVec));
            
            if (!freezeRotation)
                moveVec = faceVec;
        }
        
        public void AddRootMotion(Vector3 deltaPosition, Quaternion deltaRotation, float deltaTime)
        {
            var preserveVelocityY = __ecmMovement.velocity.y;
            __ecmMovement.Move(deltaPosition / deltaTime, deltaTime);
            __ecmMovement.velocity = Vector3.zero.AdjustY(preserveVelocityY);
            __ecmMovement.rotation *= deltaRotation;
            __rootMotionPosition += deltaPosition;
            // __Logger.LogF(gameObject, nameof(AddRootMotion), "-", "position", position, "__rootMotionPosition", __rootMotionPosition);
        }

        public void DampenRootMotion(float dampingFactor = 0.5f)
        {
            __rootMotionPosition = Vector3.Lerp(Vector3.zero, __rootMotionPosition, dampingFactor);
            if (__rootMotionPosition.sqrMagnitude < MathExtension.EPSILON_LENGTH)
                __rootMotionPosition = Vector3.zero;
        }

        public void SetMovementEnabled(bool newValue, float delayTime = 0)
        {
            if (__movementEnabledDisposable != null)
            {
                __movementEnabledDisposable.Dispose();
                __movementEnabledDisposable = null;
            }

            if (delayTime > 0)
            {
                __movementEnabledDisposable = __movementEnabledDisposable = Observable.Timer(TimeSpan.FromSeconds(delayTime)).Subscribe(_ =>
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