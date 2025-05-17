using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FinalFactory;
using UniRx;
using UnityEngine;
using Yarn.Unity;

namespace Game
{
    public class DialogueRunnerDispatcher : DialogueViewBase
    {
        public Action onDialoqueStarted;
        public Action<LocalizedLine> onRunLine;
        public Action<LocalizedLine> onInterruptLine;
        public Action onDismissLine;
        public Action<DialogueOption[]> onRunOptions;
        public Action onDialoqueComplete;

        public override void DialogueStarted()
        {
            onDialoqueStarted?.Invoke();
        }

        public override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
        {
            base.RunLine(dialogueLine, onDialogueLineFinished);
            onRunLine?.Invoke(dialogueLine);
        }

        public override void InterruptLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
        {
            base.InterruptLine(dialogueLine, onDialogueLineFinished);
            onInterruptLine?.Invoke(dialogueLine);
        }

        public override void DismissLine(Action onDismissalComplete)
        {
            base.DismissLine(onDismissalComplete);
            onDismissLine?.Invoke();
        }

        public override void RunOptions(DialogueOption[] dialogueOptions, Action<int> onOptionSelected)
        {
            base.RunOptions(dialogueOptions, onOptionSelected);
            onRunOptions?.Invoke(dialogueOptions);
        }

        public override void DialogueComplete()
        {
            base.DialogueComplete();
            onDialoqueComplete?.Invoke();
        }

        public override void UserRequestedViewAdvancement()
        {
            base.UserRequestedViewAdvancement();
        }

        public DialogueRunner runner;
        public bool IsDialogueRunning() => runner != null && runner.IsDialogueRunning;

        public void AddViews(params DialogueViewBase[] views)
        {
            runner.SetDialogueViews(runner.dialogueViews.Concat(views).ToArray());
        }

        public void RemoveViews(params DialogueViewBase[] views)
        {
            runner.SetDialogueViews(runner.dialogueViews.Where(v => !views.Contains(v)).ToArray());
        }

        public void StartDialogue(string nodeName)
        {
            runner.StartDialogue(nodeName);
        }

        void Awake()
        {
            runner = GetComponent<DialogueRunner>();
            runner.AddCommandHandler<float>("waitForSeconds", WaitForSeconds);
            runner.AddCommandHandler<float>("fadeIn", FadeIn);
            runner.AddCommandHandler<float>("fadeOut", FadeOut);
            runner.AddCommandHandler<float, float, float>("vignette", Vignetee);
            runner.AddCommandHandler<string>("jumpTo", JumpTo);
            runner.AddCommandHandler("sleep", Sleep);
            runner.AddCommandHandler("getUp", GetUp);
        }

        public void Vignetee(float intensity, float smoothness, float blendTime)
        {
            if (blendTime > 0f)
            {
                var startTimeStamp = Time.time;
                var startIntensity = GameContext.Instance.cameraCtrler.vignetteIntensity;
                var startSmoothness = GameContext.Instance.cameraCtrler.vignetteSmoothness;

                Observable.EveryUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(blendTime)))
                    .DoOnCompleted(() =>
                    {
                        GameContext.Instance.cameraCtrler.vignetteIntensity = intensity;
                        GameContext.Instance.cameraCtrler.vignetteSmoothness = smoothness;
                    })
                    .Subscribe(_ =>
                    {
                        var alpha = Mathf.Clamp01((Time.time - startTimeStamp) / blendTime);
                        GameContext.Instance.cameraCtrler.vignetteIntensity = Mathf.Lerp(startIntensity, intensity, alpha);
                        GameContext.Instance.cameraCtrler.vignetteSmoothness = Mathf.Lerp(startSmoothness, smoothness, alpha);
                    }).AddTo(this);
            }
            else
            {
                GameContext.Instance.cameraCtrler.vignetteIntensity = intensity;
                GameContext.Instance.cameraCtrler.vignetteSmoothness = smoothness;
            }
        }

        public void FadeIn(float duration)
        {
            GameContext.Instance.CanvasManager.FadeIn(Color.black, duration);
        }

        public void FadeOut(float duration)
        {
            GameContext.Instance.CanvasManager.FadeOut(duration);
        }

        public IEnumerator WaitForSeconds(float seconds)
        {
            yield return new WaitForSeconds(seconds);
        }

        public void JumpTo(string text)
        {
            Debug.Log($"From {text}");
        }

        public void Sleep()
        {
            var slayerBrain = GameContext.Instance.playerCtrler.possessedBrain;
            slayerBrain.GetComponent<SlayerAnimController>().PlaySingleClipLooping(slayerBrain.BB.dialogue.sleepAnimClip, 0.1f);
        }

        public void GetUp()
        {
            var slayerBrain = GameContext.Instance.playerCtrler.possessedBrain;
            slayerBrain.GetComponent<SlayerAnimController>().PlaySingleClip(slayerBrain.BB.dialogue.getUpAnimClip, 0.5f, 0.2f);
        }
    }
}