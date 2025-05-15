#region License

// // --------------------------------------------------------------------------------------------------------------------
// // <summary>
// //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// //   ProjectPrefsUtilities.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

#endregion


//#define F_TRACE

using System;
using System.Diagnostics;
using FinalFactory.Logging;
using UnityEngine;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace FinalFactory.Utilities
{
    public class SingletonScriptableLoader
    {
        private static readonly Log Log = LogManager.GetLogger(typeof(SingletonScriptableLoader));
        
        public static T Load<T>(string defaultPath, string assetName, Func<T, bool> validateAsset, Action<T> onNewAssetCreated) where T : ScriptableObject
        {
            if (validateAsset == null)
            {
                validateAsset = o => o != null;
            }
            T tagMap = null;
            var path = defaultPath + assetName + ".asset";
            try
            {
#if UNITY_EDITOR
                Trace("Try to load asset from default path.");
                tagMap = AssetDatabase.LoadAssetAtPath<T>(path);
                if (tagMap == null)
                {
                    Trace("Asset not found in default path. Searching for asset in project.");
                    var guids = AssetDatabase.FindAssets("t:ProjectPrefsObject");
                    if (guids.Length > 0)
                        for (var i = 0; i < guids.Length; i++)
                        {
                            var assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                            var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                            if (validateAsset(asset))
                            {
                                //check asset name
                                if (asset.name != assetName)
                                {
                                    Log.Warn($"The {asset.name} is not named correctly. It must be named {assetName}. Renaming asset to {assetName}.");
                                    EditorGUIUtility.PingObject(asset);
                                    
                                    AssetDatabase.RenameAsset(assetPath, assetName);
                                    EditorUtility.SetDirty(asset);
                                    AssetDatabase.SaveAssetIfDirty(asset);
                                    AssetDatabase.Refresh();
                                }
                                
                                //check if the parent folder is the Resources folder
                                var parentFolder = assetPath.Substring(0, assetPath.LastIndexOf("/", StringComparison.Ordinal));
                                if (!parentFolder.EndsWith("Resources"))
                                {
                                    Log.Fatal($"The {asset.name} is not in the Resources folder. It must be in the root of an Resources folder to work properly.");
                                    EditorGUIUtility.PingObject(asset);
                                }
                                
                                tagMap = asset;
                                Trace("Asset found in project.");
                                break;
                            }
                        }
                }
#else
                tagMap = Resources.Load<T>(assetName);
#endif
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            if (tagMap == null)
            {
                Trace("Create new asset.");
                tagMap = ScriptableObject.CreateInstance<T>();
                onNewAssetCreated(tagMap);
#if UNITY_EDITOR
                if (!AssetActuallyExists(path))
                {
                    if(!Directory.Exists("Assets/Resources"))
                    {	
                        Directory.CreateDirectory("Assets/Resources");
                        Trace("Resources folder created.");
                    }   
                    AssetDatabase.CreateAsset(tagMap, path);
                    Trace("Asset created at default path.");
                }
                else
                {
                    Log.Error(assetName + " could not be loaded, but it exists at the specified path.");
                }
#else
                Log.Warn(assetName + " not accessable in runtime player. Please check that the asset is in the Resources folder and the file is named correctly. The file should be named " + assetName + ".asset.");
#endif
            }
            return tagMap;
        }
        
#if UNITY_EDITOR
        private static bool AssetActuallyExists(string assetPath)
        {
            var mainAssetTypeAtPath = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            return AssetDatabase.AssetPathToGUID(assetPath) != string.Empty && mainAssetTypeAtPath != null;
        }
#endif

        [Conditional("F_TRACE")]
        private static void Trace(string message) => Log.Trace(message);
    }
}