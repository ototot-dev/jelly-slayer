#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UniRx;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Assets.Rx
{
    /// <summary>
    /// AssetBundler builder
    /// </summary>
    public static class BundleBuilder
    {

        [MenuItem("Assets.Rx//Build/iOS")]
        public static void Build_iOS()
        {
            Run(BuildTarget.iOS);
            EditorApplication.Exit(1);
        }

        [MenuItem("Assets.Rx//Build/Android")]
        public static void Build_Android()
        {
            Run(BuildTarget.Android);
        }

        [MenuItem("Assets.Rx//Build/Windows")]
        public static void Build_Windows()
        {
            Run(BuildTarget.StandaloneWindows64);
        }

        [MenuItem("Assets.Rx//Build/OSX")]
        public static void Build_OSX()
        {
            Run(BuildTarget.StandaloneOSX);
        }

        /// <summary>
        /// Runs AssetBundle build process.
        /// </summary>
        /// <param name="targetPlatform"></param>
        static void Run(BuildTarget targetPlatform)
        {
            var args = Environment.GetCommandLineArgs()
                .Select(a => a.ToLowerInvariant())
                .ToList();

            var outputPath = Application.dataPath + "/" + AssetsConfig.Instance.outputPath;

            if (args.Any(a => a == "-outputpath"))
            {
                var foundIndex = args.FindIndex(a => a == "-outputpath");

                if (foundIndex != -1 && foundIndex + 1 < args.Count)
                    outputPath = args.ElementAt(foundIndex + 1);
            }

            Debug.LogFormat("'{0}' is set to AssetBundle output path.", outputPath);

            if (!Directory.Exists(outputPath))
            {
                if (!Directory.CreateDirectory(outputPath).Exists)
                {
                    Debug.LogWarningFormat("'{0}' is not a valid path.", outputPath);
                    return;
                }
            }

            if (args.Any(a => a == "-batchmode"))
            {
                BundleBuilder.Init(targetPlatform, outputPath);

                while (true)
                {
                    var ret = BundleBuilder.Process();

                    if (ret == 0)
                        continue;

                    if (ret == 1)
                    {
                        BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.None, targetPlatform);
                        Debug.LogFormat("AssetBundle build is completed for '{0}'", targetPlatform.ToString());
                    }
                    else if (ret == -1)
                    {
                        Debug.LogFormat("AssetBundle build is canceled.");
                    }

                    break;
                }

                EditorApplication.Exit(1);
            }
            else
            {
                BundleBuildWindow.Init(targetPlatform, outputPath);
            }
        }

        /// <summary>
        /// Item1 is short path (under 'Assets/') and Item2 is full path.
        /// </summary>
        static Queue<Tuple<string, string>> __assetPaths = new Queue<Tuple<string, string>>();

        /// <summary>
        /// Target platform.
        /// </summary>
        static BuildTarget __targetPlatform = BuildTarget.NoTarget;

        /// <summary>
        /// Build output path.
        /// </summary>
        static string __outputPath;

        /// <summary>
        /// Total Asset number.
        /// </summary>
        public static int totalAssetNum;

        /// <summary>
        /// Processed Asset number
        /// </summary>
        public static int processedAssetNum;

        /// <summary>
        /// Return true if any change exist.
        /// </summary>
        static bool __hasDirtyAsset;

        /// <summary>
        /// Returns true if cancel is being pended.
        /// </summary>
        public static bool isCancelPending;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetPlatform"></param>
        /// <param name="outputPath"></param>
        public static void Init(BuildTarget targetPlatform, string outputPath)
        {
            __assetPaths.Clear();
            __targetPlatform = targetPlatform;
            __outputPath = outputPath;
            __hasDirtyAsset = false;

            isCancelPending = false;

            var query = AssetDatabase.GetAllAssetPaths()
                .Where(a => a.StartsWith("Assets/" + AssetsConfig.Instance.sourcePath));

            foreach (var q in query)
            {
                var assetPath = q.Substring("Assets/".Length);
                __assetPaths.Enqueue(new Tuple<string, string>(assetPath, Application.dataPath + "/" + assetPath));
            }

            totalAssetNum = __assetPaths.Count;
            processedAssetNum = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static int Process()
        {
            if (isCancelPending)
                return -1;

            if (__assetPaths.Count == 0)
            {
                if (__hasDirtyAsset)
                    AssetDatabase.SaveAssets();

                return 1;
            }

            var assetPath = __assetPaths.Dequeue();

            if (Directory.Exists(assetPath.Item2) || assetPath.Item1.EndsWith(".cs") || assetPath.Item1.EndsWith(".js"))
            {
                var importer = AssetImporter.GetAtPath("Assets/" + assetPath.Item1);

                if (!string.IsNullOrEmpty(importer.assetBundleName))
                {
                    Debug.LogFormat("{0} is excluded from {1} AssetBundle.", assetPath, importer.assetBundleName);

                    importer.assetBundleName = string.Empty;
                    __hasDirtyAsset = true;
                }
            }
            else
            {
                var alias = new AssetAlias(assetPath.Item1);

                if (alias.HasAssetPath && !assetPath.Item1.Contains("/Resources/"))
                {
                    var importer = AssetImporter.GetAtPath("Assets/" + assetPath.Item1);

                    if (importer.assetBundleName != alias.BundleName)
                    {
                        importer.assetBundleName = alias.BundleName;
                        __hasDirtyAsset = true;

                        Debug.LogFormat("{0} is assigned to {1} AssetBundle.", assetPath, alias.BundleName);
                    }
                }
                else
                {
                    var importer = AssetImporter.GetAtPath("Assets/" + assetPath.Item1);

                    if (importer.assetBundleName != string.Empty)
                    {
                        Debug.LogFormat("{0} is excluded from {1} AssetBundle.", assetPath, importer.assetBundleName);

                        importer.assetBundleName = string.Empty;
                        __hasDirtyAsset = true;
                    }
                }
            }

            processedAssetNum = totalAssetNum - __assetPaths.Count;

            return 0;
        }
    }
}
#endif