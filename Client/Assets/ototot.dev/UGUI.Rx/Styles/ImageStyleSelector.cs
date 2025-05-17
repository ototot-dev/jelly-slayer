using Retween.Rx;
using UnityEngine;

namespace UGUI.Rx
{

    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(TweenPlayer))]
    public class ImageStyleSelector : StyleSelector
    {
        public override string Tag => "img";
    }
}
