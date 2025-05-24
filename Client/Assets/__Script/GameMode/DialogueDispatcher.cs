using System;
using System.Collections;
using System.Linq;
using FinalFactory;
using UGUI.Rx;
using UniRx;
using UnityEngine;
using Yarn.Unity;

namespace Game
{
    public partial class DialogueDispatcher : DialogueViewBase
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

        public bool IsDialogueRunning() => __runner != null && __runner.IsDialogueRunning;

        public void AddViews(params DialogueViewBase[] views)
        {
            __runner.SetDialogueViews(__runner.dialogueViews.Concat(views).ToArray());
        }

        public void RemoveViews(params DialogueViewBase[] views)
        {
            __runner.SetDialogueViews(__runner.dialogueViews.Where(v => !views.Contains(v)).ToArray());
        }

        public void StartDialogue(string nodeName)
        {
            __runner.StartDialogue(nodeName);
        }

        protected DialogueRunner __runner;

        void Awake()
        {
            Debug.Assert(GameContext.Instance.dialogueRunner != null);
            __runner = GameContext.Instance.dialogueRunner;
            __runner.SetDialogueViews(new DialogueViewBase[] { this });

            AwakeInternal();
        }

        protected virtual void AwakeInternal()
        {
            __runner.AddCommandHandler<float>("waitForSeconds", WaitForSeconds);
            __runner.AddCommandHandler<float>("fadeIn", FadeIn);
            __runner.AddCommandHandler<float>("fadeOut", FadeOut);
            __runner.AddCommandHandler<float, float, float>("vignette", Vignetee);
            __runner.AddCommandHandler<string>("jumpTo", JumpTo);
            __runner.AddCommandHandler("sleep", Sleep);
            __runner.AddCommandHandler("getUp", GetUp);
            __runner.AddCommandHandler("showMechArm", ShowMechArm);
            __runner.AddCommandHandler("hideMechArm", HideMechArm);
            __runner.AddCommandHandler("showSword", ShowSword);
            __runner.AddCommandHandler("hideSword", HideSword);
            __runner.AddCommandHandler<string>("playsound", PlaySound);
            __runner.AddCommandHandler<string, float>("showInteractionKey", ShowInteractionKey);
            __runner.AddCommandHandler<string, float, int, bool>("tweenshake", TweenShake);
            __runner.AddCommandHandler<string, string>("sendmsg", SendMessage);            
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
            GameContext.Instance.canvasManager.FadeIn(Color.black, duration);
        }

        public void FadeOut(float duration)
        {
            GameContext.Instance.canvasManager.FadeOut(duration);
        }

        public IEnumerator WaitForSeconds(float seconds)
        {
            yield return new WaitForSeconds(seconds);
        }

        public IEnumerator ShowInteractionKey(string tagName, float radius)
        {
            GameContext.Instance.playerCtrler.interactionKeyCtrler = new InteractionKeyController("E", "RunLine", TaggerSystem.FindGameObjectWithTag(tagName).transform, radius).Load().Show(GameContext.Instance.canvasManager.body.transform as RectTransform);
            yield return new WaitUntil(() => GameContext.Instance.playerCtrler.interactionKeyCtrler == null || GameContext.Instance.playerCtrler.interactionKeyCtrler.commandName != "RunLine");
        }

        public void PlaySound(string soundName)
        {
            if (Enum.TryParse(soundName, out SoundID id))
            {
                SoundManager.Instance.Play(id);
            }
            else
            {
                Debug.LogError("Invalid enum string: " + soundName);
            }
        }

        public void SendMessage(string tag, string msg) 
        {
            var obj = TaggerSystem.FindGameObjectWithTag(tag);
            if (obj == null)
                Debug.Log("YarnCommand: Not Found Object, " + msg);

            obj.SendMessage(msg, SendMessageOptions.DontRequireReceiver);
        }
    }
}