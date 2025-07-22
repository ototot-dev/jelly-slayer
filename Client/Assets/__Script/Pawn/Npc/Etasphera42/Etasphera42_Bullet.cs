using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Game
{
    public class Etasphera42_Bullet : ProjectileMovement
    {
        [Header("Component")]
        public ParticleSystem bulletMissileFx;
        public ParticleSystem bulletExplosionFx;

        [Header("Parameter")]
        public MainTable.ActionData actionData;

        protected override void AwakeInternal()
        {
            base.AwakeInternal();

            onStartMove += () =>
            {
                bulletMissileFx.Play();
            };
        }

        protected override void StartInternal()
        {
            base.StartInternal();

            emitterBrain.Where(v => v != null).Subscribe(v =>
            {
                actionData = v.GetComponent<PawnActionController>().currActionContext.actionData;
                Debug.Assert(actionData != null);
            }).AddTo(this);

            onStopMove += () =>
            {
                bulletMissileFx.Stop();

                Observable.Timer(TimeSpan.FromSeconds(despawnWaitingTime)).Subscribe(_ =>
                {
                    Debug.Assert(IsDespawnPending);
                    ObjectPoolingSystem.Instance.ReturnObject(gameObject);
                }).AddTo(this);
            };

            onHitSomething += (collider) =>
            {
                if (collider.TryGetComponent<PawnColliderHelper>(out var colliderHelper))
                {
                    if (colliderHelper.pawnBrain != null && colliderHelper.pawnBrain != emitterBrain.Value)
                    {
                        if (colliderHelper.pawnBrain.PawnBB.common.pawnId == PawnId.Hero)
                            emitterBrain.Value.PawnHP.Send(new PawnHeartPointDispatcher.DamageContext(this, emitterBrain.Value, colliderHelper.pawnBrain, actionData, collider, false));

                        bulletExplosionFx.Play();
                        Stop(false);
                    }
                }
                else
                {
                    bulletExplosionFx.Play();
                    Stop(false);
                }
            };
        }
    }
}