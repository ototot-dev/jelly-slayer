// #region License
// // // --------------------------------------------------------------------------------------------------------------------
// // // <summary>
// // //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// // //   ITaggerDataLoader.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// // //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // // </summary>
// // // --------------------------------------------------------------------------------------------------------------------
// #endregion

namespace Finalfactory.Tagger
{
    public interface ITaggerDataLoader
    {
        /// <summary>
        ///     Returns the <see cref="TaggerData" />
        /// </summary>
        /// <returns></returns>
        TaggerData GetData();

        /// <summary>
        ///     Handles the save request from the given <see cref="TaggerData" />
        /// </summary>
        /// <param name="data"></param>
        void RequestSave(TaggerData data);
    }
}