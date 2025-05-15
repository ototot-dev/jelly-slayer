#region License

// // --------------------------------------------------------------------------------------------------------------------
// // <summary>
// //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// //   PrefsHandlerBase.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

#endregion

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using FinalFactory.Logging;

namespace FinalFactory.Preferences.Handlers
{
    [DebuggerStepThrough]
    internal abstract class PrefsHandlerBase : IPrefs
    {
        private static readonly Log Log = LogManager.GetLogger(typeof(PrefsHandlerBase));
        public event PrefsChangedHandler Changed;
        public abstract bool CanSave { get; }
        public abstract bool CanLoad { get; }
        public abstract PrefsScope Scope { get; }
        public abstract string[] Keys { get; }
        internal abstract void InternalSetString(string key, string value);
        internal abstract void InternalSetInt(string key, int value);
        internal abstract void InternalSetFloat(string key, float value);
        internal abstract void InternalSetBool(string key, bool value);

        [MethodImpl(GlobalConst.ImplOptions)]
        public void SetString(string key, string value)
        {
            InternalSetString(key, value);
            OnChanged(new ChangePrefsEventArgs(key, value, false));
        }
        public abstract string GetString(string key, string defaultValue = default);

        [MethodImpl(GlobalConst.ImplOptions)]
        public void SetInt(string key, int value)
        {
            InternalSetInt(key, value);
            OnChanged(new ChangePrefsEventArgs(key, value, false));
        }
        public abstract int GetInt(string key, int defaultValue = default);

        [MethodImpl(GlobalConst.ImplOptions)]
        public void SetFloat(string key, float value)
        {
            InternalSetFloat(key, value);
            OnChanged(new ChangePrefsEventArgs(key, value, false));
        }
        public abstract float GetFloat(string key, float defaultValue = default);

        [MethodImpl(GlobalConst.ImplOptions)]
        public void SetBool(string key, bool value)
        {
            InternalSetBool(key, value);
            OnChanged(new ChangePrefsEventArgs(key, value, false));
        }
        public abstract bool GetBool(string key, bool defaultValue = default);


