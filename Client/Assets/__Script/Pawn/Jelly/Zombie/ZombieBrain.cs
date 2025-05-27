using System;
using UniRx;
using UnityEngine;

namespace Game
{
    public class ZombieBrain : NpcBrain
    {
        [Header("Weapon")]
        public WeaponController[] _handWeaponCtrl;


        public ZombieBlackboard BB { get; private set; }
        public ZombieMovement Movement { get; private set; }
        public ZombieAnimController AnimCtrler { get; private set; }
        public ZombieActionController ActionCtrler { get; private set; }

        protected override void AwakeInternal()
        {
            base.AwakeInternal();

            BB = GetComponent<ZombieBlackboard>();
            Movement = GetComponent<ZombieMovement>();
            AnimCtrler = GetComponent<ZombieAnimController>();
            ActionCtrler = GetComponent<ZombieActionController>();
        }

        protected override void StartInternal()
        {
            base.StartInternal();

            ActionCtrler.onActionFinished += (_) => InvalidateDecision(0.2f);
            ActionCtrler.onActionCanceled += (_, __) => InvalidateDecision(0.2f);

            onTick += (_) =>
            {
                if (!BB.IsSpawnFinished || BB.IsDead || !BB.IsInCombat || BB.TargetPawn == null)
                    return;

                if (ActionCtrler.CheckActionRunning())
                {
                    if (ActionCtrler.currActionContext.actionData != null && ActionCtrler.CanInterruptAction())
                    {
                        // var nextActionData = DatasheetManager.Instance.GetActionData(BB.common.pawnId, ActionCtrler.currActionContext.actionData.nextActionName);
                        // Debug.Assert(nextActionData != null);
                        
                        // //* 콥보 (NextAction) 공격
                        // if (nextActionData.staminaCost <= BB.stat.stamina.Value && __jellyManBB.TargetBrain.coreColliderHelper.GetApproachDistance(coreColliderHelper.transform.position) <= nextActionData.actionRange && CheckTargetVisibility())
                        // {
                        //     ActionCtrler.CancelAction(false);
                        //     ActionCtrler.SetPendingAction(nextActionData.actionName);
                        // }
                    }
                }
                else
                {
                    //* 공격 시작
                    // if (string.IsNullOrEmpty(ActionCtrler.PendingActionData.Item1) && !StatusCtrler.CheckStatus(Game.PawnStatus.CanNotAction) && !StatusCtrler.CheckStatus(Game.PawnStatus.Staggered) && base.CheckTargetVisibility())
                    // {
                    //     var selection = ActionDataSelector.RandomSelection(BB.TargetBrain.coreColliderHelper.GetApproachDistance(coreColliderHelper.transform.position), BB.stat.stamina.Value, true);
                    //     if (selection != null)
                    //         ActionCtrler.SetPendingAction(selection.actionName);
                    // }
                }
            };
        }

        // public override void ShowTrail(bool isActive, int trailIndex)
        // {
        //     if (_handWeaponCtrl != null && trailIndex < _handWeaponCtrl.Length)
        //     {
        //         _handWeaponCtrl[trailIndex].ShowTrail(isActive);
        //     }
        // }

        // public override void DoAction(ActionType type, PawnBrainController attacker)
        // {
        //     switch (type)
        //     {
        //         case ActionType.Knockback:
        //             {
        //                 AnimCtrler.mainAnimator.SetBool("IsDown", true);
        //                 AnimCtrler.mainAnimator.SetTrigger("OnDown");

        //                 var vDist = CoreTransform.position - attacker.CoreTransform.position;
        //                 var knockBackVec = 1.2f * vDist.normalized;
        //                 Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(0.5f)))
        //                     .Subscribe(_ => Movement.AddRootMotion(4 * Time.fixedDeltaTime * knockBackVec, Quaternion.identity))
        //                     .AddTo(this);
        //             }
        //             break;
        //     }
        // }
    }
}
