using UniRx;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(SoldierMovement))]
    [RequireComponent(typeof(SoldierBlackboard))]
    [RequireComponent(typeof(SoldierAnimController))]
    [RequireComponent(typeof(SoldierActionController))]
    public class SoldierBrain : JellyHumanoidBrain, IPawnTargetable
    {
#region IPawnTargetable 구현
        PawnColliderHelper IPawnTargetable.StartTargeting() => bodyHitColliderHelper;
        PawnColliderHelper IPawnTargetable.NextTarget() => null;
        PawnColliderHelper IPawnTargetable.CurrTarget() => bodyHitColliderHelper;
        void IPawnTargetable.StopTargeting() {}
#endregion

#region IPawnSpawnable 재정의
        public override void OnDeadHandler()
        {
            base.OnDeadHandler();
            
            var roboDogBrain = roboDogFormationCtrler.PickRoboDog();
            while (roboDogBrain != null)
            {
                roboDogFormationCtrler.ReleaseRoboDog(roboDogBrain);
                roboDogBrain.BB.common.isDead.Value = true;
                roboDogBrain = roboDogFormationCtrler.PickRoboDog();
            }
        }
#endregion

        [Header("Component")]
        public RoboDogFormationController roboDogFormationCtrler;

        [Header("Debug")]
        public bool debugActionDisabled;

        public override PawnColliderHelper GetHookingColliderHelper() => ActionCtrler.hookingPointColliderHelper;
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
        MainTable.ActionData __leapActionData;
        MainTable.ActionData __counterActionData;
        MainTable.ActionData __combo1ActionData;
        MainTable.ActionData __combo2ActionData;

        protected override void StartInternal()
        {
            base.StartInternal();
            
            __leapActionData ??= ActionDataSelector.GetActionData("Leap");
            __counterActionData ??= ActionDataSelector.GetActionData("Counter");
            __combo1ActionData ??= ActionDataSelector.GetActionData("Attack#1");
            __combo2ActionData ??= ActionDataSelector.GetActionData("Attack#2");

            onTick += (deltaTick) =>
            {
                if (!BB.IsSpawnFinished || BB.IsDead || BB.IsGroggy || BB.IsDown || !BB.IsInCombat || BB.TargetPawn == null)
                    return;
                    
                ActionDataSelector.UpdateSelection(deltaTick);

                if (debugActionDisabled)
                    return;

                if (!ActionCtrler.CheckActionPending() && ActionCtrler.CheckActionRunning() && ActionCtrler.CanInterruptAction())
                {
                    if (ActionCtrler.CurrActionName == "Leap")
                    {
                        //* 점프 접근 후에 카운터 공격 시도
                        if (CheckTargetVisibility())
                        {
                            ActionDataSelector.ResetSelection(__counterActionData);
                            ActionCtrler.SetPendingAction("Counter");
                            ActionCtrler.CancelAction(false);
                        }
                    }
                    else if (ActionCtrler.CurrActionName == "Counter")
                    {
                        ActionDataSelector.BoostSelection(__combo1ActionData, BB.action.comboAttackRateBoostAfterCounterAttack);

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
                    var distanceConstraint = BB.TargetBrain != null ? BB.TargetBrain.coreColliderHelper.GetApproachDistance(coreColliderHelper.transform.position) : -1f;
                    if (ActionDataSelector.EvaluateSelection(__combo1ActionData, distanceConstraint, 1f) && CheckTargetVisibility())
                    {
                        ActionDataSelector.ResetSelection(__combo1ActionData);
                        ActionCtrler.SetPendingAction(__combo1ActionData.actionName, "PreMotion");
                    }
                    else if (distanceConstraint > __leapActionData.actionRange)
                    {
                        if (ActionDataSelector.EvaluateSelection(__leapActionData, -1f, 1f) && CheckTargetVisibility())
                        {
                            ActionDataSelector.ResetSelection(__leapActionData);
                            ActionCtrler.SetPendingAction(__leapActionData.actionName);

                            //* 'Leap' 액션의 루트모션 이동거리인 7m 기준으로 목표점까지의 이동 거리를 조절해준다.
                            ActionCtrler.leapRootMotionMultiplier = Mathf.Clamp01((BB.TargetBrain.GetWorldPosition() - GetWorldPosition()).Magnitude2D() / ActionCtrler.leapRootMotionDistance);
                        }
                        else
                        {
                            ActionDataSelector.BoostSelection(__leapActionData, BB.action.leapRateStep * Time.deltaTime);
                        }
                    }
                    else
                    {
                        __lastComboAttackRateStepTimeStamp = Time.time;
                        ActionDataSelector.BoostSelection(__combo1ActionData, BB.action.comboAttackIncreaseRateOnIdle);
                    }
                }
            };

            BB.action.isFalling.Skip(1).Subscribe(v =>
            {
                //* 착지 동작 완료까지 이동을 금지함
                if (!v) PawnStatusCtrler.AddStatus(PawnStatus.CanNotMove, 1f, 0.5f);
            }).AddTo(this);
        }

        protected override void DamageReceiverHandler(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            base.DamageReceiverHandler(ref damageContext);

            if (damageContext.actionResult == ActionResults.Blocked)
            {   
                if (debugActionDisabled)
                    return;
                    
                if (string.IsNullOrEmpty(ActionCtrler.PendingActionData.Item1) && ActionDataSelector.EvaluateSelection(__counterActionData, -1f, 1f) && CheckTargetVisibility())
                {
                    ActionDataSelector.ResetSelection(__counterActionData);
                    ActionCtrler.SetPendingAction("Counter");
                }
                else
                {
                    ActionDataSelector.BoostSelection(__counterActionData, BB.action.counterAttackIncreaseRateOnGuard);
                }
            }
        }

        protected override void StartJumpInternal(float jumpHeight)
        {
            Movement.StartJump(BB.body.jumpHeight);
        }

        protected override void FinishJumpInternal()
        {
            Movement.StartFalling();
        }
    }
}
