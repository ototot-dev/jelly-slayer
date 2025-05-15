using System.Diagnostics;
using FinalFactory.Preferences.Items;

namespace FinalFactory.Preferences.Utilities
{
    public static class PlayerPrefsChangeMonitor
    {
        internal static IPlayerPrefsChangeMonitor Monitor;
        
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("FF_PREFERENCES_RUNTIME_MONITOR")]
        public static void Watch(string key, bool isEncrypted) => Monitor?.Watch(key, isEncrypted);

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("FF_PREFERENCES_RUNTIME_MONITOR")]
        public static void UnWatch(string key, bool isEncrypted) => Monitor?.UnWatch(key, isEncrypted);
        
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("FF_PREFERENCES_RUNTIME_MONITOR")]
        public static void Watch(IPreferenceItem item) => Monitor?.Watch(item);
        
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("FF_PREFERENCES_RUNTIME_MONITOR")]
        public static void UnWatch(IPreferenceItem item) => Monitor?.UnWatch(item);
    }
}