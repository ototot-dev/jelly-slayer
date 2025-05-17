using UnityEngine;
using UniRx;
using UGUI.Rx;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using ZLinq;

namespace Game
{
    [Template(path: "UI/template/tutorial-mode")]
    public class TutorialController : Controller
    {
        public override void OnPreLoad(List<IObservable<Controller>> loader)
        {
            GameContext.Instance.CanvasManager.FadeInImmediately(Color.black);

            loader.Add(SceneManager.LoadSceneAsync("Tutorial-0", LoadSceneMode.Additive).AsObservable().Select(_ => this));
            loader.Add(Resources.LoadAsync<GameObject>("Pawn/Player/Slayer-K").AsObservable().Do(p => __slayerPrefab = (p as ResourceRequest).asset as GameObject).Select(_ => this));

            base.OnPreLoad(loader);
        }

        GameObject __slayerPrefab;

        public override void OnPostLoad()
        {
            base.OnPostLoad();

            GameContext.Instance.dialogueRunnerDispatcher = template.GetComponent<DialogueRunnerDispatcher>();

            var spawnPoint = GameObject.FindGameObjectsWithTag("SpawnPoint").AsValueEnumerable().First(v => v.name == "PlayerSpawnPoint").transform;

            GameContext.Instance.playerCtrler.SpawnHeroPawn(__slayerPrefab, true);
            GameContext.Instance.playerCtrler.possessedBrain.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            GameContext.Instance.playerCtrler.possessedBrain.TryGetComponent<SlayerAnimController>(out var animCtrler);

            var found = TaggerSystem.FindGameObjectWithTag("Slayer");
            if (found != null) __Logger.LogR1(template.gameObject, "FindGameObjectWithTag", "found", found);
        }

        public override void OnPreShow()
        {
            base.OnPreShow();

            new BubbleDialoqueController().Load(GameObject.Find("3d-bubble-dialogue").GetComponent<Template>()).Show(GameContext.Instance.CanvasManager.body.transform as RectTransform);

            // Observable.Timer(TimeSpan.FromSeconds(4f)).Subscribe(_ => GameContext.Instance.CanvasManager.FadeOut(2f)).AddToHide(this);
        }

        public override void OnPostShow()
        {
            base.OnPostShow();

            GameContext.Instance.dialogueRunnerDispatcher.StartDialogue("Start");
        }
    }
}