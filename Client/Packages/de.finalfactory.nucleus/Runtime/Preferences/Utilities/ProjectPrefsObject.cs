#region License

// // --------------------------------------------------------------------------------------------------------------------
// // <summary>
// //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// //   ProjectPrefsObject.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FinalFactory.Tooling;
using FinalFactory.Utilities;
using UnityEngine;

namespace FinalFactory.Preferences.Utilities
{
    internal class ProjectPrefsObject : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] [HideInInspector] private List<string> Keys;
        [SerializeField] [HideInInspector] private List<string> Values;
        [SerializeField] [HideInInspector] internal PrefsScope Scope;
        private readonly Dictionary<string, string> _data = new();


        public int Count => _data.Count;
        public bool IsEmpty => Count == 0;

        public void OnBeforeSerialize()
        {
            _data.Keys.ToList(ref Keys);
            _data.Values.ToList(ref Values);
        }

        public void OnAfterDeserialize()
        {
            if (Keys != null && Values != null)
            {
                _data.Clear();
                _data.AddRange(Keys, Values);
            }
        }

        public bool AddOrUpdate<T>(string key, T value)
        {
            if (string.IsNullOrEmpty(key)) return false;
            string strValue;
            if (value is int i)
            { 
                strValue = "I_" + i;
            }
            else if (value is float f)
            {
                strValue = "F_" + f.ToString(CultureInfo.InvariantCulture);
            }
            else if (value is bool b)
            {
                strValue = "B_" + b;
            }
            else if (value is string s)
            {
                strValue = "S_" + s;
            }
            else if (value is null)
            {
                strValue = null;
            }
            else
            {
                throw new InvalidOperationException($"The type {value.GetType()} is not supported.");
            }
            return _data.AddOrUpdate(key, value == null ? null : strValue);
        }

        public bool ContainsKey(string key) => !IsEmpty && _data.ContainsKey(key);
        
        public string GetString(string key, string defaultValue = null)
        {
            var obj = GetObject(key);
            if (obj is string s) return s;
            return defaultValue;
        }
        
        public int GetInt(string key, int defaultValue = 0)
        {
            var obj = GetObject(key);
            if (obj is int i) return i;
            return defaultValue;
        }
        
        public float GetFloat(string key, float defaultValue = 0)
        {
            var obj = GetObject(key);
            if (obj is float f) return f;
            return defaultValue;
        }
        
        public bool GetBoolean(string key, bool defaultValue = false)
        {
            var obj = GetObject(key);
            if (obj is bool b) return b;
            return defaultValue;
        }

        public object GetObject(string key)
        {
            var strValue = _data.GetOrDefault(key);
            if (strValue == null) return null;
            if (strValue.StartsWith("I_"))
            {
                return int.Parse(strValue[2..]);
            }
            if (strValue.StartsWith("L_"))
            {
                return long.Parse(strValue[2..]);
            }
            if (strValue.StartsWith("F_"))
            {
                return float.Parse(strValue[2..], CultureInfo.InvariantCulture);
            }
            if (strValue.StartsWith("D_"))
            {
                return double.Parse(strValue[2..], CultureInfo.InvariantCulture);
            }
            if (strValue.StartsWith("B_"))
            {
                return bool.Parse(strValue[2..]);
            }
            if (strValue.StartsWith("S_"))
            {
                return strValue[2..];
            }
            return strValue;
        }

        public void RemoveObject(string key) => _data.Remove(key);

        public void Clear() => _data.Clear();

        public string[] GetAllKeys() => _data.Keys.ToArray();
    }
}