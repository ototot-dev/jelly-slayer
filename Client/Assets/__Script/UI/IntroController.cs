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

            __newGameButton = template.GetComponentById<Button>("new-game");
            __continueButton = template.GetComponentById<Button>("continue");
            __settingButton = template.GetComponentById<Button>("setting");
            __exitButton = template.GetComponentById<Button>("exit");
            
            __newGameButton.interactable = __continueButton.interactable = __settingButton.interactable = __exitButton.interactable = false;

            template.GetComponentById<CanvasGroup>("options").alpha = 0f;
            template.GetComponentById<CanvasGroup>("press-any-key-focus").alpha = 0f;
            template.GetComponentById<TMPro.TextMeshProUGUI>("press-any-key-text").color = Color.clear;
        }

        Button __newGameButton;
        Button __continueButton;
        Button __settingButton;
        Button __exitButton;

        public override void OnPostShow()
        {
            base.OnPostShow();

            __newGameButton.OnPointerEnterAsObservable().Where(_ => __newGameButton.interactable).Subscribe(v => SetFocusedOption("new-game")).AddToHide(this);
            __continueButton.OnPointerEnterAsObservable().Where(_ => __continueButton.interactable).Subscribe(v => SetFocusedOption("continue")).AddToHide(this);
            __settingButton.OnPointerEnterAsObservable().Where(_ => __settingButton.interactable).Subscribe(v => SetFocusedOption("setting")).AddToHide(this);
            __exitButton.OnPointerEnterAsObservable().Where(_ => __exitButton.interactable).Subscribe(v => SetFocusedOption("exit")).AddToHide(this);

            __newGameButton.OnPointerClickAsObservable().Where(_ => __newGameButton.interactable).Subscribe(_ => HandleOptionButtonClicked("new-game")).AddToHide(this);
            __continueButton.OnPointerClickAsObservable().Where(_ => __continueButton.interactable).Subscribe(_ => HandleOptionButtonClicked("continue")).AddToHide(this);
            __settingButton.OnPointerClickAsObservable().Where(_ => __settingButton.interactable).Subscribe(_ => HandleOptionButtonClicked("setting")).AddToHide(this);
            __exitButton.OnPointerClickAsObservable().Where(_ => __exitButton.interactable).Subscribe(_ => HandleOptionButtonClicked("exit")).AddToHide(this);

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

                    __newGameButton.interactable = __continueButton.interactable = __settingButton.interactable = __exitButton.interactable = true;
                }).AddToHide(this);
            }).AddToHide(this);

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
            __optionFocusRect.SetParent(template.GetComponentById<RectTransform>(optionId), false);
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