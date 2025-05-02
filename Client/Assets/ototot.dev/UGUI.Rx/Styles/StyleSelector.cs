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
        public int HideCount { get; protected set; } = 1; //* 최초엔 Hidden 상태로 만들기 위해서 디폴트 값을 1로 셋팅함
        public int PostHideCount { get; protected set; }
        public bool IsHidden => HideCount > 0;
        readonly string __showStr = StyleStates.show.ToString();
        readonly string __hideStr = StyleStates.hide.ToString();

        public void Show()
        {
            if (IsHidden)
                ShowAsObservable().Subscribe();
        }

        public IObservable<StyleSelector> ShowAsObservable()
        {
            var runningTweenNames = new List<IObservable<TweenName>>();
            var setupObservable = Observable.Create<StyleSelector>(observer =>
            {
                if (IsHidden)
                {
                    query.activeStates.Remove(__hideStr);
                    query.activeStates.Add(__showStr);
                    query.Apply();
                }

                ++ShowCount;
                HideCount = PostHideCount = 0;

                foreach (var r in Player.animRunnings)
                {
                    if (r.Key.TweenName.stateName == __showStr)
                        runningTweenNames.Add(Player.Run(r.Key).Select(_ => r.Key.TweenName));
                }

                // foreach (var r in Player.dotweeenSeqRunnings) {
                //     if (r.Key.TweenName.stateName == __showStr)
                //         runningTweenNames.Add(Player.Run(r.Key).Select(_ => r.Key.TweenName));
                // }

                observer.OnNext(this);
                observer.OnCompleted();

                return Disposable.Empty;
            });

            return setupObservable.ContinueWith(Observable.WhenAll(runningTweenNames))
                .Do(_ =>
                {
                    if (ShowCount > 0 && ++PostShowCount == 1)
                        onFinishShow?.Invoke();
                })
                .Select(_ => this);
        }

        public void Hide()
        {
            if (!IsHidden)
                HideAsObservable().Subscribe();
        }

        public IObservable<StyleSelector> HideAsObservable()
        {
            var runningTweenNames = new List<IObservable<TweenName>>();

            var setupObservable = Observable.Create<StyleSelector>(observer =>
            {
                if (!IsHidden)
                {
                    query.activeStates.Remove(__showStr);
                    query.activeStates.Add(__hideStr);
                    query.Apply();
                }

                ++HideCount;
                ShowCount = PostShowCount = 0;

                foreach (var r in Player.animRunnings)
                {
                    // Consider show-tween as hide-tween if it has runRollback set true.
                    if (r.Key.runRollback && r.Key.TweenName.stateName == __showStr)
                        runningTweenNames.Add(Player.Rollback(r.Key).Select(_ => r.Key.TweenName));

                    if (r.Key.TweenName.stateName == __hideStr)
                        runningTweenNames.Add(Player.Run(r.Key).Select(_ => r.Key.TweenName));
                }

                // foreach (var r in Player.dotweeenSeqRunnings) {
                //     // Consider show-tween as hide-tween if it has rewindOnCancelled set true.
                //     if (r.Key.rewindOnCancelled && r.Key.TweenName.stateName == __showStr)
                //         runningTweenNames.Add(Player.Rewind(r.Key).Select(_ => r.Key.TweenName));

                //     if (r.Key.TweenName.stateName == __hideStr)
                //         runningTweenNames.Add(Player.Run(r.Key).Select(_ => r.Key.TweenName));
                // }

                observer.OnNext(this);
                observer.OnCompleted();

                return Disposable.Empty;
            });

            return setupObservable.ContinueWith(Observable.WhenAll(runningTweenNames))
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