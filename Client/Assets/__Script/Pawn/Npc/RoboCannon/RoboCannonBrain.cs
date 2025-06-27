using UGUI.Rx;
using UniRx;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(RoboCannonMovement))]
    [RequireComponent(typeof(RoboCannonBlackboard))]
    [RequireComponent(typeof(RoboCannonAnimController))]
    [RequireComponent(typeof(RoboCannonActionController))]
    public class RoboCannonBrain : NpcHumanoidBrain, IPawnTargetable
    {
        #region IPawnTargetable 구현
        PawnColliderHelper IPawnTargetable.StartTargeting() => bodyHitColliderHelper;
        PawnColliderHelper IPawnTargetable.NextTarget() => null;
        PawnColliderHelper IPawnTargetable.CurrTarget() => bodyHitColliderHelper;
        void IPawnTargetable.StopTargeting() { }
        #endregion

        public override Vector3 GetInteractionKeyAttachPoint() => BB.children.specialKeyAttachPoint.transform.position;
        public override Vector3 GetStatusBarAttachPoint() => coreColliderHelper.transform.position + coreColliderHelper.GetCapsuleHeight() * Vector3.up;
        public RoboCannonBlackboard BB { get; private set; }
        public RoboCannonMovement Movement { get; private set; }
        public RoboCannonAnimController AnimCtrler { get; private set; }
        public RoboCannonActionController ActionCtrler { get; private set; }

        protected override void AwakeInternal()
        {
            base.AwakeInternal();

            BB = GetComponent<RoboCannonBlackboard>();
            Movement = GetComponent<RoboCannonMovement>();
            AnimCtrler = GetComponent<RoboCannonAnimController>();
            ActionCtrler = GetComponent<RoboCannonActionController>();
        }

        public enum ActionPatterns : int
        {
            None = -1,
            Fire,
            Max,
        }

        protected override void StartInternal()
        {
            base.StartInternal();

            //* 액션 패턴 등록
            ActionDataSelector.ReserveSequence(ActionPatterns.Fire, "Fire").ResetProbability(1f).BeginCoolTime(BB.action.fireCoomTime);

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
            };

            onUpdate += () =>
            {
                if (!BB.IsSpawnFinished || !BB.IsInCombat || BB.IsDead)
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

                        ActionCtrler.SetPendingAction(nextActionData.actionName, string.Empty, string.Empty, ActionDataSelector.CurrSequence().GetPaddingTime());
                    }
                }
            };
        }

        protected override void OnTickInternal(float interval)
        {
            base.OnTickInternal(interval);

            if (!BB.IsSpawnFinished || !BB.IsInCombat || BB.IsDead)
                return;

            if (StatusCtrler.CheckStatus(PawnStatus.CanNotAction) || StatusCtrler.CheckStatus(PawnStatus.Staggered))
                return;

#if UNITY_EDITOR
            if (ActionDataSelector.debugActionSelectDisabled)
                return;
#endif

            if (ActionDataSelector.CurrSequence() == null && CheckTargetVisibility() && ActionDataSelector.EvaluateSequence(ActionPatterns.Fire, 1f))
            {
                ActionDataSelector.EnqueueSequence(ActionPatterns.Fire);
                ActionDataSelector.BeginCoolTime(ActionPatterns.Fire, BB.action.fireCoomTime);
            }
        }

        protected override void DamageReceiverHandler(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            base.DamageReceiverHandler(ref damageContext);

            if (damageContext.actionResult == ActionResults.Damaged)
                GameContext.Instance.damageTextManager?.Create(damageContext.finalDamage.ToString("0"), damageContext.hitPoint, 1, Color.white);
        }
    }
}
