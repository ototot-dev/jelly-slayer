using UniRx;
using UGUI.Rx;
using System.Linq;
using System.Collections;
using UnityEngine;

namespace Game
{
    [Template(path: "UI/loading-page")]
    public class LoadingPageController : Controller
    {
        public BoolReactiveProperty isLoadingCompleted = new();

        public override void OnPostShow()
        {
            base.OnPostShow();

            // isLoadingCompleted.Where(v => v).Subscribe(_ =>
            // {
            //     this.HideAsObservable().Subscribe(_ =>
            //     {
            //         GameContext.Instance.CanvasManager.FadeOut(1);
            //         new GameOverlayController().Load().Show(GameContext.Instance.CanvasManager.body.transform as RectTransform);
            //     });
            // });

            template.StartCoroutine(Loading_Coroutine());
        }

        IEnumerator Loading_Coroutine()
        {
            //* 지형 생성
            // if (!GameContext.Instance.terrainManager.IsTerrainGenerated)
            // {
            //     GameContext.Instance.terrainManager.Generate();
            //     yield return new WaitForSeconds(0.5f);
            // }

            //* 슬라임 생성
            // GameContext.Instance.jellySpawnManager.Spawn(TerrainManager.GetTerrainPoint(Vector3.zero), 1, JellySlimeBlackboard.ActiveProjectiles.Ball);
            yield return new WaitForSeconds(0.5f);

            //* 영웅 생성
            // GameContext.Instance.playerCtrler.SpawnHeroPawn(Resources.Load<GameObject>("Pawn/Hero"), true).transform.position = TerrainManager.GetTerrainPoint(Vector3.zero);
            yield return new WaitForSeconds(0.1f);

            isLoadingCompleted.Value = true;
        }
    }
}