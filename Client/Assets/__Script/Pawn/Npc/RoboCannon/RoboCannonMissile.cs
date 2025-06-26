using System;
using System.Linq;
using MainTable;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Game
{
    public class RoboCannonMissile : ProjectileMovement, IObjectPoolable
    {
#region ISpawnable/IMovable 구현
        void IObjectPoolable.OnGetFromPool() {}
        void IObjectPoolable.OnReturnedToPool() 
        {
            __onCollisionEnterDisposalbe?.Dispose();
            __onCollisionEnterDisposalbe = null;
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

            onStartMove += () =>
            {
                __onCollisionEnterDisposalbe = bodyCollider.OnCollisionEnterAsObservable().Subscribe(c =>
                {
                    if (c.gameObject.layer == LayerMask.NameToLayer("Floor"))
                    {
                        Explode();

                        __onCollisionEnterDisposalbe.Dispose();
                        __onCollisionEnterDisposalbe = null;
                    }
                    else if (c.gameObject.TryGetComponent<PawnColliderHelper>(out var helper) && helper.pawnBrain.PawnBB.common.pawnId == PawnId.Hero)
                    {
                        Explode();

                        __onCollisionEnterDisposalbe.Dispose();
                        __onCollisionEnterDisposalbe = null;
                    }
                }).AddTo(this);
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

        IDisposable __onCollisionEnterDisposalbe;

        void Explode()
        {
            Debug.Assert(base.emitterBrain.Value != null);

            __traceCollidersNonAlloc ??= new Collider[__maxTraceCount];
            __traceCount = Physics.OverlapSphereNonAlloc(__rigidBody.transform.position, explosionRadius, __traceCollidersNonAlloc, sensorLayerMask);

            if (__maxTraceCount > 0)
            {
                for (int i = 0; i < __traceCount; i++)
                {
                    var helper = __traceCollidersNonAlloc[i].GetComponent<PawnColliderHelper>();
                    Debug.Assert(helper != null);

                    if (helper.pawnBrain != emitterBrain.Value && helper.pawnBrain.PawnBB.common.pawnName != "Hero")
                        emitterBrain.Value.PawnHP.Send(new PawnHeartPointDispatcher.DamageContext(this, emitterBrain.Value, helper.pawnBrain, actionData, string.Empty, __traceCollidersNonAlloc[i], false));
                }
            }

            EffectManager.Instance.Show(explosionFx, bodyCollider.transform.position + explosionOffset, Quaternion.identity, Vector3.one);

            Stop(false);
        }
    }
}