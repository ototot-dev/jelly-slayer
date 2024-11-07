using System;
using System.Text;
using UnityEngine;


namespace Assets.Rx {

    /// <summary>
    /// AssetAliasRule defines resolve rules which converts Asset's path data to AssetAlias.
    /// </summary>
    public static class AssetAliasRule {
        
        /// <summary>
        /// Resolves Asset's path to Asset's name and AssetBundle's name.
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static bool Resolve(ref string assetPath, ref string bundleName, ref string assetName) {
            try {
                bundleName = assetPath.Substring(0, assetPath.LastIndexOf('/'))
                    .Replace('/', '-')
                    .Trim()
                    .ToLowerInvariant();

                assetName = assetPath.Substring(bundleName.Length + 1);

                return true;
            }
            catch (Exception e) {
                Debug.LogWarningFormat("'{0}' path is not valid.", assetPath);
                Debug.LogException(e);  

                return false;
            }
        }

    }

}