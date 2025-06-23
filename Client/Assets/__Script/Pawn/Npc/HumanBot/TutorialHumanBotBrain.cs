using Game.UI;
using UGUI.Rx;
using UniRx;
using UnityEngine;

namespace Game
{
    public class TutorialHumanBotBrain : HumanBotBrain
    {
        [Header("Tutorial")]
        public BoolReactiveProperty tutorialMoveEnabled = new();
        public BoolReactiveProperty tutorialGuardEnabled = new();
        public BoolReactiveProperty tutorialActionEnabled = new();
        public BoolReactiveProperty tutorialComboAttackEnbaled = new();
        public BoolReactiveProperty tutorialCounterAttackEnbaled = new();
        public BoolReactiveProperty tutorialShieldAttackEnbaled = new();
        public BoolReactiveProperty tutorialJumpActtackEnbaled = new();

        protected override void StartInternal()
        {
            base.StartInternal();

            new BossStatusBarController(this).Load(GameObject.FindFirstObjectByType<BossStatusBarTemplate>()).Show(GameContext.Instance.canvasManager.body.transform as RectTransform);
            

            void __ChangeStatus(PawnStatus status, bool v)
            {
                if (v)
                    StatusCtrler.AddStatus(status);
                else
                    StatusCtrler.RemoveStatus(status);
            }

            tutorialMoveEnabled.Subscribe(v => __ChangeStatus(PawnStatus.CanNotMove, !v)).AddTo(this);
            tutorialGuardEnabled.Subscribe(v => __ChangeStatus(PawnStatus.CanNotGuard, !v)).AddTo(this);
            tutorialActionEnabled.Subscribe(v => __ChangeStatus(PawnStatus.CanNotAction, !v)).AddTo(this);

            tutorialComboAttackEnbaled.Subscribe(v =>
            {
                ActionDataSelector.GetSequence(ActionPatterns.ComboAttackA).ResetProbability(v ? 1f : 0f);
                ActionDataSelector.GetSequence(ActionPatterns.ComboAttackB).ResetProbability(v ? 1f : 0f);
                ActionDataSelector.GetSequence(ActionPatterns.ComboAttackC).ResetProbability(v ? 1f : 0f);
            }).AddTo(this);

            tutorialCounterAttackEnbaled.Subscribe(v =>
            {
                ActionDataSelector.GetSequence(ActionPatterns.CounterA).ResetProbability(v ? 1f : 0f);
                ActionDataSelector.GetSequence(ActionPatterns.CounterB).ResetProbability(v ? 1f : 0f);
            }).AddTo(this);

            tutorialShieldAttackEnbaled.Subscribe(v =>
            {
                ActionDataSelector.GetSequence(ActionPatterns.ShieldAttackA).ResetProbability(v ? 1f : 0f);
                ActionDataSelector.GetSequence(ActionPatterns.ShieldAttackB).ResetProbability(v ? 1f : 0f);
            }).AddTo(this);

            tutorialCounterAttackEnbaled.Subscribe(v =>
            {
                ActionDataSelector.GetSequence(ActionPatterns.JumpAttack).ResetProbability(v ? 1f : 0f);
            }).AddTo(this);
        }
    }
}
