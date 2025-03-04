using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Unity.Linq;
using Assets.Rx;


namespace UGUI.Rx {

/// <summary>
/// Loader loads Controller object via async.
//! DO NOT ACCESS Loader's methods DIRECTLY. USE ControllerExtension methods INSTEAD!
/// </summary>
/// <typeparam name="Loader"></typeparam>
public class Loader : ototot.dev.Singleton<Loader> {
    
    /// <summary>
    /// Load Controller
    /// </summary>
    /// <param name="ctrler"></param>
    /// <param name="overrideTemplate"></param>
    /// <param name="overrideStyleSheets"></param>
    /// <typeparam name="T"></typeparam>
    public void Load<T>(T ctrler, Template overrideTemplate, StyleSheet[] overrideStyleSheets) where T : Controller {
        // Prevents multiple loading
        if (++ctrler.loadTryCount > 1) {
            Debug.LogWarning($"Loader => Failed to load. Controller {typeof(T)} is on loading or loaded!!");
            return;
        }

        if (overrideTemplate != null) {
            ctrler.template = overrideTemplate;
        }
        else {
            var assetAlias = ControllerAttributeAccessor.Instance.GetTemplateAssetAlias(typeof(T));
            var resourcePath = ControllerAttributeAccessor.Instance.GetTemplateResourcePath(typeof(T));

            var hasTemplatePath = !string.IsNullOrEmpty(resourcePath);

            if (!assetAlias.IsValid && string.IsNullOrEmpty(resourcePath)) {
                Debug.LogWarning($"Loader => {typeof(T)} has no Template resource nor AssetBundle!!");
                return;
            }

            ctrler.template = TemplatePool.Instance.Get(assetAlias.IsValid ? assetAlias.AssetPath : resourcePath);

            if (ctrler.template == null) {
                var loadedObj = assetAlias.IsValid ?
                    AssetsManager.Instance.GetFromBundleByAlias<GameObject>(assetAlias, "UGUI.Rx").Value :
                    Resources.Load<GameObject>(resourcePath);

                if (loadedObj == null) {
                    if (assetAlias.IsValid)
                        Debug.LogWarning($"Loader => Loading Template asset failed at `{assetAlias.BundleName}-{assetAlias.AssetName}`!! (Maybe bundle is not loaded?)");
                    else
                        Debug.LogWarning($"Loader => Loading Template resource failed at `{resourcePath}`!!");

                    return;
                }

                ctrler.template = GameObject.Instantiate(loadedObj).GetComponent<Template>();

                if (ctrler.template == null) {
                    Debug.LogWarning("${loadedObj.name} doesn't have Template component!!");
                    return;
                }

                ctrler.template.poolingName = assetAlias.IsValid ? assetAlias.AssetPath : resourcePath;

                var templateQuery = ctrler.template.gameObject.Descendants()
                    .Select(d => d.GetComponent<Template>())
                    .Where(t => t != null);

                foreach (var t in templateQuery) {
                    t.outer = ctrler.template;
                    ctrler.template.Embeddeds.Add(t);
                }

                var selectorQuery = ctrler.template.gameObject.DescendantsAndSelf()
                    .Select(d => d.GetComponent<StyleSelector>())
                    .Where(s => s != null);

                foreach (var s in selectorQuery) {
                    s.template = s.gameObject.AncestorsAndSelf()
                        .Select(a => a.GetComponent<Template>())
                        .First(t => t != null);
                }
            }
        }
        
        ctrler.template.ctrler = ctrler;
        ctrler.template.gameObject.SetActive(false);

        if (overrideStyleSheets != null) {
            ctrler.stylesheets = overrideStyleSheets;
        }
        else {
            var assetAliases = ControllerAttributeAccessor.Instance.GetStyleSheetAssetAlias(typeof(T));
            var resourcePaths = ControllerAttributeAccessor.Instance.GetStyleSheetResourcePath(typeof(T));

            var loadedStyleSheets = new List<StyleSheet>();

            if (assetAliases.Length > 0) {
                foreach (var a in assetAliases) {
                    var loadedObj = AssetsManager.Instance.GetFromBundleByAlias<GameObject>(a, "UGUI.Rx").Value;

                    if (loadedObj == null) {
                        Debug.LogWarning($"Loading StyleSheet asset failed at `{a.BundleName}-{a.AssetName}`!!");
                        continue;
                    }

                    var styleSheet = loadedObj.GetComponent<StyleSheet>();

                    if (styleSheet == null) {
                        Debug.LogWarning("${loadedObj.name} doesn't have StyleSheet component!!");
                        return;
                    }

                    loadedStyleSheets.Add(styleSheet);
                }
            }

            if (resourcePaths.Length > 0) {
                foreach (var p in resourcePaths) {
                    var loadedObj = Resources.Load<GameObject>(p);

                    if (loadedObj == null) {
                        Debug.LogWarning($"Loading StyleSheet resource failed at `{p}`!!");
                        continue;
                    }

                    var styleSheet = loadedObj.GetComponent<StyleSheet>();

                    if (styleSheet == null) {
                        Debug.LogWarning("${loadedObj.name} doesn't have StyleSheet component!!");
                        return;
                    }

                    loadedStyleSheets.Add(styleSheet);
                }
            }

            if (loadedStyleSheets.Count > 0)
                ctrler.stylesheets = loadedStyleSheets.ToArray();
        }

        var memberResourcePaths = ControllerAttributeAccessor.Instance.GetMemberResourcePath<T>(ctrler);

        if (memberResourcePaths.Length > 0) {
            foreach (var p in memberResourcePaths) {
                var loadedObj = Resources.Load<GameObject>(p);

                if (loadedObj == null) {
                    Debug.LogWarning($"Loading Controller member binding resource at `{p}`!!");
                    return;
                }
            }

            ControllerAttributeAccessor.Instance.SetMemberResource<T>(ctrler);
        }

        var memberAssetAliases = ControllerAttributeAccessor.Instance.GetMemberAssetAlias<T>(ctrler);

        if (memberAssetAliases.Length > 0) {
            foreach (var a in memberAssetAliases) {
                var loadedObj = AssetsManager.Instance.GetFromBundleByAlias<GameObject>(a, "UGUI.Rx").Value;

                if (loadedObj == null) {
                    Debug.LogWarning($"Loading Controller member binding asset failed at `{a.BundleName}-{a.AssetName}`!!");
                    return;
                }
            }

            ControllerAttributeAccessor.Instance.SetMemberAsset<T>(ctrler);
        }

        ctrler.OnPreLoad(new List<IObservable<Controller>>());
        ctrler.OnPostLoad();
        ctrler.isLoaded = true;
    }
    
