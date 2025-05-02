using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UGUI.Rx;
using System;
using UniRx.Triggers;
using Retween.Rx;

namespace Game
{
    [Template(path: "UI/template/intro")]
    public class IntroController : Controller
    {
        public override void OnPreShow()
        {
            base.OnPreShow();

            template.GetComponentById<Button>("new-game").interactable = false;
            template.GetComponentById<Button>("continue").interactable = false;
            template.GetComponentById<Button>("setting").interactable = false;
            template.GetComponentById<Button>("exit").interactable = false;

            template.GetComponentById<CanvasGroup>("options").alpha = 0f;
            template.GetComponentById<CanvasGroup>("press-any-key-focus").alpha = 0f;
            template.GetComponentById<TMPro.TextMeshProUGUI>("press-any-key-text").color = Color.clear;
        }

        public override void OnPostShow()
        {
            base.OnPostShow();

            __pressAnyKeyFocusQuery = template.GetComponentById<PanelStyleSelector>("press-any-key-focus").query;
            __optionFocusQuery = template.GetComponentById<PanelStyleSelector>("options-focus").query;

            Observable.Timer(TimeSpan.FromSeconds(0.5f)).Subscribe(_ =>
            {
                var query = template.GetComponentById<TextStyleSelector>("press-any-key-text").query;
                query.activeStates.Add("show");
                query.Apply();

                Observable.Timer(TimeSpan.FromSeconds(1f)).Subscribe(_ =>
                {
                    Observable.NextFrame().Subscribe(_ => template.GetComponentById<CanvasGroup>("press-any-key-focus").alpha = 1f);

                    __pressAnyKeyFocusQuery.activeStates.Clear();
                    __pressAnyKeyFocusQuery.activeStates.Add("show");
                    __pressAnyKeyFocusQuery.Apply();

                    Observable.Timer(TimeSpan.FromSeconds(1f)).Subscribe(_ =>
                    {
                        __pressAnyKeyFocusQuery.activeStates.Clear();
                        __pressAnyKeyFocusQuery.activeStates.Add("heartbeat");
                        __pressAnyKeyFocusQuery.Apply();
                    }).AddToHide(this);
                }).AddToHide(this);
            }).AddToHide(this);

            Observable.EveryUpdate().Where(_ => Input.anyKeyDown && __pressAnyKeyFocusQuery.activeStates.Contains("heartbeat")).Take(1).Subscribe(_ =>
            {
                var focusQuery = template.GetComponentById<PanelStyleSelector>("press-any-key-focus").query;
                focusQuery.activeStates.Clear();
                focusQuery.activeStates.Add("hide");
                focusQuery.Apply();

                var textQuery = template.GetComponentById<TextStyleSelector>("press-any-key-text").query;
                textQuery.activeStates.Clear();
                textQuery.activeStates.Add("hide");
                textQuery.Apply();

                Observable.Timer(TimeSpan.FromSeconds(0.5f)).Subscribe(_ =>
                {
                    template.GetComponentById<CanvasGroup>("options").alpha = 1f;
                    var optionsQuery = template.GetComponentById<PanelStyleSelector>("options").query;
                    optionsQuery.activeStates.Add("show");
                    optionsQuery.Apply();

                    __optionFocusQuery.activeStates.Clear();
                    __optionFocusQuery.activeStates.Add("show");
                    __optionFocusQuery.Apply();

                    template.GetComponentById<Button>("new-game").interactable = true;
                    template.GetComponentById<Button>("continue").interactable = true;
                    template.GetComponentById<Button>("setting").interactable = true;
                    template.GetComponentById<Button>("exit").interactable = true;
                }).AddToHide(this);
            }).AddToHide(this);

            template.GetComponentById<Button>("new-game").OnPointerEnterAsObservable().Subscribe(v => SetFocusedOption("new-game")).AddToHide(this);
            template.GetComponentById<Button>("continue").OnPointerEnterAsObservable().Subscribe(v => SetFocusedOption("continue")).AddToHide(this);
            template.GetComponentById<Button>("setting").OnPointerEnterAsObservable().Subscribe(v => SetFocusedOption("setting")).AddToHide(this);
            template.GetComponentById<Button>("exit").OnPointerEnterAsObservable().Subscribe(v => SetFocusedOption("exit")).AddToHide(this);

            template.GetComponentById<Button>("new-game").OnPointerClickAsObservable().Subscribe(_ => HandleOptionButtonClicked("new-game")).AddToHide(this);
            template.GetComponentById<Button>("continue").OnPointerClickAsObservable().Subscribe(_ => HandleOptionButtonClicked("continue")).AddToHide(this);
            template.GetComponentById<Button>("setting").OnPointerClickAsObservable().Subscribe(_ => HandleOptionButtonClicked("setting")).AddToHide(this);
            template.GetComponentById<Button>("exit").OnPointerClickAsObservable().Subscribe(_ => HandleOptionButtonClicked("exit")).AddToHide(this);

            // LoadingPageController loadingCtrler = null;

            // template.GetComponentById<Button>("start").OnClickAsObservable().Where(_ => loadingCtrler == null).Subscribe(_ =>
            // {
            //     this.Hide().Unload();
            //     loadingCtrler = new LoadingPageController().Load();
            //     loadingCtrler.Show(GameContext.Instance.canvasManager.body.transform as RectTransform);
            // }).AddToHide(this);
        }

        void HandleOptionButtonClicked(string optionId)
        {
            if (!template.GetComponentById<Button>(optionId).interactable)
                return;

            switch (optionId)
            {
                case "new-game": break;
                case "continue": break;
                case "setting": break;
                case "exit": break;
            }

            __optionFocusQuery.activeStates.Clear();
            __optionFocusQuery.activeStates.Add("hide");
            __optionFocusQuery.Apply();

            this.HideAsObservable().Subscribe();
        }

        void SetFocusedOption(string optionId)
        {
            __currFocusOptionId = optionId;

            __optionFocusRect ??= template.GetComponentById<RectTransform>("options-focus");
            __optionFocusRect.parent = template.GetComponentById<RectTransform>(optionId);
            __optionFocusRect.localPosition = Vector3.zero;
            __optionFocusRect.SetSiblingIndex(0);

            __optionFocusQuery.activeStates.Clear();
            __optionFocusQuery.Apply();
            __optionFocusQuery.activeStates.Add("show");
            __optionFocusQuery.Apply();
        }

        string __currFocusOptionId;
        RectTransform __optionFocusRect;
        TweenSelectorQuery __optionFocusQuery;
        TweenSelectorQuery __pressAnyKeyFocusQuery;
    }
}