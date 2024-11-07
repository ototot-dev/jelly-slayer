using System;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Game
{
    public class ProjectileMovement : MonoBehaviour
    {
        [Header("Parameter")]
        public Vector3 velocity;
        public Vector3 gravity = new Vector3(0, -30, 0);
        public bool gravityEnabled;
        public float lifeTime = 1;
        public float sensorEnabledTime = 0.1f;
        public float despawnWaitingTime = 0.1f;
        public bool destroyWhenLifeTimeOut = true;
        public bool updateEnabled = true;
        public bool fixedUpdateEnabled = false;

        [Header("Component")]
        public Rigidbody rigidBody;
        public Collider rigidBodyCollider;
        public Collider sensorCollider;

        [Header("Emitter")]
        public ReactiveProperty<GameObject> emitter = new();

        [Header("Life-Time")]
        public bool isLifeTimeOut;
        public Action onLifeTimeOut;
        public Action<GameObject> onHitSomething;
        public bool IsPendingDestroy { get; private set; }

        void Awake()
        {
            AwakeInternal();
        }

        protected virtual void AwakeInternal()
        {
            if (rigidBody != null) 
                rigidBody.isKinematic = true;
            if (rigidBodyCollider != null) 
                rigidBodyCollider.enabled = false;
            if (sensorCollider != null) 
                sensorCollider.enabled = false;
        }

        void Start()
        {
            StartInternal();
        }

        protected virtual void StartInternal()
        {
            if (updateEnabled)
                Observable.EveryUpdate().TakeWhile(_ => !IsPendingDestroy).Subscribe(_ => OnUpdateHandler()).AddTo(this);
            if (fixedUpdateEnabled)
                Observable.EveryFixedUpdate().TakeWhile(_ => !IsPendingDestroy).Subscribe(_ => OnFixedUpdateHandler()).AddTo(this);
        }

        protected virtual void OnUpdateHandler()
        {
            if (Time.time - __moveStartTimeStamp > sensorEnabledTime && sensorCollider != null && !sensorCollider.enabled)
            {
                sensorCollider.enabled = true;
                __sensorDisposable = sensorCollider.OnTriggerEnterAsObservable().Subscribe(c => onHitSomething?.Invoke(c.gameObject)).AddTo(this);
            }

            if (!isLifeTimeOut && lifeTime > 0 && lifeTime < Time.time - __moveStartTimeStamp)
            {
                isLifeTimeOut = true;
                onLifeTimeOut?.Invoke();

                Stop(destroyWhenLifeTimeOut);
            }
        }

        protected virtual void OnFixedUpdateHandler()
        {
        }

        protected IDisposable __sensorDisposable;
        protected float __moveStartTimeStamp;

        public void Pop(GameObject emitter, Vector3 position, Vector3 impulse, Vector3 scale)
        {
            __moveStartTimeStamp = Time.time;

            rigidBody.isKinematic = false;
            rigidBody.useGravity = true;
            rigidBodyCollider.enabled = true;
            transform.position = position;
            transform.localScale = scale;
            this.emitter.Value = emitter;

            Observable.NextFrame(FrameCountType.FixedUpdate).Subscribe(_ =>
            {
                rigidBody.linearVelocity = impulse;
            }).AddTo(this);
        }

        public void Pop(GameObject emitter, float impulse, float scale = 1)
        {
            Pop(emitter, transform.position, transform.forward * impulse, Vector3.one * scale);
        }

        public void Go(GameObject emitter, Vector3 position, Vector3 velocity, Vector3 scale)
        {
            //! Physics 시뮬레이션 상태면 안됨
            Debug.Assert(rigidBody == null || rigidBody.isKinematic);

            __moveStartTimeStamp = Time.time;

            if (velocity != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(velocity.normalized, Vector3.up);

            transform.position = position;
            transform.localScale = scale;
            this.emitter.Value = emitter;
            this.velocity = velocity;
        }

        public void Go(GameObject emitter, float speed, float scale = 1)
        {
            Go(emitter, transform.position, transform.forward * speed, Vector3.one * scale);
        }

        public virtual void Stop(bool destroyAfterStopped, bool destroyImmediately = false)
        {
            __Logger.WarningF(gameObject, nameof(Stop), "debugging~", "destroyAfterStopped", destroyAfterStopped, "destroyImmediately", destroyImmediately);

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

                IsPendingDestroy = true;
            }
        }
    }
}