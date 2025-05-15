#region License

// // --------------------------------------------------------------------------------------------------------------------
// // <summary>
// //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// //   IPrefs.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

#endregion

using FinalFactory.Preferences.Handlers;
using JetBrains.Annotations;

namespace FinalFactory.Preferences
{   
    /// <summary>
    /// Interface for handling preferences.
    /// </summary>
    public interface IPrefs
    {
        /// <summary>
        /// Event triggered when preferences change.
        /// </summary>
        event PrefsChangedHandler Changed;

        /// <summary>
        /// Checks if preferences can be saved.
        /// </summary>
        bool CanSave { get; }

        /// <summary>
        /// Checks if preferences can be loaded.
        /// </summary>
        bool CanLoad { get; }

        /// <summary>
        /// Gets the scope of the preferences.
        /// </summary>
        PrefsScope Scope { get; }

#if UNITY_EDITOR
        /// <summary>
        /// Gets the keys of the preferences in the Unity Editor.
        /// </summary>
        string[] Keys { get; }
#endif
        
        /// <summary>
        /// Sets a string preference.
        /// </summary>
        /// <param name="key">The key of the preference.</param>
        /// <param name="value">The value to set.</param>
        void SetString([NotNull] string key, string value);

        /// <summary>
        /// Gets a string preference.
        /// </summary>
        /// <param name="key">The key of the preference.</param>
        /// <param name="defaultValue">The default value if the key is not found.</param>
        /// <returns>The value of the preference.</returns>
        string GetString([NotNull] string key, string defaultValue = default);

        /// <summary>
        /// Sets an integer preference.
        /// </summary>
        /// <param name="key">The key of the preference.</param>
        /// <param name="value">The value to set.</param>
        void SetInt([NotNull] string key, int value);

        /// <summary>
        /// Gets an integer preference.
        /// </summary>
        /// <param name="key">The key of the preference.</param>
        /// <param name="defaultValue">The default value if the key is not found.</param>
        /// <returns>The value of the preference.</returns>
        int GetInt([NotNull] string key, int defaultValue = default);

        /// <summary>
        /// Sets a float preference.
        /// </summary>
        /// <param name="key">The key of the preference.</param>
        /// <param name="value">The value to set.</param>
        void SetFloat([NotNull] string key, float value);

        /// <summary>
        /// Gets a float preference.
        /// </summary>
        /// <param name="key">The key of the preference.</param>
        /// <param name="defaultValue">The default value if the key is not found.</param>
        /// <returns>The value of the preference.</returns>
        float GetFloat([NotNull] string key, float defaultValue = default);

        /// <summary>
        /// Sets a boolean preference.
        /// </summary>
        /// <param name="key">The key of the preference.</param>
        /// <param name="value">The value to set.</param>
        void SetBool([NotNull] string key, bool value);

        /// <summary>
        /// Gets a boolean preference.
        /// </summary>
        bool GetBool([NotNull] string key, bool defaultValue = default);
        
        /// <summary>
        /// Sets an object preference.
        /// </summary>
        /// <param name="key">The key of the preference.</param>
        /// <param name="value">The value to set.</param>
        void SetObject([NotNull] string key, object value);
        
        /// <summary>
        /// Gets an object preference.
        /// </summary>
        object GetObject([NotNull] string key);

        /// <summary>
        /// Sets a string preference.
        /// </summary>
        /// <param name="encrypted">If the preference is encrypted.</param>
        /// <param name="key">The key of the preference.</param>
        /// <param name="value">The value to set.</param>
        void SetString(bool encrypted, [NotNull] string key, string value);

        /// <summary>
        /// Gets a string preference.
        /// </summary>
        /// <param name="encrypted">If the preference is encrypted.</param>
        /// <param name="key">The key of the preference.</param>
        /// <param name="defaultValue">The default value if the key is not found.</param>
        /// <returns>The value of the preference.</returns>
        string GetString(bool encrypted, [NotNull] string key, string defaultValue = default);

        /// <summary>
        /// Sets an integer preference.
        /// </summary>
        /// <param name="encrypted">If the preference is encrypted.</param>
        /// <param name="key">The key of the preference.</param>
        /// <param name="value">The value to set.</param>
        void SetInt(bool encrypted, [NotNull] string key, int value);

        /// <summary>
        /// Gets an integer preference.
        /// </summary>
        /// <param name="encrypted">If the preference is encrypted.</param>
        /// <param name="key">The key of the preference.</param>
        /// <param name="defaultValue">The default value if the key is not found.</param>
        /// <returns>The value of the preference.</returns>
        int GetInt(bool encrypted, [NotNull] string key, int defaultValue = default);

        /// <summary>
        /// Sets a float preference.
        /// </summary>
        /// <param name="encrypted">If the preference is encrypted.</param>
        /// <param name="key">The key of the preference.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>The value of the preference.</returns>
        void SetFloat(bool encrypted, [NotNull] string key, float value);
        
        /// <summary>
        /// Gets a float preference.
        /// </summary>
        /// <param name="encrypted">If the preference is encrypted.</param>
        /// <param name="key">The key of the preference.</param>
        /// <param name="defaultValue">The default value if the key is not found.</param>
        /// <returns>The value of the preference.</returns>
        float GetFloat(bool encrypted, [NotNull] string key, float defaultValue = default);

