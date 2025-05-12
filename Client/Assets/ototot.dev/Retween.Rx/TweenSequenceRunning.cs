using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_DOTWEEN_SEQUENCE
using DG.Tweening;
#endif


namespace Retween.Rx {

public class TweenSequenceState {
    
    /// <summary>
    /// ctor.
    /// </summary>
    /// <param name="sourceAnim"></param>
    public TweenSequenceState(TweenSequence source) {
        this.source = source;
    }

    /// <summary>
    /// 
    /// </summary>
    public int runNum;

    /// <summary>
    /// 
    /// </summary>
    public TweenSequence source;

    /// <summary>
    /// 
    /// </summary>
    public bool isRewinding;


#if ENABLE_DOTWEEN_SEQUENCE
    /// <summary>
    /// 
    /// </summary>
    public DG.Tweening.Sequence dotweenSeq;
#endif

}

}