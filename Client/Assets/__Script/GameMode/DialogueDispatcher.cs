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
            __runner.AddCommandHandler<string>("playSound", PlaySound);
            __runner.AddCommandHandler<string, float>("showInteractionKey", ShowInteractionKey);
            __runner.AddCommandHandler<string, float>("showAndWaitInteractionKey", ShowAndWaitInteractionKey);
            __runner.AddCommandHandler<string, float, int, bool>("tweenShake", TweenShake);
            __runner.AddCommandHandler<string, string>("sendMsg", SendMessage);
            __runner.AddCommandHandler<string, string>("spawnPawn", SpawnPawn);
            __runner.AddCommandHandler<string>("setCameraTarget", SetCameraTarget);
            __runner.AddCommandHandler("setCameraSlayer", SetCameraSlayer);
            __runner.AddCommandHandler<string, string, float>("showMessagePopup", ShowMessagePopup);           
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

        public void ShowInteractionKey(string tagName, float radius)
        {
            GameContext.Instance.playerCtrler.interactionKeyCtrler = new InteractionKeyController("E", "RunLine", TaggerSystem.FindGameObjectWithTag(tagName).transform, radius).Load().Show(GameContext.Instance.canvasManager.body.transform as RectTransform);
        }

        public IEnumerator ShowAndWaitInteractionKey(string tagName, float radius)
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

        public void SpawnPawn(string pawnName, string spawnTag)
        {
            Debug.Log("YarnComman SpawnPawn: " + pawnName + ", " + spawnTag);
            if (Enum.TryParse(pawnName, out PawnId id) == false)
            {
                Debug.Log("SpawnPawn Enum Parse Error: " + pawnName);
                return;
            }
            var pawnData = MainTable.PawnData.PawnDataList.First(d => d.pawnId == id);
            if (pawnData == null)
            {
                Debug.Log("SpawnPawn PawnData Get Error: " + id);
                return;
            }
            var resObj = Resources.Load<GameObject>(pawnData.prefPath);
            if (resObj == null)
            {
                Debug.Log("SpawnPawn Res Load Error: " + pawnData.pawnId + ", " + pawnData.prefPath);
                return;
            }
            Vector3 pos = Vector3.zero;
            GameObject tagObj = TaggerSystem.FindGameObjectWithTag(spawnTag);
            if (tagObj != null)
                pos = tagObj.transform.position;
            else
                Debug.Log("SpawnPawn SpawnTag Error: " + spawnTag);

            var pawnObj = Instantiate(resObj, pos, Quaternion.identity);
        }

        public void SendMessage(string tag, string msg) 
        {
            var obj = TaggerSystem.FindGameObjectWithTag(tag);
            if (obj == null)
                Debug.Log("YarnCommand: Not Found Object, " + msg);

            obj.SendMessage(msg, SendMessageOptions.DontRequireReceiver);
        }

        public void SetCameraTarget(string targetTag) 
        {
            Debug.Log("YarnCommand TargetCamera : " + targetTag);
            GameObject tagObj = TaggerSystem.FindGameObjectWithTag(targetTag);
            if (tagObj == null) 
            {
                Debug.Log("YarnCommand Error : Not Exist Object : " + targetTag);
                return;
            }
            GameContext.Instance.cameraCtrler.virtualCamera.Follow = tagObj.transform;
        }
        public void SetCameraSlayer() 
        {
            var slayerBrain = GameContext.Instance.playerCtrler.possessedBrain;
            GameContext.Instance.cameraCtrler.virtualCamera.Follow = slayerBrain.coreColliderHelper.transform;
        }

        public void ShowMessagePopup(string title, string text, float showDuration) 
        {
            Debug.Log("showMessagePopup" + title + text);
            new MessagePopupController(title, text, showDuration).Load().ShowDimmed(GameContext.Instance.canvasManager.dimmed.transform as RectTransform);
        }
    }
}