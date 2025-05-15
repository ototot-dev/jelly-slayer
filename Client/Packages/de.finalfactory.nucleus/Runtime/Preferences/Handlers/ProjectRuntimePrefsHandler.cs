#region License

// // --------------------------------------------------------------------------------------------------------------------
// // <summary>
// //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// //   ProjectRuntimePrefsHandler.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

#endregion

using System.Runtime.CompilerServices;

namespace FinalFactory.Preferences.Handlers
{
    internal class ProjectRuntimePrefsHandler : PrefsHandlerBase
    {
        public override bool CanSave => true;
        public override bool CanLoad => true;
        public override PrefsScope Scope => PrefsScope.ProjectRuntime;

        [MethodImpl(GlobalConst.ImplOptions)]
        internal override void InternalSetString(string key, string value) => ProjectRuntimePrefs.SetString(key, value);

        [MethodImpl(GlobalConst.ImplOptions)]
        public override string GetString(string key, string defaultValue = default) => ProjectRuntimePrefs.GetString(key, defaultValue);

        [MethodImpl(GlobalConst.ImplOptions)]
        internal override void InternalSetInt(string key, int value) => ProjectRuntimePrefs.SetInt(key, value);

        [MethodImpl(GlobalConst.ImplOptions)]
        public override int GetInt(string key, int defaultValue = default) => ProjectRuntimePrefs.GetInt(key, defaultValue);

        [MethodImpl(GlobalConst.ImplOptions)]
        internal override void InternalSetFloat(string key, float value) => ProjectRuntimePrefs.SetFloat(key, value);

        [MethodImpl(GlobalConst.ImplOptions)]
        public override float GetFloat(string key, float defaultValue = default) => ProjectRuntimePrefs.GetFloat(key, defaultValue);

        [MethodImpl(GlobalConst.ImplOptions)]
        internal override void InternalSetBool(string key, bool value) => ProjectRuntimePrefs.SetBool(key, value);

        [MethodImpl(GlobalConst.ImplOptions)]
        public override bool GetBool(string key, bool defaultValue = default) => ProjectRuntimePrefs.GetBool(key, defaultValue);

        [MethodImpl(GlobalConst.ImplOptions)]
        public override object GetObject(string key) => ProjectRuntimePrefs.GetObject(key);

        [MethodImpl(GlobalConst.ImplOptions)]
        public override bool HasKey(string key) => ProjectRuntimePrefs.HasKey(key);

        [MethodImpl(GlobalConst.ImplOptions)]
        public override void DeleteKey(string key) => ProjectRuntimePrefs.DeleteKey(key);

        [MethodImpl(GlobalConst.ImplOptions)]
        public override void DeleteAll() => ProjectRuntimePrefs.DeleteAll();

        [MethodImpl(GlobalConst.ImplOptions)]
        public override void Load() => ProjectRuntimePrefs.Load();

        [MethodImpl(GlobalConst.ImplOptions)]
        public override void Save() => ProjectRuntimePrefs.Save();
        public override string[] Keys => ProjectRuntimePrefs.GetAllKeys();
    }
}