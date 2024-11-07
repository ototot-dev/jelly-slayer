using System;
using UnityEngine;
using UnityEngine.UI;


namespace UGUI.Rx {

/// <summary>
/// 
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class TextStyleSelector : StyleSelector {

    public enum Tags {
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
    
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
    public override string Tag => __tag.ToString();

}

}