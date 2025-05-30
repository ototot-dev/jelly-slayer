using UniRx;
using UGUI.Rx;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace Game
{
    [Template(path: "UI/loading-page")]
    public class LoadingPageController : Controller
    {
        public string[] resourcePaths;
        public string[] scenePaths;
        public ReactiveCollection<UnityEngine.Object> loadedObjects = new();
        public IntReactiveProperty loadedSceneCount = new();
        public FloatReactiveProperty loadingPregress = new();
        public Action onLoadingCompleted;

        public LoadingPageController(string[] resourcePaths, string[] scenePaths)
        {
            Debug.Assert(resourcePaths != null && scenePaths != null);

            this.resourcePaths = resourcePaths;
            this.scenePaths = scenePaths;
        }

        public override void OnPostShow()
        {
            base.OnPostShow();

            IObservable<AsyncOperation[]> loadingAll = null;

            //* 비동기 로딩 태스크 셋팅
            if (resourcePaths != null && scenePaths != null)
            {
                var resourceLoading = resourcePaths.Select(p => Resources.LoadAsync(p).AsObservable().Do(a => loadedObjects.Add((a as ResourceRequest).asset)));
                var sceneLoading = scenePaths.Select(p => SceneManager.LoadSceneAsync(p, LoadSceneMode.Additive).AsObservable().Do(_ => loadedSceneCount.Value++));
                loadingAll = Observable.WhenAll(resourceLoading.Concat(sceneLoading));
            }
            else if (resourcePaths != null)
            {
                var resourceLoading = resourcePaths.Select(p => Resources.LoadAsync(p).AsObservable().Do(a => loadedObjects.Add((a as ResourceRequest).asset)));
                loadingAll = Observable.WhenAll(resourceLoading);
            }
            else if (scenePaths != null)
            {
                var sceneLoading = scenePaths.Select(p => SceneManager.LoadSceneAsync(p, LoadSceneMode.Additive).AsObservable().Do(_ => loadedSceneCount.Value++));
                loadingAll = Observable.WhenAll(sceneLoading);
            }

            //* 로딩 시작
            loadingAll.Subscribe(_ => onLoadingCompleted?.Invoke()).AddToHide(this);

            //* 로딩 프로그레스 (리소스 로딩 시)
            loadedObjects.ObserveAdd().Subscribe(v =>
            {
                loadingPregress.Value = (loadedObjects.Count + loadedSceneCount.Value) / Mathf.Min(1f, (float)(resourcePaths?.Length ?? 0f + scenePaths?.Length ?? 0f));
            }).AddToHide(this);

            //* 로딩 프로그레스 (Scene 로딩 시)
            loadedSceneCount.Subscribe(_ =>
            {
                loadingPregress.Value = (loadedObjects.Count + loadedSceneCount.Value) / Mathf.Min(1f, (float)(resourcePaths?.Length ?? 0f + scenePaths?.Length ?? 0f));
            }).AddToHide(this);
        }
    }
}