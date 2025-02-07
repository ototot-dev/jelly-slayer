using System.Linq;
using UnityEngine;
using UniRx;
using UGUI.Rx;
using Unity.Linq;
using System;

namespace Game
{
    /// <summary>
    /// App 엔트리 포인트
    /// </summary>
    public class Launcher : MonoBehaviour
    {
        public enum GameModes
        {
            None = 0,
            Default,
            BattleTest,
            GameTest,
            Game,
            Tutorial,
            Title,
        }

        public GameModes _gameMode = GameModes.None;

        [Header("Level")]
        public GameObject hackerDen;
        public GameObject shootingRange;
        public GameObject trainingRoom;

        public GameObject[] _objTitleList;
        public GameObject[] _objGameList;
        public GameObject[] _objTutorialList;

        [Space(10)]
        public FloatReactiveProperty timeScale = new(1);

        void Start()
        {
            InitManager.Initialize();
        }

        public void SetMode(GameModes mode) 
        {
            _gameMode = mode;
            switch (_gameMode)
            {
                case GameModes.Default:
                    {
                        GameContext.Instance.playerCtrler = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
                        GameContext.Instance.cameraCtrler = GameObject.FindWithTag("MainCamera").GetComponent<CameraController>();
                        GameContext.Instance.terrainManager = GameObject.FindWithTag("TerrainManager").GetComponent<TerrainManager>();
                        GameContext.Instance.jellySpawnManager = gameObject.Children().First(c => c.name == "Manager").GetComponent<SlimeSpawnManager>();
                        GameContext.Instance.heroSpawnManager = gameObject.Children().First(c => c.name == "Manager").GetComponent<HeroSpawnManager>();
                        GameContext.Instance.mainCanvasCtrler = GameObject.FindWithTag("MainCanvas").GetComponent<MainCanvasController>();

                        new TitleController().Load().Show(GameContext.Instance.mainCanvasCtrler.body);
                    }
                    break;
                case GameModes.BattleTest:
                    {
                        GameContext.Instance.playerCtrler = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
                        GameContext.Instance.playerTargetManager = GameContext.Instance.playerCtrler.GetComponent<TargetingController>();
                        GameContext.Instance.cameraCtrler = GameObject.FindWithTag("MainCamera").GetComponent<CameraController>();
                    }
                    break;
                case GameModes.GameTest:
                    {
                        GameContext.Instance.playerCtrler = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
                        GameContext.Instance.cameraCtrler = GameObject.FindWithTag("MainCamera").GetComponent<CameraController>();

                        GameManager.Instance.ShowLevel_HackerDen(true);
                        Observable.Timer(TimeSpan.FromSeconds(1f)).Subscribe(_ => GameManager.Instance.SpawnHero());
                        Observable.Timer(TimeSpan.FromSeconds(1.1f)).Subscribe(_ => GameManager.Instance.SpawnDroneBot());
                        Observable.Timer(TimeSpan.FromSeconds(1.2f)).Subscribe(_ => GameManager.Instance.SpawnSoldier());
                        // Observable.Timer(TimeSpan.FromSeconds(1.2f)).Subscribe(_ => GameManager.Instance.SpawnEtasphera42());
                    }
                    break;
                case GameModes.Game:
                    {
                        if (GameContext.Instance.playerCtrler == null)
                            GameContext.Instance.playerCtrler = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
                        if (GameContext.Instance.cameraCtrler == null)
                            GameContext.Instance.cameraCtrler = GameObject.FindWithTag("MainCamera").GetComponent<CameraController>();

                        GameManager.Instance.Activate_Game(true);
                        GameManager.Instance.ShowLevel_ShootingRange(true);

                        Observable.Timer(TimeSpan.FromSeconds(1f)).Subscribe(_ => GameManager.Instance.SpawnHero());
                        Observable.Timer(TimeSpan.FromSeconds(1.1f)).Subscribe(_ => GameManager.Instance.SpawnDroneBot());
                        Observable.Timer(TimeSpan.FromSeconds(1.2f)).Subscribe(_ => GameManager.Instance.SpawnEtasphera42());
                    }
                    break;
                case GameModes.Tutorial:
                    {
                        if (GameContext.Instance.playerCtrler == null)
                            GameContext.Instance.playerCtrler = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
                        if (GameContext.Instance.cameraCtrler == null)
                            GameContext.Instance.cameraCtrler = GameObject.FindWithTag("MainCamera").GetComponent<CameraController>();

                        GameManager.Instance.Activate_Tutorial(true);
                        GameManager.Instance.ShowLevel_TrainingRoom(true);

                        Observable.Timer(TimeSpan.FromSeconds(1f)).Subscribe(_ => GameManager.Instance.SpawnHero());
                        Observable.Timer(TimeSpan.FromSeconds(1.1f)).Subscribe(_ => GameManager.Instance.SpawnDroneBot());
                        Observable.Timer(TimeSpan.FromSeconds(1.2f)).Subscribe(_ => GameManager.Instance.SpawnSoldier());
                    }
                    break;
                case GameModes.Title:
                    {
                        GameManager.Instance.Activate_Title(true);
                    }
                    break;
            }
            // timeScale.Subscribe(v => Time.timeScale = v);
        }
    }
}