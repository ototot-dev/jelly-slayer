using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Yarn.Unity;

namespace Game
{
    public enum GameModeTypes
    {
        None = 0,
        ActionTest,
        BattleTest,
        Tutorial,
        Intro,
        Game,
    }

    /// <summary>
    /// App 엔트리 포인트
    /// </summary>
    public class Launcher : MonoBehaviour
    {
        public GameModeTypes startGameMode = GameModeTypes.None;
        public BaseGameMode currGameMode;
        public bool IsGameModeChanging() => __changeGameModeDisposable != null;
        IDisposable __changeGameModeDisposable;

        //public TutorialScene _tutorialStartScene;
        public int _tutorialStartMission;

        void Awake()
        {
            GameContext.Instance.launcher = this;
            GameContext.Instance.dialogueRunner = GetComponent<DialogueRunner>();
            GameContext.Instance.canvasManager = FindFirstObjectByType<CanvasManager>();
            GameContext.Instance.playerCtrler = FindFirstObjectByType<PlayerController>();
            GameContext.Instance.cameraCtrler = FindFirstObjectByType<CameraController>();
        }

        void Start()
        {
            DatasheetManager.Instance.Load();
            SoundManager.Instance.Init();

            if (startGameMode != GameModeTypes.None)
                ChangeGameMode(startGameMode);
        }

        public void ChangeGameMode<T>() where T : BaseGameMode
        {
            currGameMode = new GameObject().AddComponent<T>();
            currGameMode.name = currGameMode.GetGameModeType().ToString();
            currGameMode.transform.SetParent(transform, false);
            currGameMode.Enter();
        }

        public void ChangeGameMode(GameModeTypes newGameMode)
        {
            Debug.Assert(__changeGameModeDisposable == null);

            if (currGameMode != null)
            {
                __changeGameModeDisposable = currGameMode.ExitAsObservable().Subscribe(_ =>
                {
                    __changeGameModeDisposable = null;
                    ChangeGameModeInternal(newGameMode);
                });
            }
            else
            {
                ChangeGameModeInternal(newGameMode);
            }
        }

        void ChangeGameModeInternal(GameModeTypes newGameMode)
        {
            switch (newGameMode)
            {
                case GameModeTypes.Intro: return;
                case GameModeTypes.BattleTest: ChangeGameMode<BattleTestGameMode>(); return;
                case GameModeTypes.Tutorial: ChangeGameMode<TutorialGameMode>(); return;
            }
        }
    }
}