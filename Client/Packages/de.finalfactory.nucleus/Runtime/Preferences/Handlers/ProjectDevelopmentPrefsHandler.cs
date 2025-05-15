#region License

// // --------------------------------------------------------------------------------------------------------------------
// // <summary>
// //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// //   ProjectDevelopmentPrefsHandler.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

#endregion

#if UNITY_EDITOR || DEVELOPMENT_BUILD

namespace FinalFactory.Preferences.Handlers
{
    internal class ProjectDevelopmentPrefsHandler : PrefsHandlerBase
    {
        public override bool CanSave => true;
        public override bool CanLoad => true;
        public override PrefsScope Scope => PrefsScope.ProjectDevelopment;

        internal override void InternalSetString(string key, string value) => ProjectDevelopmentPrefs.SetString(key, value);

        public override string GetString(string key, string defaultValue = default) => ProjectDevelopmentPrefs.GetString(key, defaultValue);

        internal override void InternalSetInt(string key, int value) => ProjectDevelopmentPrefs.SetInt(key, value);

        public override int GetInt(string key, int defaultValue = default) => ProjectDevelopmentPrefs.GetInt(key, defaultValue);

        internal override void InternalSetFloat(string key, float value) => ProjectDevelopmentPrefs.SetFloat(key, value);

        public override float GetFloat(string key, float defaultValue = default) => ProjectDevelopmentPrefs.GetFloat(key, defaultValue);

        internal override void InternalSetBool(string key, bool value) => ProjectDevelopmentPrefs.SetBool(key, value);

        public override bool GetBool(string key, bool defaultValue = default) => ProjectDevelopmentPrefs.GetBool(key, defaultValue);

        public override object GetObject(string key) => ProjectDevelopmentPrefs.GetObject(key);

        public override bool HasKey(string key) => ProjectDevelopmentPrefs.HasKey(key);

        public override void DeleteKey(string key) => ProjectDevelopmentPrefs.DeleteKey(key);

        public override void DeleteAll() => ProjectDevelopmentPrefs.DeleteAll();

        public override void Load() => ProjectDevelopmentPrefs.Load();

        public override void Save() => ProjectDevelopmentPrefs.Save();
        public override string[] Keys => ProjectDevelopmentPrefs.GetAllKeys();
    }
}

#endif