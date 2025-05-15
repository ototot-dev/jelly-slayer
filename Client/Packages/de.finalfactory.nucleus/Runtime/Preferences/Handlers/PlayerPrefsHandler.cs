#region License

// // --------------------------------------------------------------------------------------------------------------------
// // <summary>
// //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// //   PlayerPrefsHandler.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

#endregion

using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace FinalFactory.Preferences.Handlers
{
    internal class PlayerPrefsHandler : PrefsHandlerBase
    {
        public override bool CanSave => true;
        public override bool CanLoad => false;
        public override PrefsScope Scope => PrefsScope.Player;

        [MethodImpl(GlobalConst.ImplOptions)]
        internal override void InternalSetString(string key, string value) => PlayerPrefs.SetString(key, value);

        [MethodImpl(GlobalConst.ImplOptions)]
        public override string GetString(string key, string defaultValue = default) =>
            PlayerPrefs.GetString(key, defaultValue);

        [MethodImpl(GlobalConst.ImplOptions)]
        internal override void InternalSetInt(string key, int value) => PlayerPrefs.SetInt(key, value);

        [MethodImpl(GlobalConst.ImplOptions)]
        public override int GetInt(string key, int defaultValue = default) => PlayerPrefs.GetInt(key, defaultValue);

        [MethodImpl(GlobalConst.ImplOptions)]
        internal override void InternalSetFloat(string key, float value) => PlayerPrefs.SetFloat(key, value);

        [MethodImpl(GlobalConst.ImplOptions)]
        public override float GetFloat(string key, float defaultValue = default) =>
            PlayerPrefs.GetFloat(key, defaultValue);

        [MethodImpl(GlobalConst.ImplOptions)]
        internal override void InternalSetBool(string key, bool value) => PlayerPrefs.SetInt(key, Convert.ToInt32(value));

        [MethodImpl(GlobalConst.ImplOptions)]
        public override bool GetBool(string key, bool defaultValue = default) =>
            Convert.ToBoolean(PlayerPrefs.GetInt(key, Convert.ToInt32(defaultValue)));

        public override object GetObject(string key)
        {
            var str = PlayerPrefs.GetString(key, "$#$#$null$#$#$");
            if (str == "$#$#$null$#$#$")
            {
                var f = PlayerPrefs.GetFloat(key, float.NaN);
                if (float.IsNaN(f))
                {
                    return PlayerPrefs.GetInt(key);
                }

                return f;
            }

            return str;
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public override bool HasKey(string key) => PlayerPrefs.HasKey(key);

        [MethodImpl(GlobalConst.ImplOptions)]
        public override void DeleteKey(string key) => PlayerPrefs.DeleteKey(key);

        [MethodImpl(GlobalConst.ImplOptions)]
        public override void DeleteAll() => PlayerPrefs.DeleteAll();

        [MethodImpl(GlobalConst.ImplOptions)]
        public override void Load() =>
            throw new NotSupportedException("Unity does not support load preferences from save file.");

        [MethodImpl(GlobalConst.ImplOptions)]
        public override void Save() => PlayerPrefs.Save();

        public override string[] Keys =>
#if UNITY_EDITOR
            Prefs.Reader.RetrieveSavedPrefs(PrefsScope.Player);
#else
            throw new NotSupportedException("Unity does not support retrieve a list of keys from PlayerPrefs.");
#endif
    }
}