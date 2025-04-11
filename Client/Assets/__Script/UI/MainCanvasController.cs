using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class CanvasManager : MonoBehaviour
    {
        public Canvas overlay;
        public Canvas body;
        public Canvas fade;
        public Canvas dimmed;

        void Awake()
        {
            Debug.Assert(overlay != null);
            Debug.Assert(fade != null);
            Debug.Assert(body != null);
            Debug.Assert(dimmed != null);
        }

        public void FadeInImmediately(Color color)
        {
            __fadeDisposable?.Dispose();
            __fadeDisposable = null;

            fade.gameObject.SetActive(true);
            fade.GetComponent<RawImage>().color = color;
        }

        public void FadeOutImmediately()
        {
            __fadeDisposable?.Dispose();
            __fadeDisposable = null;

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

            var fill = fade.GetComponent<RawImage>();
            var fromColor = fill.color;
            var fromTimeStamp = Time.timeSinceLevelLoad;

            return Observable.EveryUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(duration)))
                .Do(_ => fill.color = Color.Lerp(fromColor, color, Mathf.Clamp01((Time.timeSinceLevelLoad - fromTimeStamp) / duration)))
                .DoOnCompleted(() => fill.color = color);
        }

        IObservable<long> FadeOutAsObservable(float duration)
        {
            __fadeDisposable?.Dispose();
            __fadeDisposable = null;

            var fill = fade.GetComponent<RawImage>();
            var fromColor = fill.color;
            var fromTimeStamp = Time.timeSinceLevelLoad;
            
            return Observable.EveryUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(duration)))
                .Do(_ => fill.color = Color.Lerp(fromColor, Color.clear, Mathf.Clamp01((Time.timeSinceLevelLoad - fromTimeStamp) / duration)))
                .DoOnCompleted(() => fade.gameObject.SetActive(false));
        }
    }
}
