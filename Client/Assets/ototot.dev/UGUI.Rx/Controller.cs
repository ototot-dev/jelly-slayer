using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Unity.Linq;
using Retween.Rx;

namespace UGUI.Rx
{
    public class Controller
    {
        public Template template;
        public StyleSheet[] stylesheets = new StyleSheet[] { };

        public TweenName[] GetTweenNames()
        {
            var ret = new List<TweenName>();
            foreach (var ss in stylesheets)
            {
                var query = ss.gameObject.DescendantsAndSelf()
                    .Select(d => d.GetComponent<TweenName>())
                    .Where(n => n != null);

                foreach (var q in query)
                    ret.Add(q);
            }

            return ret.ToArray();
        }

        public bool IsHidden => template == null || template.SetHidden;
        public bool isLoaded;
        public int loadTryCount;
        public int showCount;
        public int postShowCount;
        public int hideCount;
        public int postHideCount;

        /// <summary>
        /// OnPreLoad() is called before Loader start to load Controller object.
        /// The main goal of this callback is to implement an observable and add it to loader param.
        /// </summary>
        /// <param name="loader"> The collection of an observable which execute the async loading on requested resources or assets. </param>
        public virtual void OnPreLoad(List<IObservable<Controller>> loader) { }

        /// <summary>
        /// OnPostLoad() is called just after Loader has completed to load Controller object.
        /// </summary>
        public virtual void OnPostLoad() { }

        /// <summary>
        /// OnUnload() is called just after Loader unloaded Controller object
        /// </summary>
        public virtual void OnUnload() { }

        /// <summary>
        /// OnPreShow() is called when the show transition started.
        /// </summary>
        public virtual void OnPreShow()
        {
            var query = template.gameObject.DescendantsAndSelf()
                .Select(d => d.GetComponent<StyleSelector>())
                .Where(s => s != null && s.FindTemplate() == template);

            var tweenNames = GetTweenNames();
            foreach (var q in query)
                q.Init(template, tweenNames);

            template.gameObject.SetActive(true);
        }

        /// <summary>
        /// OnPostShow() is called when the show transition finished.
        /// </summary>
        public virtual void OnPostShow() { }

        /// <summary>
        /// OnPreHide() is called when the hide transition started.
        /// </summary>
        public virtual void OnPreHide() { }

        /// <summary>
        /// OnPostHide() is called when the hide transition finished.
        /// </summary>
        public virtual void OnPostHide()
        {
            var query = template.gameObject.DescendantsAndSelf()
                .Select(d => d.GetComponent<StyleSelector>())
                .Where(s => s != null && s.FindTemplate() == template);

            foreach (var q in query)
                q.CleanUp();

            template.gameObject.SetActive(false);
        }

        HashSet<IDisposable> __hideDisposables;

        public void AddToHide(IDisposable disposable)
        {
            if (__hideDisposables == null)
                __hideDisposables = new HashSet<IDisposable>();

            __hideDisposables.Add(disposable);
        }

        public void DisposeHide()
        {
            if (__hideDisposables != null)
            {
                foreach (var d in __hideDisposables)
                    d.Dispose();

                __hideDisposables = null;
            }
        }

        HashSet<IDisposable> __postHideDisposables;

        public void AddToPostHide(IDisposable disposable)
        {
            if (__postHideDisposables == null)
                __postHideDisposables = new HashSet<IDisposable>();

            __postHideDisposables.Add(disposable);
        }

        public void DisposePostHide()
        {
            if (__postHideDisposables != null)
            {
                foreach (var d in __postHideDisposables)
                    d.Dispose();

                __postHideDisposables = null;
            }
        }
        HashSet<IDisposable> __unloadDisposables;

        public void AddToUnload(IDisposable disposable)
        {
            if (__unloadDisposables == null)
                __unloadDisposables = new HashSet<IDisposable>();

            __unloadDisposables.Add(disposable);
        }

        public void DisposeUnload()
        {
            if (__hideDisposables != null)
            {
                foreach (var d in __unloadDisposables)
                    d.Dispose();

                __unloadDisposables = null;
            }
        }

#region Component Accessor
        public T GetComponentById<T>(string val) where T : Component
        {
            return template?.GetComponentById<T>(val) ?? null;
        }
        public T GetComponentByClass<T>(string val) where T : Component
        {
            return template?.GetComponentByClass<T>(val) ?? null;
        }
        public List<T> GetComponentsByClass<T>(string val) where T : Component
        {
            return template?.GetComponentsByClass<T>(val) ?? new List<T>();
        }
        public T GetComponentByTag<T>(string val) where T : Component
        {
            return template?.GetComponentByTag<T>(val) ?? null;
        }
        public List<T> GetComponentsByTag<T>(string val) where T : Component
        {
            return template?.GetComponentsByTag<T>(val) ?? new List<T>();
        }
        public Template[] GetEmbeddedTemplates()
        {
            return template?.GetEmbeddedTemplates() ?? new Template[] { };
        }
#endregion

    }
}
