using System;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using Unity.Mathematics;
using UnityEngine;

namespace Game
{
    public class ProjectileMovement : MonoBehaviour
    {
        [Header("Component")]
        public Collider BodyCollider;
        public Collider sensorCollider;

        [Header("Parameter")]
        public Vector3 velocity;
        public Vector3 gravity = new(0, -30, 0);
        public bool gravityEnabled;
        public LayerMask sensorLayerMask;
        public float sensorEnabledTime = 0.1f;
        public float despawnWaitingTime = 0.1f;
        public float lifeTime = 1;
        public bool isLifeTimeOut;
        public bool destroyWhenLifeTimeOut = true;
        public float maxTravelDistance = -1f;
        public bool destoryWhenMaxTravelDistanceReached = false;
        public bool updateEnabled = true;
        public bool fixedUpdateEnabled = false;

        [Header("Pooling")]
        public string sourceName;
        public GameObject sourcePrefab;

        [Header("Emitter")]
        public ReactiveProperty<PawnBrainController> emitterBrain = new();

        public Action onStartMove;
        public Action onStopMove;
        public Action onLifeTimeOut;
        public Action<Collider> onHitSomething;
        public bool IsDespawnPending { get; private set; }

        void Awake()
        {
            AwakeInternal();
        }

        protected virtual void AwakeInternal()
        {
            if (BodyCollider != null) 
            {
                __rigidBody = BodyCollider.GetComponent<Rigidbody>();
                __rigidBody.isKinematic = true;
                BodyCollider.enabled = false;
            }

            if (sensorCollider != null)
            {
                sensorCollider.enabled = false;
                __sensorBoxCollider = sensorCollider as BoxCollider;
                __sensorSphereCollider = sensorCollider as SphereCollider;
                __sensorCapsuleCollider = sensorCollider as CapsuleCollider;
            }
        }

        void Start() 
        { 
            StartInternal(); 
        }

        protected virtual void StartInternal() {}

        protected virtual void OnUpdateHandler()
        {
            var canMove1 = Time.time > __moveStartTimeStamp && (__rigidBody == null || __rigidBody.isKinematic);
            var canMove2 = canMove1 && (maxTravelDistance <= 0f || (transform.position - __emittedPosition).magnitude < maxTravelDistance);

            if (canMove2)
                transform.position += Time.deltaTime * velocity;

            if (Time.time - __moveStartTimeStamp > sensorEnabledTime && sensorCollider != null && !sensorCollider.enabled)
            {
                if (__sensorBoxCollider != null)
                {
                    var currPosition = sensorCollider.transform.position + __sensorBoxCollider.center;
                    __traceCount = Physics.BoxCastNonAlloc(__lastTracedPosition, 0.5f * Vector3.Scale(__sensorBoxCollider.size, sensorCollider.transform.lossyScale), (currPosition - __lastTracedPosition).normalized, __traceHitsNonAlloc, sensorCollider.transform.rotation, (currPosition - __lastTracedPosition).magnitude, sensorLayerMask);
                    __lastTracedPosition = currPosition;
                }
                else if (__sensorSphereCollider != null)
                {
                    var currPosition = sensorCollider.transform.position + __sensorSphereCollider.center;
                    __traceCount = Physics.SphereCastNonAlloc(__lastTracedPosition, __sensorSphereCollider.radius * Mathf.Max(sensorCollider.transform.lossyScale.x, sensorCollider.transform.lossyScale.y, sensorCollider.transform.lossyScale.z), (currPosition- __lastTracedPosition).normalized, __traceHitsNonAlloc, (currPosition - __lastTracedPosition).magnitude, sensorLayerMask);
                    __lastTracedPosition = currPosition;
                }
                else if (__sensorCapsuleCollider != null)
                {
                    var halfHeight = Mathf.Max(0, 0.5f * __sensorCapsuleCollider.height * sensorCollider.transform.lossyScale.y - __sensorCapsuleCollider.radius * Mathf.Max(sensorCollider.transform.lossyScale.x, sensorCollider.transform.lossyScale.y, sensorCollider.transform.lossyScale.z));
                    var currPosition = sensorCollider.transform.position + __sensorCapsuleCollider.center;
                    __traceCount = Physics.CapsuleCastNonAlloc(currPosition - halfHeight * sensorCollider.transform.up, currPosition + halfHeight * sensorCollider.transform.up, __sensorCapsuleCollider.radius, (currPosition - __lastTracedPosition).normalized, __traceHitsNonAlloc, (currPosition - __lastTracedPosition).magnitude, sensorLayerMask);
                    __lastTracedPosition = currPosition;
                }

                for (int i = 0; i < __traceCount; i++)
                    onHitSomething?.Invoke(__traceHitsNonAlloc[i].collider);
            }

            if (!isLifeTimeOut && lifeTime > 0 && lifeTime < Time.time - __moveStartTimeStamp)
            {
                isLifeTimeOut = true;
                onLifeTimeOut?.Invoke();
                Stop(destroyWhenLifeTimeOut);
            }
        }

