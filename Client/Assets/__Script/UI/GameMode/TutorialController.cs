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
            loader.Add(SceneManager.LoadSceneAsync("Tutorial-0", LoadSceneMode.Additive).AsObservable().Select(_ => this));
            loader.Add(Resources.LoadAsync<GameObject>("Pawn/Player/Slayer-K").AsObservable().Do(p => __heroPrefab = (p as ResourceRequest).asset as GameObject).Select(_ => this));

            base.OnPreLoad(loader);
        }

        GameObject __heroPrefab;

        public override void OnPostLoad()
        {
            base.OnPostLoad();

            var spawnPoint = GameObject.FindGameObjectsWithTag("SpawnPoint").AsValueEnumerable().First(v => v.name == "PlayerSpawnPoint").transform;

            GameContext.Instance.playerCtrler.SpawnHeroPawn(__heroPrefab, true);
            GameContext.Instance.playerCtrler.possessedBrain.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            GameContext.Instance.playerCtrler.possessedBrain.TryGetComponent<SlayerAnimController>(out var animCtrler);
            // animCtrler.PlaySingleClip(, 0.1f);

            // template.StartCoroutine();

            var found = TaggerSystem.FindGameObjectWithTag("Slayer");
            if (found != null) __Logger.LogR1(template.gameObject, "FindGameObjectWithTag", "found", found);
        }

        // IEnumerator Loading_Coroutine()
        // {
        //     yield return null;
        // }

        public override void OnPreShow()
        {
            base.OnPreShow();
        }

        public override void OnPostShow()
        {
            base.OnPostShow();

            var bubbleDialogueCtrler = new BubbleDialoqueController().Load(GameObject.Find("3d-bubble-dialogue").GetComponent<Template>()).Show(GameContext.Instance.CanvasManager.body.transform as RectTransform);
            Observable.NextFrame().Subscribe(_ =>
            {
                bubbleDialogueCtrler.StartDialogue("Start");
            }).AddToHide(this);
        }
    }
}