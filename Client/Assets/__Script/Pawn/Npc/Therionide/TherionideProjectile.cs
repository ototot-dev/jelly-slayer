using System;
using System.Linq;
using MainTable;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Game
{
    public class TherionideProjectile : ProjectileMovement, IObjectPoolable
    {
        #region ISpawnable/IMovable 구현
        void IObjectPoolable.OnGetFromPool() { }
        void IObjectPoolable.OnReturnedToPool()
        {
            emitterBrain.Value = null;
            reflectiveBrain.Value = null;
        }
        #endregion

        [Header("Attachment")]
        public GameObject explosionFx;

        [Header("Parameter")]
        public float explosionRadius = 1f;
        public Vector3 explosionOffset;
        public ActionData actionData;

        protected override void AwakeInternal()
        {
            base.AwakeInternal();

            emitterBrain.Where(v => v != null).Subscribe(v =>
            {
                actionData = v.GetComponent<PawnActionController>().currActionContext.actionData;
                Debug.Assert(actionData != null);
            }).AddTo(this);

            onHitSomething += (c) =>
            {
                if (c.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
                {
                    Explode();
                }
                else if (c.gameObject.TryGetComponent<PawnColliderHelper>(out var helper))
                {
                    if (reflectiveBrain.Value != null)
                    {
                        if (helper.pawnBrain != emitterBrain.Value)
                            return;

                        reflectiveBrain.Value.PawnHP.Send(new PawnHeartPointDispatcher.DamageContext(this, reflectiveBrain.Value, emitterBrain.Value, actionData, string.Empty, emitterBrain.Value.bodyHitColliderHelper.pawnCollider, false));
                        Explode();
                    }
                    else if (helper.pawnBrain.PawnBB.common.pawnId == PawnId.Hero)
                    {
                        emitterBrain.Value.PawnHP.Send(new PawnHeartPointDispatcher.DamageContext(this, emitterBrain.Value, helper.pawnBrain, actionData, string.Empty, helper.pawnBrain.bodyHitColliderHelper.pawnCollider, false));

                        Observable.NextFrame().Subscribe(_ =>
                        {
                            if (reflectiveBrain.Value == null)
                                Explode();
                        }).AddTo(this);
                    }
                }
            };

            onReflected += (b) =>
            {
                reflectiveBrain.Value = b;
                //* 진행 방향을 emitterBrain 쪽으로 변경
                velocity = velocity.magnitude * (Quaternion.LookRotation(emitterBrain.Value.bodyHitColliderHelper.GetWorldCenter() - transform.position) * Vector3.forward);
            };

            onStopMove += () =>
            {
                Observable.Timer(TimeSpan.FromSeconds(despawnWaitingTime)).Subscribe(_ =>
                {
                    Debug.Assert(IsDespawnPending);
                    ObjectPoolingSystem.Instance.ReturnObject(gameObject);
                }).AddTo(this);
            };

            onLifeTimeOut += () => { Explode(); };
        }

        void Explode()
        {
            Debug.Assert(base.emitterBrain.Value != null);

            __traceCollidersNonAlloc ??= new Collider[__maxTraceCount];
            __traceCount = Physics.OverlapSphereNonAlloc(transform.position, explosionRadius, __traceCollidersNonAlloc, sensorLayerMask);

            if (__maxTraceCount > 0)
            {
                for (int i = 0; i < __traceCount; i++)
                {
                    if (!__traceCollidersNonAlloc[i].TryGetComponent<PawnColliderHelper>(out var colliderHelper))
                        continue;

                    if (colliderHelper.pawnBrain != emitterBrain.Value && colliderHelper.pawnBrain.PawnBB.common.pawnName != "Slayer")
                        emitterBrain.Value.PawnHP.Send(new PawnHeartPointDispatcher.DamageContext(this, emitterBrain.Value, colliderHelper.pawnBrain, actionData, string.Empty, __traceCollidersNonAlloc[i], false));
                }
            }

            EffectManager.Instance.Show(explosionFx, transform.position + explosionOffset, Quaternion.identity, Vector3.one);

            Stop(false);
        }
    }
}