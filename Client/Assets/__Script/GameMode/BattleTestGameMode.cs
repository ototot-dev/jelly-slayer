using System;
using System.Collections.Generic;
using UnityEngine;
using UGUI.Rx;
using Game.UI;

namespace Game
{
    public class BattleTestGameMode : BaseGameMode
    {
        public override bool IsInCombat() => true;

        void Start()
        {

            GameContext.Instance.playerCtrler.Possess(TaggerSystem.FindGameObjectWithTags( Finalfactory.Tagger.TaggerSearchMode.And, Tags.Pawn.Slayer, Tags.Ungrouped.K).GetComponent<SlayerBrain>());
            // new GamePanelController().Load().Show(GameContext.Instance.canvasManager.body.transform as RectTransform);

            new StatusBarController().Load(GameObject.FindFirstObjectByType<StatusBarTemplate>()).Show(GameContext.Instance.canvasManager.body.transform as RectTransform);

            var confinerBoundingBox = TaggerSystem.FindGameObjectWithTag("ConfinerBoundingBox").GetComponent<BoxCollider>();
            if (confinerBoundingBox != null)
                GameContext.Instance.cameraCtrler.RefreshConfinerVolume(confinerBoundingBox, Quaternion.Euler(45f, 45f, 0f), 10f);
        }
    }
}