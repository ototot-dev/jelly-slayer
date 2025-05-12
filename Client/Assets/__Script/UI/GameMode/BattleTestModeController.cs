using System;
using System.Collections.Generic;
using UnityEngine;
using UGUI.Rx;

namespace Game
{
    [Template(path: "UI/template/battle-test-mode")]
    public class BattleTestModeController : Controller
    {
        public override void OnPreLoad(List<IObservable<Controller>> loader)
        {
            base.OnPreLoad(loader);

            // GameContext.Instance.playerCtrler = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
            // GameContext.Instance.cameraCtrler = GameObject.FindWithTag("MainCamera").GetComponent<CameraController>();

            // loader.Add(Resources.LoadAsync("BG").AsAsyncOperationObservable().Select(_ => this));
            // loader.Add(Resources.LoadAsync("Block Spritesheet - 1").AsAsyncOperationObservable().Select(_ => this));
        }

        public override void OnPreShow()
        {
            base.OnPreShow();

            GameContext.Instance.playerCtrler.Possess(GameObject.FindWithTag("Hero").GetComponent<SlayerBrain>());
            new GamePanelController().Load().Show(GameContext.Instance.CanvasManager.body.transform as RectTransform);
        }
    }
}