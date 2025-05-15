#region License

// // --------------------------------------------------------------------------------------------------------------------
// // <summary>
// //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// //   PreferenceKeyGen.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

#endregion

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace FinalFactory.Preferences
{
    [PublicAPI]
    public readonly struct PreferenceKeyGen
    {
        public readonly string DisplayModuleName;
        public readonly string DisplayProjectName;
        public readonly bool IsModule;
        public readonly string KeyPrefix;
        public readonly string ModuleName;
        public readonly string ProjectName;
        public readonly string SettingsLabel;
        public readonly string SettingsProviderPath;
        private readonly Company _company;

        public PreferenceKeyGen(Company company, string projectName = null, string moduleName = null)
        {
            _company = company;
            ProjectName = projectName ?? Application.productName;
            ModuleName = moduleName;
            IsModule = !string.IsNullOrWhiteSpace(ModuleName);
            DisplayProjectName = $"{ProjectName.Replace("_", " ")}";

            SettingsProviderPath = $"{_company.DisplayName}/{DisplayProjectName}";
            if (IsModule)
            {
                KeyPrefix = $"{_company.Name}_{ProjectName}_{ModuleName}_Preference_";
                DisplayModuleName = $"{ModuleName.Replace("_", " ")}";
                SettingsProviderPath += $"/{DisplayModuleName}";
                SettingsLabel = DisplayModuleName;
            }
            else
            {
                KeyPrefix = $"{_company.Name}_{ProjectName}_Preference_";
                DisplayModuleName = default;
                SettingsLabel = DisplayProjectName;
            }
        }
        
        
        public string this[string key] => $"{KeyPrefix}{key}";

        public IEnumerable<string> SettingsKeywords
        {
            get
            {
                yield return _company.Name;
                yield return _company.DisplayName;
                yield return _company.Initials;
                yield return DisplayProjectName;
                if (IsModule) yield return DisplayModuleName;
            }
        }
        
        public static PreferenceKeyGen operator +(PreferenceKeyGen keyGen, string moduleName)
        {
            if (keyGen.IsModule)
            {
                throw new InvalidOperationException("Can not chain a module with a module");
            }
            return new PreferenceKeyGen(keyGen._company, keyGen.ProjectName, moduleName);
        }
    }
}