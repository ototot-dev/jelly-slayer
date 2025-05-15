// #region License
// // // --------------------------------------------------------------------------------------------------------------------
// // // <summary>
// // //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// // //   TaggerIdAttribute.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// // //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // // </summary>
// // // --------------------------------------------------------------------------------------------------------------------
// #endregion

using UnityEngine;

namespace Finalfactory.Tagger
{
    public class TaggerIdAttribute : PropertyAttribute
    {
        public readonly string ExcludePropertyName;

        public TaggerIdAttribute()
        {
        }

        public TaggerIdAttribute(string excludePropertyName)
        {
            ExcludePropertyName = excludePropertyName;
        }
    }
}