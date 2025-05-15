#region License

// // --------------------------------------------------------------------------------------------------------------------
// // <summary>
// //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// //   PreferenceItem.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

#endregion

using System;
using System.Diagnostics;
using FinalFactory.Observable;
using FinalFactory.Preferences.Utilities;
using JetBrains.Annotations;

namespace FinalFactory.Preferences.Items
{
    [PublicAPI]
    public abstract class PreferenceItem<T> : P<T>, IPreferenceItem
    {
        public readonly T DefaultValue;
        public readonly string Key;

        protected readonly IPrefs PrefsHandler;

        private bool _initialized;
        private bool _isEncrypted;
        private bool _syncEnabled;

        protected PreferenceItem(IPrefs prefs, string key, T defaultValue = default, bool syncEnabled = true, bool encrypt = false)
        {
            PrefsHandler = prefs;
            Key = key;
            DefaultValue = defaultValue;
            SyncEnabled = syncEnabled;
            _isEncrypted = encrypt;
#if !FINAL_PREFERENCES
            if (encrypt)
            {
                throw new NotSupportedException("Encryption is not supported in this version. https://finalfactory.de/finalpreferences");
            }
#endif
        }

        protected PreferenceItem(PrefsScope scope, string key, T defaultValue = default, bool syncEnabled = true, bool encrypt = false) : this(Prefs.Get(scope), key, defaultValue, syncEnabled, encrypt)
        {
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// EDITOR ONLY
        /// </summary>
        protected PreferenceItem(string key, T defaultValue = default, bool syncEnabled = true, bool encrypt = false) : this(PrefsScope.Editor, key, defaultValue, syncEnabled, encrypt)
        {
        }
#endif

        /// <summary>
        /// The value of the preference item.
        /// </summary>
        public override T Value
        {
            get
            {
                if (!_initialized)
                {
                    _initialized = true;
                    base.Value = ReadFromStorage();
                }

                return base.Value;
            }
            set
            {
                if (Equals(base.Value, value)) return;
                var old = base.Value;
                base.Value = value;
                _initialized = true;
                HasChanges = true;
                OnChanged(old, base.Value);
                if (SyncEnabled) Save();
            }
        }

        public virtual bool SyncEnabled
        {
            get => _syncEnabled;
            set
            {
                if (_syncEnabled == value) return;
                if (_syncEnabled && !value)
                {
                    PrefsHandler.Changed -= OnPrefsHandlerOnChanged;
                }
                if (!_syncEnabled && value)
                {
                    PrefsHandler.Changed += OnPrefsHandlerOnChanged;
                }
                _syncEnabled = value;
            }
        }

        public bool HasChanges { get; private set; }

        public PrefsScope Scope => PrefsHandler.Scope;
        
        public bool IsEncrypted
#if FINAL_PREFERENCES
        {
            get => _isEncrypted;
            set
            {
                _isEncrypted = value;
                PrefsHandler.ChangeEncryptionState(Key, value);
            }
        }
#else
        => false;
#endif

        public bool Exists => PrefsHandler.HasKey(Key);
        
        protected abstract T ReadFromStorage();

        protected abstract void WriteToStorage();

        /// <summary>
        /// Set the value and save it to the preferences.
        /// </summary>
        /// <param name="value"></param>
        public virtual void Save(T value)
        {
            Value = value;
            Save();
        }

        public virtual void Save()
        {
            if(!HasChanges) return;
            
            WriteToStorage();
            HasChanges = false;
            PrefsHandler.TrySave();
        }

        public virtual void Discard()
        {
            HasChanges = false;
            var old = base.Value;
            var value = ReadFromStorage();
            base.Value = value;
            _initialized = true;
            HasChanges = true;
            OnChanged(old, value);
        }

        public virtual void Reset()
        {
            Value = DefaultValue;
            HasChanges = false;
            Save();
        }

        public virtual void Delete()
        {
            Value = default;
            HasChanges = false;
            PrefsHandler.DeleteKey(Key);
        }
        
        public static implicit operator T(PreferenceItem<T> item) => item.Value;
        
        /// <summary>
        /// Watch the preference item for changes.
        /// </summary>
        /// <exception cref="Exception">Thrown if the preference item has SyncEnabled set to false.</exception>
        /// <exception cref="Exception">Thrown if the preference item is not of scope PrefsScope.Player.</exception>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("FF_PREFERENCES_RUNTIME_MONITOR")]
        public void Watch()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD || FF_PREFERENCES_RUNTIME_MONITOR
            if (!SyncEnabled)
            {
                throw new Exception("Cannot watch a preference item that has SyncEnabled set to false.");
            }
            //It is a bit stupid to have the check here, but in this way we can utilise the conditional.
            if (Scope == PrefsScope.Player)
            {
                PlayerPrefsChangeMonitor.Watch(this);
            }
            else
            {
                throw new Exception("Only PrefsScope.Player can be watched.");
            }
#endif
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("FF_PREFERENCES_RUNTIME_MONITOR")]
        public void UnWatch()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD || FF_PREFERENCES_RUNTIME_MONITOR
            PlayerPrefsChangeMonitor.UnWatch(this);
#endif
        }

        private void OnPrefsHandlerOnChanged(object sender, ChangePrefsEventArgs args)
        {
            if (args.Key == Key)
            {
                var old = base.Value;
                var value = ReadFromStorage();
                base.Value = value;
                _initialized = true;
                HasChanges = true;
                OnChanged(old, value);
            }
        }
        
        string IPreferenceItem.Key => Key;
        
        object IPreferenceItem.Value => Value;
    }
}