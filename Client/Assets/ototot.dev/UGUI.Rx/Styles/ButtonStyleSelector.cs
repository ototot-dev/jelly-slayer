using UnityEngine;
using UnityEngine.UI;


namespace UGUI.Rx {

/// <summary>
/// 
/// </summary>
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Button))]
public class ButtonStyleSelector : SelectableStyleSelector {

    public override string Tag => "button";
    
}

}