using UnityEngine;
using UniRx;
using UGUI.Rx;
using System;
using Yarn.Unity;

namespace Game
{
    public class TutorialGameMode : BaseGameMode
    {
        public override GameModeTypes GetGameModeType() => GameModeTypes.Tutorial;
        public override DialogueDispatcher GetDialogueDispatcher() => __dialogueDispatcher;
        TutorialDialogueDispatcher __dialogueDispatcher;

        public override bool CanPlayerConsumeInput()
        {
            return !__dialogueDispatcher.IsDialogueRunning() || (GameContext.Instance.playerCtrler.interactionKeyCtrler != null && GameContext.Instance.playerCtrler.interactionKeyCtrler.commandName == "RunLine");
        }

        void Awake()
        {
            __dialogueDispatcher = gameObject.AddComponent<TutorialDialogueDispatcher>();
        }

        public override IObservable<Unit> EnterAsObservable()
        {
            GameContext.Instance.canvasManager.FadeInImmediately(Color.black);

            var loadingPageCtrler = new LoadingPageController(new string[] { "Pawn/Player/Slayer-K", "Pawn/Player/DrontBot" }, new string[] { "Tutorial-0" });
            loadingPageCtrler.onLoadingCompleted += () =>
            {
                var spawnPoint = TaggerSystem.FindGameObjectWithTag("PlayerSpawnPoint").transform;

                GameContext.Instance.playerCtrler.SpawnSlayerPawn(true);
                GameContext.Instance.playerCtrler.possessedBrain.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
                GameContext.Instance.playerCtrler.possessedBrain.TryGetComponent<SlayerAnimController>(out var animCtrler);

                GameContext.Instance.cameraCtrler.virtualCamera.LookAt = GameContext.Instance.playerCtrler.possessedBrain.coreColliderHelper.transform;
                GameContext.Instance.cameraCtrler.virtualCamera.Follow = GameContext.Instance.playerCtrler.possessedBrain.coreColliderHelper.transform;

                var confinerBoundingBox = TaggerSystem.FindGameObjectWithTag("ConfinerBoundingBox").GetComponent<BoxCollider>();
                if (confinerBoundingBox != null)
                    GameContext.Instance.cameraCtrler.RefreshConfinerVolume(confinerBoundingBox, Quaternion.Euler(45f, 45f, 0f), 10f);

                // Matrix4x4.TRS(Vector3.zero, Quaternion.Euler())



                // var found = TaggerSystem.FindGameObjectWithTag("Slayer");
                // if (found != null) __Logger.LogR1(template.gameObject, "FindGameObjectWithTag", "found", found);

                loadingPageCtrler.HideAsObservable().Subscribe(__ =>
                {
                    loadingPageCtrler.Unload();

                    new BubbleDialoqueController().Load(GameObject.Find("3d-bubble-dialogue").GetComponent<Template>()).Show(GameContext.Instance.canvasManager.body.transform as RectTransform);
                    __dialogueDispatcher.StartDialogue("Start");
                });

                // var found = TaggerSystem.FindGameObjectWithTags(TaggerSearchMode.And, "Prop", "Phone");
                // __Logger.LogR1(gameObject, "FindGameObjectWithTags", "found", found);
            };

            loadingPageCtrler.Load().Show(GameContext.Instance.canvasManager.dimmed.transform as RectTransform);

            return Observable.NextFrame();
        }

        public override IObservable<Unit> ExitAsObservable()
        {
            return Observable.NextFrame();
        }

        void Update()
        {
            //* 다이얼로그 진행 중에 AnyKeyDown 처리
            if (__dialogueDispatcher.IsDialogueRunning() && Input.anyKeyDown && !CanPlayerConsumeInput())
            {
                foreach (var v in GameContext.Instance.dialogueRunner.dialogueViews)
                {
                    if (v is LineView lineView)
                        lineView.UserRequestedViewAdvancement();
                }
            }
        }
    }
}