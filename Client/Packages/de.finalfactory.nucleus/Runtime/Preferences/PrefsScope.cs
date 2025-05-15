#region License

// // --------------------------------------------------------------------------------------------------------------------
// // <summary>
// //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// //   PrefsScope.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

#endregion

namespace FinalFactory.Preferences
{
    //DO NOT CHANGE THE VALUES
    //NEVER!
    //Renaming is fine.
    public enum PrefsScope
    {
        /// <summary>
        ///     Can be saved in runtime player.<br />
        ///     Project specific.<br />
        ///     Runtime: read/write<br />
        ///     Editor: read/write
        /// </summary>
        Player = 1,

        /// <summary>
        ///     Settings accessible in editor and runtime.<br />
        ///     Project specific. Accessible from every computer if shared via source control.<br />
        ///     Runtime: read only<br />
        ///     Development Build: read only<br />
        ///     Editor: read/write
        /// </summary>
        ProjectRuntime = 2,
#if UNITY_EDITOR || DEVELOPMENT_BUILD //Encapsulate in preprocessor directive to avoid runtime errors on "forgotten" values
        /// <summary>
        ///     Settings only for development.<br />
        ///     Project specific. Accessible from every computer if shared via source control.<br />
        ///     <b>Runtime: N/A</b><br />
        ///     Development Build: read only<br />
        ///     Editor: read/write
        /// </summary>
        ProjectDevelopment = 3,
#endif
#if UNITY_EDITOR
        /// <summary>
        ///     Settings only for the editor.<br />
        ///     Computer specific, not project specific. Every user has its own settings. Accessible from every project on the same
        ///     computer.<br />
        ///     <b>Runtime: N/A</b><br />
        ///     Editor: read/write
        /// </summary>
        Editor = 4,
        
        /// <summary>
        ///     Settings from the PlayerPrefs of the built standalone application.<br />
        ///     These are the settings your game uses when it is built and running on your computer.<br /> 
        ///     <b>Runtime: N/A</b><br />
        /// </summary>
        Standalone = 5,
#endif
    }
}