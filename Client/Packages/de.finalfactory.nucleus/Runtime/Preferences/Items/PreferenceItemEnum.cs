#region License

// // --------------------------------------------------------------------------------------------------------------------
// // <summary>
// //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// //   PreferenceItemEnum.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

#endregion

using System;

namespace FinalFactory.Preferences.Items
{
    /// <summary>
    /// Stores an enum value in the preferences by name.
    /// You can edit the enum value, add, remove or change the order of the enum values without losing the stored value.
    /// But if you change the name of the enum value, the stored value will be lost.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PreferenceItemEnum<T> : PreferenceItem<T>
    {
        
        public PreferenceItemEnum(IPrefs prefs, string key, T defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(prefs, key, defaultValue, syncEnabled, encrypt)
        {
            
        }

        public PreferenceItemEnum(PrefsScope scope, string key, T defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(scope, key, defaultValue, syncEnabled, encrypt)
        {
            
        }
#if UNITY_EDITOR
        /// <summary>
        /// EDITOR ONLY
        /// </summary>
        public PreferenceItemEnum(string key, T defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(key, defaultValue, syncEnabled, encrypt)
        {
            
        }
#endif

        protected override T ReadFromStorage()
        {
            string value;
            
#if FINAL_PREFERENCES
            if (IsEncrypted)
            {
                value = PrefsHandler.GetEncryptedString(Key, DefaultValue.ToString());
            }
            else
#endif
            {
                value = PrefsHandler.GetString(Key, DefaultValue.ToString());
            }
            return (T)Enum.Parse(typeof(T), value);
        }

        protected override void WriteToStorage()
        {
#if FINAL_PREFERENCES
            if (IsEncrypted)
            {
                PrefsHandler.SetEncryptedString(Key, Value.ToString());
                return;
            }
#endif
            PrefsHandler.SetString(Key, Value.ToString());
        }
    }
}