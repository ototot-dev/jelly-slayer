using System;
using System.Collections.Generic;
using System.Linq;
using FIMSpace.FProceduralAnimation;
using UniRx;
using UniRx.Triggers.Extension;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Game
{
    public class PawnAnimController : MonoBehaviour
    {
        [Header("Component")]
        public Animator mainAnimator;
        public RigBuilder rigBuilder;
        public Rig rigSetup;
        public LegsAnimator legAnimator;
        public RagdollAnimator2 ragdollAnimator;
        public Action<AnimatorStateInfo, int> onAnimStateEnter;
        public Action<AnimatorStateInfo, int> onAnimStateExit;

        readonly Dictionary<string, ObservableStateMachineTriggerEx> __observableStateMachineTriggersCached = new();
        public ObservableStateMachineTriggerEx FindObservableStateMachineTriggerEx(string stateName)
        {
            if (__observableStateMachineTriggersCached.TryGetValue(stateName, out var ret))
                return ret;
            
            var found = mainAnimator.GetBehaviours<ObservableStateMachineTriggerEx>().First(s => s.stateName == stateName);
            __observableStateMachineTriggersCached.Add(stateName, found);

            return found;
        }
    
        public virtual void OnAnimatorMoveHandler() {}
        public virtual void OnAnimatorStateEnterHandler(AnimatorStateInfo stateInfo, int layerIndex) { onAnimStateEnter?.Invoke(stateInfo, layerIndex); }
        public virtual void OnAniamtorStateExitHandler(AnimatorStateInfo stateInfo, int layerIndex) { onAnimStateExit?.Invoke(stateInfo, layerIndex); }
        public virtual void OnAnimatorFootHandler(bool isRight) {}
    }
}