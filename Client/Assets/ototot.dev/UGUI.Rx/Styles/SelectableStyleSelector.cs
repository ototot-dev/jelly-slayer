using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using Retween.Rx;

namespace UGUI.Rx
{
    public class SelectableStyleSelector : StyleSelector
    {
        public bool IsMouseHover { get; protected set; }

        public override void Init(Template owner, TweenName[] tweenNames)
        {
            base.Init(owner, tweenNames);

            var selectable = GetComponent<Selectable>();
            Debug.Assert(selectable != null);

            if (selectable.interactable)
            {
                query.activeStates.Add(__enabledStr);
                query.activeStates.Add(__normalStr);
            }
            else
            {
                query.activeStates.Add(__disabledStr);
            }

            __disposables.Add(
                gameObject.ObserveEveryValueChanged(_ => selectable.interactable).Skip(1).Subscribe(v =>
                {
                    if (v)
                    {
                        query.activeStates.Remove(__disabledStr);
                        query.activeStates.Add(__enabledStr);
                        query.activeStates.Add(__normalStr);
                    }
                    else
                    {
                        query.activeStates.Remove(__enabledStr);
                        query.activeStates.Remove(__normalStr);
                        query.activeStates.Remove(__hoverStr);
                        query.activeStates.Remove(__pressedStr);
                        query.activeStates.Add(__disabledStr);
                    }

                    query.Apply();
                }).AddTo(this));

            __disposables.Add(
                selectable.OnPointerEnterAsObservable().Where(_ => selectable.interactable).Subscribe(_ =>
                {
                    query.activeStates.Remove(__normalStr);
                    query.activeStates.Add(__hoverStr);
                    query.Apply();

                    IsMouseHover = true;
                }).AddTo(this));

            __disposables.Add(
                selectable.OnPointerExitAsObservable().Where(_ => selectable.interactable).Subscribe(_ =>
                {
                    query.activeStates.Remove(__hoverStr);
                    query.activeStates.Add(__normalStr);
                    query.Apply();

                    IsMouseHover = false;
                }).AddTo(this));

            __disposables.Add(
                selectable.OnPointerDownAsObservable().Where(_ => selectable.interactable).Subscribe(_ =>
                {
                    query.activeStates.Remove(__normalStr);
                    query.activeStates.Remove(__hoverStr);
                    query.activeStates.Add(__pressedStr);
                    query.Apply();
                }).AddTo(this));

            __disposables.Add(
                selectable.OnPointerUpAsObservable().Where(_ => selectable.interactable).Subscribe(_ =>
                {
                    query.activeStates.Remove(__pressedStr);
                    query.activeStates.Add(IsMouseHover ? __hoverStr : __normalStr);
                    query.Apply();
                }).AddTo(this));
        }

        List<IDisposable> __disposables = new List<IDisposable>();
        readonly string __enabledStr = StyleStates.enabled.ToString();
        readonly string __normalStr = StyleStates.normal.ToString();
        readonly string __disabledStr = StyleStates.disabled.ToString();
        readonly string __hoverStr = StyleStates.hover.ToString();
        readonly string __pressedStr = StyleStates.pressed.ToString();

        public override void CleanUp()
        {
            base.CleanUp();

            foreach (var d in __disposables)
                d.Dispose();

            __disposables.Clear();
        }
    }
}