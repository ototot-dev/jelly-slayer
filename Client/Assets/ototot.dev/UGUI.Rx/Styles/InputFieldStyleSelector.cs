using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using Retween.Rx;

namespace UGUI.Rx
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(TweenPlayer))]
    [RequireComponent(typeof(InputField))]
    public class InputFieldStyleSelector : StyleSelector
    {
        public override string Tag => "inputfield";
        public bool IsHover { get; protected set; }
        public StringReactiveProperty RegexPattern = new StringReactiveProperty(string.Empty);
        Regex __regex;

        public override void Init(Template owner, TweenName[] tweenNames)
        {
            base.Init(owner, tweenNames);

            var inputField = GetComponent<InputField>();

            query.activeStates.Add(inputField.interactable ? __normalStr : __disabledStr);
            query.activeStates.Add(__emptyStr);
            query.Apply();

            var skipFirst = true;

            var disposable = gameObject.ObserveEveryValueChanged(_ => inputField.interactable)
                .Where(_ => !skipFirst)
                .Subscribe(v =>
                {
                    if (v)
                    {
                        query.activeStates.Remove(__disabledStr);
                        query.activeStates.Add(__normalStr);
                    }
                    else
                    {
                        query.activeStates.Remove(__normalStr);
                        query.activeStates.Remove(__hoverStr);
                        query.activeStates.Add(__disabledStr);
                    }

                    query.Apply();
                })
                .AddTo(this);

            __disposables.Add(disposable);

            disposable = inputField.OnPointerEnterAsObservable()
                .Where(_ => !skipFirst && inputField.interactable)
                .Subscribe(_ =>
                {
                    query.activeStates.Remove(__normalStr);
                    query.activeStates.Add(__hoverStr);

                    query.Apply();

                    IsHover = true;
                })
                .AddTo(this);

            __disposables.Add(disposable);

            disposable = inputField.OnPointerExitAsObservable()
                .Where(_ => !skipFirst && inputField.interactable)
                .Subscribe(_ =>
                {
                    query.activeStates.Remove(__hoverStr);
                    query.activeStates.Add(__normalStr);
                    query.Apply();

                    IsHover = false;
                })
                .AddTo(this);

            __disposables.Add(disposable);

            if (!string.IsNullOrEmpty(RegexPattern.Value))
            {
                try
                {
                    __regex = new Regex(RegexPattern.Value);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"InputFieldStyleSelector => RegexPattern - {RegexPattern.Value} - is invalid!! => {e.ToString()}");
                }
            }

            disposable = inputField.OnValueChangedAsObservable()
                .Where(_ => !skipFirst && inputField.interactable && __regex != null)
                .Subscribe(v =>
                {
                    if (string.IsNullOrEmpty(v))
                    {
                        query.activeStates.Remove(__matchedStr);
                        query.activeStates.Remove(__unmatchedStr);
                        query.activeStates.Add(__emptyStr);
                    }
                    else
                    {
                        query.activeStates.Remove(__matchedStr);
                        query.activeStates.Remove(__unmatchedStr);

                        var isMatched = (__regex.Match(v)?.Length ?? -1) == v.Length;

                        query.activeStates.Add(isMatched ? __matchedStr : __unmatchedStr);
                        query.activeStates.Remove(__emptyStr);
                    }

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
        readonly string __emptyStr = StyleStates.empty.ToString();
        readonly string __matchedStr = StyleStates.matched.ToString();
        readonly string __unmatchedStr = StyleStates.unmatched.ToString();

        public override void CleanUp()
        {
            base.CleanUp();

            foreach (var d in __disposables)
                d.Dispose();

            __disposables.Clear();
        }
    }
}