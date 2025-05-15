#region License

// // --------------------------------------------------------------------------------------------------------------------
// // <summary>
// //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// //   PreferenceItemInt.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

#endregion

using JetBrains.Annotations;

namespace FinalFactory.Preferences.Items
{
    [PublicAPI]
    public class PreferenceItemInt : PreferenceItem<int>
    {
        
        public PreferenceItemInt(IPrefs prefs, string key, int defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(prefs, key, defaultValue, syncEnabled, encrypt)
        {
            
        }

        public PreferenceItemInt(PrefsScope scope, string key, int defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(scope, key, defaultValue, syncEnabled, encrypt)
        {
            
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// EDITOR ONLY
        /// </summary>
        public PreferenceItemInt(string key, int defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(key, defaultValue, syncEnabled, encrypt)
        {
            
        }
#endif

        protected override int ReadFromStorage()
        {
#if FINAL_PREFERENCES
            if (IsEncrypted)
            {
                return PrefsHandler.GetEncryptedInt(Key, DefaultValue);
            }
#endif
            return PrefsHandler.GetInt(Key, DefaultValue);
        }

        protected override void WriteToStorage()
        {
#if FINAL_PREFERENCES
            if (IsEncrypted)
            {
                PrefsHandler.SetEncryptedInt(Key, Value);
                return;
            }
#endif
            PrefsHandler.SetInt(Key, Value);
        }
    }
}