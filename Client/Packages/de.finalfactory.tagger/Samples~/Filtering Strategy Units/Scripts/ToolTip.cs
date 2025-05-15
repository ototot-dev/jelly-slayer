// // ***********************************************************************
// // Assembly         : Tagger.Plugins
// // Author           : HeiaSamahi
// // Created          : 17.05.2017 : 16:21
// //
// // Last Modified By : Heia Samahi
// // Last Modified On : 26.05.2017 : 10:40
// // ***********************************************************************
// // <copyright file="ToolTip.cs" company="Heikyu">
// //     Copyright (c) Heikyu. All rights reserved.
// // </copyright>
// // <summary></summary>
// // ***********************************************************************

namespace Plugins.HeiKyu.Tagger.Examples
{
    using UnityEngine;

    public class ToolTip : MonoBehaviour
    {
        private bool _mouseOver;

        private void OnGUI()
        {
            if (!_mouseOver) return;

            var x = Event.current.mousePosition.x;
            var y = Event.current.mousePosition.y;

            GUILayout.BeginArea(new Rect(x - 149, y + 21, 300, 60));
            GUILayout.Box("Tags");
            GUILayout.BeginHorizontal();

            foreach (string tag in gameObject.GetTags())
            {
                GUI.color = TaggerSystem.Data.GetColor(tag);
                GUILayout.Box(tag);
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void OnMouseEnter()
        {
            _mouseOver = true;
        }

        private void OnMouseExit()
        {
            _mouseOver = false;
        }
    }
}