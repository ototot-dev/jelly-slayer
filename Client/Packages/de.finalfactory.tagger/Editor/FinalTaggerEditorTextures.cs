// #region License
// // // --------------------------------------------------------------------------------------------------------------------
// // // <summary>
// // //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// // //   FinalTaggerEditorTextures.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// // //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // // </summary>
// // // --------------------------------------------------------------------------------------------------------------------
// #endregion

using FinalFactory.Editor;
using UnityEngine;

namespace Finalfactory.Tagger.Editor
{
    public class FinalTaggerEditorTextures
    {
        public const string IconPath = "Packages/de.finalfactory.tagger/Editor/Icons/";

        public static readonly Texture2D SingletonIcon;

        static FinalTaggerEditorTextures()
        {
            SingletonIcon = FinalEditorUtilities.LoadIcon(IconPath, "Singleton.png", false);
        }
    }
}