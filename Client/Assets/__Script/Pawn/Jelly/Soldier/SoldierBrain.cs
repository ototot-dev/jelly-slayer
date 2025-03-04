using UGUI.Rx;
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

        public override Vector3 GetSpecialKeyPosition() => BB.attachment.specialKeyAttachPoint.transform.position;
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
        SpecialKeyController __specialKeyCtrler;

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
            ActionDataSelector.ReserveSequence(ActionPatterns.Leap, "BackStep", "Missile", 0.2f, "Leap");

            onTick += (deltaTick) =>
            {
                if (!BB.IsSpawnFinished || BB.IsDead)
                    return;

                ActionDataSelector.UpdateSelection(deltaTick);

                if (BB.action.sequenceCoolDownTimeLeft > 0f)
                    BB.action.sequenceCoolDownTimeLeft -= deltaTick;
                
                if (debugActionDisabled || BB.IsGroggy || BB.IsDown || !BB.IsInCombat || BB.TargetPawn == null)
                    return;
                if (ActionCtrler.CheckActionPending()) 
                    return;

                if (!ActionCtrler.CheckActionRunning() || ActionCtrler.CanInterruptAction())
                {
                    var nextActionData = ActionDataSelector.AdvanceSequence();
                    if (nextActionData == null && BB.action.sequenceCoolDownTimeLeft <= 0f)
                    {
                        if (ActionDataSelector.TryPickRandomSelection(1f, -1f, out var randomActionData))
                        {
                            if (randomActionData == ActionDataSelector.GetSequence(ActionPatterns.Missile).Last())
                            {
                                nextActionData = ActionDataSelector.EnqueueSequence(ActionPatterns.Missile).First();
                                ActionDataSelector.ResetSelection(randomActionData);
                            }
                            else if (randomActionData == ActionDataSelector.GetSequence(ActionPatterns.Leap).Last())
                            {
                                nextActionData = ActionDataSelector.EnqueueSequence(ActionPatterns.Leap).First();
                                ActionDataSelector.ResetSelection(randomActionData);
                            }
                        }
                        else
                        {
                            ActionDataSelector.BoostSelection("Missile", BB.action.missileProbBoostRateOnIdle);
                            ActionDataSelector.BoostSelection("Leap", BB.action.leapProbBoostRateOnIdle);

                            var distanceToTarget = coreColliderHelper.GetDistanceBetween(BB.TargetBrain.coreColliderHelper);
                            if (distanceToTarget < BB.body.maxSpacingDistance && ActionDataSelector.EvaluateSelection(ActionPatterns.Combo) && CheckTargetVisibility())
                            {
                                nextActionData = ActionDataSelector.EnqueueSequence(ActionPatterns.Combo).First();
                            }
                            else if (distanceToTarget > BB.body.spacingInDistance && ActionDataSelector.EvaluateSelection(ActionPatterns.JumpAttack) && CheckTargetVisibility())
                            {
                                nextActionData = ActionDataSelector.EnqueueSequence(ActionPatterns.JumpAttack).First();
                                if (ActionDataSelector.EvaluateSelection(ActionPatterns.Combo))
                                    ActionDataSelector.EnqueueSequence(ActionPatterns.Combo);
                            }
                        }
                    }

                    if (nextActionData != null)
                    {
                        if (ActionCtrler.CheckActionRunning()) ActionCtrler.CancelAction(false);

                        ActionCtrler.SetPendingAction(nextActionData.actionName, string.Empty, ActionDataSelector.CurrSequence().GetPaddingTime());
                        ActionDataSelector.ResetSelection(nextActionData);
                    }
                }
            };

            PawnStatusCtrler.onStatusActive += (status) =>
            {
                if (status == PawnStatus.Staggered) ActionDataSelector.ClearSequences();
            };

            BB.stat.actionPoint.Skip(1).Subscribe(v =>
            {
                //* ActionPoint 전부 소모하면 CoolDown 상태로 진입
                if (v <= 0) 
                {
                    BB.action.sequenceCoolDownTimeLeft = UnityEngine.Random.Range(BB.action.minCoolDownDuration, BB.action.maxCoolDownDuration);
                    __Logger.LogR1(gameObject, "Start Cooldown", "sequenceCoolTimeLeft", BB.action.sequenceCoolDownTimeLeft);

                    Observable.NextFrame().Subscribe(_ => 
                    {
                        BB.stat.RecoverActionPoint(BB.stat.maxActionPoint.Value);
                        __Logger.LogR1(gameObject, "Recover ActionPoint", "maxActionPoint", BB.stat.maxActionPoint.Value);
                    }).AddTo(this);
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
                    
                if (!ActionCtrler.CheckActionPending() && ActionDataSelector.EvaluateSelection(ActionPatterns.Counter, UnityEngine.Random.Range(0f, 1f)))
                {
                    ActionDataSelector.ClearSequences();

                    var counterActionData = ActionDataSelector.EnqueueSequence(ActionPatterns.Counter).First();
                    if (ActionDataSelector.EvaluateSelection(ActionPatterns.Combo))
                        ActionDataSelector.EnqueueSequence(ActionPatterns.Combo);

                    ActionCtrler.SetPendingAction(counterActionData.actionName);
                    ActionDataSelector.ResetSelection(counterActionData);
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
