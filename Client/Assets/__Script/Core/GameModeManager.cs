using Game;
using UGUI.Rx;
using UnityEngine;

namespace Game
{
    public enum GameModes
    {
        None = 0,
        ActionTest,
        BattleTest,
        Tutorial,
        Title,
        Game,
    }

    public class GameModeManager : MonoSingleton<GameModeManager>
    {
        public void ChangeMode(GameModes newMode)
        {
            if (newMode == GameModes.BattleTest)
                new BattleTestModeController().Load().Show(GameObject.FindWithTag("MainCanvas").transform as RectTransform);
        }
    }
}
