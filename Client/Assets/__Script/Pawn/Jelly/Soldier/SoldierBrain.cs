using System;
using UniRx;
using UniRx.Triggers;
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
        void IPawnTargetable.StopTargeting() { }
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
        public JellyMeshController jellyMeshCtrler;

        public override Vector3 GetSpecialKeyPosition() => BB.attachment.specialKeyAttachPoint.transform.position;
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
            ShieldAttack,
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

            ActionDataSelector.ReserveSequence(ActionPatterns.JumpAttack, "JumpAttack");
            ActionDataSelector.ReserveSequence(ActionPatterns.ShieldAttack, "ShieldAttack");
            ActionDataSelector.ReserveSequence(ActionPatterns.Backstep, "Backstep");
            ActionDataSelector.ReserveSequence(ActionPatterns.Counter, "Counter");
            ActionDataSelector.ReserveSequence(ActionPatterns.Missile, 0.5f, "Missile");
            ActionDataSelector.ReserveSequence(ActionPatterns.ComboAttack, "Attack#1", "Attack#2", "Attack#3");
            ActionDataSelector.ReserveSequence(ActionPatterns.CounterCombo, "Counter", "Counter", 0.1f, "Attack#3");
            ActionDataSelector.ReserveSequence(ActionPatterns.Leap, "Backstep", 0.2f, "Missile", 1f, "Leap");

            ActionCtrler.onActionStart += (_, __) =>
            {
                // shieldHitColliderHelper.gameObject.layer = LayerMask.NameToLayer("HitBox");
            };

            ActionCtrler.onActionFinished += (actionContext) =>
            {
                // shieldHitColliderHelper.gameObject.layer = LayerMask.NameToLayer("HitBoxBlocking");

                if (actionContext.actionName == "Missile")
                {
                    ActionDataSelector.GetSequence(ActionPatterns.Backstep).SetCoolTime();
                    ActionDataSelector.GetSequence(ActionPatterns.ComboAttack).SetCoolTime();
                    ActionDataSelector.GetSequence(ActionPatterns.CounterCombo).SetCoolTime();
                }
                else if (actionContext.actionName == "Leap")
                {
                    ActionDataSelector.GetSequence(ActionPatterns.JumpAttack).SetCoolTime();
                    ActionDataSelector.GetSequence(ActionPatterns.Backstep).SetCoolTime();
                    ActionDataSelector.GetSequence(ActionPatterns.Missile).SetCoolTime();
                    ActionDataSelector.GetSequence(ActionPatterns.ComboAttack).SetCoolTime();
                    ActionDataSelector.GetSequence(ActionPatterns.CounterCombo).SetCoolTime();
                }
            };

            PawnStatusCtrler.onStatusActive += (status) =>
            {
                if (status == PawnStatus.Staggered)
                {
                    ActionDataSelector.ClearSequences();
                    ActionDataSelector.GetSequence(ActionPatterns.ComboAttack).SetCoolTime();
                    ActionDataSelector.GetSequence(ActionPatterns.CounterCombo).SetCoolTime();
                }
                else if (status == PawnStatus.Groggy)
                {
                    ActionDataSelector.ClearSequences();
                    PawnEventManager.Instance.SendPawnStatusEvent(this, PawnStatus.Groggy, 1f, PawnStatusCtrler.GetDuration(PawnStatus.Groggy));
                }
            };

            PawnStatusCtrler.onStatusDeactive += (status) =>
            {
                if (status == PawnStatus.Groggy)
                {
                    ActionDataSelector.GetSequence(ActionPatterns.JumpAttack).SetCoolTime();
                    ActionDataSelector.GetSequence(ActionPatterns.Backstep).SetCoolTime();
                    ActionDataSelector.GetSequence(ActionPatterns.Missile).SetCoolTime();
                    ActionDataSelector.GetSequence(ActionPatterns.Leap).SetCoolTime();
                    ActionDataSelector.GetSequence(ActionPatterns.ComboAttack).SetCoolTime();
                    ActionDataSelector.GetSequence(ActionPatterns.CounterCombo).SetCoolTime();

                    jellyMeshCtrler.FadeOut(0.5f);
                    jellyMeshCtrler.FinishHook();

                    //* 막타 Hit 애님이 오전히 출력되는 시간 딜레이 
                    Observable.Timer(TimeSpan.FromSeconds(1.5f)).Subscribe(_ => __pawnAnimCtrler.mainAnimator.SetBool("IsGroggy", false)).AddTo(this);

                    //* 일어나는 모션 동안은 무적
                    PawnStatusCtrler.AddStatus(PawnStatus.Invincible, 1f, 2f);
                    PawnStatusCtrler.AddStatus(PawnStatus.CanNotMove, 1f, 3f);
                    PawnStatusCtrler.AddStatus(PawnStatus.CanNotAction, 1f, 3f);
                    InvalidateDecision(4f);
                }
            };

            BB.common.isDown.Skip(1).Subscribe(v =>
            {
                if (v)
                {
                    AnimCtrler.mainAnimator.SetBool("IsDown", true);
                    AnimCtrler.mainAnimator.SetTrigger("OnDown");
                }
                else
                {
                    AnimCtrler.mainAnimator.SetBool("IsDown", false);

                    //* 일어나는 모션 동안은 무적
                    PawnStatusCtrler.AddStatus(PawnStatus.Invincible, 1f, 1f);
                    PawnStatusCtrler.AddStatus(PawnStatus.CanNotMove, 1f, 2f);
                    PawnStatusCtrler.AddStatus(PawnStatus.CanNotAction, 1f, 2f);
                    InvalidateDecision(3f);
                }
            }).AddTo(this);

            BB.body.isFalling.Skip(1).Subscribe(v =>
            {
                //* 착지 동작 완료까지 이동을 금지함
                if (!v) PawnStatusCtrler.AddStatus(PawnStatus.CanNotMove, 1f, 0.5f);
            }).AddTo(this);

            //* 방패 터치 시에 ShieldAttack 발동 조건
            BB.action.shieldTouchSensor.OnTriggerStayAsObservable().Subscribe(c =>
            {
                if (!BB.IsSpawnFinished || !BB.IsInCombat || BB.IsDead || BB.IsDown || BB.IsGroggy)
                    return;
                if (ActionCtrler.CheckActionPending() || (ActionCtrler.CheckActionRunning() && !ActionCtrler.CanInterruptAction()))
                    return;
                if (StatusCtrler.CheckStatus(PawnStatus.CanNotAction) || StatusCtrler.CheckStatus(PawnStatus.Staggered))
                    return;

                if (ActionDataSelector.EvaluateSequence(ActionPatterns.ShieldAttack) && c.TryGetComponent<PawnColliderHelper>(out var colliderHelper) && colliderHelper.pawnBrain.PawnBB.common.pawnName == "Hero")
                {
                    ActionDataSelector.EnqueueSequence(ActionPatterns.ShieldAttack);
                    ActionDataSelector.EnqueueSequence(ActionPatterns.CounterCombo);
                }
            }).AddTo(this);

            onUpdate += () =>
            {
                if (!BB.IsSpawnFinished || !BB.IsInCombat || BB.IsDead)
                    return;

                if (!ActionCtrler.CheckActionPending() && (!ActionCtrler.CheckActionRunning() || ActionCtrler.CanInterruptAction()))
                {
                    var nextActionData = ActionDataSelector.AdvanceSequence();
                    if (nextActionData != null)
                    {   
                        if (ActionCtrler.CheckActionRunning()) ActionCtrler.CancelAction(false);

                        ActionCtrler.SetPendingAction(nextActionData.actionName, string.Empty, string.Empty, ActionDataSelector.CurrSequence().GetPaddingTime());
                        ActionDataSelector.SetCoolTime(nextActionData);

                        if (nextActionData.actionName == "Backstep")
                            BB.action.backstepRootMotionMultiplier = Mathf.Lerp(0.7f, 0.2f, Mathf.Clamp01(GetWorldPosition().Magnitude2D() / 5f));
                    }
                }
            };

        }

        protected override void OnTickInternal(float interval)
        {
            base.OnTickInternal(interval);

            if (!BB.IsSpawnFinished || !BB.IsInCombat || BB.IsDead || BB.IsGroggy || BB.IsDown)
                return;

            if (StatusCtrler.CheckStatus(PawnStatus.CanNotAction) || StatusCtrler.CheckStatus(PawnStatus.Staggered))
                return;

#if UNITY_EDITOR
            if (ActionDataSelector.debugActionSelectDisabled)
                return;
#endif

            if (!ActionCtrler.CheckActionPending() && (!ActionCtrler.CheckActionRunning() || ActionCtrler.CanInterruptAction()) && BB.TargetPawn != null)
            {
                var randomPick = ActionDataSelector.TryRandomPick<ActionPatterns>(UnityEngine.Random.Range(0.5f, 1f), -1f, 1f);
                if (randomPick != ActionPatterns.None)
                {
                    ActionDataSelector.ResetProbability(randomPick);
                    ActionDataSelector.EnqueueSequence(randomPick);
                }
                else
                {
                    ActionDataSelector.BoostProbability(ActionPatterns.Leap, BB.action.leapBoostProbOnTick * interval, BB.action.leapMaxProb);
                    ActionDataSelector.BoostProbability(ActionPatterns.Missile, BB.action.missileBoostProbOnTick * interval, BB.action.missileMaxProb);

                    var distanceToTarget = coreColliderHelper.GetDistanceSimple(BB.TargetBrain.coreColliderHelper);

                    if (distanceToTarget < BB.action.backstepTriggerDistance)
                        ActionDataSelector.BoostProbability(ActionPatterns.Backstep, BB.action.backstepBoostProbOnTick * interval);

                    if (distanceToTarget < BB.action.counterComboAttackDistance)
                    {
                        if (distanceToTarget > BB.action.comboAttackDistance || BB.action.counterComboAttachProb > UnityEngine.Random.Range(0f, 1f))
                        {
                            if (CheckTargetVisibility() && ActionDataSelector.EvaluateSequence(ActionPatterns.CounterCombo, -1f, -1f, BB.action.counterComboAttackInterval))
                                ActionDataSelector.EnqueueSequence(ActionPatterns.CounterCombo);
                        }
                        else if (CheckTargetVisibility() && ActionDataSelector.EvaluateSequence(ActionPatterns.ComboAttack, -1f, -1f, BB.action.comboAttackInterval))
                        {
                            ActionDataSelector.EnqueueSequence(ActionPatterns.ComboAttack);
                        }
                    }
                    else if (BB.action.jumpAttackProb > UnityEngine.Random.Range(0f, 1f))
                    {
                        if (CheckTargetVisibility() && ActionDataSelector.EvaluateSequence(ActionPatterns.JumpAttack, -1f, -1f, BB.action.jumpAttackInterval))
                            ActionDataSelector.EnqueueSequence(ActionPatterns.JumpAttack);
                    }
                }
            }
        }

        protected override void DamageReceiverHandler(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            base.DamageReceiverHandler(ref damageContext);

            if (damageContext.actionResult == ActionResults.Blocked)
            {
                if (!ActionCtrler.CheckActionPending() && BB.action.counterAttackProbOnGuard > UnityEngine.Random.Range(0f, 1f))
                {
                    ActionDataSelector.ClearSequences();

                    if (BB.action.counterComboAttachProb > UnityEngine.Random.Range(0f, 1f))
                    {
                        ActionDataSelector.EnqueueSequence(ActionPatterns.CounterCombo);
                    }
                    else
                    {
                        ActionDataSelector.EnqueueSequence(ActionPatterns.Counter);

                        if (ActionDataSelector.EvaluateSequence(ActionPatterns.ComboAttack))
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
