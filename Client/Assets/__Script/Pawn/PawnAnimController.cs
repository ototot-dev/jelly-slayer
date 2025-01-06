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
        public LegsAnimator legAnimator;
        public RagdollAnimator2 ragdollAnimator;
        Dictionary<string, ObservableStateMachineTriggerEx> __observableStateMachineTriggersCached = new();
        
        public ObservableStateMachineTriggerEx FindObservableStateMachineTriggerEx(string stateName)
        {
            if (__observableStateMachineTriggersCached.TryGetValue(stateName, out var ret))
                return ret;
            
            var found = mainAnimator.GetBehaviours<ObservableStateMachineTriggerEx>().First(s => s.stateName == stateName);
            __observableStateMachineTriggersCached.Add(stateName, found);

            return found;
        }
    }
}