        /// <summary>
        /// Sets a boolean preference.
        /// </summary>
        /// <param name="encrypted">If the preference is encrypted.</param>
        /// <param name="key">The key of the preference.</param>
        /// <param name="value">The value to set.</param>
        void SetBool(bool encrypted, [NotNull] string key, bool value);

        /// <summary>
        /// Gets a boolean preference.
        /// </summary>
        bool GetBool(bool encrypted, [NotNull] string key, bool defaultValue = default);

        /// <summary>
        /// Sets an object preference.
        /// </summary>
        /// <param name="encrypted">If the preference is encrypted.</param>
        /// <param name="key">The key of the preference.</param>
        /// <param name="value">The value to set.</param>
        void SetObject(bool encrypted, [NotNull] string key, object value);

        /// <summary>
        /// Gets an object preference.
        /// </summary>
        object GetObject(bool encrypted, [NotNull] string key);
        
        /// <summary>
        /// Checks if a preference key exists. Encrypted specifies if the check should be done for the encrypted version or not. Will not check both at the same time.
        /// </summary>
        /// <param name="encrypted">If the preference is encrypted.</param>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key exists.</returns>
        bool HasKey(bool encrypted, [NotNull] string key);

        /// <summary>
        /// Deletes a preference key.
        /// </summary>
        /// <param name="encrypted">If the preference is encrypted.</param>
        /// <param name="key">The key to delete.</param>
        void DeleteKey(bool encrypted, [NotNull] string key);

        /// <summary>
        /// Sets an encrypted string preference.
        /// </summary>
        /// <param name="key">The key of the preference.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>The value of the preference.</returns>
        void SetEncryptedString([NotNull] string key, string value);
        
        /// <summary>
        /// Gets an encrypted string preference.
        /// </summary>
        /// <param name="key">The key of the preference.</param>
        /// <param name="defaultValue">The default value if the key is not found.</param>
        /// <returns>The value of the preference.</returns>
        string GetEncryptedString([NotNull] string key, string defaultValue = default);

        /// <summary>
        /// Sets an encrypted integer preference.
        /// </summary>
        /// <param name="key">The key of the preference.</param>
        /// <param name="value">The value to set.</param>
        void SetEncryptedInt([NotNull] string key, int value);

        /// <summary>
        /// Gets an encrypted integer preference.
        /// </summary>
        /// <param name="key">The key of the preference.</param>
        /// <param name="defaultValue">The default value if the key is not found.</param>
        /// <returns>The value of the preference.</returns>
        int GetEncryptedInt([NotNull] string key, int defaultValue = default);

        /// <summary>
        /// Sets an encrypted float preference.
        /// </summary>
        /// <param name="key">The key of the preference.</param>
        /// <param name="value">The value to set.</param>
        void SetEncryptedFloat([NotNull] string key, float value);

        /// <summary>
        /// Gets an encrypted float preference.
        /// </summary>
        /// <param name="key">The key of the preference.</param>
        /// <param name="defaultValue">The default value if the key is not found.</param>
        /// <returns>The value of the preference.</returns>
        float GetEncryptedFloat([NotNull] string key, float defaultValue = default);

        /// <summary>
        /// Sets an encrypted boolean preference.
        /// </summary>
        /// <param name="key">The key of the preference.</param>
        /// <param name="value">The value to set.</param>
        void SetEncryptedBool([NotNull] string key, bool value);

        /// <summary>
        /// Gets an encrypted boolean preference.
        /// </summary>
        /// <param name="key">The key of the preference.</param>
        /// <param name="defaultValue">The default value if the key is not found.</param>
        /// <returns>The value of the preference.</returns>
        bool GetEncryptedBool([NotNull] string key, bool defaultValue = default);
        
        /// <summary>
        /// Sets an encrypted object preference.
        /// </summary>
        /// <param name="key">The key of the preference.</param>
        /// <param name="value">The value to set.</param>
        void SetEncryptedObject([NotNull] string key, object value);
        
        /// <summary>
        /// Gets an encrypted object preference.
        /// </summary>
        object GetEncryptedObject([NotNull] string key);
        
        /// <summary>
        /// Checks if an encrypted preference key exists. Will not check for clear text keys.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key exists.</returns>
        bool HasEncryptedKey([NotNull] string key);

        /// <summary>
        /// Changes the encryption state of a key.
        /// </summary>
        /// <param name="key">The key to change.</param>
        /// <param name="encrypted">If the key should be encrypted.</param>
        void ChangeEncryptionState(string key, bool encrypted);
        
        /// <summary>
        /// Deletes an encrypted preference key.
        /// </summary>
        /// <param name="key">The key to delete.</param>
        void DeleteEncryptedKey([NotNull] string key);

        /// <summary>
        /// Checks if a preference key exists. Will not check if the key exists as encrypted version.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key exists.</returns>
        bool HasKey([NotNull] string key);

        /// <summary>
        /// Deletes a preference key.
        /// </summary>
        /// <param name="key">The key to delete.</param>
        void DeleteKey([NotNull] string key);

        /// <summary>
        /// Deletes all preferences.
        /// </summary>
        void DeleteAll();

        /// <summary>
        /// Loads the preferences.
        /// </summary>
        void Load();

        /// <summary>
        /// Saves the preferences.
        /// </summary>
        void Save();

        /// <summary>
        /// Saves the preferences if possible.
        /// </summary>
        void TrySave();
    }
}