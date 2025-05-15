#region License

// // --------------------------------------------------------------------------------------------------------------------
// // <summary>
// //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// //   PreferenceItemListString.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

#endregion

using System.Collections.Generic;
using JetBrains.Annotations;

namespace FinalFactory.Preferences.Items
{
    [PublicAPI]
    public class PreferenceItemListString : PreferenceItemList<string>
    {
        public PreferenceItemListString(IPrefs prefs, string key, List<string> defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(prefs, key, defaultValue, syncEnabled, encrypt)
        {
        }

        public PreferenceItemListString(PrefsScope scope, string key, List<string> defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(scope, key, defaultValue, syncEnabled, encrypt)
        {
        }
#if UNITY_EDITOR
        /// <summary>
        /// EDITOR ONLY
        /// </summary>
        public PreferenceItemListString(string key, List<string> defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(key, defaultValue, syncEnabled, encrypt)
        {
        }
#endif

        protected override string StringToValue(string str)
        {
            return str;
        }

        protected override string ValueToString(string value)
        {
            return value;
        }
    }
}