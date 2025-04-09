using UniRx.Triggers;
using UnityEngine;

namespace UniRx.Triggers.Extension
{
    public class ObservableStateMachineTriggerEx : ObservableStateMachineTrigger
    {
        public string stateName;
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            if (animator.TryGetComponent<PawnAnimatorHandler>(out var animHandler))
                animHandler.OnAnimatorStateEnter(stateInfo, layerIndex);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);

            if (animator.TryGetComponent<PawnAnimatorHandler>(out var animHandler))
                animHandler.OnAniamtorStateExit(stateInfo, layerIndex);
        }
    }
}