#region License

// // --------------------------------------------------------------------------------------------------------------------
// // <summary>
// //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// //   EditorPrefsHandler.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

#endregion

#if UNITY_EDITOR

using System;
using UnityEditor;

namespace FinalFactory.Preferences.Handlers
{
    internal class EditorPrefsHandler : PrefsHandlerBase
    {
        public override bool CanSave => false;
        public override bool CanLoad => false;
        public override PrefsScope Scope => PrefsScope.Editor;

        internal override void InternalSetString(string key, string value) => EditorPrefs.SetString(key, value);

        public override string GetString(string key, string defaultValue = default) => EditorPrefs.GetString(key, defaultValue);

        internal override void InternalSetInt(string key, int value) => EditorPrefs.SetInt(key, value);

        public override int GetInt(string key, int defaultValue = default) => EditorPrefs.GetInt(key, defaultValue);

        internal override void InternalSetFloat(string key, float value) => EditorPrefs.SetFloat(key, value);

        public override float GetFloat(string key, float defaultValue = default) => EditorPrefs.GetFloat(key, defaultValue);

        internal override void InternalSetBool(string key, bool value) => EditorPrefs.SetBool(key, value);

        public override bool GetBool(string key, bool defaultValue = default) => EditorPrefs.GetBool(key, defaultValue);

        public override object GetObject(string key)
        {
            var str = EditorPrefs.GetString(key, "$#$#$null$#$#$");
            if (str == "$#$#$null$#$#$")
            {
                var f = EditorPrefs.GetFloat(key, float.NaN);
                if (float.IsNaN(f))
                {
                    var i = EditorPrefs.GetInt(key, int.MinValue + 1);
                    if (i == int.MinValue + 1)
                    {
                        return EditorPrefs.GetBool(key);
                    }
                    return i;
                }

                return f;
            }
            
            return str;
        }

        public override bool HasKey(string key) => EditorPrefs.HasKey(key);

        public override void DeleteKey(string key) => EditorPrefs.DeleteKey(key);

        public override void DeleteAll() => EditorPrefs.DeleteAll();
        public override void Load() => throw new NotSupportedException("Unity does not support load preferences from save file.");

        public override void Save() => throw new NotSupportedException("Unity does not support save preferences to save file.");

        public override string[] Keys => Prefs.Reader.RetrieveSavedPrefs(PrefsScope.Editor);
    }
}
#endif