using System.Linq;
using UniRx;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(WarrirorMovement))]
    [RequireComponent(typeof(WarriorBlackboard))]
    [RequireComponent(typeof(WarriorAnimController))]
    [RequireComponent(typeof(WarriorActionController))]
    public class WarriorBrain : JellyBrain
    {
        public WarriorBlackboard BB { get; private set; }
        public WarrirorMovement Movement { get; private set; }
        public WarriorAnimController AnimCtrler { get; private set; }
        public WarriorActionController ActionCtrler { get; private set; }

        protected override void AwakeInternal()
        {
            base.AwakeInternal();

            BB = GetComponent<WarriorBlackboard>();
            Movement = GetComponent<WarrirorMovement>();
            AnimCtrler = GetComponent<WarriorAnimController>();
            ActionCtrler = GetComponent<WarriorActionController>();
        }

        protected override void StartInternal()
        {
            base.StartInternal();
            
            onTick += (deltaTick) =>
            {
                if (!BB.IsSpawnFinished || BB.IsDead || BB.IsGroggy || BB.IsDown || !BB.IsInCombat || BB.TargetPawn == null)
                    return;
                    
                //* 공격
                // if (!ActionCtrler.CheckActionRunning() && string.IsNullOrEmpty(ActionCtrler.PendingActionData.Item1) && !StatusCtrler.CheckStatus(Game.PawnStatus.Staggered) && base.CheckTargetVisibility())
                // {
                //     var selection = ActionDataSelector.RandomSelection(BB.TargetBrain.coreColliderHelper.GetApproachDistance(coreColliderHelper.transform.position), BB.stat.stamina.Value, true);
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
                // if (string.IsNullOrEmpty(ActionCtrler.PendingActionData.Item1) && CheckTargetVisibility())
                // {
                //     var selection = ActionDataSelector.RandomSelection(0, 100, true);
                //     if (selection != null)
                //         ActionCtrler.SetPendingAction(selection.actionName);
                // }
            }
        }
    }
}
