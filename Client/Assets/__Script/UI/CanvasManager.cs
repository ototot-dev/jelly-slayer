using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class CanvasManager : MonoBehaviour
    {
        public CanvasGroup overlay;
        public CanvasGroup body;
        public CanvasGroup fade;
        public CanvasGroup dimmed;

        void Awake()
        {
            Debug.Assert(overlay != null);
            Debug.Assert(fade != null);
            Debug.Assert(body != null);
            Debug.Assert(dimmed != null);

            fade.gameObject.SetActive(false);
            dimmed.gameObject.SetActive(false);
        }

        public void FadeInImmediately(Color color)
        {
            __fadeDisposable?.Dispose();
            __fadeDisposable = null;

            fade.alpha = 1f;
            fade.gameObject.SetActive(true);
            fade.GetComponent<RawImage>().color = color;
        }

        public void FadeOutImmediately()
        {
            __fadeDisposable?.Dispose();
            __fadeDisposable = null;

            fade.alpha = 0f;
            fade.gameObject.SetActive(false);
            fade.GetComponent<RawImage>().color = Color.clear;
        }

        IDisposable __fadeDisposable;

        public void FadeIn(Color color, float duration)
        {
            __fadeDisposable = FadeInAsObservable(color, duration).Subscribe().AddTo(this);
        }

        public void FadeOut(float duration)
        {
            __fadeDisposable = FadeOutAsObservable(duration).Subscribe().AddTo(this);
        }

        IObservable<long> FadeInAsObservable(Color color, float duration)
        {
            __fadeDisposable?.Dispose();
            __fadeDisposable = null;

            fade.gameObject.SetActive(true);
            fade.GetComponent<RawImage>().color = color;

            var fromAlpha = fade.alpha;
            var fromTimeStamp = Time.time;

            return Observable.EveryUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(duration)))
                .Do(_ => fade.alpha = Mathf.Lerp(fromAlpha, 1f, Mathf.Clamp01((Time.time - fromTimeStamp) / duration)))
                .DoOnCompleted(() => fade.alpha = 1f);
        }

        IObservable<long> FadeOutAsObservable(float duration)
        {
            __fadeDisposable?.Dispose();
            __fadeDisposable = null;

            var fromAlpha = fade.alpha;
            var fromTimeStamp = Time.time;
            
            return Observable.EveryUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(duration)))
                .Do(_ => fade.alpha = Mathf.Lerp(fromAlpha, 0f, Mathf.Clamp01((Time.timeSinceLevelLoad - fromTimeStamp) / duration)))
                .DoOnCompleted(() =>
                {
                    fade.alpha = 0f;
                    fade.gameObject.SetActive(false);
                });
        }
    }
}
