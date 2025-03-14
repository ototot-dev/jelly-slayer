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
            Backstep,
            Counter,
            Missile,
            ComboAttack,
            CounterCombo,
            Leap,
            Max,
        }

        protected override void StartInternal()
        {
            base.StartInternal();

            ActionDataSelector.ReserveSequence(ActionPatterns.JumpAttack, 1f, "JumpAttack");
            ActionDataSelector.ReserveSequence(ActionPatterns.Backstep, "Backstep");
            ActionDataSelector.ReserveSequence(ActionPatterns.Counter, "Counter");
            ActionDataSelector.ReserveSequence(ActionPatterns.Missile, "Backstep", 0.2f, "Missile");
            ActionDataSelector.ReserveSequence(ActionPatterns.ComboAttack, "Attack#1", "Attack#2", "Attack#3");
            ActionDataSelector.ReserveSequence(ActionPatterns.CounterCombo, "Counter", "Counter", 0.1f , "Attack#3");
            ActionDataSelector.ReserveSequence(ActionPatterns.Leap, "Backstep", 0.2f, "Missile", 1f, "Leap");

            onUpdate += () =>
            {
                if (!BB.IsSpawnFinished || !BB.IsInCombat || BB.IsDead || BB.IsGroggy || BB.IsDown)
                    return;

                if (!ActionCtrler.CheckActionPending() && (!ActionCtrler.CheckActionRunning() || ActionCtrler.CanInterruptAction()) && BB.TargetPawn != null)
                {
                    var nextActionData = ActionDataSelector.AdvanceSequence();
                    if (nextActionData != null)
                    {
                        if (ActionCtrler.CheckActionRunning()) ActionCtrler.CancelAction(false);

                        ActionCtrler.SetPendingAction(nextActionData.actionName, string.Empty, ActionDataSelector.CurrSequence().GetPaddingTime());
                        ActionDataSelector.ResetSelection(nextActionData);
                    }
                } 
            };

            onTick += (deltaTick) =>
            {
                if (!BB.IsSpawnFinished || !BB.IsInCombat || BB.IsDead || BB.IsGroggy || BB.IsDown)
                    return;
                
                ActionDataSelector.UpdateSelection(deltaTick);

                if (debugActionDisabled || StatusCtrler.CheckStatus(PawnStatus.CanNotAction))
                    return;

                if (!ActionCtrler.CheckActionPending() && (!ActionCtrler.CheckActionRunning() || ActionCtrler.CanInterruptAction()) && BB.TargetPawn != null)
                {
                    if (ActionDataSelector.TryPickRandomSelection(UnityEngine.Random.Range(0.8f, 1f), -1f, out var randomActionData))
                    {
                        if (randomActionData == ActionDataSelector.GetSequence(ActionPatterns.Missile).Last())
                        {
                            ActionDataSelector.EnqueueSequence(ActionPatterns.Missile);
                            if (ActionDataSelector.EvaluateSelection(ActionDataSelector.GetSequence(ActionPatterns.JumpAttack).Last(), UnityEngine.Random.Range(0f, 1f)))
                                ActionDataSelector.EnqueueSequence(ActionPatterns.JumpAttack);
                            else
                                ActionDataSelector.BoostSelection(ActionDataSelector.GetSequence(ActionPatterns.JumpAttack).Last(), BB.action.jumpAttackProbBoostRate);
                        }
                        else if (randomActionData == ActionDataSelector.GetSequence(ActionPatterns.Leap).Last())
                        {
                            ActionDataSelector.EnqueueSequence(ActionPatterns.Leap);
                        }
                    }
                    else
                    {
                        ActionDataSelector.BoostSelection("Leap", BB.action.leapProbBoostRateOnTick * deltaTick);
                        ActionDataSelector.BoostSelection("Missile", BB.action.missileProbBoostRateOnTick * deltaTick);

                        var distanceToTarget = coreColliderHelper.GetDistanceBetween(BB.TargetBrain.coreColliderHelper);
                        if (distanceToTarget < BB.action.counterComboAttackDistance)
                        {
                            if (distanceToTarget > BB.action.comboAttackDistance || BB.action.counterComboAttachProb > UnityEngine.Random.Range(0f, 1f))
                            {
                                if (ActionDataSelector.EvaluateSelection(ActionPatterns.CounterCombo) && CheckTargetVisibility())
                                {
                                    ActionDataSelector.EnqueueSequence(ActionPatterns.CounterCombo);
                                    ActionDataSelector.ResetSelection(ActionDataSelector.GetSequence(ActionPatterns.ComboAttack).First());
                                }
                            }
                            else if (ActionDataSelector.EvaluateSelection(ActionPatterns.ComboAttack) && CheckTargetVisibility())
                            {
                                ActionDataSelector.EnqueueSequence(ActionPatterns.ComboAttack);
                                ActionDataSelector.ResetSelection(ActionDataSelector.GetSequence(ActionPatterns.CounterCombo).First());
                            }
                        }
                    }
                }
                else if (!ActionCtrler.CheckActionRunning())
                {
                    var distanceToTarget = coreColliderHelper.GetDistanceBetween(BB.TargetBrain.coreColliderHelper);
                    if (distanceToTarget < BB.action.backstepTriggerDistance && ActionDataSelector.EvaluateSelection(ActionPatterns.Backstep, UnityEngine.Random.Range(0f, 1f)))
                        ActionDataSelector.EnqueueSequence(ActionPatterns.Backstep);
                }
            };

            __pawnActionCtrler.onActionFinished += (actionContext) =>
            {
                if (actionContext.actionName == "Missile")
                {
                    ActionDataSelector.ResetSelection(ActionDataSelector.GetSequence(ActionPatterns.ComboAttack).First());
                    ActionDataSelector.ResetSelection(ActionDataSelector.GetSequence(ActionPatterns.CounterCombo).First());
                }
                else if (actionContext.actionName == "Leap")
                {
                    ActionDataSelector.ResetSelection(ActionDataSelector.GetSequence(ActionPatterns.Missile).Last());
                    ActionDataSelector.ResetSelection(ActionDataSelector.GetSequence(ActionPatterns.JumpAttack).First());
                    ActionDataSelector.ResetSelection(ActionDataSelector.GetSequence(ActionPatterns.ComboAttack).First());
                    ActionDataSelector.ResetSelection(ActionDataSelector.GetSequence(ActionPatterns.CounterCombo).First());
                }
            };

            PawnStatusCtrler.onStatusActive += (status) =>
            {
                if (status == PawnStatus.Staggered) ActionDataSelector.ClearSequences();
            };

            PawnStatusCtrler.onStatusDeactive += (status) =>
            {
                if (status == PawnStatus.CanNotAction && BB.stat.actionPoint.Value <= 0) 
                {
                    BB.stat.RecoverActionPoint(BB.stat.maxActionPoint.Value);
                    __Logger.LogR2(gameObject, "onStatusDeactive(CanNotAction)", "RecoverActionPoint()", "maxActionPoint", BB.stat.maxActionPoint.Value);
                }
            };

            BB.stat.actionPoint.Skip(1).Where(v => v <= 0).Subscribe(v =>
            {
                var coolDownDuration = UnityEngine.Random.Range(BB.action.minCoolDownDuration, BB.action.maxCoolDownDuration);
                StatusCtrler.AddStatus(PawnStatus.CanNotAction, coolDownDuration);

                __Logger.LogR1(gameObject, "AddStatus(CanNotAction)", "duration", coolDownDuration);
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

                if (!ActionCtrler.CheckActionPending() && BB.action.counterAttackProbOnGuard > UnityEngine.Random.Range(0f, 1f))
                {
                    ActionDataSelector.ClearSequences();

                    if (UnityEngine.Random.Range(0f, 1f) <= BB.action.counterComboAttachProb)
                    {                
                        ActionDataSelector.EnqueueSequence(ActionPatterns.CounterCombo);
                    }
                    else
                    {
                        ActionDataSelector.EnqueueSequence(ActionPatterns.Counter);
                        if (ActionDataSelector.EvaluateSelection(ActionPatterns.ComboAttack))
                            ActionDataSelector.EnqueueSequence(ActionPatterns.ComboAttack);
                    }
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
