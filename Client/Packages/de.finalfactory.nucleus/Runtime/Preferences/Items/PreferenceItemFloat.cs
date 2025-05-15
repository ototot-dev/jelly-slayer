#region License

// // --------------------------------------------------------------------------------------------------------------------
// // <summary>
// //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// //   PreferenceItemFloat.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

#endregion

using JetBrains.Annotations;

namespace FinalFactory.Preferences.Items
{
    [PublicAPI]
    public class PreferenceItemFloat : PreferenceItem<float>
    {
        
        public PreferenceItemFloat(IPrefs prefs, string key, float defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(prefs, key, defaultValue, syncEnabled, encrypt)
        {
            
        }

        public PreferenceItemFloat(PrefsScope scope, string key, float defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(scope, key, defaultValue, syncEnabled, encrypt)
        {
            
        }
#if UNITY_EDITOR
        /// <summary>
        /// EDITOR ONLY
        /// </summary>
        public PreferenceItemFloat(string key, float defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(key, defaultValue, syncEnabled, encrypt)
        {
            
        }
#endif

        protected override float ReadFromStorage()
        {
#if FINAL_PREFERENCES
            if (IsEncrypted)
            {
                return PrefsHandler.GetEncryptedFloat(Key, DefaultValue);
            }
#endif
            return PrefsHandler.GetFloat(Key, DefaultValue);
        }

        protected override void WriteToStorage()
        {
            
#if FINAL_PREFERENCES
            if (IsEncrypted)
            {
                PrefsHandler.SetEncryptedFloat(Key, Value);
                return;
            }
#endif
            PrefsHandler.SetFloat(Key, Value);
        }
    }
}