        protected virtual void OnFixedUpdateHandler() {}
        protected Rigidbody __rigidBody;
        protected BoxCollider __sensorBoxCollider;
        protected SphereCollider __sensorSphereCollider;
        protected CapsuleCollider __sensorCapsuleCollider;
        protected IDisposable __sensorDisposable;
        protected float __moveStartTimeStamp;
        protected Vector3 __emittedPosition;
        protected Vector3 __lastTracedPosition;
        protected int __traceCount;
        protected static RaycastHit[] __traceHitsNonAlloc = new RaycastHit[48];

        public void Pop(PawnBrainController emitter, Vector3 position, Vector3 impulse, Vector3 scale)
        {
            __moveStartTimeStamp = Time.time;
            __emittedPosition = position;
            __rigidBody.isKinematic = false;
            __rigidBody.useGravity = true;
            BodyCollider.enabled = true;
            transform.position = position;
            transform.rotation = Quaternion.identity;
            transform.localScale = scale;

            isLifeTimeOut = false;
            IsDespawnPending = false;
            emitterBrain.Value = emitter;

            Observable.NextFrame(FrameCountType.FixedUpdate).Subscribe(_ => __rigidBody.linearVelocity = impulse).AddTo(this);
            onStartMove?.Invoke();

            if (updateEnabled)
                Observable.EveryUpdate().TakeWhile(_ => !IsDespawnPending).Subscribe(_ => OnUpdateHandler()).AddTo(this);
            if (fixedUpdateEnabled)
                Observable.EveryFixedUpdate().TakeWhile(_ => !IsDespawnPending).Subscribe(_ => OnFixedUpdateHandler()).AddTo(this);
        }

        public void Pop(PawnBrainController emitter, float impulse, float scale = 1)
        {
            Pop(emitter, transform.position, transform.forward * impulse, Vector3.one * scale);
        }

        public void Go(PawnBrainController emitter, Vector3 position, Vector3 velocity, Vector3 scale)
        {
            //! Physics 시뮬레이션 상태면 안됨
            Debug.Assert(__rigidBody == null || __rigidBody.isKinematic);

            isLifeTimeOut = false;
            IsDespawnPending = false;
            __moveStartTimeStamp = Time.time;
            __emittedPosition = position;
            __lastTracedPosition = sensorCollider.transform.position;

            if ( __sensorBoxCollider != null) __lastTracedPosition += __sensorBoxCollider.center;
            else if ( __sensorSphereCollider != null) __lastTracedPosition += __sensorSphereCollider.center;
            else if ( __sensorCapsuleCollider != null) __lastTracedPosition += __sensorCapsuleCollider.center;

            if (velocity != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(velocity.normalized, Vector3.up);

            transform.position = position;
            transform.localScale = scale;
            emitterBrain.Value = emitter;
            this.velocity = velocity;

            onStartMove?.Invoke();

            if (updateEnabled)
                Observable.EveryUpdate().TakeWhile(_ => !IsDespawnPending).Subscribe(_ => OnUpdateHandler()).AddTo(this);
            if (fixedUpdateEnabled)
                Observable.EveryFixedUpdate().TakeWhile(_ => !IsDespawnPending).Subscribe(_ => OnFixedUpdateHandler()).AddTo(this);
        }

        public void Go(PawnBrainController emitter, float speed, float scale = 1)
        {
            Go(emitter, transform.position, transform.forward * speed, Vector3.one * scale);
        }

        public virtual void Stop(bool destroyAfterStopped, bool destroyImmediately = false)
        {
            onStopMove?.Invoke();

            if (sensorCollider != null)
                sensorCollider.enabled = false;

            if (__sensorDisposable != null)
            {
                __sensorDisposable.Dispose();
                __sensorDisposable = null;
            }

            if (destroyAfterStopped)
            {
                if (!destroyImmediately && despawnWaitingTime > 0)
                    Destroy(gameObject, despawnWaitingTime);
                else
                    Destroy(gameObject);
            }

            IsDespawnPending = true;
        }
    }
}