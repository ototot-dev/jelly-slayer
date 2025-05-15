#region License

// // --------------------------------------------------------------------------------------------------------------------
// // <summary>
// //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// //   PrefsReaderDummy.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

#endregion

using System;
using FinalFactory.Logging;

namespace FinalFactory.Preferences.Utilities
{
    internal class PrefsReaderDummy : IPrefsReader
    {
        private static readonly Log Log = LogManager.GetLogger(typeof(PrefsReaderDummy));

        public string[] RetrieveSavedPrefs(PrefsScope scope)
        {
            Log.Warn("Retrieving saved prefs is not supported on this platform due to .NET Standard or you are missing the package https://u3d.as/3hny");
            return Array.Empty<string>();
        }

        public bool HasKey(PrefsScope scope, string key)
        {
            Log.Warn("Checking key is not supported on this platform due to .NET Standard or you are missing the package https://u3d.as/3hny");
            return false;
        }

        public void SetValue(PrefsScope scope, string key, object value)
        {
            Log.Warn("Setting value is not supported on this platform due to .NET Standard or you are missing the package https://u3d.as/3hny");
        }

        public object GetValue(PrefsScope scope, string key)
        {
            Log.Warn("Getting value is not supported on this platform due to .NET Standard or you are missing the package https://u3d.as/3hny");
            return null;
        }

        public void DeleteKey(PrefsScope scope, string key)
        {
            Log.Warn("Deleting key is not supported on this platform due to .NET Standard or you are missing the package https://u3d.as/3hny");
        }

        public void DeleteAll(PrefsScope scope)
        {
            Log.Warn("Deleting all is not supported on this platform due to .NET Standard or you are missing the package https://u3d.as/3hny");
        }
    }
}