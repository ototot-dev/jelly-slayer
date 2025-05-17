using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using Unity.Linq;
using Retween.Rx;

namespace UGUI.Rx
{
    public class StyleSelector : TweenSelector
    {
        public virtual string Tag => string.Empty;
        public string id;
        public Template template;

        public virtual void Init(Template template, TweenName[] tweenNames)
        {
            this.template = template;

            foreach (var n in tweenNames)
                query.sources.Add(n);

            query.sources = query.sources.Distinct().ToList();
#if UNITY_EDITOR
            query.BuildSelectables();
#endif
        }

        public virtual void CleanUp()
        {
            ShowCount = PostShowCount = PostHideCount = 0;
            HideCount = 1;
        }

        public Action onFinishShow;
        public Action onFinishHide;
        public int ShowCount { get; protected set; }
        public int PostShowCount { get; protected set; }
        public int HideCount { get; protected set; }
        public int PostHideCount { get; protected set; }
        public bool IsHidden => HideCount > 0;
        readonly string __showStateName = StyleStates.show.ToString();
        readonly string __hideStateName = StyleStates.hide.ToString();

        public void Show()
        {
            if (ShowCount <= 0)
                ShowAsObservable().Subscribe();
        }

        public IObservable<StyleSelector> ShowAsObservable()
        {
            var currTweenNames = new List<IObservable<TweenName>>();
            var setupObservable = Observable.Create<StyleSelector>(observer =>
            {
                if (ShowCount <= 0)
                {
                    query.activeStates.Remove(__hideStateName);
                    query.activeStates.Add(__showStateName);
                    query.Apply();
                }

                ++ShowCount;
                HideCount = PostHideCount = 0;

                foreach (var r in Player.tweenStates)
                {
                    if (r.Key.TweenName.stateName == __showStateName)
                        currTweenNames.Add(Player.Run(r.Key).Select(_ => r.Key.TweenName));
                }

#if ENABLE_DOTWEEN_SEQUENCE
                foreach (var r in Player.dotweeenSeqRunnings) {
                    if (r.Key.TweenName.stateName == __showStr)
                        runningTweenNames.Add(Player.Run(r.Key).Select(_ => r.Key.TweenName));
                }
#endif

                observer.OnNext(this);
                observer.OnCompleted();

                return Disposable.Empty;
            });

            return setupObservable.ContinueWith(Observable.WhenAll(currTweenNames))
                .Do(_ =>
                {
                    if (ShowCount > 0 && ++PostShowCount == 1)
                        onFinishShow?.Invoke();
                })
                .Select(_ => this);
        }

        public void Hide()
        {
            if (HideCount <= 0)
                HideAsObservable().Subscribe();
        }

        public IObservable<StyleSelector> HideAsObservable()
        {
            var currTweenNames = new List<IObservable<TweenName>>();
            var setupObservable = Observable.Create<StyleSelector>(observer =>
            {
                if (HideCount <= 0)
                {
                    query.activeStates.Remove(__showStateName);
                    query.activeStates.Add(__hideStateName);
                    query.Apply();
                }

                ++HideCount;
                ShowCount = PostShowCount = 0;

                foreach (var r in Player.tweenStates)
                {
                    // Consider show-tween as hide-tween if it has runRollback set true.
                    if (r.Key.runRollback && r.Key.TweenName.stateName == __showStateName)
                        currTweenNames.Add(Player.Rollback(r.Key).Select(_ => r.Key.TweenName));

                    if (r.Key.TweenName.stateName == __hideStateName)
                        currTweenNames.Add(Player.Run(r.Key).Select(_ => r.Key.TweenName));
                }

#if ENABLE_DOTWEEN_SEQUENCE
                    // foreach (var r in Player.dotweeenSeqRunnings) {
                    //     // Consider show-tween as hide-tween if it has rewindOnCancelled set true.
                    //     if (r.Key.rewindOnCancelled && r.Key.TweenName.stateName == __showStr)
                    //         runningTweenNames.Add(Player.Rewind(r.Key).Select(_ => r.Key.TweenName));

                    //     if (r.Key.TweenName.stateName == __hideStr)
                    //         runningTweenNames.Add(Player.Run(r.Key).Select(_ => r.Key.TweenName));
                    // }
#endif

                observer.OnNext(this);
                observer.OnCompleted();

                return Disposable.Empty;
            });

            return setupObservable.ContinueWith(Observable.WhenAll(currTweenNames))
                .Do(_ =>
                {
                    if (HideCount > 0 && ++PostHideCount == 1)
                        onFinishHide?.Invoke();
                })
                .Select(_ => this);
        }

        public Template FindTemplate()
        {
            return gameObject.AncestorsAndSelf()
                .Select(a => a.GetComponent<Template>())
                .Where(t => t != null)
                .FirstOrDefault();
        }

#if UNITY_EDITOR
        public override void OnValidateInternal()
        {
            base.OnValidateInternal();

            query.usingTag = true;
            query.tag = Tag;
        }
#endif
    }
}