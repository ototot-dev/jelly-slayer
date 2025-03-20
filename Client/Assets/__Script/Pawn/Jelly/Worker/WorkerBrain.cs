using System;
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
    public class WorkerBrain : JellyHumanoidBrain
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
        MainTable.ActionData __chargeActionData;
        MainTable.ActionData __rollingActionData;
        MainTable.ActionData __combo1ActionData;
        MainTable.ActionData __combo2ActionData;
        MainTable.ActionData __combo3ActionData;
        MainTable.ActionData __combo4ActionData;

        protected override void StartInternal()
        {
            base.StartInternal();
            
            BB.decision.currDecision.Subscribe(v =>
            {
                Movement.moveSpeed = v == Decisions.Spacing ? BB.body.walkSpeed : BB.body.runSpeed;
            }).AddTo(this);

            onTick += (deltaTick) =>
            {
                if (!BB.IsSpawnFinished || BB.IsDead || BB.IsGroggy || BB.IsDown || !BB.IsInCombat || BB.TargetPawn == null)
                    return;

                if (debugActionDisabled)
                    return;

                __counterActionData ??= ActionDataSelector.GetActionData("Counter");
                __chargeActionData ??= ActionDataSelector.GetActionData("Charge");
                __rollingActionData ??= ActionDataSelector.GetActionData("Rolling");
                __combo1ActionData ??= ActionDataSelector.GetActionData("Attack#1");
                __combo2ActionData ??= ActionDataSelector.GetActionData("Attack#2");
                __combo3ActionData ??= ActionDataSelector.GetActionData("Attack#3");
                __combo4ActionData ??= ActionDataSelector.GetActionData("Attack#4");

                if (!ActionCtrler.CheckActionPending() && ActionCtrler.CheckActionRunning() && ActionCtrler.CanInterruptAction())
                {
                    if (ActionCtrler.CurrActionName == "Counter")
                    {
                        //* 반격 후에 콤보 3타 공격
                        ActionCtrler.SetPendingAction(__combo3ActionData.actionName);
                        ActionCtrler.CancelAction(false);
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
                    else if (ActionCtrler.CurrActionName == "Attack#3") 
                    {
                        //* 3타 후에 4타 콤보 공격
                        ActionCtrler.SetPendingAction(__combo4ActionData.actionName);
                        ActionCtrler.CancelAction(false);
                    }
                    else if (ActionCtrler.CurrActionName == "Attack#4") 
                    {
                        //* 롤링 회피
                        ActionCtrler.SetPendingAction(__rollingActionData.actionName);
                        ActionCtrler.CancelAction(false);
                    }
                }

                if (ActionCtrler.CheckActionPending() || ActionCtrler.CheckActionRunning())
                {
                    __lastComboAttackRateStepTimeStamp = Time.time;
                }
                else if (Time.time - PawnHP.LastDamageTimeStamp >= 1f && Time.time - __lastComboAttackRateStepTimeStamp >= 1f)
                {
                    // var distanceConstraint = BB.TargetBrain != null ? BB.TargetBrain.coreColliderHelper.GetApproachDistance(coreColliderHelper.transform.position) : -1f;
                    // if (ActionDataSelector.CheckCoolTime(__combo1ActionData) && ActionDataSelector.EvaluateSelection(__combo1ActionData, 0f, 1f) && CheckTargetVisibility())
                    // {
                    //     ActionDataSelector.ResetCoolTime(__combo1ActionData);
                    //     ActionCtrler.SetPendingAction(__combo1ActionData.actionName);
                    // }
                    // else if (ActionDataSelector.CheckCoolTime(__counterActionData) && ActionDataSelector.EvaluateSelection(__counterActionData, 0f, 1f) && CheckTargetVisibility())
                    // {
                    //     ActionDataSelector.ResetCoolTime(__counterActionData);
                    //     ActionCtrler.SetPendingAction(__counterActionData.actionName);
                    // }
                    // else if (ActionDataSelector.CheckCoolTime(__chargeActionData) && ActionDataSelector.EvaluateSelection(__chargeActionData, 0f, 1f) && CheckTargetVisibility())
                    // {
                    //     ActionDataSelector.ResetCoolTime(__chargeActionData);
                    //     ActionCtrler.SetPendingAction(__chargeActionData.actionName);
                    // }
                    // else
                    // {
                    //     __lastComboAttackRateStepTimeStamp = Time.time;
                    //     ActionDataSelector.BoostSelection(__combo1ActionData, BB.action.comboAttackRateStep);
                    // }
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
                    __counterActionData ??= ActionDataSelector.GetActionData("Counter");
                    ActionCtrler.SetPendingAction(__counterActionData.actionName);
                    ActionDataSelector.SetCoolTime(__combo1ActionData);
                }
            }
        }
    }
}
