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
    public class PreferenceItemTimeSpan : PreferenceItem<TimeSpan>
    {
        public PreferenceItemTimeSpan(IPrefs prefs, string key, TimeSpan defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(prefs, key, defaultValue, syncEnabled, encrypt)
        {
        }

        public PreferenceItemTimeSpan(PrefsScope scope, string key, TimeSpan defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(scope, key, defaultValue, syncEnabled, encrypt)
        {
        }
#if UNITY_EDITOR
        /// <summary>
        /// EDITOR ONLY
        /// </summary>
        public PreferenceItemTimeSpan(string key, TimeSpan defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(key, defaultValue, syncEnabled, encrypt)
        {
        }
#endif

#if FINAL_PREFERENCES
        protected override TimeSpan ReadFromStorage() => TimeSpan.FromTicks(long.Parse(PrefsHandler.GetString(IsEncrypted, Key, DefaultValue.Ticks.ToString(CultureInfo.InvariantCulture))));
        protected override void WriteToStorage() => PrefsHandler.SetString(IsEncrypted, Key, Value.Ticks.ToString(CultureInfo.InvariantCulture));
#else
        protected override TimeSpan ReadFromStorage() => TimeSpan.FromTicks(long.Parse(PrefsHandler.GetString(Key, DefaultValue.Ticks.ToString(CultureInfo.InvariantCulture))));
        protected override void WriteToStorage() => PrefsHandler.SetString(Key, Value.Ticks.ToString(CultureInfo.InvariantCulture));
#endif
    }
}