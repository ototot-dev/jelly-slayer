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

        protected override void StartInternal()
        {
            base.StartInternal();
            
            onTick += (deltaTick) =>
            {
                if (!BB.IsSpawnFinished || BB.IsDead || BB.IsStunned || BB.IsDown || !BB.IsInCombat || BB.TargetPawn == null)
                    return;
                    
                ActionDataSelector.UpdateSelection(deltaTick);
                return;
                //* 공격
                if (!ActionCtrler.CheckActionRunning() && string.IsNullOrEmpty(ActionCtrler.PendingActionData.Item1) && !BuffCtrler.CheckBuff(BuffTypes.Staggered) && CheckTargetVisibility())
                {
                    var selection = ActionDataSelector.PickSelection(BB.TargetBrain.coreColliderHelper.GetApproachDistance(coreColliderHelper.transform.position), BB.stat.stamina.Value);
                    if (selection != null)
                        ActionCtrler.SetPendingAction(selection.actionName);
                }
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