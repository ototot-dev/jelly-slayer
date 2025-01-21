using System;
using System.Linq;
using MainTable;
using UniRx;
using UnityEngine;
using UnityEngine.Analytics;

namespace Game
{
    public class Etasphera42_Bullet : ProjectileMovement
    {
        [Header("Component")]
        public ParticleSystem bulletMissileFx;
        public ParticleSystem bulletExplosionFx;

        [Header("Parameter")]
        public ActionData actionData;
        Etasphera42_Brain __etasphera42_brain;
        GameObject __sourcePrefab;

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
                __etasphera42_brain = v as Etasphera42_Brain;
                __sourcePrefab = __etasphera42_brain.BB.action.bulletPrefab;
                Debug.Assert(__sourcePrefab != null);

                if (v.TryGetComponent<PawnActionController>(out var actionCtrler))
                {
                    actionData = actionCtrler.currActionContext.actionData;
                    Debug.Assert(actionData != null);
                }
            }).AddTo(this);

            onStopMove += () =>
            {
                bulletMissileFx.Stop();

                Observable.Timer(TimeSpan.FromSeconds(despawnWaitingTime)).Subscribe(_ =>
                {
                    Debug.Assert(IsDespawnPending);
                    ProjectilePoolingSystem.Instance.ReturnProjectile(this, __sourcePrefab);
                }).AddTo(this);
            };

            onHitSomething += (collider) =>
            {
                if (collider.TryGetComponent<PawnColliderHelper>(out var colliderHelper))
                {
                    if (colliderHelper.pawnBrain != null && colliderHelper.pawnBrain != __etasphera42_brain)
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