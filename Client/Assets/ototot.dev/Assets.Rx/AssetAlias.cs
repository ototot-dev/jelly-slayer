using UnityEngine;

namespace Assets.Rx {

    /// <summary>
    /// AssetAlias holds Asset's location data.
    /// </summary>
    public struct AssetAlias {

        /// <summary>
        /// Invalid value.
        /// </summary>
        /// <returns></returns>
        public static readonly AssetAlias Invalid = new AssetAlias();

        /// <summary>
        /// Returns Asset's name.
        /// </summary>
        /// <value></value>
        public string AssetName {
            get { return __assetName; }
        }

        /// <summary>
        /// Returns AssetBundle's name.
        /// </summary>
        /// <value></value>
        public string BundleName {
            get { return __bundleName; }
        }

        /// <summary>
        /// Returns path (Relative path under 'Assets' folder).
        /// </summary>
        /// <value></value>
        public string AssetPath {
            get { return HasAssetPath ? __assetPath : string.Empty; }
        }

        /// <summary>
        /// Returns if AssetAlias has path data.
        /// </summary>
        /// <value></value>
        public bool HasAssetPath {
            get { return __hasAssetPath; }
        }

        /// <summary>
        /// Returns if AssetAlias is valid.
        /// </summary>
        /// <value></value>
        public bool IsValid {
            get { return HasAssetPath || (!string.IsNullOrEmpty(__bundleName) && !string.IsNullOrEmpty(__assetName)); }
        }

        string __bundleName;

        string __assetName;

        string __assetPath;

        bool __hasAssetPath;

        /// <summary>
        /// Overrides ToString().
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            if (!IsValid)
                return "Invalid";

            if (HasAssetPath) {
                return string.Format("//{0}", __assetPath);
            }
            else {
                return string.Format("{0}/{1}", __bundleName, __assetName);
            }
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        public AssetAlias(string bundleName, string assetName) {
            __bundleName = bundleName;
            __assetName = assetName;
            __assetPath = string.Empty;
            __hasAssetPath = false;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="assetPath"></param>
        public AssetAlias(string assetPath) {
            __bundleName = string.Empty;
            __assetName = string.Empty;
            __assetPath = assetPath;
            __hasAssetPath = AssetAliasRule.Resolve(ref __assetPath, ref __bundleName, ref __assetName);
        }

    }

}