    /// <summary>
    /// Load Controller async
    /// </summary>
    /// <param name="ctrler"></param>
    /// <param name="overrideTemplate"></param>
    /// <param name="overrideStyleSheets"></param>
    /// <param name="loader"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public IObservable<T> LoadAsObservable<T>(T ctrler, Template overrideTemplate, StyleSheet[] overrideStyleSheets, List<IObservable<Controller>> loader) where T : Controller {
        if (loader == null)
            loader = new List<IObservable<Controller>>();
            
        return PreLoadAsObservable<T>(ctrler, overrideTemplate, overrideStyleSheets, loader)
            .ContinueWith(Observable.WhenAll(loader))
            .Do(cc => {
                foreach (var c in cc.Reverse()) {
                    if (!c.isLoaded) {
                        c.isLoaded = true;
                        c.OnPostLoad();
                    }
                }
            })
            .Select(_ => ctrler);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ctrler"></param>
    /// <param name="overrideTemplate"></param>
    /// <param name="overrideStyleSheets"></param>
    /// <param name="loader"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public IObservable<T> PreLoadAsObservable<T>(T ctrler, Template overrideTemplate, StyleSheet[] overrideStyleSheets, List<IObservable<Controller>> loader) where T : Controller {
        return Observable.Create<T>(observer => {
            if (++ctrler.loadTryCount > 1) {
                observer.OnError(new Exception($"Loader => Failed to load. Controller {typeof(T)} is on loading or loaded!!"));
                return Disposable.Empty;
            }

            if (overrideTemplate != null) {
                ctrler.template = overrideTemplate;
                ctrler.template.ctrler = ctrler;
                ctrler.template.gameObject.SetActive(false);
            }
            else {
                var assetAlias = ControllerAttributeAccessor.Instance.GetTemplateAssetAlias(typeof(T));
                var resourcePath = ControllerAttributeAccessor.Instance.GetTemplateResourcePath(typeof(T));

                if (!assetAlias.IsValid && string.IsNullOrEmpty(resourcePath)) {
                    Debug.LogWarning($"Loader => {typeof(T)} has no Template resource nor AssetBundle!!");
                    return null;
                }

                ctrler.template = TemplatePool.Instance.Get(assetAlias.IsValid ? assetAlias.AssetPath : resourcePath);

                if (ctrler.template == null) {
                    var loadingObj = assetAlias.IsValid ?
                        AssetsManager.Instance.GetByAlias<GameObject>(assetAlias, "UGUI.Rx").Select(v => v.Value) :
                        Resources.LoadAsync(resourcePath).AsAsyncOperationObservable().Select(v => v.asset as GameObject);

                    var loadingTemplate = loadingObj
                        .Do(o => {
                            ctrler.template = GameObject.Instantiate(o).GetComponent<Template>();
                            
                            if (ctrler.template == null)
                                throw new Exception(string.Format($"{o.name} doesn't have StyleSheet component!!"));

                            ctrler.template.poolingName = assetAlias.IsValid ? assetAlias.AssetPath : resourcePath;

                            var templateQuery = ctrler.template.gameObject.Descendants()
                                .Select(d => d.GetComponent<Template>())
                                .Where(t => t != null);

                            foreach (var t in templateQuery) {
                                t.outer = ctrler.template;
                                ctrler.template.Embeddeds.Add(t);
                            }

                            var selectorQuery = ctrler.template.gameObject.DescendantsAndSelf()
                                .Select(d => d.GetComponent<StyleSelector>())
                                .Where(s => s != null);

                            foreach (var s in selectorQuery) {
                                s.template = s.gameObject.AncestorsAndSelf()
                                    .Select(a => a.GetComponent<Template>())
                                    .First(t => t != null);
                            }

                            ctrler.template.ctrler = ctrler;
                            ctrler.template.gameObject.SetActive(false);

                            if (assetAlias.IsValid)
                                Debug.Log($"Loader => Template at '{assetAlias.AssetPath}' is loaded for {typeof(T)} Controller~");
                            else
                                Debug.Log($"Loader => Template at '{resourcePath}' is loaded for {typeof(T)} Controller~");
                        })
                        .Select(_ => ctrler as Controller);

                    loader.Add(loadingTemplate);
                }
                else {
                    ctrler.template.ctrler = ctrler;
                    ctrler.template.gameObject.SetActive(false);
                }
            }

            if (overrideStyleSheets != null) {
                ctrler.stylesheets = overrideStyleSheets;
            }
            else {
                var assetAliases = ControllerAttributeAccessor.Instance.GetStyleSheetAssetAlias(typeof(T));
                var resourcePaths = ControllerAttributeAccessor.Instance.GetStyleSheetResourcePath(typeof(T));

                if (assetAliases.Length > 0 || resourcePaths.Length > 0) {
                    var loadingObjs = new List<IObservable<GameObject>>();

                    foreach (var a in assetAliases)
                        loadingObjs.Add(AssetsManager.Instance.GetByAlias<GameObject>(a, "UGUI.Rx").Select(v => v.Value));

                    foreach (var p in resourcePaths)
                        loadingObjs.Add(Resources.LoadAsync(p).AsAsyncOperationObservable().Select(v => v.asset as GameObject));

                    var loadingStyleSheets = Observable.WhenAll(loadingObjs)
                        .Do(loadedObjs => {
                            var temp = new List<StyleSheet>();

                            foreach (var o in loadedObjs) {
                                var styleSheet = o.GetComponent<StyleSheet>();

                                if (styleSheet == null)
                                    throw new Exception(string.Format($"{o.name} doesn't have StyleSheet component!!"));

                                temp.Add(styleSheet);
                            }

                            ctrler.stylesheets = temp.ToArray();
                        })
                        .Select(_ => ctrler as Controller);

                    loader.Add(loadingStyleSheets);
                }
            }

            var memberResourcePaths = ControllerAttributeAccessor.Instance.GetMemberResourcePath<T>(ctrler);

            if (memberResourcePaths.Length > 0) {
                var loadingObjs = new List<IObservable<GameObject>>();

                foreach (var p in memberResourcePaths)
                    loadingObjs.Add(Resources.LoadAsync(p).AsAsyncOperationObservable().Select(v => v.asset as GameObject));
                
                var loadingResources = Observable.WhenAll(loadingObjs)
                    .Do(_ => ControllerAttributeAccessor.Instance.SetMemberResource<T>(ctrler))
                    .Select(_ => ctrler as Controller);

                loader.Add(loadingResources);
            }

            var memberAssetAliases = ControllerAttributeAccessor.Instance.GetMemberAssetAlias<T>(ctrler);

            if (memberAssetAliases.Length > 0) {
                var loadingObjs = new List<IObservable<GameObject>>();

                foreach (var a in memberAssetAliases)
                    loadingObjs.Add(AssetsManager.Instance.GetByAlias<GameObject>(a, "UGUI.Rx").Select(v => v.Value));
                
                var loadingAssets = Observable.WhenAll(loadingObjs)
                    .Do(rr => ControllerAttributeAccessor.Instance.SetMemberAsset<T>(ctrler))
                    .Select(_ => ctrler as Controller);

                loader.Add(loadingAssets);
            }

            //! This useless observable is very important. it assures OnPostLoad() to be called even if there's no other observables added to 'loader'.
            loader.Add(Observable.NextFrame()
                // .Do(_ => Debug.Log($"Loader => Loading {typeof(T)} Controller..."))
                .Select(_ => ctrler as Controller)
            );

            ctrler.OnPreLoad(loader);

            observer.OnNext(ctrler);
            observer.OnCompleted();

            return Disposable.Empty;
        });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ctrler"></param>
    /// <param name="destroyTemplate"></param>
    public void Unload(Controller ctrler, bool destroyTemplate) {
        var query = ctrler.template.gameObject.DescendantsAndSelf()
            .Select(d => d.GetComponent<Template>())
            .Where(t => t != null)
            .Select(t => t.ctrler);

        foreach (var q in query)
            UnloadInternal(q, destroyTemplate);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ctrler"></param>
    /// <param name="destroyTemplate"></param>
    void UnloadInternal(Controller ctrler, bool destroyTemplate) {
        if (!ctrler.isLoaded) {
            Debug.LogWarning($"Loader => Can't unload Controller which is not loaded!!");
            return;
        }

        if (ctrler.template.isActiveAndEnabled) {
            var query = ctrler.template.gameObject.DescendantsAndSelf()
                .Select(d => d.GetComponent<StyleSelector>())
                .Where(s => s != null && s.FindTemplate() == ctrler.template);

            foreach (var q in query)
                q.CleanUp();
        }

        ctrler.DisposeHide();
        ctrler.DisposePostHide();
        ctrler.DisposeUnload();
        
        ctrler.OnUnload();

        ctrler.template.ctrler = null;

        if (!ctrler.template.IsEmbedded) {
            if (destroyTemplate)
                GameObject.Destroy(ctrler.template.gameObject);
            else
                TemplatePool.Instance.Return(ctrler.template);
        }

        ctrler.template = null;
        ctrler.isLoaded = false;
        ctrler.loadTryCount = 0;
        ctrler.hideCount = 0; 
        ctrler.postHideCount = 0;
    }

}

}
