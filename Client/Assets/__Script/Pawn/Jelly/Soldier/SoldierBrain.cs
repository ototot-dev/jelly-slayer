using System.Linq;
using UniRx;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(SoldierMovement))]
    [RequireComponent(typeof(SoldierBlackboard))]
    [RequireComponent(typeof(SoldierAnimController))]
    [RequireComponent(typeof(SoldierActionController))]
    public class SoldierBrain : JellyManBrain
    {
        [Header("Debug")]
        public bool debugActionDisabled;

        public SoldierBlackboard BB { get; private set; }
        public SoldierMovement Movement { get; private set; }
        public SoldierAnimController AnimCtrler { get; private set; }
        public SoldierActionController ActionCtrler { get; private set; }

        protected override void AwakeInternal()
        {
            base.AwakeInternal();

            BB = GetComponent<SoldierBlackboard>();
            Movement = GetComponent<SoldierMovement>();
            AnimCtrler = GetComponent<SoldierAnimController>();
            ActionCtrler = GetComponent<SoldierActionController>();
        }

        protected override void StartInternal()
        {
            base.StartInternal();
            
            onTick += (deltaTick) =>
            {
                if (!BB.IsSpawnFinished || BB.IsDead || BB.IsStunned || BB.IsDown || !BB.IsInCombat || BB.TargetPawn == null)
                    return;
                    
                ActionDataSelector.UpdateSelection(deltaTick);

                // if (!ActionCtrler.CheckActionRunning() && string.IsNullOrEmpty(ActionCtrler.PendingActionData.Item1) && !BuffCtrler.CheckBuff(BuffTypes.Staggered) && CheckTargetVisibility())
                // {
                //     var selection = ActionDataSelector.PickSelection(BB.TargetBrain.coreColliderHelper.GetApproachDistance(coreColliderHelper.transform.position), BB.stat.stamina.Value);
                //     if (selection != null)
                //         ActionCtrler.SetPendingAction(selection.actionName);
                // }
            };
        }

        protected override void DamageReceiverHandler(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            base.DamageReceiverHandler(ref damageContext);

            if (damageContext.actionResult == ActionResults.Blocked)
            {   
                //* 반격
                if (string.IsNullOrEmpty(ActionCtrler.PendingActionData.Item1) && CheckTargetVisibility())
                {
                    var selection = ActionDataSelector.PickSelection(0, 100);
                    if (selection != null)
                        ActionCtrler.SetPendingAction(selection.actionName);
                }
            }
        }
    }
}
