#region License

// // --------------------------------------------------------------------------------------------------------------------
// // <summary>
// //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// //   ProjectPrefsUtilities.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

#endregion

using System.Runtime.CompilerServices;
using FinalFactory.Logging;
using FinalFactory.Utilities;


[assembly:InternalsVisibleTo("FinalFactory.Preferences.Editor")]
[assembly:InternalsVisibleTo("FinalFactory.Preferences")]

namespace FinalFactory.Preferences.Utilities
{
    internal class ProjectPrefsUtilities
    {
        private const string DefaultAssetPath = "Assets/Resources/";
        private static readonly Log Log = LogManager.GetLogger(typeof(ProjectPrefsUtilities));
        
        public static ProjectPrefsObject Load(PrefsScope scope)
        {
            if (
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                scope != PrefsScope.ProjectDevelopment &&
#endif
                scope != PrefsScope.ProjectRuntime)
            {
                Log.Error("Only ProjectDevelopment and ProjectRuntime are supported.");
                return null;
            }
            
            var assetName = scope == PrefsScope.ProjectRuntime ? "ProjectRuntimePrefs" : "ProjectDevelopmentPrefs";
            
            return SingletonScriptableLoader.Load<ProjectPrefsObject>(DefaultAssetPath, assetName, o => o.Scope == scope, o =>
            {
                o.Scope = scope;
            });
        }
    }
}