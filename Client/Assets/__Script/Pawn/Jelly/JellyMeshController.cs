using System;
using System.Linq;
using FIMSpace.FEyes;
using FIMSpace.FTail;
using ParadoxNotion.Animation;
using UniRx;
using Unity.Linq;
using UnityEngine;
using VInspector.Libs;

namespace Game
{
    public class JellyMeshController : MonoBehaviour
    {
        [Header("Component")]
        public JellySpringMassSystem springMassSystem;
        public JellyMeshBuilder meshBuilder;
        public FEyesAnimator eyeAnimator;
        public TailAnimator2[] tailAnimators;

        [Header("Parameter")]
        public float cubeSpreadRadius = 1f;
        public float cubeScaleMin = 1f;
        public float cubeScaleMax = 1f;
        public float eyeOpenSpeed = 1f;
        public float tailLength = 1f;

        SkinnedMeshRenderer[] __tailMeshRenderers;
        Transform[] __cubes;
        Vector3[] __cubeSpreadPositions;
        Vector3 __coreAttachPointLocalPositionCached;
        float __eyeLocalScaleCached;
        IDisposable __heartBeatDisposable;
        IDisposable __lookAtDisposable;

        void Awake()
        {
            __tailMeshRenderers = tailAnimators.Select(t => t.gameObject.Children().Select(c => c.GetComponent<SkinnedMeshRenderer>()).First(r => r != null)).ToArray();
            __cubes = gameObject.Descendants().Where(d => d.name == "Cube").Select(c => c.transform).ToArray();
            __cubeSpreadPositions = __cubes.Select(c => c.localPosition).ToArray();

            for (int i = 0; i < __cubes.Length; ++i)
                __cubes[i].localScale = Vector3.zero;

            __eyeLocalScaleCached = eyeAnimator.transform.localScale.x;
        }

        void Start()
        {
            Debug.Assert(springMassSystem.coreAttachPoint != null);
            __coreAttachPointLocalPositionCached = springMassSystem.coreAttachPoint.localPosition;

            for (int i = 0; i < __cubes.Length; ++i)
            {
                __cubes[i].position = springMassSystem.core.position + cubeSpreadRadius * (__cubes[i].position - springMassSystem.core.position).normalized;
                __cubeSpreadPositions[i] = __cubes[i].localPosition;
            }
        }

        public void FadeIn(float duration)
        {
            var fadeInTimeStamp = Time.time;

            __heartBeatDisposable?.Dispose();
            __heartBeatDisposable = Observable.EveryUpdate().Subscribe(_ =>
            {
                var fadeScaleMultiplier = Mathf.Clamp01((Time.time - fadeInTimeStamp) / duration);

                for (int i = 0; i < __cubes.Length; i++)
                    __cubes[i].transform.localScale = fadeScaleMultiplier * Mathf.Lerp(cubeScaleMin, cubeScaleMax, Mathf.PerlinNoise(Time.time + i, Time.time + i * i)) * Vector3.one;
            }).AddTo(this);

            eyeAnimator.transform.localScale = Vector3.zero;
            eyeAnimator.MinOpenValue = 0f;
            meshBuilder.meshRenderer.enabled = false;
            foreach (var t in tailAnimators) t.enabled = true;
            foreach (var r in __tailMeshRenderers) r.enabled = false;

            __lookAtDisposable?.Dispose();
            __lookAtDisposable = Observable.EveryLateUpdate().Subscribe(_ =>
            {
                var lookAtCameraVec = -GameContext.Instance.cameraCtrler.viewCamera.transform.forward;
                springMassSystem.coreAttachPoint.transform.localPosition = __coreAttachPointLocalPositionCached;
                springMassSystem.coreAttachPoint.transform.position += lookAtCameraVec;
                springMassSystem.coreAttachPoint.transform.LookAt(springMassSystem.coreAttachPoint.transform.position + lookAtCameraVec);

                for (int i = 0; i < __cubes.Length; ++i)
                {
                    var cubeAlpha = Mathf.Clamp01((Time.time - fadeInTimeStamp - i * 0.05f) / duration);
                    __cubes[i].transform.position = Easing.Ease(EaseType.BounceOut, springMassSystem.core.position, __cubes[i].transform.parent.localToWorldMatrix.MultiplyPoint(__cubeSpreadPositions[i]), cubeAlpha);

                    if (i + 1 == __cubes.Length && cubeAlpha >= 1f && !meshBuilder.meshRenderer.enabled)
                        meshBuilder.meshRenderer.enabled = true;
                }

                var eyeAlpha = Mathf.Clamp01((Time.time - fadeInTimeStamp - __cubes.Length * 0.05f) / duration);
                eyeAnimator.transform.position = Easing.Ease(EaseType.BounceOut, springMassSystem.core.position, eyeAnimator.transform.parent.localToWorldMatrix.GetPosition(), eyeAlpha);
                eyeAnimator.transform.localScale = Vector3.Lerp(Vector3.zero, __eyeLocalScaleCached * Vector3.one, eyeAlpha);

                if (eyeAlpha >= 1f)
                {
                    eyeAnimator.MinOpenValue = Mathf.Clamp01(eyeAnimator.MinOpenValue + eyeOpenSpeed * Time.deltaTime);
                    if (!__tailMeshRenderers[0].enabled)
                        foreach (var r in __tailMeshRenderers) r.enabled = true;
                }
            }).AddTo(this);
        }

        public void FadeOut(float duration)
        {
            __heartBeatDisposable?.Dispose();
            __heartBeatDisposable = null;

            __lookAtDisposable?.Dispose();
            __lookAtDisposable = null;

            foreach (var t in tailAnimators) t.enabled = false;
            foreach (var r in __tailMeshRenderers) r.enabled = false;
        }
    }
}