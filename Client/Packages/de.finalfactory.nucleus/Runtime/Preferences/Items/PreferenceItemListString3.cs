#region License

// // --------------------------------------------------------------------------------------------------------------------
// // <summary>
// //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// //   PreferenceItemListString3.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

#endregion

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using static System.String;

namespace FinalFactory.Preferences.Items
{
    [PublicAPI]
    public class PreferenceItemListString3 : PreferenceItemList<(string, string, string)>
    {
        private const string SpecialSplitString = " ##___!!_:_:_!!___## ";


        public PreferenceItemListString3(IPrefs prefs, string key, List<(string, string, string)> defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(prefs, key, defaultValue, syncEnabled, encrypt)
        {
        }

        public PreferenceItemListString3(PrefsScope scope, string key, List<(string, string, string)> defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(scope, key, defaultValue, syncEnabled, encrypt)
        {
        }
#if UNITY_EDITOR
        /// <summary>
        /// EDITOR ONLY
        /// </summary>
        public PreferenceItemListString3(string key, List<(string, string, string)> defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(key, defaultValue, syncEnabled, encrypt)
        {
        }
#endif

        protected override (string, string, string) StringToValue(string str)
        {
            var s = str.Split(new[] { SpecialSplitString }, StringSplitOptions.None);
            switch (s.Length)
            {
                default:
                    return (Empty, Empty, Empty);
                case 1:
                    return (s[0], Empty, Empty);
                case 2:
                    return (s[0], s[1], Empty);
                case 3:
                    return (s[0], s[1], s[2]);
            }
        }

        protected override string ValueToString((string, string, string) value)
        {
            return value.Item1 + SpecialSplitString + value.Item2 + SpecialSplitString + value.Item3;
        }
    }
}