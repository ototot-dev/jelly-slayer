#region License
// // --------------------------------------------------------------------------------------------------------------------
// // <summary>
// //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// //   Prefs.Encryptor.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------
#endregion
#if FINAL_PREFERENCES

using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using FinalFactory.Preferences.Handlers;
using FinalFactory.Preferences.Utilities;
using UnityEngine.Scripting;

namespace FinalFactory.Preferences.Encryption
{
    /// <summary>
    /// Provides methods for encrypting and decrypting preference keys and values.
    /// </summary>
    [Preserve]
    public class PrefsEncryptor : IPrefsEncryptor
    {
        private const string KeyPrefix = "$SEC$-";
        private const string IntPrefix = "$SEI$-";
        private const string FloatPrefix = "$SEF$-";
        private const string BoolPrefix = "$SEB$-";
        private const string StringPrefix = "$SES$-";
        
        
        /// <summary>
        /// Checks if a key is encrypted.
        /// </summary>
        public bool IsEncrypted(string key) => key.StartsWith(KeyPrefix);
        
        /// <summary>
        /// Checks if a value is encrypted.
        /// </summary>
        public bool IsEncryptedValue(string key) => key.StartsWith(IntPrefix) || key.StartsWith(FloatPrefix) || key.StartsWith(BoolPrefix) || key.StartsWith(StringPrefix);
        
        /// <summary>
        /// Encrypts a key and a value, adding a prefix to the key.
        /// </summary>
        public (string, string) Encrypt(PrefsScope scope, string key, object value) => (EncryptKey(scope, key), EncryptValue(scope, value));

        /// <summary>
        /// Encrypts a key, adding a prefix to the key.
        /// </summary>
        public string EncryptKey(PrefsScope scope, string key) => KeyPrefix + EncryptKeyString(key, GetKeyString(scope));

        /// <summary>
        /// Encrypts a value based on its type.
        /// </summary>
        public string EncryptValue(PrefsScope scope, object value)
        {
            if (value == null)
            {
                return null;
            }
            
            var keyString = GetKeyString(scope);
            if (value is int i)
            {
                return IntPrefix + EncryptString(i.ToString(CultureInfo.InvariantCulture), keyString);
            }
            if (value is float f)
            {
                return FloatPrefix + EncryptString(f.ToString(CultureInfo.InvariantCulture), keyString);
            }
            if (value is bool b)
            {
                return BoolPrefix + EncryptString(b.ToString(CultureInfo.InvariantCulture), keyString);
            }
            return StringPrefix + EncryptString(value.ToString(), keyString);
        }
        
        /// <summary>
        /// Decrypts a key or a value, also removing the prefix.
        /// </summary>
        public string DecryptKey(PrefsScope scope, string value)
        {
            // Check for the prefix and remove it
            if (!IsEncrypted(value))
            {
                throw new InvalidOperationException("The key is not encrypted.");
            }
            value = value[KeyPrefix.Length..];
            return DecryptKeyString(value, GetKeyString(scope));
        }   
        
        /// <summary>
        /// Decrypts a value based on its prefix.
        /// </summary>
        public object DecryptValue(PrefsScope scope, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            
            // Check for the prefix and remove it
            
            var keyString = GetKeyString(scope);
            if (value.StartsWith(IntPrefix))
            {
                value = value[IntPrefix.Length..];
                return int.Parse(DecryptString(value, keyString));
            }
            if (value.StartsWith(FloatPrefix))
            {
                value = value[FloatPrefix.Length..];
                return float.Parse(DecryptString(value, keyString));
            }
            if (value.StartsWith(BoolPrefix))
            {
                value = value[BoolPrefix.Length..];
                return bool.Parse(DecryptString(value, keyString));
            }
            if (value.StartsWith(StringPrefix))
            {
                value = value[StringPrefix.Length..];
                return DecryptString(value, keyString);
            }
            
            throw new InvalidOperationException("The value is not encrypted.");
        }
        
        private string GetKeyString(PrefsScope scope)
        {
#if UNITY_EDITOR
            if (scope == PrefsScope.Standalone)
            {
                throw new InvalidOperationException("Standalone scope is not supported. It shares the same key as the player scope.");
            }
#endif
            var prefs = Prefs.Get(PrefsScope.ProjectRuntime) as ProjectRuntimePrefsHandler;
            string master;
            var msc = $"$MSC$-{(int)scope}-DO NOT DELETE OR CHANGE ME!";
            if (!prefs.HasKey(msc))
            {
                master = GenerateKeyString();
                prefs.InternalSetString(msc, master);
                prefs.Save();
            }
            else
            {
                master = prefs.GetString(msc);
            }
            return master;
        }
        
        private string EncryptString(string text, string keyString)
        {
            var key = Convert.FromBase64String(keyString);
            using var aesAlg = new AesManaged();
            aesAlg.Key = key;
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;

            aesAlg.GenerateIV();
            var iv = aesAlg.IV;

            using var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, iv);
            var bytes = Encoding.UTF8.GetBytes(text);
            var encrypted = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
            var combined = new byte[iv.Length + encrypted.Length];
            Array.Copy(iv, 0, combined, 0, iv.Length);
            Array.Copy(encrypted, 0, combined, iv.Length, encrypted.Length);

            return Convert.ToBase64String(combined);
        }

        private string DecryptString(string combinedBase64, string keyString)
        {
            var combined = Convert.FromBase64String(combinedBase64);
            var key = Convert.FromBase64String(keyString);

            using var aesAlg = new AesManaged();
            aesAlg.Key = key;
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;

            var iv = new byte[aesAlg.BlockSize / 8];
            var cipherText = new byte[combined.Length - iv.Length];

            Array.Copy(combined, iv, iv.Length);
            Array.Copy(combined, iv.Length, cipherText, 0, cipherText.Length);
            
            aesAlg.IV = iv;

            using var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            var decrypted = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
            return Encoding.UTF8.GetString(decrypted);
        }

        
        private string EncryptKeyString(string text, string key)
        {
            using var aesAlg = Aes.Create();
            aesAlg.Key = Convert.FromBase64String(key);
            aesAlg.IV = new byte[16]; // Zero initialization vector for simplicity

            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using var msEncrypt = new MemoryStream();
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                using (var swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(text);
                }
            }
            return Convert.ToBase64String(msEncrypt.ToArray());
        }

        private string DecryptKeyString(string cipherText, string key)
        {
            using var aesAlg = Aes.Create();
            aesAlg.Key = Convert.FromBase64String(key);
            aesAlg.IV = new byte[16]; // Zero initialization vector for simplicity

            var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (var msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
            {
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
        
        private string GenerateKeyString()
        {
            using var rng = new RNGCryptoServiceProvider();
            var key = new byte[32];
            rng.GetBytes(key);
            return Convert.ToBase64String(key);
        }

        /// <summary>
        /// Pre-generates the player key to prevent loss of encrypted data at runtime.
        /// </summary>
        public void PreGenerateKey()
        {
            //We need to pre-generate the player key, because it could happen that the player saves encrypted data
            //at runtime and would generate a new key that could not be stored in the project prefs.
            //The project prefs are read-only at runtime.
            //So the data would be lost after a restart and could also not be read by the editor.
            GetKeyString(PrefsScope.Player);
        }
    }
}
#endif