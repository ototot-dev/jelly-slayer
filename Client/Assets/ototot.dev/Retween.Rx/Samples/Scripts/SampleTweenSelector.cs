using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Retween.Rx;


/// <summary>
/// This code sample shows how to match tweens via TweenSelector component.
/// </summary>
public class SampleTweenSelector : MonoBehaviour {

    void Start() {
        // Get TweenSelector component.
        var tweenSelector = GetComponent<TweenSelector>();

        // Add 'show' state.
        tweenSelector.query.activeStates.Add("show");

        // Apply it.
        tweenSelector.query.Execute();
    }
        
}
