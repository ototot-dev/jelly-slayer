using System;
using System.Collections.Generic;
using UnityEngine;
using UGUI.Rx;
using Game.UI;
using NUnit.Framework.Constraints;

namespace Game
{
    public class BattleTestGameMode : BaseGameMode
    {
        public override bool IsInCombat() => true;

        void Start()
        {

            GameContext.Instance.playerCtrler.Possess(TaggerSystem.FindGameObjectWithTags(Finalfactory.Tagger.TaggerSearchMode.And, Tags.Pawn.Slayer, Tags.Ungrouped.K).GetComponent<SlayerBrain>());
            // new GamePanelController().Load().Show(GameContext.Instance.canvasManager.body.transform as RectTransform);

            new PlayerStatusBarController().Load(GameObject.FindFirstObjectByType<PlayerStatusBarTemplate>()).Show(GameContext.Instance.canvasManager.body.transform as RectTransform);
            new BossStatusBarController(GameContext.Instance.playerCtrler.possessedBrain).Load(GameObject.FindFirstObjectByType<BossStatusBarTemplate>()).Show(GameContext.Instance.canvasManager.body.transform as RectTransform);
            new FloatingStatusBarController(GameContext.Instance.playerCtrler.possessedBrain).Load(GameObject.FindFirstObjectByType<FloatingStatusBarTemplate>()).Show(GameContext.Instance.canvasManager.body.transform as RectTransform);

            var confinerBoundingBox = TaggerSystem.FindGameObjectWithTag("ConfinerBoundingBox").GetComponent<BoxCollider>();
            if (confinerBoundingBox != null)
                GameContext.Instance.cameraCtrler.RefreshConfinerVolume(confinerBoundingBox, Quaternion.Euler(45f, 45f, 0f), 10f);
        }
    }
}