using System;
using System.Collections.Generic;
using System.Linq;
using Packets;
using UniRx;
using Unity.Linq;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(DroneBotMovement))]
    [RequireComponent(typeof(DroneBotBlackboard))]
    [RequireComponent(typeof(DroneBotAnimController))]
    [RequireComponent(typeof(DroneBotActionController))]
    public class DroneBotBrain : JellyManBrain
    {
        [Header("Debug")]
        public bool debugActionDisabled;

        public DroneBotBlackboard BB { get; private set; }
        public DroneBotMovement Movement { get; private set; }
        public DroneBotAnimController AnimCtrler { get; private set; }
        public DroneBotActionController ActionCtrler { get; private set; }

        protected override void AwakeInternal()
        {
            base.AwakeInternal();

            BB = GetComponent<DroneBotBlackboard>();
            Movement = GetComponent<DroneBotMovement>();
            AnimCtrler = GetComponent<DroneBotAnimController>();
            ActionCtrler = GetComponent<DroneBotActionController>();
        }

        float __lastFireRateStepTimeStamp;
        MainTable.ActionData __fireActionData;

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

                __fireActionData ??= ActionDataSelector.GetActionData("Fire");

                if (ActionCtrler.CheckActionPending() || ActionCtrler.CheckActionRunning())
                {
                    __lastFireRateStepTimeStamp = Time.time;
                }
                else if (ActionDataSelector.CheckExecutable(__fireActionData) && Time.time - PawnHP.LastDamageTimeStamp >= 1f && Time.time - __lastFireRateStepTimeStamp >= 1f)
                {
                    if (ActionDataSelector.EvaluateSelection(__fireActionData, -1f, 1f) && CheckTargetVisibility())
                    {
                        ActionDataSelector.ResetSelection(__fireActionData);
                        ActionCtrler.SetPendingAction(__fireActionData.actionName);
                    }
                    else
                    {
                        __lastFireRateStepTimeStamp = Time.time;
                        ActionDataSelector.BoostSelection(__fireActionData, BB.selection.fireAttackRateStep);
                    }
                }
            };
        }
    }
}