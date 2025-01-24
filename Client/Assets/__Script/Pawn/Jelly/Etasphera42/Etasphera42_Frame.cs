using System;
using System.Linq;
using MainTable;
using UniRx;
using UnityEngine;

namespace Game
{
    public class Etasphera42_Frame : ProjectileMovement
    {
        [Header("Parameter")]
        public ActionData actionData;

        protected override void StartInternal()
        {
            base.StartInternal();

            emitterBrain.Where(v => v != null).Subscribe(v => 
            {
                if (v.TryGetComponent<PawnActionController>(out var actionCtrler))
                {
                    sourcePrefab = (v as Etasphera42_Brain).BB.action.framePrefab;
                    actionData = actionCtrler.currActionContext.actionData;
                    Debug.Assert(actionData != null);
                }
            }).AddTo(this);

            onStopMove += () =>
            {
                Observable.Timer(TimeSpan.FromSeconds(despawnWaitingTime)).Subscribe(_ =>
                {
                    Debug.Assert(IsDespawnPending);
                    ProjectilePoolingSystem.Instance.ReturnProjectile(this, sourcePrefab);
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

                        Stop(false);
                    }
                }
                else
                {
                    Stop(false);
                }
            };
        }
    }
}