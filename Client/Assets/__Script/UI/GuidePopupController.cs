using UniRx;
using UGUI.Rx;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    [Template(path: "UI/template/guide-popup")]
    public class GuidePopupController : Controller
    {
        string _title;
        string _text;
        
        DialogueDispatcher __dialogueDispatcher;

        BoolReactiveProperty visibilityFlag = new();

        public GuidePopupController(string title, string text, DialogueDispatcher dispatcher)
        {
            _title = title;
            _text = text;

            __dialogueDispatcher = dispatcher;
        }
        public override void OnPreShow()
        {
            base.OnPreShow();

            GetComponentById<TMPro.TMP_Text>("title").text = _title;
            GetComponentById<TMPro.TMP_Text>("desc").text = _text;

            Button button = GetComponentById<Button>("ok");
            button?.onClick.AddListener(OnClickOK);
        }
        public override void OnPostShow()
        {
            base.OnPostShow();  
        }
        public override void OnPreHide()
        {
            base.OnPreHide();

            Time.timeScale = 1.0f;
            __dialogueDispatcher._isWaitCheck = true;
        }
        void OnClickOK() 
        {
            Debug.Log("OnClickOK");
            this.Hide().Unload();
        }
    }
}