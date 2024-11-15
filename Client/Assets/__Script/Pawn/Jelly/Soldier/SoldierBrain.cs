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

        float __lastComboAttackRateStepTimeStamp;
        MainTable.ActionData __counterActionData;
        MainTable.ActionData __comboActionData;

        protected override void StartInternal()
        {
            base.StartInternal();
            
            onTick += (deltaTick) =>
            {
                if (!BB.IsSpawnFinished || BB.IsDead || BB.IsStunned || BB.IsDown || !BB.IsInCombat || BB.TargetPawn == null)
                    return;
                    
                ActionDataSelector.UpdateSelection(deltaTick);

                //* 콤보 공격
                __comboActionData ??= ActionDataSelector.GetActionData("Attack#1");
                if (ActionDataSelector.CheckExecutable(__comboActionData) && Time.time - PawnHP.LastDamageTimeStamp >= 1f && Time.time - __lastComboAttackRateStepTimeStamp >= 1f)
                {
                    if (string.IsNullOrEmpty(ActionCtrler.PendingActionData.Item1) && ActionDataSelector.EvaluateSelection(__comboActionData, -1f, 1f) && CheckTargetVisibility())
                    {
                        ActionDataSelector.ResetSelection(__comboActionData);
                        ActionCtrler.SetPendingAction(__comboActionData.actionName);
                    }
                    else
                    {
                        __lastComboAttackRateStepTimeStamp = Time.time;
                        ActionDataSelector.BoostSelection(__comboActionData, BB.selection.comboAttackRateStep);
                    }
                }
            };
        }

        protected override void DamageReceiverHandler(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            base.DamageReceiverHandler(ref damageContext);

            if (damageContext.actionResult == ActionResults.Blocked)
            {   
                __counterActionData ??= ActionDataSelector.GetActionData("Counter");
                if (string.IsNullOrEmpty(ActionCtrler.PendingActionData.Item1) && ActionDataSelector.EvaluateSelection(__counterActionData, -1f, 1f) && CheckTargetVisibility())
                {
                    ActionDataSelector.ResetSelection(__counterActionData);
                    ActionCtrler.SetPendingAction("Counter");
                }
                else
                {
                    ActionDataSelector.BoostSelection(__counterActionData, BB.selection.counterAttackRateStep);
                }
            }
        }
    }
}
