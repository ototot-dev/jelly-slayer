#region License

// // --------------------------------------------------------------------------------------------------------------------
// // <summary>
// //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// //   PreferenceItemString.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

#endregion

namespace FinalFactory.Preferences.Items
{
    public class PreferenceItemString : PreferenceItem<string>
    {
        
        public PreferenceItemString(IPrefs prefs, string key, string defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(prefs, key, defaultValue, syncEnabled, encrypt)
        {
            
        }

        public PreferenceItemString(PrefsScope scope, string key, string defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(scope, key, defaultValue, syncEnabled, encrypt)
        {
            
        }
#if UNITY_EDITOR
        /// <summary>
        /// EDITOR ONLY
        /// </summary>
        public PreferenceItemString(string key, string defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(key, defaultValue, syncEnabled, encrypt)
        {
            
        }
#endif

        protected override string ReadFromStorage()
        {
#if FINAL_PREFERENCES
            if (IsEncrypted)
            {
                return PrefsHandler.GetEncryptedString(Key, DefaultValue);
            }
#endif
            return PrefsHandler.GetString(Key, DefaultValue);
        }

        protected override void WriteToStorage()
        {
#if FINAL_PREFERENCES
            if (IsEncrypted)
            {
                PrefsHandler.SetEncryptedString(Key, Value);
                return;
            }
#endif
            PrefsHandler.SetString(Key, Value);
        }
    }
}