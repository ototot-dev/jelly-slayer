using UniRx;
using UGUI.Rx;
using System.Linq;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;
using Unity.VisualScripting;

namespace Game
{
    [Template(path: "UI/template/message_popup")]
    public class MessagePopupController : Controller
    {
        string _title;
        string _text;
        float _showDuration;

        BoolReactiveProperty visibilityFlag = new();


        public MessagePopupController(string title, string text, float showDuration = 10f) 
        {
            _title = title;
            _text = text;

            _showDuration = showDuration;
        }
        public override void OnPreShow()
        {
            base.OnPreShow();

            GetComponentById<TMPro.TMP_Text>("title").text = _title;
            GetComponentById<TMPro.TMP_Text>("text").text = _text;
        }
        public override void OnPostShow() 
        {
            base.OnPostShow();

            Observable.Timer(TimeSpan.FromSeconds(_showDuration)).Subscribe(_ =>
            {
                this.HideDimmed().Unload();
            });
        }
    }
}