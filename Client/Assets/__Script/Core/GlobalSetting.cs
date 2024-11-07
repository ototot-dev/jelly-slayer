using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 전역 설정값
    /// </summary>
    public class GlobalSetting : MonoSingleton<GlobalSetting>
    {
        /// <summary>
        /// 
        /// </summary>
        public string TerrainLayerName => "Terrain";


    }

}
