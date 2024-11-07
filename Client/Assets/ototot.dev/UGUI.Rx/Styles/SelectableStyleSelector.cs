using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using Retween.Rx;

namespace UGUI.Rx {

/// <summary>
/// 
/// </summary>
public class SelectableStyleSelector : StyleSelector {

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public bool IsHover { get; protected set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="tweenNames"></param>
    public override void Init(Template owner, TweenName[] tweenNames) {
        base.Init(owner, tweenNames);
        
        var selectable = GetComponent<Selectable>();

        if (selectable.interactable) {
            query.activeStates.Add(__enabledStr);
            query.activeStates.Add(__normalStr);
        }
        else {
            query.activeStates.Add(__disabledStr);
        }

        var skipFirst = true;

        var disposable = gameObject.ObserveEveryValueChanged(_ => selectable.interactable)
            .Where(_ => !skipFirst)
            .Subscribe(v => {   
                if (v) {
                    query.activeStates.Remove(__disabledStr);
                    query.activeStates.Add(__enabledStr);
                    query.activeStates.Add(__normalStr);
                }
                else {
                    query.activeStates.Remove(__enabledStr);
                    query.activeStates.Remove(__normalStr);
                    query.activeStates.Remove(__hoverStr);
                    query.activeStates.Remove(__pressedStr);
                    query.activeStates.Add(__disabledStr);
                }

                query.Apply();
            })
            .AddTo(this);

        __disposables.Add(disposable);

        disposable = selectable.OnPointerEnterAsObservable()
            .Where(_ => !skipFirst && selectable.interactable)
            .Subscribe(_ => {
                query.activeStates.Remove(__normalStr);
                query.activeStates.Add(__hoverStr);
                query.Apply();

                IsHover = true;
            }).AddTo(this);

        __disposables.Add(disposable);

        disposable = selectable.OnPointerExitAsObservable()
            .Where(_ => !skipFirst && selectable.interactable)
            .Subscribe(_ => {
                query.activeStates.Remove(__hoverStr);
                query.activeStates.Add(__normalStr);
                query.Apply();

                IsHover = false;
            }).AddTo(this);

        __disposables.Add(disposable);

        disposable = selectable.OnPointerDownAsObservable()
            .Where(_ => !skipFirst && selectable.interactable)
            .Subscribe(_ => {
                query.activeStates.Remove(__normalStr);
                query.activeStates.Remove(__hoverStr);
                query.activeStates.Add(__pressedStr);
                query.Apply();
            })
            .AddTo(this);

        __disposables.Add(disposable);

        disposable = selectable.OnPointerUpAsObservable()
            .Where(_ => !skipFirst && selectable.interactable)
            .Subscribe(_ => {
                query.activeStates.Remove(__pressedStr);
                query.activeStates.Add(IsHover ? __hoverStr : __normalStr);
                query.Apply();
            })
            .AddTo(this);

        __disposables.Add(disposable);

        skipFirst = false;
    }

    List<IDisposable> __disposables = new List<IDisposable>();
    readonly string __enabledStr = StyleStates.enabled.ToString();
    readonly string __normalStr = StyleStates.normal.ToString();
    readonly string __disabledStr = StyleStates.disabled.ToString();
    readonly string __hoverStr = StyleStates.hover.ToString();
    readonly string __pressedStr = StyleStates.pressed.ToString();

    /// <summary>
    /// 
    /// </summary>
    public override void CleanUp() {
        base.CleanUp();

        foreach (var d in __disposables)
            d.Dispose();

        __disposables.Clear();
    }

}

}