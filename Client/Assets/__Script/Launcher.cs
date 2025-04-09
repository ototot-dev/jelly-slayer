using UnityEngine;

namespace Game
{
    /// <summary>
    /// App 엔트리 포인트
    /// </summary>
    public class Launcher : MonoBehaviour
    {
        public GameModes startGameMode = GameModes.None;

        void Start()
        {
            DatasheetManager.Instance.Load();
            SoundManager.Instance.Init();

            if (startGameMode != GameModes.None)
                GameModeManager.Instance.ChangeMode(startGameMode);
        }

        // public void SetMode(GameModes mode) 
        // {
        //     startGameMode = mode;
        //     switch (startGameMode)
        //     {
        //         case GameModes.Default:
        //             {
        //                 GameContext.Instance.playerCtrler = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        //                 GameContext.Instance.mainCameraCtrler = GameObject.FindWithTag("MainCamera").GetComponent<CameraController>();
        //                 GameContext.Instance.terrainManager = GameObject.FindWithTag("TerrainManager").GetComponent<TerrainManager>();
        //                 GameContext.Instance.jellySpawnManager = gameObject.Children().First(c => c.name == "Manager").GetComponent<SlimeSpawnManager>();
        //                 GameContext.Instance.heroSpawnManager = gameObject.Children().First(c => c.name == "Manager").GetComponent<HeroSpawnManager>();
        //                 GameContext.Instance.mainCanvasCtrler = GameObject.FindWithTag("MainCanvas").GetComponent<CanvasController>();

        //                 new TitleController().Load().Show(GameContext.Instance.mainCanvasCtrler.body);
        //             }
        //             break;
        //         case GameModes.BattleTest:
        //             {
        //                 GameContext.Instance.playerCtrler = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        //                 GameContext.Instance.playerTargetManager = GameContext.Instance.playerCtrler.GetComponent<TargetingController>();
        //                 GameContext.Instance.mainCameraCtrler = GameObject.FindWithTag("MainCamera").GetComponent<CameraController>();
        //                 GameContext.Instance.mainCanvasCtrler = GameObject.FindWithTag("MainCanvas").GetComponent<CanvasController>();
        //             }
        //             break;
        //         case GameModes.GameTest:
        //             {
        //                 GameContext.Instance.playerCtrler = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        //                 GameContext.Instance.mainCameraCtrler = GameObject.FindWithTag("MainCamera").GetComponent<CameraController>();

        //                 GameManager.Instance.ShowLevel_HackerDen(true);
        //                 Observable.Timer(TimeSpan.FromSeconds(1f)).Subscribe(_ => GameManager.Instance.SpawnHero(new UnityEngine.Vector3(-2, 0, -3)));
        //                 //Observable.Timer(TimeSpan.FromSeconds(1.1f)).Subscribe(_ => GameManager.Instance.SpawnDroneBot());
        //                 //Observable.Timer(TimeSpan.FromSeconds(1.2f)).Subscribe(_ => GameManager.Instance.SpawnSoldier());
        //                 // Observable.Timer(TimeSpan.FromSeconds(1.2f)).Subscribe(_ => GameManager.Instance.SpawnEtasphera42());
        //             }
        //             break;
        //         case GameModes.Game:
        //             {
        //                 if (GameContext.Instance.playerCtrler == null)
        //                     GameContext.Instance.playerCtrler = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        //                 if (GameContext.Instance.mainCameraCtrler == null)
        //                     GameContext.Instance.mainCameraCtrler = GameObject.FindWithTag("MainCamera").GetComponent<CameraController>();

        //                 GameManager.Instance.Activate_Game(true);
        //                 GameManager.Instance.ShowLevel_ShootingRange(true);

        //                 SoundManager.Instance.PlayBGM(SoundID.BGM_GAME);

        //                 GameManager.Instance.StartGame();
        //                 /*
        //                 Observable.Timer(TimeSpan.FromSeconds(1f)).Subscribe(_ => GameManager.Instance.SpawnHero(new UnityEngine.Vector3(-2, 0, -3)));
        //                 Observable.Timer(TimeSpan.FromSeconds(1.1f)).Subscribe(_ => GameManager.Instance.SpawnDroneBot());
        //                 Observable.Timer(TimeSpan.FromSeconds(2.0f)).Subscribe(_ => GameManager.Instance.SpawnSoldier());
        //                 Observable.Timer(TimeSpan.FromSeconds(3.0f)).Subscribe(_ => GameManager.Instance.SpawnDroid(UnityEngine.Vector3.right * 5));
        //                 Observable.Timer(TimeSpan.FromSeconds(4.0f)).Subscribe(_ => GameManager.Instance.SpawnDroid(UnityEngine.Vector3.left * 5));
        //                 */
        //                 // Observable.Timer(TimeSpan.FromSeconds(2.0f)).Subscribe(_ => GameManager.Instance.SpawnEtasphera42());
        //             }
        //             break;
        //         case GameModes.Tutorial:
        //             {
        //                 if (GameContext.Instance.playerCtrler == null)
        //                     GameContext.Instance.playerCtrler = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        //                 if (GameContext.Instance.mainCameraCtrler == null)
        //                     GameContext.Instance.mainCameraCtrler = GameObject.FindWithTag("MainCamera").GetComponent<CameraController>();

        //                 GameManager.Instance.Activate_Tutorial(true);
        //                 GameManager.Instance.ShowLevel_HackerDen(true);

        //                 Observable.Timer(TimeSpan.FromSeconds(1f)).Subscribe(_ => GameManager.Instance.SpawnHero(new UnityEngine.Vector3(-2, 0, -3)));
        //                 //Observable.Timer(TimeSpan.FromSeconds(1.1f)).Subscribe(_ => GameManager.Instance.SpawnDroneBot());
        //                 //Observable.Timer(TimeSpan.FromSeconds(2.0f)).Subscribe(_ => GameManager.Instance.SpawnSoldier());
        //             }
        //             break;
        //         case GameModes.Title:
        //             {
        //                 GameManager.Instance.Activate_Title(true);

        //                 SoundManager.Instance.PlayBGM(SoundID.BGM_TITLE);
        //             }
        //             break;
        //     }
        //     // timeScale.Subscribe(v => Time.timeScale = v);
        // }
    }
}