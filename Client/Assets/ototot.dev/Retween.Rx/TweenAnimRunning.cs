using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Retween.Rx {

/// <summary>
/// 
/// </summary>
public class TweenAnimRunning {

    /// <summary>
    /// ctor.
    /// </summary>
    /// <param name="source"></param>
    public TweenAnimRunning(TweenAnim source) {
        this.source = source;
    }

    /// <summary>
    /// 
    /// </summary>
    public int runNum;

    /// <summary>
    /// 
    /// </summary>
    public TweenAnim source;

    /// <summary>
    /// 
    /// </summary>
    public float duration;

    /// <summary>
    /// 
    /// </summary>
    public float elapsed;

    /// <summary>
    /// 
    /// </summary>
    public bool animEnabled;

    /// <summary>
    /// 
    /// </summary>
    public bool IsRunning => animEnabled && !IsRewinding && !IsRollBack;

    /// <summary>
    /// 
    /// </summary>
    public bool IsRewinding { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool IsRollBack { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool togglePingPong;

}

}