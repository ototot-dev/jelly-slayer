using UnityEngine;
using UniRx;
using UGUI.Rx;
using ZLinq;
using System;
using Finalfactory.Tagger;

namespace Game
{
    public class TutorialGameMode : BaseGameMode
    {
        public override GameModeTypes GetGameModeType() => GameModeTypes.Tutorial;
        public override DialogueDispatcher GetDialogueDispatcher() => __dialogueDispatcher;
        TutorialDialogueDispatcher __dialogueDispatcher;

        public override bool CanPlayerConsumeInput()
        {
            return !__dialogueDispatcher.IsDialogueRunning();
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
                var spawnPoint = GameObject.FindGameObjectsWithTag("SpawnPoint").AsValueEnumerable().First(v => v.name == "PlayerSpawnPoint").transform;

                GameContext.Instance.playerCtrler.SpawnSlayerPawn(true);
                GameContext.Instance.playerCtrler.possessedBrain.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
                GameContext.Instance.playerCtrler.possessedBrain.TryGetComponent<SlayerAnimController>(out var animCtrler);

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
    }
}