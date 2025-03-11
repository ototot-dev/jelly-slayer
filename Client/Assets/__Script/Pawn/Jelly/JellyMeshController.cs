using System;
using System.Linq;
using FIMSpace.FEyes;
using FIMSpace.FTail;
using ParadoxNotion.Animation;
using UniRx;
using Unity.Linq;
using UnityEngine;

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
        MeshRenderer[]  __eyeMeshRenderes;
        MeshRenderer[] __cubeMeshRenderers;
        Vector3[] __cubeSpreadPositions;
        Vector3 __coreAttachPointLocalPositionCached;
        float __eyeLocalScaleCached;
        IDisposable __fadeUpdateDisposable;

        void Awake()
        {
            __eyeMeshRenderes = eyeAnimator.gameObject.Descendants().Select(c => c.GetComponent<MeshRenderer>()).Where(m => m != null).ToArray();
            __tailMeshRenderers = tailAnimators.Select(t => t.gameObject.Children().Select(c => c.GetComponent<SkinnedMeshRenderer>()).First(r => r != null)).ToArray();
            __cubeMeshRenderers = gameObject.Descendants().Where(d => d.name == "Cube").Select(c => c.GetComponent<MeshRenderer>()).ToArray();
            __cubeSpreadPositions = __cubeMeshRenderers.Select(c => c.transform.localPosition).ToArray();

            foreach (var r in __eyeMeshRenderes) { r.enabled = false; }
            foreach (var r in __cubeMeshRenderers) { r.enabled = false; r.transform.localScale = Vector3.zero; }
            foreach (var r in __tailMeshRenderers) { r.enabled = false; r.transform.localScale = Vector3.zero; }
            foreach (var t in tailAnimators) t.enabled = false;
        }

        void Start()
        {
            Debug.Assert(springMassSystem.coreAttachPoint != null);

            __coreAttachPointLocalPositionCached = springMassSystem.coreAttachPoint.localPosition;
            __eyeLocalScaleCached = eyeAnimator.transform.localScale.x;

            for (int i = 0; i < __cubeMeshRenderers.Length; ++i)
            {
                __cubeMeshRenderers[i].transform.position = springMassSystem.core.position + cubeSpreadRadius * (__cubeMeshRenderers[i].transform.position - springMassSystem.core.position).normalized;
                __cubeSpreadPositions[i] = __cubeMeshRenderers[i].transform.localPosition;
            }
        }

        public void FadeIn(float duration)
        {
            var fadeStartTimeStamp = Time.time;

            eyeAnimator.transform.localScale = Vector3.zero;
            eyeAnimator.MinOpenValue = 0f;
            meshBuilder.meshRenderer.enabled = false;

            foreach (var r in __eyeMeshRenderes) r.enabled = true;
            foreach (var r in __cubeMeshRenderers) r.enabled = true;
            foreach (var r in __tailMeshRenderers) r.enabled = true;
            foreach (var t in tailAnimators) t.enabled = true;

            __fadeUpdateDisposable?.Dispose();
            __fadeUpdateDisposable = Observable.EveryLateUpdate().Subscribe(_ =>
            {
                UpdateCoreAttachPointLookAt();

                for (int i = 0; i < __cubeMeshRenderers.Length; ++i)
                {
                    var cubeFadeAlpha = Mathf.Clamp01((Time.time - fadeStartTimeStamp - i * 0.05f) / duration);
                    __cubeMeshRenderers[i].transform.localScale = cubeFadeAlpha * Mathf.Lerp(cubeScaleMin, cubeScaleMax, Mathf.PerlinNoise(Time.time + i, Time.time + i * i)) * Vector3.one;
                    __cubeMeshRenderers[i].transform.position = Easing.Ease(EaseType.BounceOut, springMassSystem.core.position, __cubeMeshRenderers[i].transform.parent.localToWorldMatrix.MultiplyPoint(__cubeSpreadPositions[i]), cubeFadeAlpha);
                    __cubeMeshRenderers[i].material.SetVector("_CenterPosition", springMassSystem.core.position);
                    __cubeMeshRenderers[i].material.SetFloat("_FadeStartLength", Mathf.Lerp(2f, 0f, cubeFadeAlpha));
                    // __cubeMeshRenderers[i].material.SetFloat("_AlphaMultiplier", cubeFadeAlpha);
                }

                var eyeFadeAlpha = Mathf.Clamp01((Time.time - fadeStartTimeStamp - __cubeMeshRenderers.Length * 0.05f) / duration);
                eyeAnimator.transform.position = Easing.Ease(EaseType.BounceOut, springMassSystem.core.position, eyeAnimator.transform.parent.localToWorldMatrix.GetPosition(), eyeFadeAlpha);
                eyeAnimator.transform.localScale = Vector3.Lerp(Vector3.zero, __eyeLocalScaleCached * Vector3.one, eyeFadeAlpha);

                if (eyeFadeAlpha > 0f)
                {
                    if (!meshBuilder.meshRenderer.enabled)
                        meshBuilder.meshRenderer.enabled = true;

                    for (int i = 0; i < __tailMeshRenderers.Length; i++)
                    {
                        __tailMeshRenderers[i].material.SetVector("_CenterPosition", springMassSystem.core.position);
                        __tailMeshRenderers[i].material.SetFloat("_AlphaMultiplier", eyeFadeAlpha);
                    }

                    if (eyeFadeAlpha > 0.5f)
                        eyeAnimator.MinOpenValue = Mathf.Clamp01(eyeAnimator.MinOpenValue + eyeOpenSpeed * Time.deltaTime);
                }
            }).AddTo(this);
        }

        public void FadeOut(float duration)
        {
            var fadeStartTimeStamp = Time.time;

            meshBuilder.meshRenderer.enabled = false;

            __fadeUpdateDisposable?.Dispose();
            __fadeUpdateDisposable = Observable.EveryLateUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(duration +  __cubeMeshRenderers.Length * 0.1f)))
                .DoOnCompleted(() =>
                {
                    foreach (var r in __eyeMeshRenderes) r.enabled = false;
                    foreach (var r in __cubeMeshRenderers) r.enabled = false;
                    foreach (var r in __tailMeshRenderers) r.enabled = false;
                    foreach (var t in tailAnimators) t.enabled = false;
                })
                .Subscribe(_ =>
                {
                    UpdateCoreAttachPointLookAt();

                    for (int i = 0; i < __cubeMeshRenderers.Length; ++i)
                    {
                        var cubeFadeAlpha = Mathf.Clamp01((Time.time - fadeStartTimeStamp - (i + 1) * 0.05f) / duration);
                        __cubeMeshRenderers[i].transform.localScale = (1f - cubeFadeAlpha) * Mathf.Lerp(cubeScaleMin, cubeScaleMax, Mathf.PerlinNoise(Time.time + i, Time.time + i * i)) * Vector3.one;
                        __cubeMeshRenderers[i].transform.position = Easing.Ease(EaseType.CubicIn, __cubeMeshRenderers[i].transform.parent.localToWorldMatrix.MultiplyPoint(__cubeSpreadPositions[i]), springMassSystem.core.position, cubeFadeAlpha);
                        __cubeMeshRenderers[i].material.SetVector("_CenterPosition", springMassSystem.core.position);
                        // __cubeMeshRenderers[i].material.SetFloat("_AlphaMultiplier", 1f - cubeFadeAlpha);
                    }

                    var eyeFadeAlpha = Mathf.Clamp01((Time.time - fadeStartTimeStamp) / duration);
                    eyeAnimator.transform.position = Easing.Ease(EaseType.CubicIn, eyeAnimator.transform.parent.localToWorldMatrix.GetPosition(), springMassSystem.core.position, eyeFadeAlpha);
                    eyeAnimator.transform.localScale = Vector3.Lerp(__eyeLocalScaleCached * Vector3.one, Vector3.zero, eyeFadeAlpha);

                    for (int i = 0; i < __tailMeshRenderers.Length; i++)
                    {
                        __tailMeshRenderers[i].material.SetVector("_CenterPosition", springMassSystem.core.position);
                        __tailMeshRenderers[i].material.SetFloat("_AlphaMultiplier", 1f - eyeFadeAlpha);
                    }

                    eyeAnimator.MinOpenValue = Mathf.Clamp01(eyeAnimator.MinOpenValue - eyeOpenSpeed * Time.deltaTime);
                }).AddTo(this);
        }

        void UpdateCoreAttachPointLookAt()
        {
            var lookAtCameraVec = -GameContext.Instance.mainCameraCtrler.viewCamera.transform.forward;
            springMassSystem.coreAttachPoint.transform.localPosition = __coreAttachPointLocalPositionCached;
            springMassSystem.coreAttachPoint.transform.position += lookAtCameraVec;
            springMassSystem.coreAttachPoint.transform.LookAt(springMassSystem.coreAttachPoint.transform.position + lookAtCameraVec);
        }

        void ShowHitColor()
        {
            // if (hitColliderHelper == __brain.bodyHitColliderHelper)
            // {
            //     foreach (var r in __brain.BB.attachment.bodyMeshRenderers)
            //         r.materials = new Material[] { r.material, new(__brain.BB.graphics.hitColor) };

            //     __hitColorDisposable?.Dispose();
            //     __hitColorDisposable = Observable.Timer(TimeSpan.FromMilliseconds(100)).Subscribe(_ => 
            //     {
            //         __hitColorDisposable = null;
            //         foreach (var r in __brain.BB.attachment.bodyMeshRenderers)
            //             r.materials = new Material[] { r.materials[0] };
            //     }).AddTo(this);
            // }
            // else if (hitColliderHelper == __brain.shieldHitColliderHelper)
            // {
            //     __brain.BB.attachment.shieldMeshRenderer.materials = new Material[] { __brain.BB.attachment.shieldMeshRenderer.material, new(__brain.BB.graphics.hitColor) };

            //     __hitColorDisposable?.Dispose();
            //     __hitColorDisposable = Observable.Timer(TimeSpan.FromMilliseconds(100)).Subscribe(_ => 
            //     {
            //         __hitColorDisposable = null;
            //         __brain.BB.attachment.shieldMeshRenderer.materials = new Material[] { __brain.BB.attachment.shieldMeshRenderer.material };
            //     }).AddTo(this);
            // }
            // else
            // {
            //     Debug.Assert(false);
            // }
        }
    }
}