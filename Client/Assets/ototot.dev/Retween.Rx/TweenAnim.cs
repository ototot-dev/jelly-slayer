using System;
using System.Collections.Generic;
using UnityEngine;

namespace Retween.Rx
{
    public class TweenAnim : MonoBehaviour
    {
        public enum Loopings
        {
            None,
            Loop,
            PingPong,
        }

        [Header("Animation Clips")]
        public List<AnimationClip> animClips = new();

        [Serializable]
        public class Transition
        {
            public Transition(bool isRollback) { this.isRollback = isRollback; }
            public Loopings loop = Loopings.None;
            public TweenFunc.Easings easing = TweenFunc.Easings.Linear;
            public float duration = 1f;
            public bool isRollback = false;
            public bool IsLooping => loop != Loopings.None;

#if UNITY_EDITOR
            public AnimationCurve easingCurve = AnimationCurve.Linear(0, 0, 1, 1);
#endif
        }

        [Header("Transition")]
        public Transition transition = new(false);
        public bool rewindOnCancelled = false;
        public bool rewindOnRollback = false;
        public bool runRollback = false;

        [Header("Rollback Transition")]
        public Transition rollbackTransition = new(true);

        public TweenName TweenName
        {
            get
            {
                if (__tweenName == null)
                    __tweenName = GetComponent<TweenName>();
                return __tweenName;
            }
        }

        TweenName __tweenName;
    }
}
