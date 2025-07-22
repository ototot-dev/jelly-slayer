using System;
using System.Linq;
using MainTable;
using UniRx;
using UnityEngine;

namespace Game
{
    public class Etasphera42_Frame : ProjectileMovement, IObjectPoolable
    {
#region ISpawnable/IMovable 구현
        void IObjectPoolable.OnGetFromPool() {}
        void IObjectPoolable.OnReturnedToPool() {}
#endregion

        [Header("Parameter")]
        public ActionData actionData;

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