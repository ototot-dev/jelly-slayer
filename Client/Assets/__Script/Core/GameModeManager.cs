using Game;
using UGUI.Rx;
using UniRx;
using UnityEngine;

namespace Game
{
    public enum GameModes
    {
        None = 0,
        ActionTest,
        BattleTest,
        Tutorial,
        Intro,
        Game,
    }

    public class GameModeManager : MonoSingleton<GameModeManager>
    {
        public void ChangeMode(GameModes newMode)
        {
            if (newMode == GameModes.Intro)
                new IntroController().Load(GameObject.Find("intro").GetComponent<Template>()).Show(GameContext.Instance.CanvasManager.body.transform as RectTransform);
            else if (newMode == GameModes.Tutorial)
            {
                var loadingPageCtrler = new LoadingPageController().Load().ShowDimmed(GameContext.Instance.CanvasManager.dimmed.transform as RectTransform);
                new TutorialController().LoadAsObservable().Subscribe(tutorialCtrler =>
                {
                    loadingPageCtrler.HideDimmedAsObservable().Subscribe(_ =>
                    {
                        tutorialCtrler.Show(GameContext.Instance.CanvasManager.body.transform as RectTransform);
                    }).AddTo(this);
                }).AddTo(this);
            }
            else if (newMode == GameModes.BattleTest)
                new BattleTestModeController().Load().Show(GameObject.FindWithTag("MainCanvas").transform as RectTransform);
        }
    }
}
