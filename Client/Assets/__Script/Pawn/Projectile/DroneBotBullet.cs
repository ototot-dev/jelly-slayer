using System.Linq;
using MainTable;
using UniRx;
using UnityEngine;

namespace Game
{
    public class DroneBotBullet : ProjectileMovement
    {
        [Header("Param")]
        public ActionData actionData;

        protected override void StartInternal()
        {
            base.StartInternal();

            emitter.Where(v => v != null).Subscribe(v => 
            {
                if (v.TryGetComponent<PawnActionController>(out var actionCtrler))
                    actionData = actionCtrler.currActionContext.actionData;
                Debug.Assert(actionData != null);
            }).AddTo(this);

            onHitSomething += (collider) =>
            {
                if (collider.TryGetComponent<PawnColliderHelper>(out var colliderHelper) && colliderHelper.pawnBrain != null)
                {
                    if (colliderHelper.pawnBrain.PawnBB.common.pawnId == PawnId.Hero)
                    {
                        emitter.Value.PawnHP.Send(new PawnHeartPointDispatcher.DamageContext(this, emitter.Value, colliderHelper.pawnBrain, actionData, collider, false));
                        Stop(true);
                    }
                }
            };
        }
    }
}