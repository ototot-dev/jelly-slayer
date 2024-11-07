using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Retween.Rx {

/// <summary>
/// 
/// </summary>
public class TweenAnim : MonoBehaviour {

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="AnimationClip"></typeparam>
    /// <returns></returns>
    [Header("Animation Clips")]
    public List<AnimationClip> animClips = new List<AnimationClip>();

    /// <summary>
    /// 
    /// </summary>
    public enum Loopings {
        None,
        Loop,
        PingPong,
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class Transition {
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

    /// <summary>
    /// 
    /// </summary>
    [Header("Transition")]
    public Transition transition = new Transition(false);
    
    /// <summary>
    /// 
    /// </summary>
    public bool rewindOnCancelled = false;
    
    /// <summary>
    /// 
    /// </summary>
    public bool rewindOnRollback = false;
    
    /// <summary>
    /// 
    /// </summary>
    public bool runRollback = false;
    
    /// <summary>
    /// 
    /// </summary>
    [Header("Rollback Transition")]
    public Transition rollbackTransition = new Transition(true);

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public TweenName TweenName {
        get {
            if (__tweenName == null)
                __tweenName = GetComponent<TweenName>();

            return __tweenName;
        }
    }

    TweenName __tweenName;

}
    
}
