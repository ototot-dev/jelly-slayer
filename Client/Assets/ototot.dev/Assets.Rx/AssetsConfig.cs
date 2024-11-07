using UnityEngine;

namespace Assets.Rx {

    /// <summary>
    /// Assets.Rx configuration.
    /// </summary>
    // [CreateAssetMenu(fileName = "Config", menuName = "Assets.Rx/Create Config", order = 1)]
    public class AssetsConfig : ScriptableObject {

        /// <summary>
        /// The single root path under which all assets to be built as AssetBundle are.
        /// </summary>
        public string sourcePath = "__Data";

        /// <summary>
        /// The path where AssetBundle build result is saved.
        /// </summary>
        public string outputPath = "../Build/AssetBundle";

        /// <summary>
        /// AssetBundle remote url.
        /// </summary>
        public string remoteUrl;

        /// <summary>
        /// In Editor Mode, forces to use remoteUrl instead of local AssetBundle outputPath.
        /// </summary>
        public bool useRemoteUrlInEditorMode = false;

        /// <summary>
        /// Set as simulation mode.
        /// </summary>
        public bool setSimulationMode = false;

        /// <summary>
        /// 
        /// </summary>
        public string[] activeVariants;

        /// <summary>
        /// Returns AssetBundle download url.
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        public string GetDownloadUrl(string bundleName) {
#if UNITY_EDITOR
            if (useRemoteUrlInEditorMode)
                return (remoteUrl + "/" + bundleName.Replace(" ", "%20"));
            else
                return ("file://" + Application.dataPath + "/" + outputPath + "/" + bundleName);
#else
            return (remoteUrl + "/" + bundleName.Replace(" ", "%20"));
#endif
        }

        /// <summary>
        /// Returns Manifest download url.
        /// </summary>
        /// <returns></returns>
        public string GetManifestUrl() {
#if UNITY_EDITOR
            if (useRemoteUrlInEditorMode)
                return remoteUrl.Substring(remoteUrl.LastIndexOf('/') + 1).ToLowerInvariant();
            else
                return outputPath.Substring(outputPath.LastIndexOf('/') + 1).ToLowerInvariant();
#else
            return remoteUrl.Substring(remoteUrl.LastIndexOf('/') + 1).ToLowerInvariant();
#endif
        }

        /// <summary>
        /// Returns AssetsConfig full path.
        /// </summary>
        /// <value></value>
        public static string FullPath { 
            get { return "Assets/ototot.dev/Assets.Rx/Resources/AssetsConfig.asset"; } 
        }

        /// <summary>
        /// Returns AssetsConfig file name.
        /// </summary>
        /// <value></value>
        public static  string FileName { 
            get { return "AssetsConfig"; } 
        }

        /// <summary>
        /// Singleton.
        /// </summary>
        /// <value></value>
        public static AssetsConfig Instance {
            get {
#if UNITY_EDITOR
                if (__instance == null) {
                    if (Application.isPlaying)
                        __instance = Resources.Load<AssetsConfig>(FileName);
                    else
                        __instance = UnityEditor.AssetDatabase.LoadAssetAtPath<AssetsConfig>(FullPath);
                }
#else
                if (__instance == null)
                    __instance = Resources.Load<AssetsConfig>(FileName);
#endif

                return __instance;
            }
        }

        static AssetsConfig __instance;

    }

}
