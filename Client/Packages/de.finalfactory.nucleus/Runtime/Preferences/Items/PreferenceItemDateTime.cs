#region License

// // --------------------------------------------------------------------------------------------------------------------
// // <summary>
// //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// //   PreferenceItemTimeSpan.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

#endregion

using System;
using System.Globalization;

namespace FinalFactory.Preferences.Items
{
    public class PreferenceItemDateTime : PreferenceItem<DateTime>
    {
        public PreferenceItemDateTime(IPrefs prefs, string key, DateTime defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(prefs, key, defaultValue, syncEnabled, encrypt)
        {
        }

        public PreferenceItemDateTime(PrefsScope scope, string key, DateTime defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(scope, key, defaultValue, syncEnabled, encrypt)
        {
        }
#if UNITY_EDITOR
        /// <summary>
        /// EDITOR ONLY
        /// </summary>
        public PreferenceItemDateTime(string key, DateTime defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(key, defaultValue, syncEnabled, encrypt)
        {
                
        }
#endif

#if FINAL_PREFERENCES
        protected override DateTime ReadFromStorage() => new(long.Parse(PrefsHandler.GetString(IsEncrypted, Key, DefaultValue.Ticks.ToString(CultureInfo.InvariantCulture))));
        protected override void WriteToStorage() => PrefsHandler.SetString(IsEncrypted, Key, Value.Ticks.ToString(CultureInfo.InvariantCulture));
#else
        protected override DateTime ReadFromStorage() => new(long.Parse(PrefsHandler.GetString(Key, DefaultValue.Ticks.ToString(CultureInfo.InvariantCulture))));
        protected override void WriteToStorage() => PrefsHandler.SetString(Key, Value.Ticks.ToString(CultureInfo.InvariantCulture));
#endif
    }
}