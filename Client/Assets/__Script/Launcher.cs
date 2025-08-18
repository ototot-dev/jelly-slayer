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

        Intro,      // (미구현), 인트로 모드
        BaseCamp,   // 베이스 캠프
        Tutorial,   // 튜토리얼 모드
        Game,       // 일반 게임 모드

        // 테스트
        ActionTest, // (미구현)
        BattleTest, // 전투 테스트 모드
    }

    /// <summary>
    /// App 엔트리 포인트
    /// </summary>
    public class Launcher : MonoBehaviour
    {
        public BaseGameMode currGameMode;

        public bool IsGameModeChanging() => __changeGameModeDisposable != null;
        IDisposable __changeGameModeDisposable;

        public GameModeTypes startGameMode = GameModeTypes.None;
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

                case GameModeTypes.BaseCamp: ChangeGameMode<BaseCampMode>(); return;
                case GameModeTypes.Tutorial: ChangeGameMode<TutorialGameMode>(); return;
                case GameModeTypes.Game: ChangeGameMode<MissionGameMode>(); return;

                // 테스트
                case GameModeTypes.BattleTest: ChangeGameMode<BattleTestGameMode>(); return;
            }
        }
    }
}