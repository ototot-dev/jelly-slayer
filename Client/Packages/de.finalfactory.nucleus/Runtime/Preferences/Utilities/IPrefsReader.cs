#region License

// // --------------------------------------------------------------------------------------------------------------------
// // <summary>
// //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// //   IPrefsReader.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

#endregion

namespace FinalFactory.Preferences.Utilities
{
    internal interface IPrefsReader
    {
        string[] RetrieveSavedPrefs(PrefsScope scope);
        
        bool HasKey(PrefsScope scope, string key);
        
        void SetValue(PrefsScope scope, string key, object value);
        
        object GetValue(PrefsScope scope, string key);
        
        void DeleteKey(PrefsScope scope, string key);
        
        void DeleteAll(PrefsScope scope);
    }
}