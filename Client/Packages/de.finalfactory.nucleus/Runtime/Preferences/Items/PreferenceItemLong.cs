#region License

// // --------------------------------------------------------------------------------------------------------------------
// // <summary>
// //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// //   PreferenceItemLong.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

#endregion

using System.Globalization;
using JetBrains.Annotations;

namespace FinalFactory.Preferences.Items
{
    [PublicAPI]
    public class PreferenceItemLong : PreferenceItem<long>
    {
        public PreferenceItemLong(IPrefs prefs, string key, long defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(prefs, key, defaultValue, syncEnabled, encrypt)
        {
            
        }

        public PreferenceItemLong(PrefsScope scope, string key, long defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(scope, key, defaultValue, syncEnabled, encrypt)
        {
            
        }
#if UNITY_EDITOR
        /// <summary>
        /// EDITOR ONLY
        /// </summary>
        public PreferenceItemLong(string key, long defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(key, defaultValue, syncEnabled, encrypt)
        {
            
        }
#endif

#if FINAL_PREFERENCES
        protected override long ReadFromStorage() => long.Parse(PrefsHandler.GetString(IsEncrypted, Key, DefaultValue.ToString(CultureInfo.InvariantCulture)));

        protected override void WriteToStorage() => PrefsHandler.SetString(IsEncrypted, Key, Value.ToString(CultureInfo.InvariantCulture));
#else
        protected override long ReadFromStorage() => long.Parse(PrefsHandler.GetString(Key, DefaultValue.ToString(CultureInfo.InvariantCulture)));

        protected override void WriteToStorage() => PrefsHandler.SetString(Key, Value.ToString(CultureInfo.InvariantCulture));
#endif
    }
}