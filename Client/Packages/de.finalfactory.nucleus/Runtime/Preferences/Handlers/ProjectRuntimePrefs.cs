#region License

// // --------------------------------------------------------------------------------------------------------------------
// // <summary>
// //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// //   ProjectRuntimePrefs.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

#endregion

using FinalFactory.Diagnostics;
using FinalFactory.Logging;
using FinalFactory.Preferences.Utilities;
using JetBrains.Annotations;
using UnityEditor;

namespace FinalFactory.Preferences.Handlers
{
    [PublicAPI]
    internal sealed class ProjectRuntimePrefs
    {
        private static readonly Log Log = LogManager.GetLogger(typeof(ProjectRuntimePrefs));
        private static ProjectPrefsObject _tagMap;
        private static bool _loaded;

        public static void SetString([NotNull] string key, string value)
        {
            DebugCheck.NotNull(key, "The key can not be null");
            CheckLoad();
            _tagMap.AddOrUpdate(key, value);
        }

        public static string GetString([NotNull] string key, string defaultValue = default)
        {
            DebugCheck.NotNull(key, "The key can not be null");
            CheckLoad();
            return _tagMap.GetString(key, defaultValue);
        }

        public static void SetInt([NotNull] string key, int value)
        {
            DebugCheck.NotNull(key, "The key can not be null");
            CheckLoad();
            _tagMap.AddOrUpdate(key, value);
        }

        public static int GetInt([NotNull] string key, int defaultValue = default)
        {
            DebugCheck.NotNull(key, "The key can not be null");
            CheckLoad();
            return _tagMap.GetInt(key, defaultValue);
        }

        public static void SetFloat([NotNull] string key, float value)
        {
            DebugCheck.NotNull(key, "The key can not be null");
            CheckLoad();
            _tagMap.AddOrUpdate(key, value);
        }

        public static float GetFloat([NotNull] string key, float defaultValue = default)
        {
            DebugCheck.NotNull(key, "The key can not be null");
            CheckLoad();
            return _tagMap.GetFloat(key, defaultValue);
        }

        public static void SetBool([NotNull] string key, bool value)
        {
            DebugCheck.NotNull(key, "The key can not be null");
            CheckLoad();
            _tagMap.AddOrUpdate(key, value);
        }

        public static bool GetBool([NotNull] string key, bool defaultValue = default)
        {
            DebugCheck.NotNull(key, "The key can not be null");
            CheckLoad();
            return _tagMap.GetBoolean(key, defaultValue);
        }

        public static object GetObject(string key)
        {
            DebugCheck.NotNull(key, "The key can not be null");
            CheckLoad();
            return _tagMap.GetObject(key);
        }

        public static bool HasKey([NotNull] string key)
        {
            DebugCheck.NotNull(key, "The key can not be null");
            CheckLoad();
            return _tagMap.ContainsKey(key);
        }

        public static void DeleteKey([NotNull] string key)
        {
            DebugCheck.NotNull(key, "The key can not be null");
            CheckLoad();
            _tagMap.RemoveObject(key);
        }

        public static string[] GetAllKeys()
        {
            CheckLoad();
            return _tagMap.GetAllKeys();
        }

        public static void DeleteAll()
        {
            CheckLoad();
            //We can not delete the master encryption keys. This would screw up all encrypted data.
            foreach (var key in _tagMap.GetAllKeys())
            {
                if (key.StartsWith("$MSC$-")) continue;
                _tagMap.RemoveObject(key);
            }
        }

        public static void Load()
        {
            _tagMap = ProjectPrefsUtilities.Load(PrefsScope.ProjectRuntime);
            _loaded = _tagMap != null;
        }

        public static void Save()
        {
#if UNITY_EDITOR
            if (_tagMap == null) Load();
            EditorUtility.SetDirty(_tagMap);
            AssetDatabase.SaveAssetIfDirty(_tagMap);
#else
            Log.Error("Save is only available in the editor");
#endif
        }

        private static void CheckLoad()
        {
            if (!_loaded) Load();
        }
    }
}