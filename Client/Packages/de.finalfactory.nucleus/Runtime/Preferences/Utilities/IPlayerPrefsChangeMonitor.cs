using System.Diagnostics;
using FinalFactory.Preferences.Items;

namespace FinalFactory.Preferences.Utilities
{
    public interface IPlayerPrefsChangeMonitor
    {
        void Watch(string key, bool isEncrypted);
        void UnWatch(string key, bool isEncrypted);
        void Watch(IPreferenceItem item);
        void UnWatch(IPreferenceItem item);
    }
}