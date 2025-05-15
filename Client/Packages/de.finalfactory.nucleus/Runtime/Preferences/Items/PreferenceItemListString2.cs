#region License

// // --------------------------------------------------------------------------------------------------------------------
// // <summary>
// //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// //   PreferenceItemListString2.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

#endregion

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace FinalFactory.Preferences.Items
{
    [PublicAPI]
    public class PreferenceItemListString2 : PreferenceItemList<(string, string)>
    {
        private const string SpecialSplitString = " ##___!!_:_:_!!___## ";


        public PreferenceItemListString2(IPrefs prefs, string key, List<(string, string)> defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(prefs, key, defaultValue, syncEnabled, encrypt)
        {
        }

        public PreferenceItemListString2(PrefsScope scope, string key, List<(string, string)> defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(scope, key, defaultValue, syncEnabled, encrypt)
        {
        }
#if UNITY_EDITOR
        /// <summary>
        /// EDITOR ONLY
        /// </summary>
        public PreferenceItemListString2(string key, List<(string, string)> defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(key, defaultValue, syncEnabled, encrypt)
        {
        }
#endif

        protected override (string, string) StringToValue(string str)
        {
            var s = str.Split(new[] { SpecialSplitString }, StringSplitOptions.None);
            switch (s.Length)
            {
                default:
                    return (string.Empty, string.Empty);
                case 1:
                    return (s[0], string.Empty);
                case 2:
                    return (s[0], s[1]);
            }
        }

        protected override string ValueToString((string, string) value)
        {
            return value.Item1 + SpecialSplitString + value.Item2;
        }
    }
}