using UnityEngine;
using System;
using System.Linq;

namespace Assets.Rx {

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AssetRef<T> : __AssetRef where T : UnityEngine.Object {

        /// <summary>
        /// Returns asset as type of T.
        /// </summary>
        /// <value></value>
        public T Value { 
            get { return GetValue() as T; } 
        }

        /// <summary>
        /// Returns PoolingGroup name.
        /// </summary>
        /// <value></value>
        public string PoolingGroupName { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="poolingGroupName"></param>
        /// <returns></returns>
        public AssetRef(__AssetRef source, string poolingGroupName) : base(source.GetValue(), source.Bundle, source.Alias, null) {
            PoolingGroupName = poolingGroupName;

            if (GetValue() == null)
                Debug.LogWarning("AssetRef => Asset is null.");
            else if (Value == null) 
                Debug.LogWarningFormat("AssetRef => '{0}' Asset is not type of {1}", GetValue().name, typeof(T).Name);
        }

        /// <summary>
        /// Overrides Dispose() to be only released and returned to AssetsManager PoolingGroup.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing) {
            if (__isDisposed)
                return;

            var poolingGroup = AssetsManager.Instance.GetPoolingGroup(PoolingGroupName);

            if (poolingGroup != null) 
                poolingGroup.Release(Alias);

            __isDisposed = true;
        }

    }

    /// <summary>
    /// __AssetRef is base class for AssetRef<T> class.
    /// </summary>
    public class __AssetRef : IDisposable {

        /// <summary>
        /// Asset object.
        /// </summary>
        UnityEngine.Object __value;

        /// <summary>
        /// Returns asset. 
        /// </summary>
        /// <returns></returns>
        public UnityEngine.Object GetValue() {
            return __value;
        }

        int __refCount;

        public int IncreaseRefCount() {
            return ++__refCount;
        }

        public int DecreaseRefCount() {
            return Math.Max(--__refCount, 0);
        }

        /// <summary>
        /// Returns AssetBundle.
        /// </summary>
        public AssetBundle Bundle { get; private set; }

        /// <summary>
        /// Returns AssetAlias.
        /// </summary>
        /// <value></value>
        public AssetAlias Alias { get; private set; }

        /// <summary>
        /// Returns Trackers 
        /// </summary>
        /// <value></value>
        public BundleTracker[] Trackers { get; private set; }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="bundle"></param>
        /// <param name="alias"></param>
        /// <param name="trackers"></param>
        public __AssetRef(UnityEngine.Object val, UnityEngine.AssetBundle bundle, AssetAlias alias, BundleTracker[] trackers) {
            __value = val;
            __refCount = 0;
            Bundle = bundle;
            Alias = alias;
            Trackers = trackers;

            if (Trackers != null) {
                foreach (var t in Trackers) 
                    t.IncreaseRefCount();
            }
        }

        /// <summary>
        /// Implements Dispose pattern.
        /// </summary>
        public void Dispose() {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///  AssetsManager PoolingGroup has an instance of __AssetRef, not AssetRef<T> object.
        /// when Dispose() is called for __AssetRef, it will be discared from PoolingGroup and Trackers need to be cleaned up.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (__isDisposed)
                return;

            foreach (var t in Trackers) 
                t.DecreaseRefCount();

            __isDisposed = true;
        }

        protected bool __isDisposed = false;

    }

}