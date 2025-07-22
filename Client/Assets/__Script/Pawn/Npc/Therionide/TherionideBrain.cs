using System;
using UGUI.Rx;
using UniRx;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(TherionideMovement))]
    [RequireComponent(typeof(TherionideBlackboard))]
    [RequireComponent(typeof(TherionideAnimController))]
    [RequireComponent(typeof(TherionideActionController))]
    public class TherionideBrain : NpcHumanoidBrain, IPawnTargetable
    {
        #region IPawnTargetable 구현
        PawnColliderHelper IPawnTargetable.StartTargeting() => bodyHitColliderHelper;
        PawnColliderHelper IPawnTargetable.NextTarget() => null;
        PawnColliderHelper IPawnTargetable.PrevTarget() => null;
        PawnColliderHelper IPawnTargetable.CurrTarget() => bodyHitColliderHelper;
        void IPawnTargetable.StopTargeting() { }
        #endregion

        public override Vector3 GetInteractionKeyAttachPoint() => BB.children.specialKeyAttachPoint.transform.position;
        public override Vector3 GetStatusBarAttachPoint() => coreColliderHelper.transform.position + coreColliderHelper.GetCapsuleHeight() * Vector3.up;
        public TherionideBlackboard BB { get; private set; }
        public TherionideMovement Movement { get; private set; }
        public TherionideAnimController AnimCtrler { get; private set; }
        public TherionideActionController ActionCtrler { get; private set; }

        protected override void AwakeInternal()
        {
            base.AwakeInternal();

            BB = GetComponent<TherionideBlackboard>();
            Movement = GetComponent<TherionideMovement>();
            AnimCtrler = GetComponent<TherionideAnimController>();
            ActionCtrler = GetComponent<TherionideActionController>();
        }

        public enum ActionPatterns : int
        {
            None = -1,
            ComboAttackA,
            ComboAttackB,
            Shoot,
            Max,
        }

        protected override void StartInternal()
        {
            base.StartInternal();

            //* 액션 패턴 등록
            ActionDataSelector.ReserveSequence(ActionPatterns.ComboAttackA, "Attack#1", 0.2f, "Attack#2", 0.2f, "Attack#2", 0.2f, "Attack#3").ResetProbability(1f);
            ActionDataSelector.ReserveSequence(ActionPatterns.ComboAttackB, "Attack#2", 0.2f, "Attack#2", 0.1f, "Attack#2", 0.2f, "Attack#3").ResetProbability(1f);
            ActionDataSelector.ReserveSequence(ActionPatterns.Shoot, "Leap", 0.2f, "Shoot").ReduceCoolTime(BB.action.comboAttackCoolTime).ResetProbability(1f);

            BB.common.isSpawnFinished.Skip(1).Subscribe(v =>
            {
                new Game.UI.FloatingStatusBarController(this).Load().Show(GameContext.Instance.canvasManager.body.transform as RectTransform);
            }).AddTo(this);

            BB.common.isDead.Skip(1).Subscribe(v =>
            {
                if (v)
                    bodyHitColliderHelper.pawnCollider.enabled = false;
            }).AddTo(this);

            PawnStatusCtrler.onStatusActive += (status) =>
            {
                if (status == PawnStatus.Staggered)
                    ActionDataSelector.CancelSequences();

                //* 그로기 시작 이벤트 
                if (status == PawnStatus.Groggy)
                    PawnEventManager.Instance.SendPawnStatusEvent(this, PawnStatus.Groggy, 1f, PawnStatusCtrler.GetDuration(PawnStatus.Groggy));
            };

            PawnStatusCtrler.onStatusDeactive += (status) =>
            {
                if (status == PawnStatus.Groggy)
                {
                    //* 막타 Hit 애님이 온전 출력되는 시간 딜레이 
                    Observable.Timer(TimeSpan.FromSeconds(1.5f)).Subscribe(_ => __pawnAnimCtrler.mainAnimator.SetBool("IsGroggy", false)).AddTo(this);

                    //* 리커버 동작 동안은 무적 및 이동, 액션 금지
                    PawnStatusCtrler.AddStatus(PawnStatus.Invincible, 1f, 2f);
                    PawnStatusCtrler.AddStatus(PawnStatus.CanNotMove, 1f, 3f);
                    PawnStatusCtrler.AddStatus(PawnStatus.CanNotAction, 1f, 3f);

                    InvalidateDecision(3f);
                }
            };

            onUpdate += () =>
            {
                if (!BB.IsSpawnFinished || !BB.IsInCombat || BB.IsDead || BB.IsDown || BB.IsGroggy)
                    return;

                if (StatusCtrler.CheckStatus(PawnStatus.CanNotAction) || StatusCtrler.CheckStatus(PawnStatus.Staggered))
                    return;

                if (!ActionCtrler.CheckActionPending() && (!ActionCtrler.CheckActionRunning() || ActionCtrler.CanInterruptAction()))
                {
                    var nextActionData = ActionDataSelector.AdvanceSequence();
                    if (nextActionData != null)
                    {
                        if (ActionCtrler.CheckActionRunning())
                            ActionCtrler.CancelAction(false);

                        ActionCtrler.SetPendingAction(nextActionData.actionName, -1f, ActionDataSelector.CurrSequence().GetPaddingTime());
                    }
                }
            };
        }

        protected override void OnTickInternal(float interval)
        {
            base.OnTickInternal(interval);

            if (!BB.IsSpawnFinished || !BB.IsInCombat || BB.IsDead || BB.IsDown || BB.IsGroggy)
                return;

            if (StatusCtrler.CheckStatus(PawnStatus.CanNotAction) || StatusCtrler.CheckStatus(PawnStatus.Staggered))
                return;

#if UNITY_EDITOR
            if (ActionDataSelector.debugActionSelectDisabled)
                return;
#endif

            if (ActionDataSelector.CurrSequence() == null)
            {
                if (ActionDataSelector.EvaluateSequence(ActionPatterns.Shoot))
                {
                    ActionDataSelector.EnqueueSequence(ActionPatterns.Shoot);
                    ActionDataSelector.BeginCoolTime(ActionPatterns.Shoot, BB.action.shootCoolTime);
                }
                else
                {
                    var distanceToTarget = coreColliderHelper.GetDistanceSimple(BB.TargetBrain.coreColliderHelper);

                    if (distanceToTarget < BB.action.comboAttackDistance)
                    {
                        var comboAttackPick = ActionPatterns.None;

                        switch (Rand.Dice(2))
                        {
                            case 1: comboAttackPick = ActionPatterns.ComboAttackA; break;
                            case 2: comboAttackPick = ActionPatterns.ComboAttackB; break;
                        }

                        if (CheckTargetVisibility() && ActionDataSelector.EvaluateSequence(comboAttackPick))
                        {
                            ActionDataSelector.EnqueueSequence(comboAttackPick);
                            ActionDataSelector.BeginCoolTime(ActionPatterns.ComboAttackA, BB.action.comboAttackCoolTime);
                            ActionDataSelector.BeginCoolTime(ActionPatterns.ComboAttackB, BB.action.comboAttackCoolTime);
                        }
                    }
                }
            }
        }

        protected override void DamageReceiverHandler(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            base.DamageReceiverHandler(ref damageContext);

            if (damageContext.actionResult == ActionResults.Damaged)
                GameContext.Instance.damageTextManager?.Create(damageContext.finalDamage.ToString("0"), damageContext.hitPoint, 1, Color.white);
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