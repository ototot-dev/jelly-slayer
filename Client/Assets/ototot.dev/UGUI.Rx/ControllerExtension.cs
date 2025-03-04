using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Unity.Linq;

namespace UGUI.Rx
{
    /// <summary>
    /// ControllerExtension provides helper methods for Controller object.
    /// </summary>
    public static class ControllerExtension
    {

        /// <summary>
        /// Load a Controller object.
        /// </summary>
        /// <param name="ctrler"></param>
        /// <param name="overrideTemplate"></param>
        /// <param name="overrideStyleSheets"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Load<T>(this T ctrler, Template overrideTemplate = null, StyleSheet[] overrideStyleSheets = null) where T : Controller
        {
            Loader.Instance.Load(ctrler, overrideTemplate, overrideStyleSheets);
            return ctrler;
        }

        /// <summary>
        /// Load a Controller object in async.
        /// </summary>
        /// <param name="ctrler"></param>
        /// <param name="overrideTemplate"></param>
        /// <param name="overrideStyleSheets"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IObservable<T> LoadAsObservable<T>(this T ctrler, Template overrideTemplate = null, StyleSheet[] overrideStyleSheets = null) where T : Controller
        {
            return Loader.Instance.LoadAsObservable<T>(ctrler, overrideTemplate, overrideStyleSheets, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctrler"></param>
        /// <param name="loader"></param>
        /// <param name="overrideTemplate"></param>
        /// <param name="overrideStyleSheets"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T PreLoad<T>(this T ctrler, List<IObservable<Controller>> loader, Template overrideTemplate = null, StyleSheet[] overrideStyleSheets = null) where T : Controller
        {
            Loader.Instance.PreLoadAsObservable<T>(ctrler, overrideTemplate, overrideStyleSheets, loader).Subscribe();
            return ctrler;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctrler"></param>
        /// <param name="destroyTemplate"></param>
        public static void Unload(this Controller ctrler, bool destroyTemplate = false)
        {
            Loader.Instance.Unload(ctrler, destroyTemplate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctrler"></param>
        /// <param name="parent"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Show<T>(this T ctrler, RectTransform parent) where T : Controller
        {
            if (!ctrler.isLoaded)
            {
                Debug.LogWarning($"ControllerExtension => Controller {typeof(T)} is not loaded yet!!)");
                return ctrler;
            }

            ctrler.template.transform.SetParent(parent, false);

            // Ensure that OnPostHide() to be called
            if (ctrler.hideCount > 0 && ctrler.postHideCount == 0)
            {
                ctrler.DisposePostHide();
                ctrler.OnPostHide();
            }

            ctrler.hideCount = 0;
            ctrler.postHideCount = 0;

            if (++ctrler.showCount == 1)
                ctrler.OnPreShow();

            ctrler.template.ShowAsObservable().Subscribe(_ =>
            {
                if (ctrler.showCount > 0 && ++ctrler.postShowCount == 1)
                    ctrler.OnPostShow();
            });

            return ctrler;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctrler"></param>
        /// <param name="parent"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IObservable<T> ShowAsObservable<T>(this T ctrler, RectTransform parent) where T : Controller
        {
            if (!ctrler.isLoaded)
            {
                Debug.LogWarning($"ControllerExtension => Controller {typeof(T)} is not loaded!!");
                return null;
            }

            var preShowObservable = Observable.Create<T>(observer =>
            {
                ctrler.template.transform.SetParent(parent, false);
                ctrler.template.gameObject.SetActive(true);

                // Ensure that OnPostHide() to be called
                if (ctrler.hideCount > 0 && ctrler.postHideCount == 0)
                {
                    ctrler.DisposePostHide();
                    ctrler.OnPostHide();
                }

                ctrler.hideCount = 0;
                ctrler.postHideCount = 0;

                if (++ctrler.showCount == 1)
                    ctrler.OnPreShow();

                observer.OnNext(ctrler);
                observer.OnCompleted();

                return Disposable.Empty;
            });

            return preShowObservable.ContinueWith(ctrler.template.ShowAsObservable())
                .Do(_ =>
                {
                    if (ctrler.showCount > 0 && ++ctrler.postShowCount == 1)
                        ctrler.OnPostShow();
                })
                .Select(_ => ctrler);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctrler"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Hide<T>(this T ctrler) where T : Controller
        {
            if (ctrler.template == null)
            {
                Debug.LogWarning($"ControllerExtension => ctrler.Template is null!! (Maybe ctrler in not loaded yet??)");
                return ctrler;
            }

            // Ensure that OnPostShow() to be called
            if (ctrler.showCount > 0 && ctrler.postShowCount == 0)
                ctrler.OnPostShow();

            ctrler.showCount = 0;
            ctrler.postShowCount = 0;

            if (++ctrler.hideCount == 1)
            {
                ctrler.DisposeHide();
                ctrler.OnPreHide();
            }

            ctrler.template.HideAsObservable().Subscribe(_ =>
            {
                if (ctrler.hideCount > 0 && ++ctrler.postHideCount == 1)
                {
                    ctrler.DisposePostHide();
                    ctrler.OnPostHide();
                }
            });

            return ctrler;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctrler"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IObservable<T> HideAsObservable<T>(this T ctrler) where T : Controller
        {
            if (!ctrler.isLoaded)
            {
                Debug.LogWarning($"ControllerExtension => Controller {typeof(T)} is not loaded!!");
                return null;
            }

            var preHideObservable = Observable.Create<T>(observer =>
            {
                // Ensure that OnPostShow() to be called
                if (ctrler.showCount > 0 && ctrler.postShowCount == 0)
                    ctrler.OnPostShow();

                ctrler.showCount = 0;
                ctrler.postShowCount = 0;

                if (++ctrler.hideCount == 1)
                {
                    ctrler.DisposeHide();
                    ctrler.OnPreHide();
                }

                observer.OnNext(ctrler);
                observer.OnCompleted();

                return Disposable.Empty;
            });

            return preHideObservable.ContinueWith(ctrler.template.HideAsObservable())
                .Do(_ =>
                {
                    if (ctrler.hideCount > 0 && ++ctrler.postHideCount == 1)
                    {
                        ctrler.DisposePostHide();
                        ctrler.OnPostHide();
                    }
                })
                .Select(_ => ctrler);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctrler"></param>
        /// <param name="sortingOrder"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ShowPopup<T>(this T ctrler, RectTransform blocker) where T : Controller
        {
            blocker.gameObject.SetActive(true);
            return Show<T>(ctrler, blocker.transform as RectTransform);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctrler"></param>
        /// <param name="blocker"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IObservable<T> ShowPopupAsObservable<T>(this T ctrler, RectTransform blocker) where T : Controller
        {
            blocker.gameObject.SetActive(true);
            return ShowAsObservable<T>(ctrler, blocker);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctrler"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T HidePopup<T>(this T ctrler) where T : Controller
        {
            var blockerTM = ctrler.template.transform.parent;

            HideAsObservable<T>(ctrler).Subscribe(_ =>
            {
                if (blockerTM.childCount == 0 || blockerTM.gameObject.Children().All(c => !c.activeSelf))
                    blockerTM.gameObject.SetActive(false);
            });

            return ctrler;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctrler"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IObservable<T> HidePopupAsObservable<T>(this T ctrler) where T : Controller
        {
            var blockerTM = ctrler.template.transform.parent;

            return HideAsObservable<T>(ctrler).Do(_ =>
            {
                if (blockerTM.childCount == 0 || blockerTM.gameObject.Children().All(c => !c.activeSelf))
                    blockerTM.gameObject.SetActive(false);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposable"></param>
        /// <param name="ctrler"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T AddToHide<T>(this T disposable, Controller ctrler) where T : IDisposable
        {
            if (ctrler == null)
            {
                disposable.Dispose();
                return disposable;
            }
            else
            {
                ctrler.AddToHide(disposable);
                return disposable;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposable"></param>
        /// <param name="ctrler"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T AddToPostHide<T>(this T disposable, Controller ctrler) where T : IDisposable
        {
            if (ctrler == null)
            {
                disposable.Dispose();
                return disposable;
            }
            else
            {
                ctrler.AddToPostHide(disposable);
                return disposable;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposable"></param>
        /// <param name="ctrler"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T AddToUnload<T>(this T disposable, Controller ctrler) where T : IDisposable
        {
            if (ctrler == null)
            {
                disposable.Dispose();
                return disposable;
            }
            else
            {
                ctrler.AddToUnload(disposable);
                return disposable;
            }
        }
    }
}
