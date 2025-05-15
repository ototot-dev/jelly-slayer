#region License

// // --------------------------------------------------------------------------------------------------------------------
// // <summary>
// //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// //   PreferenceItemString2.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

#endregion

using System;

namespace FinalFactory.Preferences.Items
{
    public class PreferenceItemString2 : PreferenceItem<(string, string)>
    {
        private const string SpecialSplitString = " ##___!!_:_:_!!___## ";
        

        public PreferenceItemString2(IPrefs prefs, string key, (string, string) defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(prefs, key, defaultValue, syncEnabled, encrypt)
        {
            
        }

        public PreferenceItemString2(PrefsScope scope, string key, (string, string) defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(scope, key, defaultValue, syncEnabled, encrypt)
        {
            
        }
#if UNITY_EDITOR
        /// <summary>
        /// EDITOR ONLY
        /// </summary>
        public PreferenceItemString2(string key, (string, string) defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(key, defaultValue, syncEnabled, encrypt)
        {
            
        }
#endif

        protected override (string, string) ReadFromStorage()
        {
            string v;
#if FINAL_PREFERENCES
            if ( IsEncrypted)
            {
                v = PrefsHandler.GetEncryptedString(Key);
            }
            else
#endif
            {
                v = PrefsHandler.GetString(Key);
            }
            if (string.IsNullOrWhiteSpace(v)) return DefaultValue;
            var s = v.Split(new[] { SpecialSplitString }, StringSplitOptions.None);
            return (s[0], s[1]);
        }

        protected override void WriteToStorage()
        {
            var specialString = Value.Item1 + SpecialSplitString + Value.Item2;
#if FINAL_PREFERENCES
            if (IsEncrypted)
            {
                PrefsHandler.SetEncryptedString(Key, specialString);
                return;
            }
#endif
            PrefsHandler.SetString(Key, specialString);
        }
    }
}