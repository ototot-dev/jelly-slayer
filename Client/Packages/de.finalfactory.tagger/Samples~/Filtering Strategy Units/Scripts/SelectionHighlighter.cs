// // ***********************************************************************
// // Assembly         : Tagger.Plugins
// // Author           : HeiaSamahi
// // Created          : 13.05.2017 : 14:07
// //
// // Last Modified By : Heia Samahi
// // Last Modified On : 26.05.2017 : 10:40
// // ***********************************************************************
// // <copyright file="SelectionHighlighter.cs" company="Heikyu">
// //     Copyright (c) Heikyu. All rights reserved.
// // </copyright>
// // <summary></summary>
// // ***********************************************************************

namespace FinalFactory.Tagger.Examples
{
    using UnityEngine;

    // ReSharper disable once ClassNeverInstantiated.Global
    public class SelectionHighlighter : MonoBehaviour
    {
        private Light _light;

        public void Start()
        {
            _light = GetComponentInChildren<Light>();
            _light.color = gameObject.GetComponent<Renderer>().material.GetColor("_Color");
            TurnOff();
        }

        public void TurnOff()
        {
            _light.enabled = false;
        }

        public void TurnOn()
        {
            _light.enabled = true;
        }
    }
}