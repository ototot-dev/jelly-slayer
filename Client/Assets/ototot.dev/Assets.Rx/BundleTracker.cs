using System;
using UnityEngine;

namespace Assets.Rx {

    /// <summary>
    /// BundleTracker, as its name implies~, tracks an AssetBundle object whether it's referred of not by 'RefCount' value.
    /// When RefCount become 0, Dispose() will be called and the 'Target' AssetBundle object will be destoyed.
    /// </summary>
    public class BundleTracker : IDisposable {

        /// <summary>
        /// The loaded AssetBundle property
        /// </summary>
        public AssetBundle Target { get; private set; }

        /// <summary>
        /// AssetBundle unique hash property
        /// </summary>
        public Hash128 Hash { get; private set; }

        /// <summary>
        /// AssetBundle's name property
        /// </summary>
        public string BundleName { get; private set; }

        /// <summary>
        /// Reference count which means how many BundleRef objects are refering this AssetBundleTracker object
        /// </summary>
        public int RefCount { get; private set; }

        /// <summary>
        /// True if AssetBundle load operation is finished (It doesn't mean the load operation is successed!!)
        /// </summary>
        public bool IsLoadFinished { get; private set; }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="hash"></param>
        public BundleTracker(string bundleName, Hash128 hash) {
            BundleName = bundleName;
            Hash = hash;
        }

        /// <summary>
        /// Set Bundle property
        /// </summary>
        /// <param name="bundle"></param>
        public void SetAssetBundle(UnityEngine.AssetBundle bundle) {
            if (bundle != null)
                Target = bundle;

            IsLoadFinished = true;
        }

        /// <summary>
        /// Increase RefCount property by one
        /// </summary>
        public int IncreaseRefCount() {
            return ++RefCount;
        }

        /// <summary>
        /// Decrease RefCount property by one
        /// </summary>
        public int DecreaseRefCount() {
            return --RefCount;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (__isDisposed)
                return;

            // Unload AssetBundle object and also destroy all instanciated objects
            if (Target != null)
                Target.Unload(true);

            __isDisposed = true;
        }

        bool __isDisposed = false;

        /// <summary>
        /// This value keeps decreasing and when it hits 0, Dispose() will be called.
        /// </summary>
        int __unloadTimer;

        /// <summary>
        /// 900 fixed ticks which usually means 30 secs
        /// </summary>
        public const int defaultUnloadCountResetValue = 900;

        public void ResetUnloadCounter(int val) {
            __unloadTimer = val;
        }

        /// <summary>
        /// Decreases _unloadTimer field by val
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public int DecreaseUnloadCounter(int val) {
            return (__unloadTimer -= val);
        }

    }
    
}