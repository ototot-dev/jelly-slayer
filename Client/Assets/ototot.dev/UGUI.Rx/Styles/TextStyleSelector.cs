using Retween.Rx;
using UnityEngine;

namespace UGUI.Rx
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(TweenPlayer))]
    public class TextStyleSelector : StyleSelector
    {
        public enum Tags
        {
            p,
            h1,
            h2,
            h3,
            h4,
            h5,
            h6,
        }

        [SerializeField, HideInInspector]
        Tags __tag = Tags.p;
        public override string Tag => __tag.ToString();
    }
}