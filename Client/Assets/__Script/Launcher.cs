using System.Linq;
using UnityEngine;
using UniRx;
using UGUI.Rx;
using Unity.Linq;

namespace Game
{
    /// <summary>
    /// App 엔트리 포인트
    /// </summary>
    public class Launcher : MonoBehaviour
    {
        public enum GameModes
        {
            Default,
            Test,
            Game,
        }
        public GameModes gameMode = GameModes.Default;
        public FloatReactiveProperty timeScale = new(1);

        void Start()
        {
            InitManager.Initialize();

            if (gameMode == GameModes.Default)
            {
                GameContext.Instance.playerCtrler = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
                GameContext.Instance.cameraCtrler = GameObject.FindWithTag("MainCamera").GetComponent<CameraController>();
                GameContext.Instance.terrainManager = GameObject.FindWithTag("TerrainManager").GetComponent<TerrainManager>();
                GameContext.Instance.jellySpawnManager = gameObject.Children().First(c => c.name == "Manager").GetComponent<SlimeSpawnManager>();
                GameContext.Instance.heroSpawnManager = gameObject.Children().First(c => c.name == "Manager").GetComponent<HeroSpawnManager>();
                GameContext.Instance.mainCanvasCtrler = GameObject.FindWithTag("MainCanvas").GetComponent<MainCanvasController>();

                new TitleController().Load().Show(GameContext.Instance.mainCanvasCtrler.body);
            }
            else if (gameMode == GameModes.Test)
            {
                GameContext.Instance.playerCtrler = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
                GameContext.Instance.playerTargetManager = GameContext.Instance.playerCtrler.GetComponent<PlayerTargetManager>();
                GameContext.Instance.cameraCtrler = GameObject.FindWithTag("MainCamera").GetComponent<CameraController>();
            }
            else if (gameMode == GameModes.Game)
            {
                GameContext.Instance.playerCtrler = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
                GameContext.Instance.playerTargetManager = GameContext.Instance.playerCtrler.GetComponent<PlayerTargetManager>();
                GameContext.Instance.cameraCtrler = GameObject.FindWithTag("MainCamera").GetComponent<CameraController>();
            }
            
            GameManager.Instance.CheckInstance();

            timeScale.Subscribe(v => Time.timeScale = v);
        }

    }

}