using System.Linq;
using MainTable;
using UniRx;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(WorkerMovement))]
    [RequireComponent(typeof(WorkerBlackboard))]
    [RequireComponent(typeof(WorkerAnimController))]
    [RequireComponent(typeof(WorkerActionController))]
    public class WorkerBrain : JellyManBrain
    {
        [Header("Debug")]
        public bool debugActionDisabled;

        public WorkerBlackboard BB { get; private set; }
        public WorkerMovement Movement { get; private set; }
        public WorkerAnimController AnimCtrler { get; private set; }
        public WorkerActionController ActionCtrler { get; private set; }

        protected override void AwakeInternal()
        {
            base.AwakeInternal();

            BB = GetComponent<WorkerBlackboard>();
            Movement = GetComponent<WorkerMovement>();
            AnimCtrler = GetComponent<WorkerAnimController>();
            ActionCtrler = GetComponent<WorkerActionController>();
        }

        float __lastComboAttackRateStepTimeStamp;
        MainTable.ActionData __counterActionData;
        MainTable.ActionData __combo1ActionData;
        MainTable.ActionData __combo2ActionData;
        MainTable.ActionData __combo3ActionData;

        protected override void StartInternal()
        {
            base.StartInternal();
            
            onTick += (deltaTick) =>
            {
                if (!BB.IsSpawnFinished || BB.IsDead || BB.IsGroggy || BB.IsDown || !BB.IsInCombat || BB.TargetPawn == null)
                    return;
                    
                ActionDataSelector.UpdateSelection(deltaTick);

                if (debugActionDisabled)
                    return;

                __combo1ActionData ??= ActionDataSelector.GetActionData("Attack#1");
                __combo2ActionData ??= ActionDataSelector.GetActionData("Attack#2");
                __combo3ActionData ??= ActionDataSelector.GetActionData("Attack#3");

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
                    else if (ActionCtrler.CurrActionName == "Attack#2") 
                    {
                        //* 2타 후에 3타 콤보 공격
                        ActionCtrler.SetPendingAction(__combo3ActionData.actionName);
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
                    
                if (string.IsNullOrEmpty(ActionCtrler.PendingActionData.Item1) && CheckTargetVisibility())
                {
                    ActionCtrler.SetPendingAction("Attack#1");
                    ActionDataSelector.ResetSelection(__combo1ActionData);
                }
            }
        }
    }
}