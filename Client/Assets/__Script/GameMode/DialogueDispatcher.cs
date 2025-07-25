﻿using System;
using System.Collections;
using System.Linq;
using FinalFactory;
using Obi;
using UGUI.Rx;
using UniRx;
using UnityEngine;
using Yarn.Unity;
using Game.UI;

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

        public bool _isWaitCheck = false;

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

        public void StopDialogue()
        {
            __runner.Stop();
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
            __runner.AddCommandHandler<string>("showInteractionKey", ShowInteractionKey);
            __runner.AddCommandHandler<string>("showAndWaitInteractionKey", ShowAndWaitInteractionKey);
            __runner.AddCommandHandler("waitCheck", WaitCheck);
            __runner.AddCommandHandler<string, float, int, bool>("tweenShake", TweenShake);
            __runner.AddCommandHandler<string, bool>("setActive", SetActive);
            __runner.AddCommandHandler<string, string>("sendMsg", SendMessage);
            __runner.AddCommandHandler<string>("sendMsgToGameMode", SendMessageToGameMode);
            __runner.AddCommandHandler<string, string>("sendMsgToGameModeParam", SendMessageToGameModeParam);
            __runner.AddCommandHandler<string, string>("spawnPawn", SpawnPawn);
            __runner.AddCommandHandler<string, float, float, float>("spawnPawnByVector", SpawnPawnByVector);
            __runner.AddCommandHandler<string>("setBossStatusController", SetBossStatusController);
            __runner.AddCommandHandler<string>("setCameraTarget", SetCameraTarget);
            __runner.AddCommandHandler("setCameraSlayer", SetCameraSlayer);
            __runner.AddCommandHandler<string>("setConfinerVolume", SetConfinerVolume);
            __runner.AddCommandHandler<string, string, float>("showMessagePopup", ShowMessagePopup);
            __runner.AddCommandHandler<string, string>("showGuidePopup", ShowGuidePopup);
            __runner.AddCommandHandler<string, float>("setLightIntensity", SetLightIntensity);

            // Timeline
            __runner.AddCommandHandler<string>("playTimeline", PlayTimeline);
            __runner.AddCommandHandler<string>("loadTimeline", LoadTimeline);
            __runner.AddCommandHandler("playLoadedTimeline", PlayLoadedTimeline);
            __runner.AddCommandHandler<string, int>("bindAnimatorToTrack", BindAnimatorToTrack);
            __runner.AddCommandHandler<string, string>("bindAnimatorToTrackByName", BindAnimatorToTrackByName);
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

        public void ShowInteractionKey(string tagName)
        {
            new InteractionKeyController("E", "RunLine", -1f, TaggerSystem.FindGameObjectWithTag(tagName).GetComponent<InteractableHandler>()).Load().Show(GameContext.Instance.canvasManager.body.transform as RectTransform);
        }

        public IEnumerator ShowAndWaitInteractionKey(string tagName)
        {
            var targetInteractable = TaggerSystem.FindGameObjectWithTag(tagName).GetComponent<InteractableHandler>();
            Debug.Assert(targetInteractable != null);

            var interactableKeyCtrler = new InteractionKeyController("E", targetInteractable.GetCommand(), -1f, targetInteractable).Load().Show(GameContext.Instance.canvasManager.body.transform as RectTransform);
            yield return new WaitUntil(() => interactableKeyCtrler.IsInteractionFinished);
        }

        public IEnumerator WaitCheck() 
        {
            _isWaitCheck = false;
            yield return new WaitUntil(() => { return _isWaitCheck; });
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

        private GameObject InstantiatePawn(string pawnName) 
        {
            if (Enum.TryParse(pawnName, out PawnId id) == false)
            {
                Debug.Log("SpawnPawn Enum Parse Error: " + pawnName);
                return null;
            }
            var pawnData = MainTable.PawnData.PawnDataList.First(d => d.pawnId == id);
            if (pawnData == null)
            {
                Debug.Log("SpawnPawn PawnData Get Error: " + id);
                return null;
            }
            var resObj = Resources.Load<GameObject>(pawnData.prefPath);
            if (resObj == null)
            {
                Debug.Log("SpawnPawn Res Load Error: " + pawnData.pawnId + ", " + pawnData.prefPath);
                return null;
            }
            return Instantiate(resObj);
        }

        public void SpawnPawnByVector(string pawnName, float tx, float ty, float tz) 
        {
            Debug.Log("YarnComman SpawnPawn: " + pawnName + ", " + tx + ", " + ty + ", " + tz);
            var pawnObj = InstantiatePawn(pawnName);
            if (pawnObj == null)
                return;

            pawnObj.transform.position = new Vector3(tx, ty, tz);

            // Spawn Message
            var mode = GameContext.Instance.launcher.currGameMode;
            mode?.SendMessage("PawnSpawned", pawnObj, SendMessageOptions.DontRequireReceiver);
        }

        public void SpawnPawn(string pawnName, string spawnTag)
        {
            Debug.Log("YarnComman SpawnPawn: " + pawnName + ", " + spawnTag);            
            var pawnObj = InstantiatePawn(pawnName);
            if (pawnObj == null)
                return;

            if (spawnTag.Length > 0)
            {
                if (spawnTag == "Player")
                {
                    var brain = GameContext.Instance.playerCtrler.possessedBrain;
                    if (brain != null) 
                    {
                        pawnObj.transform.position = brain.GetWorldPosition();
                    }
                }
                else
                {
                    GameObject tagObj = TaggerSystem.FindGameObjectWithTag(spawnTag);
                    if (tagObj != null)
                    {
                        pawnObj.transform.position = tagObj.transform.position;
                        pawnObj.transform.rotation = tagObj.transform.rotation;
                    }
                    else
                        Debug.Log("SpawnPawn SpawnTag Error: " + spawnTag);


                }
            }
            // Spawn Message
            var mode = GameContext.Instance.launcher.currGameMode;
            mode?.SendMessage("PawnSpawned", pawnObj, SendMessageOptions.DontRequireReceiver);
        }

        public void SetBossStatusController(string pawnTag) 
        {
            GameObject tagObj = TaggerSystem.FindGameObjectWithTag(pawnTag);
            if (tagObj == null)
                return;

            var brain = tagObj.GetComponent<PawnBrainController>();
            if (brain == null)
                return;

            new BossStatusBarController(brain).Load(GameObject.FindFirstObjectByType<BossStatusBarTemplate>()).Show(GameContext.Instance.canvasManager.body.transform as RectTransform);
        }

        public void SetActive(string tag, bool isActive) 
        {
            var obj = TaggerSystem.FindGameObjectWithTag(tag);
            if (obj == null)
            {
                Debug.Log("YarnCommand: Not Found Object, " + isActive);
                return;
            }
            obj.SetActive(isActive);
        }

        public void SendMessage(string tag, string msg) 
        {
            var obj = TaggerSystem.FindGameObjectWithTag(tag);
            if (obj == null)
            {
                Debug.Log("YarnCommand: Not Found Object, " + msg);
                return;
            }
            obj.SendMessage(msg, SendMessageOptions.DontRequireReceiver);
        }
        public void SetLightIntensity(string tag, float intensity)
        {
            var obj = TaggerSystem.FindGameObjectWithTag(tag);
            if (obj == null)
            {
                Debug.Log("YarnCommand: Not Found Object, " + tag);
                return;
            }
            Light light = obj.GetComponent<Light>();
            if (light != null)
                light.intensity = intensity;
        }

        public void SendMessageToGameMode(string msg) 
        {
            var mode = GameContext.Instance.launcher.currGameMode;
            if (mode == null)
                return;

            mode.SendMessage(msg, SendMessageOptions.DontRequireReceiver);
        }
        public void SendMessageToGameModeParam(string msg, string param)
        {
            var mode = GameContext.Instance.launcher.currGameMode;
            if (mode == null)
                return;

            mode.SendMessage(msg, param, SendMessageOptions.DontRequireReceiver);
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

        public void SetConfinerVolume(string volumeTag) 
        {
            //* 카메라 이동 영역 셋팅
            var confinerBoundingBox = TaggerSystem.FindGameObjectWithTag(volumeTag).GetComponent<BoxCollider>();
            if (confinerBoundingBox != null)
                GameContext.Instance.cameraCtrler.RefreshConfinerVolume(confinerBoundingBox, Quaternion.Euler(45f, 45f, 0f), 10f);
        }

        public void ShowMessagePopup(string title, string text, float showDuration) 
        {
            Debug.Log("showMessagePopup" + title + text);
            new MessagePopupController(title, text, showDuration).Load().ShowDimmed(GameContext.Instance.canvasManager.dimmed.transform as RectTransform);
        }
        public IEnumerator ShowGuidePopup(string title, string text)
        {
            Debug.Log("showGuidePopup" + title + text);
            Time.timeScale = 0.1f;
            _isWaitCheck = false;

            new GuidePopupController(title, text, this).Load().ShowDimmed(GameContext.Instance.canvasManager.dimmed.transform as RectTransform);

            yield return new WaitUntil(() => { return _isWaitCheck; });
        }

        public void PlayTimeline(string timelineName)
        {
            Debug.Log("YarnCommand PlayTimeline: " + timelineName);
            TimelineManager.Instance.PlayTimelineByName(timelineName);
        }

        public void LoadTimeline(string timelineName)
        {
            Debug.Log("YarnCommand LoadTimeline: " + timelineName);
            TimelineManager.Instance.LoadTimeline(timelineName);
        }

        public void PlayLoadedTimeline()
        {
            Debug.Log("YarnCommand PlayLoadedTimeline");
            TimelineManager.Instance.Play();
        }

        public void BindAnimatorToTrack(string tagName, int trackIndex)
        {
            Debug.Log($"YarnCommand BindAnimatorToTrack: {tagName} -> Track {trackIndex}");
            TimelineManager.Instance.BindAnimatorToTrack(tagName, trackIndex);
        }

        public void BindAnimatorToTrackByName(string tagName, string trackName)
        {
            Debug.Log($"YarnCommand BindAnimatorToTrackByName: {tagName} -> Track '{trackName}'");
            TimelineManager.Instance.BindAnimatorToTrackByName(tagName, trackName);
        }
    }
}