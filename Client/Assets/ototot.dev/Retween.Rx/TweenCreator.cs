using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Retween.Rx {

/// <summary>
/// 
/// </summary>
public static class TweenCreator {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="rewindOnCancelled"></param>
    /// <param name="rewindOnRollback"></param>
    /// <param name="runRollback"></param>
    /// <param name="name"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public static TweenAnim FromAnimation(AnimationClip clip, bool rewindOnCancelled = false, bool rewindOnRollback = false, bool runRollback = false, string name = null, Transform parent = null) {
        var tweenName = new GameObject(name).AddComponent<TweenName>();

        if (!string.IsNullOrEmpty(name))
            tweenName.Parse();

        var tweenAnim = tweenName.gameObject.AddComponent<TweenAnim>();

        tweenAnim.rewindOnCancelled = rewindOnCancelled;
        tweenAnim.rewindOnRollback = rewindOnRollback;
        tweenAnim.runRollback = runRollback;
        
        tweenAnim.animClips.Add(clip);
        tweenAnim.gameObject.SetActive(false);

        if (parent != null) {
            tweenAnim.transform.SetParent(parent, false);
            tweenAnim.transform.SetSiblingIndex(0);
        }

        return tweenAnim;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="clips"></param>
    /// <param name="rewindOnCancelled"></param>
    /// <param name="rewindOnRollback"></param>
    /// <param name="runRollback"></param>
    /// <param name="name"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public static TweenAnim FromAnimation(AnimationClip[] clips, bool rewindOnCancelled = false, bool rewindOnRollback = false, bool runRollback = false, string name = null, Transform parent = null) {
        var tweenName = new GameObject(name).AddComponent<TweenName>();

        if (!string.IsNullOrEmpty(name))
            tweenName.Parse();

        var tweenAnim = tweenName.gameObject.AddComponent<TweenAnim>();

        tweenAnim.rewindOnCancelled = rewindOnCancelled;
        tweenAnim.rewindOnRollback = rewindOnRollback;
        tweenAnim.runRollback = runRollback;
        
        tweenAnim.animClips.AddRange(clips);
        tweenAnim.gameObject.SetActive(false);

        if (parent != null) {
            tweenAnim.transform.SetParent(parent, false);
            tweenAnim.transform.SetSiblingIndex(0);
        }
        
        return tweenAnim;
    }

#if ENABLE_DOTWEEN_SEQUENCE

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dotweenSeqMaker"></param>
    /// <param name="reinitializeOnRun"></param>
    /// <param name="rewindOnCancelled"></param>
    /// <param name="rewindOnRollback"></param>
    /// <param name="name"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public static TweenSequence FromDotween(Func<GameObject, DG.Tweening.Sequence> dotweenSeqMaker, bool reinitializeOnRun = false, bool rewindOnCancelled = false, bool rewindOnRollback = false, string name = null, Transform parent = null) {
       var tweenName = new GameObject(name).AddComponent<TweenName>();

        if (!string.IsNullOrEmpty(name))
            tweenName.Parse();

        var tweenSeq = tweenName.gameObject.AddComponent<TweenSequence>();

        tweenSeq.reinitializeOnRun = reinitializeOnRun;
        tweenSeq.rewindOnCancelled = rewindOnCancelled;
        tweenSeq.rewindOnRollback = rewindOnRollback;
        
        tweenSeq.dotweenSeqMaker = dotweenSeqMaker;
        tweenSeq.gameObject.SetActive(false);

        if (parent != null) {
            tweenSeq.transform.SetParent(parent, false);
            tweenSeq.transform.SetSiblingIndex(0);
        }
        
        return tweenSeq;
    }

#endif
    
}

}