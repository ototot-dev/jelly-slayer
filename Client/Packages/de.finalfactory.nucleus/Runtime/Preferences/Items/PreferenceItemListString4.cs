#region License

// // --------------------------------------------------------------------------------------------------------------------
// // <summary>
// //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// //   PreferenceItemListString4.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
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
    public class PreferenceItemListString4 : PreferenceItemList<(string, string, string, string)>
    {
        private const string SpecialSplitString = " ##___!!_:_:_!!___## ";

        public PreferenceItemListString4(IPrefs prefs, string key, List<(string, string, string, string)> defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(prefs, key, defaultValue, syncEnabled, encrypt)
        {
        }

        public PreferenceItemListString4(PrefsScope scope, string key, List<(string, string, string, string)> defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(scope, key, defaultValue, syncEnabled, encrypt)
        {
        }
#if UNITY_EDITOR
        /// <summary>
        /// EDITOR ONLY
        /// </summary>
        public PreferenceItemListString4(string key, List<(string, string, string, string)> defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(key, defaultValue, syncEnabled, encrypt)
        {
        }
#endif
        
        protected override (string, string, string, string) StringToValue(string str)
        {
            var s = str.Split(new[] { SpecialSplitString }, StringSplitOptions.None);
            switch (s.Length)
            {
                default:
                    return (Empty, Empty, Empty, Empty);
                case 1:
                    return (s[0], Empty, Empty, Empty);
                case 2:
                    return (s[0], s[1], Empty, Empty);
                case 3:
                    return (s[0], s[1], s[2], Empty);
                case 4:
                    return (s[0], s[1], s[2], s[3]);
            }
        }

        protected override string ValueToString((string, string, string, string) value)
        {
            return value.Item1 + SpecialSplitString + value.Item2 + SpecialSplitString + value.Item3 + SpecialSplitString + value.Item4;
        }
    }
}