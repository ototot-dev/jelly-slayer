using UnityEngine;
using UniRx;
using UGUI.Rx;
using ZLinq;
using System;

namespace Game
{
    public class TutorialGameMode : BaseGameMode
    {
        public override GameModeTypes GetGameModeType() => GameModeTypes.Tutorial;
        public override DialogueRunnerDispatcher GetDialogueRunnerDispatcher() => __dialogueRunnerDispatcher;
        DialogueRunnerDispatcher __dialogueRunnerDispatcher;

        void Awake()
        {
            __dialogueRunnerDispatcher = gameObject.AddComponent<DialogueRunnerDispatcher>();
        }

        public override IObservable<Unit> EnterAsObservable()
        {
            GameContext.Instance.canvasManager.FadeInImmediately(Color.black);

            var loadingPageCtrler = new LoadingPageController(new string[] { "Pawn/Player/Slayer-K" }, new string[] { "Tutorial-0" });
            loadingPageCtrler.onLoadingCompleted += () =>
            {
                var spawnPoint = GameObject.FindGameObjectsWithTag("SpawnPoint").AsValueEnumerable().First(v => v.name == "PlayerSpawnPoint").transform;

                GameContext.Instance.playerCtrler.SpawnHeroPawn(Resources.Load<GameObject>("Pawn/Player/Slayer-K"), true);
                GameContext.Instance.playerCtrler.possessedBrain.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
                GameContext.Instance.playerCtrler.possessedBrain.TryGetComponent<SlayerAnimController>(out var animCtrler);

                // var found = TaggerSystem.FindGameObjectWithTag("Slayer");
                // if (found != null) __Logger.LogR1(template.gameObject, "FindGameObjectWithTag", "found", found);

                loadingPageCtrler.HideAsObservable().Subscribe(__ =>
                {
                    loadingPageCtrler.Unload();

                    new BubbleDialoqueController().Load(GameObject.Find("3d-bubble-dialogue").GetComponent<Template>()).Show(GameContext.Instance.canvasManager.body.transform as RectTransform);
                    __dialogueRunnerDispatcher.StartDialogue("Start");
                });
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