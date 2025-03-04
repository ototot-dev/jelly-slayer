using System;
using System.Linq;
using MainTable;
using UniRx;
using UnityEngine;

namespace Game
{
    public class SoldierMissile : ProjectileMovement, IObjectPoolable
    {
#region ISpawnable/IMovable 구현
        void IObjectPoolable.OnGetFromPool() {}
        void IObjectPoolable.OnReturnedToPool() 
        {
            __hoveringStartTimeStamp = -1f;
            __homingStartTimeStamp = -1f;
            __homingTargetBrain = null;
            emitterBrain.Value = null;
            reflectiveBrain.Value = null;
        }
#endregion

        [Header("Attachment")]
        public GameObject hitFx;

        [Header("Parameter")]
        public float hoveringDuration = 1f;
        public float homingAccel = 1f;
        public float homingMaxSpeed = 1f;
        public float homingRotateSpeed = 1f;

        protected override void AwakeInternal()
        {
            base.AwakeInternal();

            emitterBrain.Where(v => v != null).Subscribe(v => 
            {
                __homingTargetBrain = v.PawnBB.TargetBrain;
                __actionData ??= v.GetComponent<PawnActionController>().currActionContext.actionData;
                Debug.Assert(__actionData != null);
            }).AddTo(this);

            onStartMove += () =>
            {
                gravity = new(0f, -30f, 0f);
            };

            onStopMove += () =>
            {
                Observable.Timer(TimeSpan.FromSeconds(despawnWaitingTime)).Subscribe(_ =>
                {
                    Debug.Assert(IsDespawnPending);
                    ObjectPoolingSystem.Instance.ReturnObject(gameObject);
                }).AddTo(this);
            };

            onHitSomething += (c) =>
            {
                if (c.TryGetComponent<PawnColliderHelper>(out var helper))
                {
                    if (reflectiveBrain.Value != null)
                    {
                        if (helper.pawnBrain != emitterBrain.Value) return;
                        emitterBrain.Value.PawnHP.Send(new PawnHeartPointDispatcher.DamageContext(this, reflectiveBrain.Value, emitterBrain.Value, __actionData, __homingTargetBrain.bodyHitColliderHelper.pawnCollider, false));
                    }
                    else
                    {
                        if (helper.pawnBrain != __homingTargetBrain) return;
                        // emitterBrain.Value.PawnHP.Send(new PawnHeartPointDispatcher.DamageContext(this, emitterBrain.Value, __homingTargetBrain, __actionData, __homingTargetBrain.bodyHitColliderHelper.pawnCollider, false));
                        onReflected?.Invoke(helper.pawnBrain);

                        //* 가드 패링에 의해 반사된 경우엔 파괴
                        if (reflectiveBrain.Value != null) 
                            return;
                    }
                }

                EffectManager.Instance.Show(hitFx, transform.position, Quaternion.identity, Vector3.one);
                Stop(false);
            };

            onReflected += (b) =>
            {
                reflectiveBrain.Value = b;
                //* 진행 방향을 emitterBrain 쪽으로 변경
                transform.rotation = Quaternion.LookRotation(emitterBrain.Value.bodyHitColliderHelper.GetWorldCenter() - transform.position);
            };

            onLifeTimeOut += () => { Stop(false); };
        }

        float __hoveringStartTimeStamp;
        float __homingStartTimeStamp;
        PawnBrainController __homingTargetBrain;
        ActionData __actionData;

        protected override void OnUpdateHandler()
        {
            if (__hoveringStartTimeStamp <= 0f)
            {
                velocity += Time.deltaTime * gravity;

                //* 정점에 도달했다면 Hovering 시작함
                if (velocity.y < 0f)
                    __hoveringStartTimeStamp = Time.time;
            }
            else
            {
                if ((Time.time - __hoveringStartTimeStamp) < hoveringDuration)
                {
                    velocity.y = Perlin.Noise(Time.time - __hoveringStartTimeStamp);
                }
                else
                {
                    if (__homingStartTimeStamp <= 0f)
                    {
                        __homingStartTimeStamp = Time.time;
                        transform.rotation = Quaternion.LookRotation(__homingTargetBrain.bodyHitColliderHelper.GetWorldCenter() - transform.position);
                    }
                    else
                    {
                        if (reflectiveBrain.Value != null)
                            transform.rotation = transform.rotation.LerpAngleSpeed(Quaternion.LookRotation(emitterBrain.Value.bodyHitColliderHelper.GetWorldCenter() - transform.position), homingRotateSpeed, Time.deltaTime);
                        else
                            transform.rotation = transform.rotation.LerpAngleSpeed(Quaternion.LookRotation(__homingTargetBrain.bodyHitColliderHelper.GetWorldCenter() - transform.position), homingRotateSpeed, Time.deltaTime);
                    }
                    velocity = Mathf.Min(homingMaxSpeed, velocity.magnitude + homingAccel * Time.deltaTime) * transform.forward;
                }
            }

            base.OnUpdateHandler();
        }

        void Explode()
        {
            Debug.Assert(base.emitterBrain.Value != null);

            // __traceCollidersNonAlloc ??= new Collider[__maxTraceCount];
            // __traceCount = Physics.OverlapSphereNonAlloc(__rigidBody.transform.position, explosionRadius, __traceCollidersNonAlloc, sensorLayerMask);

            // if (__maxTraceCount > 0)
            // {
            //     for (int i = 0; i < __traceCount; i++)
            //     {
            //         var helper = __traceCollidersNonAlloc[i].GetComponent<PawnColliderHelper>();
            //         Debug.Assert(helper != null);

            //         if (helper.pawnBrain != emitterBrain.Value && helper.pawnBrain.PawnBB.common.pawnName != "Hero")
            //             emitterBrain.Value.PawnHP.Send(new PawnHeartPointDispatcher.DamageContext(this, emitterBrain.Value, helper.pawnBrain, actionData, __traceCollidersNonAlloc[i], false));
            //     }
            // }

            // EffectManager.Instance.Show(explosionFx, BodyCollider.transform.position + explosionOffset, Quaternion.identity, Vector3.one);

            Stop(false);
        }
    }
}