using System;
using DG.Tweening;
using Retween.Rx;
using UniRx;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 
    /// </summary>
    public class HeartDropHandler : DropItemController
    {
        /// <summary>
        /// 
        /// </summary>
        public int heartPoint = 1;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="picker"></param>
        /// <returns></returns>
        public override bool TryPick(GameObject picker)
        {
            if (!base.TryPick(picker))
                return false;

            // if (picker.TryGetComponent<HeroBrain>(out var heroBrain))
            //     heroBrain.PawnHP.Gain(heartPoint);

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public GameObject glowEffect;

        /// <summary>
        /// 
        /// </summary>
        protected override void StartInternal()
        {
            base.StartInternal();

            onPickUpEnabled += () =>
            {
                glowEffect.SetActive(true);
            };

            onPickUp += (picker) =>
            {
                GetComponent<SphereCollider>().enabled = false;
            };
        }
    }
}