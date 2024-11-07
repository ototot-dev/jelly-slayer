using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Retween.Rx;


/// <summary>
/// This code sample shows how to create tween GameObject and play it.
/// </summary>
public class SamplePlayTween : MonoBehaviour {

    IEnumerator Start() {
        // Make '.slide' tween from single AnimationClip resource.
        var slideTweenAnim = TweenCreator.FromAnimation(
            Resources.Load<AnimationClip>("Anim/slide-cheese-pong"),
            false, // rewindOnCancelled 
            false, // rewindOnRollback
            true, // runRollback
            ".slide" // GameObject name
            );

        // Set transition data.
        slideTweenAnim.transition.easing = TweenFunc.Easings.EaseOutBounce;
        slideTweenAnim.transition.duration = 2f;

        // Set rollback-transition data.
        slideTweenAnim.rollbackTransition.easing = TweenFunc.Easings.Linear;
        slideTweenAnim.rollbackTransition.duration = 1f;

        // Add TweenPlayer component.
        var tweenPlayer = gameObject.AddComponent<TweenPlayer>();

        Debug.Log("Run '.slide' tween~");

        // Run '.slide' tween and wait it ends.
        yield return tweenPlayer.Run(slideTweenAnim).ToYieldInstruction();

        Debug.Log("Rollback '.slide' tween~");

        // Rollback '.slide' tween and wait it ends.
        yield return tweenPlayer.Rollback(slideTweenAnim).ToYieldInstruction();
            
        // Make '.fade-slide' from multiple AnimationClips resource.
        var fadeSlideTweenAnim = TweenCreator.FromAnimation(
            new AnimationClip[] {
                Resources.Load<AnimationClip>("Anim/fade-cheese-pong"),
                Resources.Load<AnimationClip>("Anim/slide-cheese-pong"),
            },
            true, // rewindOnCancelled 
            false, // rewindOnRollback
            false, // runRollback
            ".fade-slide" // GameObject name
            );
            
        // Set transition data.
        fadeSlideTweenAnim.transition.easing = TweenFunc.Easings.EaseOutBounce;
        fadeSlideTweenAnim.transition.duration = 2f;

        Debug.Log("Run '.fade-slide' tween~");

        // Run '.fade-slide' tween'.
        tweenPlayer.Run(fadeSlideTweenAnim).Subscribe();

        Debug.Log("wait for 1 second~");

        // Wait 1 second. '.fade-slide' tween progresses to 50%.
        yield return new WaitForSeconds(1f);
        
        Debug.Log("Rewind '.fade-slide' tween~");

        // Rewind '.fade-slide' tween which make it being played in backwards (from 50% to 0%).
        tweenPlayer.Rewind(fadeSlideTweenAnim).Subscribe();
    }
        
}
