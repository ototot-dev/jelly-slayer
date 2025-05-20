using System;
using System.Collections.Generic;
using UnityEngine;
using UGUI.Rx;

namespace Game
{
    public class BattleTestGameMode : BaseGameMode
    {
        public override bool IsInCombat() => true;

        void Start()
        {
            GameContext.Instance.playerCtrler.Possess(GameObject.FindWithTag("Hero").GetComponent<SlayerBrain>());
            new GamePanelController().Load().Show(GameContext.Instance.canvasManager.body.transform as RectTransform);
        }
    }
}