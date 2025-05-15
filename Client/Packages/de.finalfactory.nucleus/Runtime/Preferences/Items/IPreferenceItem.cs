namespace FinalFactory.Preferences.Items
{
    public interface IPreferenceItem
    {
        /// <summary>
        /// The key of the preference.
        /// </summary>
        string Key { get; } 
        
        /// <summary>
        /// The scope of the preference.
        /// </summary>
        PrefsScope Scope { get; }
        
        /// <summary>
        /// Returns the value is encrypted.
        /// </summary>
        bool IsEncrypted { get; }
        
        /// <summary>
        /// Checks if a value exists in the preferences with the given key.
        /// </summary>
        bool Exists { get; }
        
        /// <summary>
        /// Setting this to true will enable syncing with the preferences handler.
        /// This means that the value will be saved immediately when it changes.
        /// You won't need to call Save() manually.
        /// But you can not discard changes.
        /// </summary>
        bool SyncEnabled { get; }
        
        /// <summary>
        /// The value of the preference.
        /// </summary>
        object Value { get; }
        
        /// <summary>
        /// Returns true if the value has changed. Does not work if sync is enabled.
        /// </summary>
        bool HasChanges { get; }
        
        /// <summary>
        /// Save the value to the preferences.
        /// </summary>
        void Save();
        
        /// <summary>
        /// Discard any changes.
        /// Only works if sync is disabled.
        /// If Sync is enabled, it will immediately save the value, so it will not be possible to discard the changes.
        /// </summary>
        void Discard();
        
        /// <summary>
        /// Resets the value to the default value.
        /// </summary>
        void Reset();
        
        /// <summary>
        /// Delete the entry from the preferences.
        /// </summary>
        void Delete();
        
        /// <summary>
        /// Refire the changed event.
        /// </summary>
        void Refire();
    }
}