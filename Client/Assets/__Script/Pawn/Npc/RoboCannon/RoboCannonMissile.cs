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
        void IObjectPoolable.OnGetFromPool() { }
        void IObjectPoolable.OnReturnedToPool() { }
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
                    Explode();
                else if (c.gameObject.TryGetComponent<PawnColliderHelper>(out var helper) && helper.pawnBrain.PawnBB.common.pawnId == PawnId.Hero)
                    Explode();
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
                    var helper = __traceCollidersNonAlloc[i].GetComponent<PawnColliderHelper>();
                    Debug.Assert(helper != null);

                    if (helper.pawnBrain != emitterBrain.Value && helper.pawnBrain.PawnBB.common.pawnName != "Slayer")
                        emitterBrain.Value.PawnHP.Send(new PawnHeartPointDispatcher.DamageContext(this, emitterBrain.Value, helper.pawnBrain, actionData, __traceCollidersNonAlloc[i], false));
                }
            }

            EffectManager.Instance.Show(explosionFx, transform.position + explosionOffset, Quaternion.identity, Vector3.one);

            Stop(false);
        }
    }
}