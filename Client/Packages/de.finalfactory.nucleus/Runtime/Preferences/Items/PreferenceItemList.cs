#region License

// // --------------------------------------------------------------------------------------------------------------------
// // <summary>
// //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// //   PreferenceItemList.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace FinalFactory.Preferences.Items
{
    public abstract class PreferenceItemList<T> : PreferenceItem<List<T>>
    {
        private const string SpecialSplitString = " :_: ";
        

        protected PreferenceItemList(IPrefs prefs, string key, List<T> defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(prefs, key, defaultValue, syncEnabled, encrypt)
        {
            
        }

        protected PreferenceItemList(PrefsScope scope, string key, List<T> defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(scope, key, defaultValue, syncEnabled, encrypt)
        {
            
        }
#if UNITY_EDITOR
        /// <summary>
        /// EDITOR ONLY
        /// </summary>
        protected PreferenceItemList(string key, List<T> defaultValue = default, bool syncEnabled = true, bool encrypt = false) : base(key, defaultValue, syncEnabled, encrypt)
        {
            
        }
#endif

        protected override List<T> ReadFromStorage()
        {
            string str;
            
#if FINAL_PREFERENCES
            if (IsEncrypted)
            {
                str = PrefsHandler.GetEncryptedString(Key, ListToString(Value));
            }
            else
#endif
            {
                str = PrefsHandler.GetString(Key, ListToString(Value));
            }
            return StringToList(str);
        }

        protected override void WriteToStorage()
        {
#if FINAL_PREFERENCES
            if (IsEncrypted)
            {
                PrefsHandler.SetEncryptedString(Key, ListToString(Value));
                return;
            }
#endif
            PrefsHandler.SetString(Key, ListToString(Value));
        }

        protected abstract T StringToValue(string str);
        protected abstract string ValueToString(T value);

        private string ListToString(List<T> list)
        {
            if (list == null || list.Count == 0) return string.Empty;

            return string.Join(SpecialSplitString, list.Select(ValueToString));
        }

        private List<T> StringToList(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return new List<T>();

            return str.Split(new[] { SpecialSplitString }, StringSplitOptions.None).Select(StringToValue).ToList();
        }
    }
}