using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;using UnityEngine;
using UniRx;


namespace Assets.Rx {

    /// <summary>
    /// Assets.Rx manager class.
    /// </summary>
    /// <typeparam name="AssetsManager"></typeparam>
    public class AssetsManager : ototot.dev.MonoSingleton<AssetsManager> {

        /// <summary>
        /// Initializes AssetsManager as Observable.
        /// </summary>
        /// <returns></returns>
        public IObservable<bool> Init() {
            return AssetsLoader.Instance.Init();
        }
        
        /// <summary>
        /// Get asset via AssetBundle name and asset name as Observable.
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <param name="groupName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IObservable<AssetRef<T>> Get<T>(string bundleName, string assetName, string groupName = "default") where T : UnityEngine.Object {
            return GetByAlias<T>(new AssetAlias(bundleName, assetName), groupName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="groupName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IObservable<AssetRef<T>> GetByPath<T>(string assetPath, string groupName = "default") where T : UnityEngine.Object {
            return GetByAlias<T>(new AssetAlias(assetPath), groupName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="groupName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IObservable<AssetRef<T>> GetByAlias<T>(AssetAlias alias, string groupName = "default") where T : UnityEngine.Object {
            if (!__poolingGroups.ContainsKey(groupName))
                __poolingGroups.Add(groupName, new PoolingGroup(groupName));

            return Observable.FromCoroutine<AssetRef<T>>((observer, cancelToken) => __poolingGroups[groupName].Load(alias, observer, cancelToken));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <param name="groupName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public AssetRef<T> GetLoaded<T>(string bundleName, string assetName, string groupName = "default") where T : UnityEngine.Object {
            return GetLoadedByAlias<T>(new AssetAlias(bundleName, assetName), groupName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="groupName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public AssetRef<T> GetLoadedByPath<T>(string assetPath, string groupName = "default") where T : UnityEngine.Object {
            return GetLoadedByAlias<T>(new AssetAlias(assetPath), groupName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="groupName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public AssetRef<T> GetLoadedByAlias<T>(AssetAlias alias, string groupName = "default") where T : UnityEngine.Object {
            if (__poolingGroups.ContainsKey(groupName)) 
                return __poolingGroups[groupName].GetLoaded<T>(alias);
            else
                return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <param name="groupName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public AssetRef<T> GetFromBundle<T>(string bundleName, string assetName, string groupName = "default") where T : UnityEngine.Object {
            return GetFromBundleByAlias<T>(new AssetAlias(bundleName, assetName), groupName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="groupName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public AssetRef<T> GetFromBundleByPath<T>(string assetPath, string groupName = "default") where T : UnityEngine.Object {
            return GetFromBundleByAlias<T>(new AssetAlias(assetPath), groupName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="groupName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public AssetRef<T> GetFromBundleByAlias<T>(AssetAlias alias, string groupName = "default") where T : UnityEngine.Object {
            if (__poolingGroups.ContainsKey(groupName)) 
                return __poolingGroups[groupName].GetFromBundle<T>(alias);
            else
                return null;
        }

        /// <summary>
        /// 
        /// </summary>
        public class PoolingGroup {

            /// <summary>
            /// 
            /// </summary>
            /// <value></value>
            public string GroupName { get; private set; }

            /// <summary>
            /// Ctor.
            /// </summary>
            /// <param name="groupName"></param>
            public PoolingGroup(string groupName) {
                GroupName = groupName;
            }

            /// <summary>
            /// Key is asset path and value is a paif of { AssetRef object, AssetRef's ref-count }.
            /// </summary>
            /// <returns></returns>
            readonly Dictionary<string, __AssetRef> __assetRefs = new Dictionary<string, __AssetRef>();

            /// <summary>
            /// Loads AssetBundle object in asynchronous and return AssetBundleRef as IObservable object
            /// </summary>
            /// <param name="assetPath"></param>
            /// <param name="observer"></param>
            /// <param name="cancelToken"></param>
            /// <returns></returns>
            public IEnumerator Load<T>(AssetAlias alias, IObserver<AssetRef<T>> observer, CancellationToken cancelToken) where T : UnityEngine.Object {
                var loadedItem = GetLoaded<T>(alias);

                if (loadedItem == null) {
                    var loadObservable = AssetsLoader.Instance.Load(alias).ToYieldInstruction(false);

                    yield return loadObservable;

                    if (cancelToken.IsCancellationRequested)
                        yield break;

                    if (loadObservable.HasError) {
                        observer.OnError(loadObservable.Error);
                        yield break;
                    }

                    if (!__assetRefs.ContainsKey(alias.AssetPath))
                        __assetRefs.Add(alias.AssetPath, loadObservable.Result);

                    var refCount = __assetRefs[alias.AssetPath].IncreaseRefCount();
                    Debug.LogFormat("AssetsManager => '{0}' AssetRef's count become '{1}' at '{2}' pool.", alias.AssetPath, refCount, GroupName);
       
                    observer.OnNext(new AssetRef<T>(loadObservable.Result, GroupName));
                    observer.OnCompleted();
                }
                else {
                    observer.OnNext(loadedItem);
                    observer.OnCompleted();
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="alias"></param>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public AssetRef<T> GetLoaded<T>(AssetAlias alias) where T : UnityEngine.Object {
                if (!__assetRefs.ContainsKey(alias.AssetPath)) {
                    Debug.LogFormat("AssetsManager => '{0}' asset is not loaded at '{1}'", alias.AssetPath, GroupName);
                    return null;
                }

                var refCount = __assetRefs[alias.AssetPath].IncreaseRefCount();
                Debug.LogFormat("AssetsManager => '{0}' AssetRef's count become '{1}' at '{2}' pool.", alias.AssetPath, refCount, GroupName);

                return new AssetRef<T>(__assetRefs[alias.AssetPath], GroupName);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="alias"></param>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public AssetRef<T> GetFromBundle<T>(AssetAlias alias) where T : UnityEngine.Object {
                var ret = GetLoaded<T>(alias);

                if (ret != null)
                    return ret;

                foreach (var v in __assetRefs.Values) {
                    if (v.Alias.BundleName != alias.BundleName)
                        continue;

                    var asset = v.Bundle.LoadAsset<T>(alias.AssetName);

                    if (asset != null) {
                        __assetRefs.Add(alias.AssetPath, new __AssetRef(asset, v.Bundle, alias, v.Trackers));
                        return new AssetRef<T>(__assetRefs[alias.AssetPath], GroupName);
                    }
                    else {
                        Debug.LogFormat("AssetsManager => '{0}' asset doesn't exist in '{1}' AssetBundle at '{1}' pool.", alias.AssetPath, alias.BundleName, GroupName);
                        return null;
                    }
                }

                Debug.LogFormat("AssetsManager => '{0}' AssetBundle is not loaded at '{1}' pool.", alias.BundleName, GroupName);
                return null;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="alias"></param>
            public void Release(AssetAlias alias) {
                if (!__assetRefs.ContainsKey(alias.AssetPath)) {
                    Debug.LogWarningFormat("AssetsManager => '{0}' asset is not loaded at {1} pool.", alias.AssetPath, GroupName);
                    return;
                }

                if (__assetRefs[alias.AssetPath].DecreaseRefCount() == 0) {
                     Debug.LogFormat("AssetsManager => '{0}' AssetRef's count become '{1}' at '{2}' pool.", alias.AssetPath, 0, GroupName);

                    __assetRefs[alias.AssetPath].Dispose();
                    __assetRefs.Remove(alias.AssetPath);
                }
            }

        }

        readonly Dictionary<string, PoolingGroup> __poolingGroups = new Dictionary<string, PoolingGroup>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public PoolingGroup GetPoolingGroup(string groupName) {
            if (__poolingGroups.ContainsKey(groupName))
                return __poolingGroups[groupName];
            else
                return null;
        }

    }

}