using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using Unity.Linq;
using Retween.Rx;

namespace UGUI.Rx
{

    /// <summary>
    /// 
    /// </summary>
    public class Template : MonoBehaviour
    {

        public Controller ctrler;

        [HideInInspector]
        public Template outer;

        [HideInInspector]
        public string poolingName;

        /// <summary>
        /// Return true if the Template object is embedded in another Template object.
        /// </summary>
        /// <returns></returns>
        public bool IsEmbedded => outer != null;
        public List<Template> Embeddeds { get; private set; }
        public bool SetHidden { get; private set; } = true;

        public List<StyleSelector> showTweens = new();
        public List<StyleSelector> hideTweens = new();

        public IObservable<Template> ShowAsObservable()
        {
            if (showTweens.Count > 0)
            {
                var setupObservable = Observable.Create<Template>(observer =>
                {
                    if (SetHidden)
                    {
                        PreventBlinkOnShow();
                        SetHidden = false;
                    }

                    observer.OnNext(this);
                    observer.OnCompleted();

                    return Disposable.Empty;
                });

                return setupObservable.ContinueWith(
                    Observable.WhenAll(showTweens.Select(t => t.ShowAsObservable())).Select(_ => this)
                    );
            }
            else
            {
                return Observable.Create<Template>(observer =>
                {
                    SetHidden = false;

                    observer.OnNext(this);
                    observer.OnCompleted();

                    return Disposable.Empty;
                });
            }
        }

        /// <summary>
        /// Forces to set Canvas alpha value to 0 on the first frame which prevents the blinking screen on the show transition.
        /// </summary>
        void PreventBlinkOnShow()
        {
            if (TryGetComponent<CanvasGroup>(out var canvasGroup))
            {
                var prevAlpha = canvasGroup.alpha;
                canvasGroup.alpha = 0f;

                Observable.NextFrame().Subscribe(_ => canvasGroup.alpha = prevAlpha).AddTo(this);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IObservable<Template> HideAsObservable()
        {
            if (hideTweens.Count > 0)
            {
                var setupObservable = Observable.Create<Template>(observer =>
                {
                    SetHidden = true;

                    observer.OnNext(this);
                    observer.OnCompleted();

                    return Disposable.Empty;
                });

                return setupObservable.ContinueWith(
                    Observable.WhenAll(hideTweens.Select(t => t.HideAsObservable())).Select(_ => this)
                    );
            }
            else
            {
                return Observable.Create<Template>(observer =>
                {
                    SetHidden = true;
                    observer.OnNext(this);
                    observer.OnCompleted();

                    return Disposable.Empty;
                });
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetComponentById<T>(string val) where T : Component
        {
            var query = gameObject.DescendantsAndSelf()
                .Select(d => d.GetComponent<StyleSelector>())
                .Where(s => s != null && s.template == this && s.GetComponent<T>() != null && s.id == val)
                .Select(s => s.GetComponent<T>());

            return query.FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetComponentByClass<T>(string val) where T : Component
        {
            var query = gameObject.DescendantsAndSelf()
                .Select(d => d.GetComponent<StyleSelector>())
                .Where(s => s != null && s.template == this && s.GetComponent<T>() != null && s.query.activeClasses.Contains(val))
                .Select(s => s.GetComponent<T>());

            return query.FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> GetComponentsByClass<T>(string val) where T : Component
        {
            var query = gameObject.DescendantsAndSelf()
                .Where(d => d.GetComponent<StyleSelector>() != null)
                .Select(d => d.GetComponent<StyleSelector>())
                .Where(s => s != null && s.template == this && s.GetComponent<T>() != null && s.query.activeClasses.Contains(val))
                .Select(s => s.GetComponent<T>());

            return query.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetComponentByTag<T>(string val) where T : Component
        {
            var query = gameObject.DescendantsAndSelf()
                .Select(d => d.GetComponent<StyleSelector>())
                .Where(s => s != null && s.template == this && s.GetComponent<T>() != null && s.Tag == val)
                .Select(s => s.GetComponent<T>());

            return query.FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> GetComponentsByTag<T>(string val) where T : Component
        {
            var query = gameObject.DescendantsAndSelf()
                .Select(d => d.GetComponent<StyleSelector>())
                .Where(s => s != null && s.template == this && s.GetComponent<T>() != null && s.Tag == val)
                .Select(s => s.GetComponent<T>());

            return query.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Template[] GetEmbeddedTemplates()
        {
            var query = gameObject.Descendants()
                .Select(d => d.GetComponent<Template>())
                .Where(t => t != null && t.outer == this);

            return query.ToArray();
        }

        // /// <summary>
        // /// 
        // /// </summary>
        // /// <typeparam name="T"></typeparam>
        // /// <returns></returns>
        // public T GetAncestor<T>() where T : Controller {
        //     var query = gameObject
        //         .Ancestors()
        //         .Select(a => a.GetComponent<Template>())
        //         .Where(t => t != null && t.Owner is T );

        //     if (query.Any())
        //         return query.First().Owner as T;
        //     else
        //         return null;
        // }

        // /// <summary>
        // /// 
        // /// </summary>
        // /// <typeparam name="T"></typeparam>
        // /// <returns></returns>
        // public T[] GetDescendants<T>() where T : Controller {
        //     var query = gameObject
        //         .Descendants()
        //         .Select(d => d.GetComponent<Template>())
        //         .Where(t => t != null && t.Owner is T)
        //         .Select(t => t.Owner as T);

        //     return query.ToArray();
        // }

#if UNITY_EDITOR

        /// <summary>
        /// 
        /// </summary>
        public bool runTestMode;

        /// <summary>
        /// 
        /// </summary>
        public StyleSheet[] testStyleSheets;

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public TweenName[] GetTestTweenNames()
        {
            var ret = new List<TweenName>();

            foreach (var ss in testStyleSheets)
            {
                var query = ss.gameObject.DescendantsAndSelf()
                    .Select(d => d.GetComponent<TweenName>())
                    .Where(n => n != null);

                foreach (var q in query)
                    ret.Add(q);
            }

            return ret.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        void Start()
        {
            if (runTestMode && ctrler == null && Application.isPlaying)
            {
                var query = gameObject.DescendantsAndSelf()
                    .Select(d => d.GetComponent<StyleSelector>())
                    .Where(s => s != null && s.FindTemplate() == this);

                var tweenNames = GetTestTweenNames();

                foreach (var q in query)
                    q.Initialize(this, tweenNames);

                ShowAsObservable().Subscribe();
            }
        }

#endif

    }

}