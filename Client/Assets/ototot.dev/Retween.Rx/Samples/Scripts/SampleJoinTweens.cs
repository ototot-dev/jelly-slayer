using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Retween.Rx;


/// <summary>
/// This code sample shows how to combine tweens with IObservable interface.
/// </summary>
public class SampleJoinTweens : MonoBehaviour {

    void Start() {
        // Make '.slide' tween.
        var slideTween = TweenCreator.FromAnimation(
            Resources.Load<AnimationClip>("Anim/slide-cheese-pong"),
            true, // rewindOnCancelled 
            true, // rewindOnRollback
            true, // runRollback
            ".slide" // GameObject name
            );

        // Set transition data.
        slideTween.transition.easing = TweenFunc.Easings.EaseInQuad;
        slideTween.transition.duration = 1f;
    
        // Make '.fade' tween.
        var fadeTween = TweenCreator.FromAnimation(
            Resources.Load<AnimationClip>("Anim/fade-cheese-pong"),
            true, // rewindOnCancelled 
            true, // rewindOnRollback
            true, // runRollback
            ".fade" // GameObject name
            );
            
        // Set transition data.
        fadeTween.transition.easing = TweenFunc.Easings.Linear;
        fadeTween.transition.duration = 2f;

        // Add TweenPlayer component.
        var tweenPlayer = gameObject.AddComponent<TweenPlayer>();
        
        Debug.Log("Run '.slide' and '.fade' tween~");

        // Run '.slide' and '.fade' tween together and wait all end.
        // wait 1 second and then rollback '.slide' and '.fade' tween.
        Observable.WhenAll(new IObservable<TweenAnimRunning>[] {
            tweenPlayer.Run(slideTween),
            tweenPlayer.Run(fadeTween)
        }).ContinueWith(_ => {
            Debug.Log("Wait for 1 second~");

            return Observable.Timer(TimeSpan.FromSeconds(1f));
        }).Subscribe(_ => {
            tweenPlayer.Rollback(slideTween).Subscribe();
            tweenPlayer.Rollback(fadeTween).Subscribe();
            
            Debug.Log("Rollback '.slide' and '.fade' tween~");
        });

    }
        
}
