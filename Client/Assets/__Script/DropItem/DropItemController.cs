using System;
using Retween.Rx;
using UnityEngine;
using UniRx;
using Unity.Linq;
using UniRx.Triggers;
using System.Linq;
using DG.Tweening;

namespace Game
{
    /// <summary>
    /// 
    /// </summary>
    public class DropItemController : MonoBehaviour
    {

        /// <summary>
        /// 
        /// </summary>
        public SpriteRenderer spriteRenderer;

        /// <summary>
        /// 
        /// </summary>
        public SphereCollider touchCollider;

        /// <summary>
        /// 
        /// </summary>
        public Rigidbody rigidBody;

        void Awake()
        {
            AwakeInternal();
        }

        protected void AwakeInternal()
        {
            touchCollider.enabled = false;
        }

        void Start()
        {
            StartInternal();

            //? test
            if (spawner == null)
                Pop(null, 1, Vector3.forward, 180);
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void StartInternal()
        {
            rigidBody.GetComponent<SphereCollider>().OnCollisionEnterAsObservable().Subscribe(c =>
            {
                // 땅과 접촉했다면 enablePickUpWaitingTime을 무시하고 바로 Trigger를 활성화한다.
                if (!touchCollider.enabled && c.gameObject.layer == LayerMask.NameToLayer("Terrain"))
                {
                    touchCollider.enabled = true;
                    onPickUpEnabled?.Invoke();
                }
            }).AddTo(this);

            if (pickUpOnTouch)
            {
                touchCollider.OnTriggerEnterAsObservable().Subscribe(c =>
                {
                    if (c.gameObject != spawner && ((1 << c.gameObject.layer) & LayerMask.GetMask("Pawn", "Hero")) != 0)
                        TryPick(c.gameObject);
                }).AddTo(this);
            }

            if (spriteRenderer != null)
            {
                var randOffset = Rand.Range11();

                Observable.EveryUpdate().Subscribe(_ =>
                {
                    if (GameContext.Instance.mainCameraCtrler != null)
                    {
                        spriteRenderer.transform.rotation = GameContext.Instance.mainCameraCtrler.SpriteLookRotation;
                        spriteRenderer.transform.rotation *= Quaternion.Euler(0, 0, Perlin.Noise(Time.time + randOffset) * 30);
                    }
                }).AddTo(this);
            }

            owner.Where(v => v != null).Subscribe(v =>
            {
                onPickUp?.Invoke(owner.Value);

                rigidBody.GetComponent<SphereCollider>().enabled = false;
                rigidBody.isKinematic = true;
                touchCollider.enabled = false;

                Destroy(gameObject, despawnWaitingTime);
            }).AddTo(this);
        }

        /// <summary>
        /// 
        /// </summary>
        public GameObject spawner;

        /// <summary>
        /// 
        /// </summary>
        public float despawnWaitingTime = 1;

        /// <summary>
        /// 
        /// </summary>
        public float pickUpEnabledWaitTime = 1;

        /// <summary>
        /// 
        /// </summary>
        public bool pickUpOnTouch = true;

        /// <summary>
        /// 
        /// </summary>
        public Action onPickUpEnabled;

        /// <summary>
        /// 
        /// </summary>
        public Action<GameObject> onPickUp;

        /// <summary>
        /// 
        /// </summary>
        public ReactiveProperty<GameObject> owner = new();

        /// <summary>
        /// 
        /// </summary>
        public bool IsOwned => owner.Value != null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="picker"></param>
        public virtual bool TryPick(GameObject picker)
        {
            if (this.owner.Value == null)
            {
                this.owner.Value = picker;
                __Logger.VerboseR(gameObject, nameof(TryPick), nameof(picker), picker.name);

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Vector2 poppingStrength = new Vector2(3, 4);

        /// <summary>
        /// 
        /// </summary>
        public Vector2 poppingUpwardsModifier = new Vector2(1, 2);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spawner"></param>
        /// <param name="waitingTime"></param>
        /// <param name="domainPoppingVec"></param>
        /// <param name="randAngle"></param>
        public virtual void Pop(GameObject spawner, float waitingTime, Vector3 domainPoppingVec, float randAngle)
        {
            this.spawner = spawner;

            Observable.Timer(TimeSpan.FromSeconds(waitingTime)).Subscribe(_ =>
            {
                rigidBody.isKinematic = false;

                rigidBody.AddExplosionForce(
                   UnityEngine.Random.Range(poppingStrength.x, poppingStrength.y),
                   transform.position - Quaternion.Euler(0, UnityEngine.Random.Range(-randAngle, randAngle), 0) * domainPoppingVec * 0.5f,
                   1,
                   UnityEngine.Random.Range(poppingUpwardsModifier.x, poppingUpwardsModifier.y),
                   ForceMode.Impulse
                   );

                if (!touchCollider.enabled)
                {
                    if (pickUpEnabledWaitTime > 0)
                    {
                        Observable.Timer(TimeSpan.FromSeconds(pickUpEnabledWaitTime)).Subscribe(_ =>
                        {
                            touchCollider.enabled = true;
                            onPickUpEnabled?.Invoke();
                        }).AddTo(this);
                    }
                    else
                    {
                        touchCollider.enabled = true;
                        onPickUpEnabled?.Invoke();
                    }
                }
            }).AddTo(this);
        }
    }
}