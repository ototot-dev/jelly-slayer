// #region License
// // // --------------------------------------------------------------------------------------------------------------------
// // // <summary>
// // //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// // //   FinalTaggerEditor.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// // //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // // </summary>
// // // --------------------------------------------------------------------------------------------------------------------
// #endregion

using UnityEditor;

namespace Finalfactory.Tagger.Editor
{
    [CustomEditor(typeof(FinalTagger))]

    // ReSharper disable once UnusedMember.Global
    public class FinalTaggerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var finalTagger = (FinalTagger)target;
            FinalTaggerEditorUI.AnyEditorWindowReference = this;
            FinalTaggerEditorUI.DrawInspectorUI(finalTagger.gameObject);
        }
    }
}