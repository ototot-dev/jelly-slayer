#region License

// // --------------------------------------------------------------------------------------------------------------------
// // <summary>
// //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// //   PreferenceItemBool.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

#endregion

using JetBrains.Annotations;

namespace FinalFactory.Preferences.Items
{
    [PublicAPI]
    public class PreferenceItemBool : PreferenceItem<bool>
    {
        public PreferenceItemBool(IPrefs prefs, string key, bool defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(prefs, key, defaultValue, syncEnabled, encrypt)
        {
        }

        public PreferenceItemBool(PrefsScope scope, string key, bool defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(scope, key, defaultValue, syncEnabled, encrypt)
        {
        }
#if UNITY_EDITOR
        /// <summary>
        /// EDITOR ONLY
        /// </summary>
        public PreferenceItemBool(string key, bool defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(key, defaultValue, syncEnabled, encrypt)
        {
        }
#endif

        protected override bool ReadFromStorage()
        {
#if FINAL_PREFERENCES
            if (IsEncrypted)
            {
                return PrefsHandler.GetEncryptedBool(Key, DefaultValue);
            }
#endif
            return PrefsHandler.GetBool(Key, DefaultValue);
        }

        protected override void WriteToStorage()
        {
#if FINAL_PREFERENCES
            if (IsEncrypted)
            {
                PrefsHandler.SetEncryptedBool(Key, Value);
                return;
            }
#endif
            PrefsHandler.SetBool(Key, Value);
        }
    }
}