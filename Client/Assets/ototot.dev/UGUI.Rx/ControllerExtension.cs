using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Unity.Linq;
using ZLinq;

namespace UGUI.Rx
{
    /// <summary>
    /// ControllerExtension provides helper methods for Controller object.
    /// </summary>
    public static class ControllerExtension
    {
        public static T Load<T>(this T ctrler, Template overrideTemplate = null, StyleSheet[] overrideStyleSheets = null) where T : Controller
        {
            Loader.Instance.Load(ctrler, overrideTemplate, overrideStyleSheets);
            return ctrler;
        }

        public static IObservable<T> LoadAsObservable<T>(this T ctrler, Template overrideTemplate = null, StyleSheet[] overrideStyleSheets = null) where T : Controller
        {
            return Loader.Instance.LoadAsObservable<T>(ctrler, overrideTemplate, overrideStyleSheets, null);
        }

        public static T PreLoad<T>(this T ctrler, List<IObservable<Controller>> loader, Template overrideTemplate = null, StyleSheet[] overrideStyleSheets = null) where T : Controller
        {
            Loader.Instance.PreLoadAsObservable<T>(ctrler, overrideTemplate, overrideStyleSheets, loader).Subscribe();
            return ctrler;
        }

        public static void Unload(this Controller ctrler, bool destroyTemplate = false)
        {
            Loader.Instance.Unload(ctrler, destroyTemplate);
        }

        public static T Show<T>(this T ctrler, RectTransform parent) where T : Controller
        {
            if (!ctrler.isLoaded)
            {
                Debug.LogWarning($"ControllerExtension => Controller {typeof(T)} is not loaded yet!!)");
                return ctrler;
            }

            ctrler.template.transform.SetParent(parent, false);

            //* Hide 상태라면 Hide 상태는 강제로 종료시킴
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

                //* Hide 상태라면 Hide 상태는 강제로 종료시킴
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

        public static T Hide<T>(this T ctrler) where T : Controller
        {
            //* ShowDimmed() / HideDimmed() 페어링 체크
            Debug.Assert(!ctrler.isDimmed);

            return ctrler.HideInternal();
        }

        public static IObservable<T> HideAsObservable<T>(this T ctrler) where T : Controller
        {
            //* ShowDimmed() / HideDimmed() 페어링 체크
            Debug.Assert(!ctrler.isDimmed);

            return ctrler.HideAsObservableInternal();
        }

        static T HideInternal<T>(this T ctrler) where T : Controller
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

        public static IObservable<T> HideAsObservableInternal<T>(this T ctrler) where T : Controller
        {
            if (!ctrler.isLoaded)
            {
                Debug.LogWarning($"ControllerExtension => Controller {typeof(T)} is not loaded!!");
                return null;
            }

            var preHideObservable = Observable.Create<T>(observer =>
            {
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

        public static T ShowDimmed<T>(this T ctrler, RectTransform dimmed) where T : Controller
        {
            ctrler.isDimmed = true;
            dimmed.gameObject.SetActive(true);
            return Show<T>(ctrler, dimmed.transform as RectTransform);
        }

        public static IObservable<T> ShowDimmedAsObservable<T>(this T ctrler, RectTransform dimmed) where T : Controller
        {
            ctrler.isDimmed = true;
            dimmed.gameObject.SetActive(true);
            return ShowAsObservable<T>(ctrler, dimmed);
        }

        public static T HideDimmed<T>(this T ctrler) where T : Controller
        {
            //* ShowDimmed() / HideDimmed() 페어링 체크
            Debug.Assert(ctrler.isDimmed);

            var dimmed = ctrler.template.transform.parent;

            ctrler.HideAsObservableInternal().Subscribe(_ =>
            {
                if (dimmed.childCount == 0 || dimmed.gameObject.Children().All(c => !c.activeSelf))
                    dimmed.gameObject.SetActive(false);
            });

            return ctrler;
        }

        public static IObservable<T> HideDimmedAsObservable<T>(this T ctrler) where T : Controller
        {
            //* ShowDimmed() / HideDimmed() 페어링 체크
            Debug.Assert(ctrler.isDimmed);

            var dimmed = ctrler.template.transform.parent;

            return ctrler.HideAsObservableInternal().Do(_ =>
            {
                if (dimmed.childCount == 0 || dimmed.gameObject.Children().All(c => !c.activeSelf))
                    dimmed.gameObject.SetActive(false);
            });
        }

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
