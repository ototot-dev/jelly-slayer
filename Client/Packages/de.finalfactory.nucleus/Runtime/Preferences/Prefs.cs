#region License

// // --------------------------------------------------------------------------------------------------------------------
// // <summary>
// //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// //   Prefs.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

#endregion

#if FINAL_PREFERENCES
using FinalFactory.Preferences.Encryption;
#endif

using System;
using FinalFactory.Preferences.Handlers;
using FinalFactory.Preferences.Utilities;
using JetBrains.Annotations;

namespace FinalFactory.Preferences
{
    /// <summary>
    /// This class provides static access to different types of preferences handlers.
    /// </summary>
    [PublicAPI]
    public static class Prefs
    {
#if UNITY_EDITOR
        internal static IPrefsReader Reader = new PrefsReaderDummy();
#endif
        
#if FINAL_PREFERENCES
        public static IPrefsEncryptor Encryptor = new PrefsEncryptor();
#endif
        /// <summary>
        /// Player preferences handler.
        /// </summary>
        public static readonly IPrefs Player = new PlayerPrefsHandler();

        /// <summary>
        /// Project runtime preferences handler.
        /// </summary>
        public static readonly IPrefs ProjectRuntime = new ProjectRuntimePrefsHandler();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        /// <summary>
        /// Project development preferences handler.
        /// </summary>
        public static readonly IPrefs ProjectDevelopment = new ProjectDevelopmentPrefsHandler();
#endif

#if UNITY_EDITOR
        /// <summary>
        /// Editor preferences handler.
        /// </summary>
        public static readonly IPrefs Editor = new EditorPrefsHandler();
        
        /// <summary>
        /// Standalone player preferences handler.
        /// </summary>
        public static readonly IPrefs Standalone = new StandalonePlayerPrefsHandler();
#endif
        /// <summary>
        /// Preferences always available. Editor and Runtime do not share the data.
        /// </summary>
        public static IPrefs EditorOrRuntimeNotShared
        {
            get
            {
#if UNITY_EDITOR
                return Editor;
#else
                return Player;
#endif
            }
        }

        /// <summary>
        /// Returns the preferences handler for the given scope.
        /// </summary>
        /// <param name="scope">The scope of the preferences.</param>
        /// <returns>The preferences handler for the given scope.</returns>
        public static IPrefs Get(PrefsScope scope)
        {
            switch (scope)
            {
                case PrefsScope.Player:
                    return Player;
                case PrefsScope.ProjectRuntime:
                    return ProjectRuntime;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                case PrefsScope.ProjectDevelopment:
                    return ProjectDevelopment;
#endif
#if UNITY_EDITOR
                case PrefsScope.Editor:
                    return Editor;
                case PrefsScope.Standalone:
                    return Standalone;
#endif
                default:
                    throw new ArgumentOutOfRangeException(nameof(scope));
            }
        }
    }
}