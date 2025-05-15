namespace FinalFactory.Preferences.Utilities
{
    public interface IPrefsEncryptor
    {
        /// <summary>
        /// Checks if a key is encrypted.
        /// </summary>
        bool IsEncrypted(string key);

        /// <summary>
        /// Checks if a value is encrypted.
        /// </summary>
        bool IsEncryptedValue(string key);

        /// <summary>
        /// Encrypts a key and a value, adding a prefix to the key.
        /// </summary>
        (string, string) Encrypt(PrefsScope scope, string key, object value);

        /// <summary>
        /// Encrypts a key, adding a prefix to the key.
        /// </summary>
        string EncryptKey(PrefsScope scope, string key);

        /// <summary>
        /// Encrypts a value based on its type.
        /// </summary>
        string EncryptValue(PrefsScope scope, object value);

        /// <summary>
        /// Decrypts a key or a value, also removing the prefix.
        /// </summary>
        string DecryptKey(PrefsScope scope, string value);

        /// <summary>
        /// Decrypts a value based on its prefix.
        /// </summary>
        object DecryptValue(PrefsScope scope, string value);

        /// <summary>
        /// Pre-generates the player key to prevent loss of encrypted data at runtime.
        /// </summary>
        void PreGenerateKey();
    }
}