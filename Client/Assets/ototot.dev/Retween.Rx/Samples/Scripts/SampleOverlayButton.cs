using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

namespace Retween.Rx {

public class SampleOverlayButton : MonoBehaviour {

    public enum ButtonStates : int {
        hover,
        pressed,
    }
    
    readonly string __hoverStr = ButtonStates.hover.ToString();
    readonly string __pressedStr = ButtonStates.pressed.ToString();

    public bool IsHover { get; private set; }

    public void Start() {
        var button = GetComponent<Button>();
        var tweenSelector = GetComponent<TweenSelector>();

        button.OnPointerEnterAsObservable()
            .Where(_ => button.interactable)
            .Subscribe(_ => {
                tweenSelector.query.activeStates.Add(__hoverStr);
                tweenSelector.query.Apply();

                IsHover = true;
            }).AddTo(this);

        button.OnPointerExitAsObservable()
            .Where(_ => button.interactable)
            .Subscribe(_ => {
                tweenSelector.query.activeStates.Remove(__hoverStr);
                tweenSelector.query.Apply();

                IsHover = false;
            }).AddTo(this);

        button.OnPointerDownAsObservable()
            .Where(_ => button.interactable)
            .Subscribe(_ => {
                tweenSelector.query.activeStates.Remove(__hoverStr);
                tweenSelector.query.activeStates.Add(__pressedStr);
                tweenSelector.query.Apply();
            })
            .AddTo(this);

        button.OnPointerUpAsObservable()
            .Where(_ => button.interactable)
            .Subscribe(_ => {
                tweenSelector.query.activeStates.Remove(__pressedStr);
                if (IsHover)
                    tweenSelector.query.activeStates.Add(__hoverStr);
                tweenSelector.query.Apply();
            })
            .AddTo(this);
    }

}

}