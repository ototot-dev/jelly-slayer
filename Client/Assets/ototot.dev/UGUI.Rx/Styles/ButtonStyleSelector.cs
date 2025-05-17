using Retween.Rx;
using UnityEngine;
using UnityEngine.UI;

namespace UGUI.Rx
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(TweenPlayer))]
    [RequireComponent(typeof(Button))]
    public class ButtonStyleSelector : SelectableStyleSelector
    {
        public override string Tag => "button";
    }
}