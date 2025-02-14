using System;
using System.Linq;
using MainTable;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Game
{
    public class SoldierMissile : ProjectileMovement, IObjectPoolable
    {
#region ISpawnable/IMovable 구현
        void IObjectPoolable.OnGetFromPool() {}
        void IObjectPoolable.OnReturnedToPool() 
        {
        }
#endregion

        [Header("Attachment")]
        public GameObject explosionFx;

        [Header("Parameter")]
        public ActionData actionData;
        // public float hoveringAccel;
        // public float 

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
                __onCollisionEnterDisposalbe = BodyCollider.OnCollisionEnterAsObservable().Subscribe(c =>
                {
                    if (LayerMask.LayerToName(c.gameObject.layer) == "Terrain")
                    {
                        __isGrounded = true;
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

            onHitSomething += (c) =>
            {
                if (__isGrounded && c.TryGetComponent<PawnColliderHelper>(out var helper) && helper.pawnBrain != emitterBrain.Value && helper.pawnBrain.PawnBB.common.pawnName == "Hero")
                    Explode();
            };

            onLifeTimeOut += () => { Explode(); };
        }

        IDisposable __onCollisionEnterDisposalbe;
        bool __isGrounded;

        protected override void OnUpdateHandler()
        {
            if ((Time.time - __moveStartTimeStamp) < sensorEnabledTime)
            {


                return;
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