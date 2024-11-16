using System.Linq;
using MainTable;
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
        MainTable.ActionData __combo1ActionData;
        MainTable.ActionData __combo2ActionData;

        protected override void StartInternal()
        {
            base.StartInternal();
            
            onTick += (deltaTick) =>
            {
                if (!BB.IsSpawnFinished || BB.IsDead || BB.IsStunned || BB.IsDown || !BB.IsInCombat || BB.TargetPawn == null)
                    return;
                    
                ActionDataSelector.UpdateSelection(deltaTick);

                if (debugActionDisabled)
                    return;

                __combo1ActionData ??= ActionDataSelector.GetActionData("Attack#1");
                __combo2ActionData ??= ActionDataSelector.GetActionData("Attack#2");

                if (!ActionCtrler.CheckActionPending() && ActionCtrler.CheckActionRunning() && ActionCtrler.CanInterruptAction())
                {
                    if (ActionCtrler.CurrActionName == "Counter")
                    {
                        //* 반격 후에 1타 공격
                        if (ActionDataSelector.EvaluateSelection(__combo1ActionData, -1f, 1f))
                        {
                            ActionDataSelector.ResetSelection(__combo1ActionData);
                            ActionCtrler.SetPendingAction(__combo1ActionData.actionName);
                            ActionCtrler.CancelAction(false);
                        }
                    }
                    else if (ActionCtrler.CurrActionName == "Attack#1") 
                    {
                        //* 1타 후에 2타 콤보 공격
                        ActionCtrler.SetPendingAction(__combo2ActionData.actionName);
                        ActionCtrler.CancelAction(false);
                    }
                }

                if (ActionCtrler.CheckActionPending() || ActionCtrler.CheckActionRunning())
                {
                    __lastComboAttackRateStepTimeStamp = Time.time;
                }
                else if (ActionDataSelector.CheckExecutable(__combo1ActionData) && Time.time - PawnHP.LastDamageTimeStamp >= 1f && Time.time - __lastComboAttackRateStepTimeStamp >= 1f)
                {
                    if (ActionDataSelector.EvaluateSelection(__combo1ActionData, -1f, 1f) && CheckTargetVisibility())
                    {
                        ActionDataSelector.ResetSelection(__combo1ActionData);
                        ActionCtrler.SetPendingAction(__combo1ActionData.actionName);
                    }
                    else
                    {
                        __lastComboAttackRateStepTimeStamp = Time.time;
                        ActionDataSelector.BoostSelection(__combo1ActionData, BB.selection.comboAttackRateStep);
                    }
                }
            };
        }

        protected override void DamageReceiverHandler(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            base.DamageReceiverHandler(ref damageContext);

            if (damageContext.actionResult == ActionResults.Blocked)
            {   
                if (debugActionDisabled)
                    return;
                    
                __counterActionData ??= ActionDataSelector.GetActionData("Counter");
                if (string.IsNullOrEmpty(ActionCtrler.PendingActionData.Item1) && ActionDataSelector.EvaluateSelection(__counterActionData, -1f, 1f) && CheckTargetVisibility())
                {
                    ActionDataSelector.ResetSelection(__counterActionData);
                    ActionDataSelector.BoostSelection(__combo1ActionData, BB.selection.comboAttackRateBoostAfterCounterAttack);
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
