using DG.Tweening;
using System;
using System.Linq;
using UniRx;
using Unity.Linq;
using UnityEngine;

namespace Game
{

    /// <summary>
    /// 
    /// </summary>
    public class HeroEffectController : PawnDamageEffectController
    {

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override Renderer[] CollectRenderers()
        {
            return gameObject.DescendantsAndSelf()
                .Where(d => d.name == "Base" || d.CompareTag("PawnPartsMesh"))
                .Select(d => d.GetComponent<Renderer>())
                .Where(r => r != null)
                .ToArray();
        }

        // public void 

    }

}