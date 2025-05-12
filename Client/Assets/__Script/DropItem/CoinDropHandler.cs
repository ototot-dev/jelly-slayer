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
    public class CoinDropHandler : DropItemController
    {

        /// <summary>
        /// 
        /// </summary>
        public Sprite[] spriteTable;

        /// <summary>
        /// 
        /// </summary>
        public int coinPoint = 1;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="picker"></param>
        /// <returns></returns>
        public override bool TryPick(GameObject picker)
        {
            if (!base.TryPick(picker))
                return false;

            // if (picker.TryGetComponent<SlayerBrain>(out var heroBrain))
            //     heroBrain.BB.body.coinPoint.Value += coinPoint;

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

            Debug.Assert(spriteTable.Length == 3);

            if (coinPoint < 100)
                spriteRenderer.sprite = spriteTable[0];
            else if (coinPoint < 1000)
                spriteRenderer.sprite = spriteTable[1];
            else
                spriteRenderer.sprite = spriteTable[2];

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