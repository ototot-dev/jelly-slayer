using System;
using UnityEngine;
using UniRx;
using System.Linq;

namespace Game
{    
    public class RoboSoldierLaserRenderer : MonoBehaviour
    {
        [Header("Component")]
        public LineRenderer lineRenderer;
        public ParticleSystem flashFx;
        public ParticleSystem hitFx;

        [Header("Parameter")]
        public LayerMask hitLayerMask;
        public float hitOffset = 0.1f;
        public float lineWidth = 0.1f;
        public float mainTextureLength = 1f;
        Vector2 __mainTextureTiling = new(1f, 1f);

        [Header("Debug")]
        public BoolReactiveProperty debug_OnOff = new();

        public void FadeIn(float lineWidth, float duration)
        {
            this.lineWidth = lineWidth;
            FadeIn(duration);
        }

        public void FadeIn(float duration)
        {
            if (duration <= 0f)
            {
                lineRenderer.enabled = true;
                lineRenderer.startWidth = lineRenderer.endWidth = lineWidth;
                return;
            }

            var fadeTimeStamp = Time.time;

            __fadeDisposable?.Dispose();
            __fadeDisposable = Observable.EveryUpdate().Subscribe(_ =>
            {
                if (!lineRenderer.enabled) lineRenderer.enabled = true;

                lineRenderer.startWidth = lineRenderer.endWidth = Mathf.Max(lineRenderer.endWidth, Mathf.Lerp(0f, lineWidth, (Time.time - fadeTimeStamp) / duration));
                if (lineRenderer.startWidth >= lineWidth)
                {
                    __fadeDisposable.Dispose();
                    __fadeDisposable = null;
                }
            }).AddTo(this);
        }

        public void FadeOut(float duration)
        {
            if (duration <= 0f)
            {
                lineRenderer.startWidth = lineRenderer.endWidth = 0f;
                lineRenderer.enabled = false;
                return;
            }

            var fadeTimeStamp = Time.time;

            __fadeDisposable?.Dispose();
            __fadeDisposable = Observable.EveryUpdate().Subscribe(_ =>
            {
                lineRenderer.startWidth = lineRenderer.endWidth = Mathf.Min(lineRenderer.endWidth, Mathf.Lerp(lineWidth, 0f, (Time.time - fadeTimeStamp) / duration));
                if (lineRenderer.startWidth <= 0f)
                {
                    lineRenderer.enabled = false;
                    __fadeDisposable.Dispose();
                    __fadeDisposable = null;
                }
            }).AddTo(this);
        }

        IDisposable __fadeDisposable;

        void Awake()
        {
            debug_OnOff.Skip(1).Subscribe(v =>
            {
                if (v) FadeIn(0.2f);
                else FadeOut(0.2f);
            }).AddTo(this);
        }

        void Start()
        {
            FadeOut(0.1f);
        }
        
        void LateUpdate()
        {
            if (!lineRenderer.enabled)
                return;

            __mainTextureTiling.x = mainTextureLength * Vector3.Distance(transform.position, hitFx.transform.position);
            lineRenderer.material.SetTextureScale("_MainTex", __mainTextureTiling);
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, hitFx.transform.position);
        }
    }
}
