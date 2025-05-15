#if UNITY_EDITOR

using System;
using System.Runtime.CompilerServices;
using FinalFactory.Mathematics;

namespace FinalFactory.Preferences.Handlers
{
    internal class StandalonePlayerPrefsHandler : PrefsHandlerBase
    {
        public override bool CanSave => false;
        public override bool CanLoad => false;
        public override PrefsScope Scope => PrefsScope.Standalone;

        [MethodImpl(GlobalConst.ImplOptions)]
        internal override void InternalSetString(string key, string value) => Prefs.Reader.SetValue(Scope, key, value);

        [MethodImpl(GlobalConst.ImplOptions)]
        public override string GetString(string key, string defaultValue = default) => Convert.ToString(Prefs.Reader.GetValue(Scope, key) ?? defaultValue);

        [MethodImpl(GlobalConst.ImplOptions)]
        internal override void InternalSetInt(string key, int value) => Prefs.Reader.SetValue(Scope, key, value);

        [MethodImpl(GlobalConst.ImplOptions)]
        public override int GetInt(string key, int defaultValue = default) => Convert.ToInt32(Prefs.Reader.GetValue(Scope, key) ?? defaultValue);

        [MethodImpl(GlobalConst.ImplOptions)]
        internal override void InternalSetFloat(string key, float value) => Prefs.Reader.SetValue(Scope, key, value);

        [MethodImpl(GlobalConst.ImplOptions)]
        public override float GetFloat(string key, float defaultValue = default) => Convert.ToSingle(Prefs.Reader.GetValue(Scope, key) ?? defaultValue);

        [MethodImpl(GlobalConst.ImplOptions)]
        internal override void InternalSetBool(string key, bool value) => Prefs.Reader.SetValue(Scope, key, value.ToInt());

        [MethodImpl(GlobalConst.ImplOptions)]
        public override bool GetBool(string key, bool defaultValue = default) => Convert.ToBoolean(Prefs.Reader.GetValue(Scope, key) ?? defaultValue);

        public override object GetObject(string key)
        {
            var str = GetString(key, "$#$#$null$#$#$");
            if (str == "$#$#$null$#$#$")
            {
                var f = GetFloat(key, float.NaN);
                if (float.IsNaN(f))
                {
                    return GetInt(key);
                }

                return f;
            }
            
            return str;
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public override bool HasKey(string key) => Prefs.Reader.HasKey(Scope, key);

        [MethodImpl(GlobalConst.ImplOptions)]
        public override void DeleteKey(string key) => Prefs.Reader.DeleteKey(Scope, key);

        [MethodImpl(GlobalConst.ImplOptions)]
        public override void DeleteAll() => Prefs.Reader.DeleteAll(Scope);
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public override void Load() => throw new NotSupportedException("Load is not supported for standalone player prefs. Changes are directly saved to disk.");

        [MethodImpl(GlobalConst.ImplOptions)]
        public override void Save() => throw new NotSupportedException("Save is not supported for standalone player prefs. Changes are directly saved to disk.");
        
        public override string[] Keys => Prefs.Reader.RetrieveSavedPrefs(Scope);
    }
}

#endif