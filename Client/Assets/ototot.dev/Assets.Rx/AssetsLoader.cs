#define SUPPORT_ASSET_BUNDLE_ASYNC_LOADING

using System;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UniRx;

namespace Assets.Rx {

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="AssetsLoader"></typeparam>
    public class AssetsLoader : ototot.dev.MonoSingleton<AssetsLoader> {

        /// <summary>
        /// BundleTrackers
        /// </summary>
        /// <typeparam name="Hash128"></typeparam>
        /// <typeparam name="BundleTracker"></typeparam>
        /// <returns></returns>
        readonly Dictionary<Hash128, BundleTracker> __trackers = new Dictionary<Hash128, BundleTracker>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tracker"></param>
        /// <returns></returns>
        IEnumerator UnloadCounter(BundleTracker tracker) {
            while (true) {
                if (tracker.RefCount > 0) {
                    tracker.ResetUnloadCounter(BundleTracker.defaultUnloadCountResetValue);
                    yield return null;
                    continue;
                }

                if (tracker.DecreaseUnloadCounter(1) > 0) {
                    yield return null;
                    continue;
                }

                break;
            }

            Debug.LogFormat("AssetLoader => '{0}' AssetBundle is timed out and will be unload now.", tracker.BundleName);

            tracker.Dispose();

            __trackers.Remove(tracker.Hash);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bundleHash"></param>
        void StartUnloadCounter(Hash128 bundleHash) {
            var tracker = __trackers[bundleHash];

            tracker.ResetUnloadCounter(BundleTracker.defaultUnloadCountResetValue);
            MainThreadDispatcher.StartFixedUpdateMicroCoroutine(UnloadCounter(tracker));

            Debug.LogFormat("AssetLoader => Start to count down UnloadCounter on '{0}' AssetBundle.", tracker.BundleName);
        }

        /// <summary>
        /// Set UnloadCounter to be 0 for all BundleTracker which will make all AssetBunle unloaded.
        /// </summary>
        public void ForceToUnloadAllBundles() {
            foreach (var t in __trackers.Where(t => t.Value.RefCount == 0))
                t.Value.ResetUnloadCounter(0);
        }

        string __manifestName;
        AssetBundleManifest __manifest;

        /// <summary>
        /// General exception objest which cover all errors occuring during the series of LoadXxxx(..) method.
        /// </summary>
        public class LoadException : Exception {
            public LoadException(string errorMsg) : base(errorMsg) {}
        }

        public IObservable<bool> Init() {
            __manifestName = AssetsConfig.Instance.GetManifestUrl();

            return Observable.FromCoroutine<bool>((observer, cancelToken) =>
                InitCore(observer, cancelToken)
            );
        }

        IEnumerator InitCore(IObserver<bool> observer, CancellationToken cancelToken) {
            if (AssetsConfig.Instance.setSimulationMode) {
                Debug.Log("AssetLoader => Running on simulation mode.");

                observer.OnNext(true);
                observer.OnCompleted();

                yield break;
            }

            var manifestDownloadUrl = AssetsConfig.Instance.GetDownloadUrl(__manifestName) + string.Format("?timeStamp={0}", DateTime.UtcNow.Ticks.ToString());
            
            Debug.LogFormat("AssetLoader => Start downloading AssetBundle Manifest at '{0}'.", manifestDownloadUrl);

            var www = UnityWebRequestAssetBundle.GetAssetBundle(manifestDownloadUrl);

            yield return www.SendWebRequest();

            if (cancelToken.IsCancellationRequested)
                yield break;
            
            if (www.result == UnityWebRequest.Result.ConnectionError) {
                observer.OnError(new Exception(www.error));
                yield break;
            }

            __manifest = DownloadHandlerAssetBundle.GetContent(www).LoadAsset<AssetBundleManifest>("AssetBundleManifest");

            if (__manifest == null) {
                observer.OnError(new Exception(string.Format("AssetsLoader => Failed to load AssetBundle Manifest at '{0}'", manifestDownloadUrl)));
                yield break;
            }

            observer.OnNext(true);
            observer.OnCompleted();
        }

        public IObservable<__AssetRef> Load(AssetAlias alias, IProgress<float> progress = null) {
            return LoadCore(alias, progress);
        }

        /// <summary>
        /// If bundleName is empty string, It's called by LoadResource<T>() method.
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="bundleName"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        IObservable<__AssetRef> LoadCore(AssetAlias alias, IProgress<float> progress = null) {
#if UNITY_EDITOR
            if (AssetsConfig.Instance.setSimulationMode) {
                return Observable.FromCoroutine<__AssetRef>(observer =>
                    LoadSimulationMode(alias, observer)
                    );
            }
#endif

            return Observable.FromCoroutine<__AssetRef>((observer, cancelToken) =>
                LoadAssetBundleCore(alias, progress, observer, cancelToken)
                );
        }

#if UNITY_EDITOR
        IEnumerator LoadSimulationMode(AssetAlias alias, IObserver<__AssetRef> observer) {
            var assetPaths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(alias.BundleName, alias.AssetName);

            if (assetPaths.Length == 0) {
                observer.OnError(new LoadException(string.Format("<SimulationMode> AssetsLoader => '{0}' asset doesn't exist at '{1}' AssetBundle.", alias.AssetName, alias.BundleName)));
                yield break;
            }

            var asset = UnityEditor.AssetDatabase.LoadMainAssetAtPath(assetPaths[0]);

            if (asset == null) {
                observer.OnError(new LoadException(string.Format("<SimulationMode> AssetsLoader => Asset doesn't exist at '{0}'.", assetPaths[0])));
                yield break;
            }

        Debug.LogFormat("'{0}' is loaded.", alias.AssetName);

            observer.OnNext(new __AssetRef(asset, null, new AssetAlias(alias.BundleName, alias.AssetName), new BundleTracker[] {}));
            observer.OnCompleted();
        }
#endif

        IEnumerator LoadAssetBundleCore(AssetAlias alias, IProgress<float> progress, IObserver<__AssetRef> observer, CancellationToken cancelToken) {
            var dependencies = __manifest.GetAllDependencies(alias.BundleName);
            var totalProgress = 0f;

            ScheduledNotifier<float> innerProgress = null;

            if (progress != null) {
                innerProgress = new ScheduledNotifier<float>();

                innerProgress.Subscribe(p => {
                    totalProgress += p / (dependencies.Length + 2f);
                    progress.Report(totalProgress);
                });
            }

            // Download sub-assets, first~
            foreach (var d in dependencies) {
                var hash = __manifest.GetAssetBundleHash(d);

                if (__trackers.ContainsKey(hash)) {
                    var tracker = __trackers[hash];

                    // Reset unload counter to prevent an asset-bundle unloaded
                    tracker.ResetUnloadCounter(BundleTracker.defaultUnloadCountResetValue);

                    yield return new WaitUntil(() => tracker.IsLoadFinished || cancelToken.IsCancellationRequested);
                }
                else {
                    __trackers.Add(hash, new BundleTracker(d, hash));

                    var downloadUrl = AssetsConfig.Instance.GetDownloadUrl(d) + string.Format("?hash={0}", hash.ToString());
                    var www = UnityWebRequestAssetBundle.GetAssetBundle(downloadUrl, hash);

                    yield return www.SendWebRequest();

                    if (www.result == UnityWebRequest.Result.ConnectionError || DownloadHandlerAssetBundle.GetContent(www) == null) {
                        __trackers[hash].SetAssetBundle(null);
                        StartUnloadCounter(hash);

                        observer.OnError(new Exception(string.Format("AssetsLoader => Failed to download dependency '{0}' AssetBundle.", d)));
                        yield break;
                    }

                    __trackers[hash].SetAssetBundle(DownloadHandlerAssetBundle.GetContent(www));
                    StartUnloadCounter(hash);
                }

                if (cancelToken.IsCancellationRequested)
                    yield break;
            }

            var mainHash = __manifest.GetAssetBundleHash(alias.BundleName);

            BundleTracker mainTracker = null;

            if (__trackers.ContainsKey(mainHash)) {
                mainTracker = __trackers[mainHash];

                // Reset unload counter to prevent  asset-bundle unloaded
                mainTracker.ResetUnloadCounter(BundleTracker.defaultUnloadCountResetValue);

                yield return new WaitUntil(() => mainTracker.IsLoadFinished || cancelToken.IsCancellationRequested);

                if (cancelToken.IsCancellationRequested)
                    yield break;
            }
            else {
                mainTracker = new BundleTracker(alias.BundleName, mainHash);

                __trackers.Add(mainHash, mainTracker);

                var downloadUrl = AssetsConfig.Instance.GetDownloadUrl(alias.BundleName) + string.Format("?hash={0}", mainHash.ToString());
                var www = UnityWebRequestAssetBundle.GetAssetBundle(downloadUrl, mainHash);

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || DownloadHandlerAssetBundle.GetContent(www) == null) {
                    observer.OnError(new Exception(string.Format("AssetsLoader => Failed to download '{0}' AssetBundle.", alias.BundleName)));
                    yield break;
                }

                mainTracker.SetAssetBundle(DownloadHandlerAssetBundle.GetContent(www));
                StartUnloadCounter(mainHash);
            }

            if (cancelToken.IsCancellationRequested)
                yield break;

            // Collects all AssetBundleTracker refs
            var dependencyTrackers = dependencies
                .Select(d => __manifest.GetAssetBundleHash(d))
                .Select(h => __trackers[h])
                .Where(t => t != null)
                .ToList();

            dependencyTrackers.Add(mainTracker);

            if (string.IsNullOrEmpty(alias.AssetName)) {
                Debug.LogFormat("AssetsLoader => Complete to download '{0}' AssetBundle.", alias.BundleName);

                observer.OnNext(new __AssetRef(null, mainTracker.Target, alias, dependencyTrackers.ToArray()));
                observer.OnCompleted();
            }
            else {
#if SUPPORT_ASSET_BUNDLE_ASYNC_LOADING
                var loadingAsset = mainTracker.Target.LoadAssetAsync<UnityEngine.Object>(alias.AssetName);

                yield return new WaitUntil(() => loadingAsset.isDone);

                if (cancelToken.IsCancellationRequested) {
                    Debug.LogFormat("AssetsLoader => Cancel to load '{0}' asset in '{1}' AssetBundle.", alias.AssetName, alias.BundleName);
                    yield break;
                }

                if (loadingAsset.asset == null) {
                    observer.OnError(new LoadException(string.Format("AssetsLoader => Failed to load '{0}' Asset in '{1}' AssetBundle.", alias.AssetName, alias.BundleName)));
                    yield break;
                }

                Debug.LogFormat("AssetsLoader => Complete to download '{0}' Asset in '{1}' AssetBundle.", alias.AssetName, alias.BundleName);

                observer.OnNext(new __AssetRef(loadingAsset.asset, mainTracker.Target, alias, dependencyTrackers.ToArray()));
                observer.OnCompleted();
#else
                var loadedAsset = mainTracker.Bundle.LoadAsset(alias.AssetName);

                if (loadedAsset == null) {
                    observer.OnError(new LoadException(string.Format("AssetsLoader => Failed to load '{0}' Asset in '{1}' AssetBundle.", alias.AssetName, alias.BundleName)));
                    yield break;
                }

                observer.OnNext(new __AssetRef(loadedAsset, mainTracker.Target, alias, dependencyTrackers.ToArray()));
                observer.OnCompleted();
#endif
            }
        }

    }

}
