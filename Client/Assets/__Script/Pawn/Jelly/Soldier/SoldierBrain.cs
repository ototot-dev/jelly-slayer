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

        float __coolDownFinishTimeStamp;

        public enum ActionPatterns : int
        {
            None = -1,
            JumpAttack,
            Counter,
            Missile,
            Combo,
            Leap,
            Max,
        }

        protected override void StartInternal()
        {
            base.StartInternal();

            ActionDataSelector.ReserveSequence(ActionPatterns.JumpAttack, "JumpAttack");
            ActionDataSelector.ReserveSequence(ActionPatterns.Counter, "Counter");
            ActionDataSelector.ReserveSequence(ActionPatterns.Missile, "BackStep", "Missile");
            ActionDataSelector.ReserveSequence(ActionPatterns.Combo, "Attack#1", "Attack#2", "Attack#3");
            ActionDataSelector.ReserveSequence(ActionPatterns.Leap, "BackStep", "Missile", "Leap");

            onTick += (deltaTick) =>
            {
                if (!BB.IsSpawnFinished || BB.IsDead)
                    return;

                ActionDataSelector.UpdateSelection(deltaTick);

                if (BB.action.sequenceCoolTimeLeft > 0f)
                    BB.action.sequenceCoolTimeLeft -= deltaTick;
                
                if (debugActionDisabled || BB.IsGroggy || BB.IsDown || !BB.IsInCombat || BB.TargetPawn == null)
                    return;
                if (ActionCtrler.CheckActionPending()) 
                    return;

                if (!ActionCtrler.CheckActionRunning() || ActionCtrler.CanInterruptAction())
                {
                    var nextActionData = ActionDataSelector.AdvanceSequence();
                    if (nextActionData == null && BB.action.sequenceCoolTimeLeft <= 0f)
                    {
                        var distanceToTarget = coreColliderHelper.GetDistanceBetween(BB.TargetBrain.coreColliderHelper);
                        if (distanceToTarget < BB.body.maxSpacingDistance && ActionDataSelector.EvaluateSelection(ActionDataSelector.GetSequenceData(ActionPatterns.Combo)) && CheckTargetVisibility())
                        {
                            nextActionData = ActionDataSelector.EnqueueSequence(ActionPatterns.Combo).Curr();
                        }
                        else if (distanceToTarget > BB.body.spacingInDistance && ActionDataSelector.EvaluateSelection(ActionDataSelector.GetSequenceData(ActionPatterns.JumpAttack)) && CheckTargetVisibility())
                        {
                            nextActionData = ActionDataSelector.EnqueueSequence(ActionPatterns.JumpAttack).Curr();
                            if (ActionDataSelector.EvaluateSelection(ActionDataSelector.GetSequenceData(ActionPatterns.Combo)))
                                ActionDataSelector.EnqueueSequence(ActionPatterns.Combo);
                        }
                    }

                    if (nextActionData != null)
                    {
                        if (ActionCtrler.CheckActionRunning()) ActionCtrler.CancelAction(false);

                        ActionCtrler.SetPendingAction(nextActionData.actionName);
                        ActionDataSelector.ResetSelection(nextActionData);
                    }
                }

                // else if (ActionDataSelector.CheckExecutable(__combo1ActionData) && Time.time - PawnHP.LastDamageTimeStamp >= 1f && Time.time - __lastComboAttackRateStepTimeStamp >= 1f)
                // {
                //     var distanceConstraint = BB.TargetBrain != null ? BB.TargetBrain.coreColliderHelper.GetApproachDistance(coreColliderHelper.transform.position) : -1f;
                //     if (ActionDataSelector.EvaluateSelection(__combo1ActionData, 1f) && CheckTargetVisibility())
                //     {
                //         ActionDataSelector.ResetSelection(__combo1ActionData);
                //         ActionCtrler.SetPendingAction(__combo1ActionData.actionName, "PreMotion");
                //     }
                //     else if (distanceConstraint > __leapActionData.actionRange)
                //     {
                //         if (ActionDataSelector.EvaluateSelection(__leapActionData, 1f) && CheckTargetVisibility())
                //         {
                //             ActionDataSelector.ResetSelection(__leapActionData);
                //             ActionCtrler.SetPendingAction(__leapActionData.actionName);

                //             //* 'Leap' 액션의 루트모션 이동거리인 7m 기준으로 목표점까지의 이동 거리를 조절해준다.
                //             ActionCtrler.leapRootMotionMultiplier = Mathf.Clamp01((BB.TargetBrain.GetWorldPosition() - GetWorldPosition()).Magnitude2D() / ActionCtrler.leapRootMotionDistance);
                //         }
                //         else
                //         {
                //             ActionDataSelector.BoostSelection(__leapActionData, BB.action.leapIncreaseRate * Time.deltaTime);
                //         }
                //     }
                //     else
                //     {
                //         __lastComboAttackRateStepTimeStamp = Time.time;
                //         ActionDataSelector.BoostSelection(__combo1ActionData, BB.action.comboAttackIncreaseRateOnIdle);
                //     }
                // }
            };

            PawnStatusCtrler.onStatusActive += (status) =>
            {
                if (status == PawnStatus.Staggered) ActionDataSelector.ClearSequences();
            };

            // ActionCtrler.onActionStart += (actionContext, __) =>
            // {
            //     if (actionContext.actionData == __randomPickActionData)
            //         __randomPickActionData = null;
            // };

            // ActionCtrler.onActionFinished += (actionContext) =>
            // {
            //     if (__randomPickActionData == null) return;

            //     if ((actionContext.actionData?.actionName ?? string.Empty) == "BackStep")
            // };

            BB.stat.actionPoint.Skip(1).Subscribe(v =>
            {
                //* ActionPoint 전부 소모하면 CoolDown 상태로 진입
                if (v <= 0) 
                {
                    BB.action.sequenceCoolTimeLeft = UnityEngine.Random.Range(BB.action.minCoolDownDuration, BB.action.maxCoolDownDuration);
                    Observable.NextFrame().Subscribe(_ => BB.stat.RecoverActionPoint(BB.stat.maxActionPoint.Value)).AddTo(this);
                }
            }).AddTo(this);

            BB.body.isFalling.Skip(1).Subscribe(v =>
            {
                //* 착지 동작 완료까지 이동을 금지함
                if (!v) PawnStatusCtrler.AddStatus(PawnStatus.CanNotMove, 1f, 0.5f);
            }).AddTo(this);
        }

        protected override void OnTickInternal(float interval)
        {
            base.OnTickInternal(interval);
        }

        protected override void DamageReceiverHandler(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            base.DamageReceiverHandler(ref damageContext);

            if (damageContext.actionResult == ActionResults.Blocked)
            {   
                if (debugActionDisabled)
                    return;
                    
                if (!ActionCtrler.CheckActionPending() && ActionDataSelector.EvaluateSelection(ActionDataSelector.GetSequenceData(ActionPatterns.Counter), UnityEngine.Random.Range(0f, 1f)))
                {
                    ActionDataSelector.ClearSequences();

                    var newActionData = ActionDataSelector.EnqueueSequence(ActionPatterns.Counter).Curr();
                    if (ActionDataSelector.EvaluateSelection(ActionDataSelector.GetSequenceData(ActionPatterns.Combo)))
                        ActionDataSelector.EnqueueSequence(ActionPatterns.Combo);

                    ActionCtrler.SetPendingAction(newActionData.actionName);
                    ActionDataSelector.ResetSelection(newActionData);
                }
                else
                {
                    ActionDataSelector.BoostSelection(ActionDataSelector.GetSequenceData(ActionPatterns.Counter), BB.action.counterProbBoostRateOnGuard);
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
