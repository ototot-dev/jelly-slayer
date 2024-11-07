using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_DOTWEEN_SEQUENCE
using DG.Tweening;
#endif


namespace Retween.Rx {

/// <summary>
/// 
/// </summary>
public class TweenSequence : MonoBehaviour {

    /// <summary>
    /// 
    /// </summary>
    public bool reinitializeOnRun;

    /// <summary>
    /// 
    /// </summary>
    public bool rewindOnCancelled;

    /// <summary>
    /// 
    /// </summary>
    public bool rewindOnRollback;


#if ENABLE_DOTWEEN_SEQUENCE
    /// <summary>
    /// 
    /// /// </summary>
    /// <value></value>
    public Func<GameObject, DG.Tweening.Sequence> dotweenSeqMaker;
#endif

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