        [MethodImpl(GlobalConst.ImplOptions)]
        public void SetObject(string key, object value)
        {
            switch (value)
            {
                case int i:
                    SetInt(key, i);
                    break;
                case float f:
                    SetFloat(key, f);
                    break;
                case bool b:
                    SetBool(key, b);
                    break;
                case string s:
                    SetString(key, s);
                    break;
                default:
                    throw new NotSupportedException(
                        $"Type {value.GetType()} is not supported. Only int, float, bool and string are supported. You can use JsonUtility to serialize complex objects.");
            }
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public object GetObject(bool encrypted, string key)
        {
            return encrypted ? GetEncryptedObject(key) : GetObject(key);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public bool HasKey(bool encrypted, string key)
        {
            return encrypted ? HasEncryptedKey(key) : HasKey(key);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public void DeleteKey(bool encrypted, string key)
        {
            if (encrypted)
                DeleteEncryptedKey(key);
            else
                DeleteKey(key);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public virtual void SetEncryptedString(string key, string value)
        {
#if FINAL_PREFERENCES
            CheckEncryptedKey(key);
            (key, value) = Prefs.Encryptor.Encrypt(Scope, key, value);
            InternalSetString(key, value);
            OnChanged(new ChangePrefsEventArgs(key, value, true));
#else
            throw new NotSupportedException("Encryption is not supported in this version. https://finalfactory.de/finalpreferences");
#endif
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public virtual string GetEncryptedString(string key, string defaultValue = default)
        {
#if FINAL_PREFERENCES
            var encryptedKey = Prefs.Encryptor.EncryptKey(Scope, key);
            if (HasKey(encryptedKey))
            {
                var encryptedValue = GetString(encryptedKey);
                if (Prefs.Encryptor.IsEncryptedValue(encryptedValue))
                    return Prefs.Encryptor.DecryptValue(Scope, encryptedValue) as string;
            }
            return defaultValue;
#else
            throw new NotSupportedException("Encryption is not supported in this version. https://finalfactory.de/finalpreferences");
#endif
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public virtual void SetEncryptedInt(string key, int value)
        {
#if FINAL_PREFERENCES
            CheckEncryptedKey(key);
            var (encryptedKey, encryptedValue) = Prefs.Encryptor.Encrypt(Scope, key, value);
            InternalSetString(encryptedKey, encryptedValue);
            OnChanged(new ChangePrefsEventArgs(key, value, true));
#else
            throw new NotSupportedException("Encryption is not supported in this version. https://finalfactory.de/finalpreferences");
#endif
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public virtual int GetEncryptedInt(string key, int defaultValue = default)
        {
#if FINAL_PREFERENCES
            var encryptedKey = Prefs.Encryptor.EncryptKey(Scope, key);
            if (HasKey(encryptedKey))
            {
                var encryptedValue = GetString(encryptedKey);
                if (Prefs.Encryptor.IsEncryptedValue(encryptedValue))
                {
                    var decryptValue = Prefs.Encryptor.DecryptValue(Scope, encryptedValue);
                    return decryptValue is int value ? value : defaultValue;
                }
            }
            return defaultValue;
            
#else
            throw new NotSupportedException("Encryption is not supported in this version. https://finalfactory.de/finalpreferences");
#endif
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public virtual void SetEncryptedFloat(string key, float value)
        {
#if FINAL_PREFERENCES
            CheckEncryptedKey(key);
            var (encryptedKey, encryptedValue) = Prefs.Encryptor.Encrypt(Scope, key, value);
            InternalSetString(encryptedKey, encryptedValue);
            OnChanged(new ChangePrefsEventArgs(key, value, true));
            
#else
            throw new NotSupportedException("Encryption is not supported in this version. https://finalfactory.de/finalpreferences");
#endif
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public virtual float GetEncryptedFloat(string key, float defaultValue = default)
        {
#if FINAL_PREFERENCES
            var encryptedKey = Prefs.Encryptor.EncryptKey(Scope, key);
            if (HasKey(encryptedKey))
            {
                var encryptedValue = GetString(encryptedKey);
                if (Prefs.Encryptor.IsEncryptedValue(encryptedValue))
                {
                    var decryptValue = Prefs.Encryptor.DecryptValue(Scope, encryptedValue);
                    return decryptValue is float value ? value : defaultValue;
                }
            }
            return defaultValue;
            
#else
            throw new NotSupportedException("Encryption is not supported in this version. https://finalfactory.de/finalpreferences");
#endif
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public virtual void SetEncryptedBool(string key, bool value)
        {
#if FINAL_PREFERENCES
            CheckEncryptedKey(key);
            var (encryptedKey, encryptedValue) = Prefs.Encryptor.Encrypt(Scope, key, value);
            InternalSetString(encryptedKey, encryptedValue);
            OnChanged(new ChangePrefsEventArgs(key, value, true));
            
#else
            throw new NotSupportedException("Encryption is not supported in this version. https://finalfactory.de/finalpreferences");
#endif
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public virtual bool GetEncryptedBool(string key, bool defaultValue = default)
        {
#if FINAL_PREFERENCES
            var encryptedKey = Prefs.Encryptor.EncryptKey(Scope, key);
            if (HasKey(encryptedKey))
            {
                var encryptedValue = GetString(encryptedKey);
                if (Prefs.Encryptor.IsEncryptedValue(encryptedValue))
                {
                    var decryptValue = Prefs.Encryptor.DecryptValue(Scope, encryptedValue);
                    return decryptValue is bool value ? value : defaultValue;
                }
            }
            return defaultValue;
            
#else
            throw new NotSupportedException("Encryption is not supported in this version. https://finalfactory.de/finalpreferences");
#endif
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public void SetEncryptedObject(string key, object value)
        {
            switch (value)
            {
                case int i:
                    SetEncryptedInt(key, i);
                    break;
                case float f:
                    SetEncryptedFloat(key, f);
                    break;
                case bool b:
                    SetEncryptedBool(key, b);
                    break;
                case string s:
                    SetEncryptedString(key, s);
                    break;
                default:
                    throw new NotSupportedException(
                        $"Type {value.GetType()} is not supported. Only int, float, bool and string are supported. You can use JsonUtility to serialize complex objects.");
            }
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public virtual object GetEncryptedObject(string key)
        {
#if FINAL_PREFERENCES
            var encryptedKey = Prefs.Encryptor.EncryptKey(Scope, key);
            if (HasKey(encryptedKey))
            {
                var encryptedValue = GetString(encryptedKey);
                if (Prefs.Encryptor.IsEncryptedValue(encryptedValue))
                    return Prefs.Encryptor.DecryptValue(Scope, encryptedValue);
            }
            return null;
            
#else
            throw new NotSupportedException("Encryption is not supported in this version. https://finalfactory.de/finalpreferences");
#endif
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public bool HasEncryptedKey(string key)
        {
#if FINAL_PREFERENCES
            var encryptedKey = Prefs.Encryptor.EncryptKey(Scope, key);
            return HasKey(encryptedKey);
            
#else
            throw new NotSupportedException("Encryption is not supported in this version. https://finalfactory.de/finalpreferences");
#endif
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public void DeleteEncryptedKey(string key)
        {
#if FINAL_PREFERENCES
            var encryptedKey = Prefs.Encryptor.EncryptKey(Scope, key);
            DeleteKey(encryptedKey);
            
#else
            throw new NotSupportedException("Encryption is not supported in this version. https://finalfactory.de/finalpreferences");
#endif
        }
        
        public void ChangeEncryptionState(string key, bool encrypted)
        {
            var isEncrypted = HasEncryptedKey(key);
            var isNotEncrypted = HasKey(key);
            var exists = isEncrypted || isNotEncrypted;
            if (!exists)
            {
                throw new InvalidOperationException($"The key '{key}' does not exist.");
            }
            
            if (encrypted && isEncrypted || !encrypted && isNotEncrypted)
            {
                return;
            }

            if (encrypted)
            {
                var value = GetObject(key);
                DeleteKey(key);
                SetEncryptedObject(key, value);
            }
            else
            {
                var value = GetEncryptedObject(key);
                DeleteEncryptedKey(key);
                SetObject(key, value);
            }
        }

        public abstract object GetObject(string key);

        [MethodImpl(GlobalConst.ImplOptions)]
        public void SetString(bool encrypted, string key, string value)
        {
            if (encrypted)
                SetEncryptedString(key, value);
            else
                SetString(key, value);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public string GetString(bool encrypted, string key, string defaultValue = default)
        {
            return encrypted ? GetEncryptedString(key, defaultValue) : GetString(key, defaultValue);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public void SetInt(bool encrypted, string key, int value)
        {
            if (encrypted)
                SetEncryptedInt(key, value);
            else
                SetInt(key, value);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public int GetInt(bool encrypted, string key, int defaultValue = default)
        {
            return encrypted ? GetEncryptedInt(key, defaultValue) : GetInt(key, defaultValue);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public void SetFloat(bool encrypted, string key, float value)
        {
            if (encrypted)
                SetEncryptedFloat(key, value);
            else
                SetFloat(key, value);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public float GetFloat(bool encrypted, string key, float defaultValue = default)
        {
            return encrypted ? GetEncryptedFloat(key, defaultValue) : GetFloat(key, defaultValue);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public void SetBool(bool encrypted, string key, bool value)
        {
            if (encrypted)
                SetEncryptedBool(key, value);
            else
                SetBool(key, value);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public bool GetBool(bool encrypted, string key, bool defaultValue = default)
        {
            return encrypted ? GetEncryptedBool(key, defaultValue) : GetBool(key, defaultValue);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public void SetObject(bool encrypted, string key, object value)
        {
            if (encrypted)
                SetEncryptedObject(key, value);
            else
                SetObject(key, value);
        }

        public abstract bool HasKey(string key);
        public abstract void DeleteKey(string key);
        public abstract void DeleteAll();
        public abstract void Load();
        public abstract void Save();
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public void TrySave()
        {
            if (CanSave)
                Save();
        }

        internal virtual void OnChanged(ChangePrefsEventArgs args)
        {
            Changed?.Invoke(this, args);
        }

        private void CheckEncryptedKey(string key)
        {
            if (HasKey(key))
            {
                throw new InvalidOperationException($"The key '{key}' already exists. It is not allowed to have the same key in encrypted and non-encrypted form.");
            }
        }
    }
}