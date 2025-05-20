using UniRx;
using UGUI.Rx;
using System.Linq;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;
using Unity.VisualScripting;

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
            this.resourcePaths = resourcePaths;
            this.scenePaths = scenePaths;
        }

        public override void OnPostShow()
        {
            base.OnPostShow();

            var resourceLoading = resourcePaths.Select(p => Resources.LoadAsync(p).AsObservable().Do(a => loadedObjects.Add((a as ResourceRequest).asset)));
            var sceneLoading = scenePaths.Select(p => SceneManager.LoadSceneAsync(p, LoadSceneMode.Additive).AsObservable().Do(_ => loadedSceneCount.Value++));

            Observable.WhenAll(resourceLoading.Concat(sceneLoading)).Subscribe(_ =>
            {
                onLoadingCompleted?.Invoke();
            }).AddToHide(this);

            loadedObjects.ObserveAdd().Subscribe(v =>
            {
                loadingPregress.Value = (loadedObjects.Count + loadedSceneCount.Value) / Mathf.Min(1f, (float)(resourcePaths?.Length ?? 0f + scenePaths?.Length ?? 0f));
            }).AddToHide(this);

            loadedSceneCount.Subscribe(_ =>
            {
                loadingPregress.Value = (loadedObjects.Count + loadedSceneCount.Value) / Mathf.Min(1f, (float)(resourcePaths?.Length ?? 0f + scenePaths?.Length ?? 0f));
            }).AddToHide(this);
        }
    }
}