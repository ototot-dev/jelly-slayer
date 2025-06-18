using UGUI.Rx;
using UnityEngine;

namespace Game.UI
{
    public class BossStatusBarTemplate : Template
    {
        [Header("Blur")]
        public float heartPointBlurIntensity = 1f;
        public float stanceBlurIntensity = 1f;
    }
}