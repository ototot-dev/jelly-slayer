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
    public class FlawlessDropHandler : DropItemController
    {

        /// <summary>
        /// 
        /// </summary>
        public Sprite[] spriteTable;

        /// <summary>
        /// 
        /// </summary>
        public int enchantPoint = 1;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="picker"></param>
        /// <returns></returns>
        public override bool TryPick(GameObject picker)
        {
            if (!base.TryPick(picker))
                return false;

            // if (picker.TryGetComponent<SlayerBrain>(out var heroBrain) && heroBrain.BB.action.currBombNum.Value < heroBrain.BB.action.maxBombNum.Value)
            //     heroBrain.BB.action.bombFillAlpha.Value = Mathf.Clamp01(heroBrain.BB.action.bombFillAlpha.Value + enchantPoint * heroBrain.BB.shared.ranged.bombFillRatePerAction.max);

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


            spriteRenderer.sprite = spriteTable[0];

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