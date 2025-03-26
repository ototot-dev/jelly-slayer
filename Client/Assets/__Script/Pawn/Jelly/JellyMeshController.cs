using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
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
        public JellyBrain jellyBrain;
        public JellySpringMassSystem springMassSystem;
        public JellyMeshBuilder meshBuilder;
        public FEyesAnimator eyeAnimator;
        public TailAnimator2[] tailAnimators;
        public RopeHookController ropeHookCtrler;

        [Header("Parameter")]
        public float cubeSpreadRadius = 1f;
        public float cubeScaleMin = 1f;
        public float cubeScaleMax = 1f;
        public float eyeOpenSpeed = 1f;
        public float tailLength = 1f;
        public float impulseStrength = 1f;

        [Header("Graphics")]
        public GameObject hoookingPointFx;
        public GameObject onHitFx;
        public Material cubeHitColorMaterial;
        public Material tailHitColorMaterial;
        public Material bodyHitColorMaterial;

        SkinnedMeshRenderer[] __tailMeshRenderers;
        MeshRenderer[]  __eyeMeshRenderes;
        Dictionary<int, ValueTuple<MeshRenderer, Vector3>> __cubeMeshRenderers = new();
        Dictionary<ValueTuple<int, int>, MeshRenderer> __edgeCubeMeshRenderers = new();
        float __eyeLocalScaleCached;
        IDisposable __fadeUpdateDisposable;
        IDisposable __hitColorDisposable;

        void Awake()
        {
            __eyeMeshRenderes = eyeAnimator.gameObject.Descendants().Select(c => c.GetComponent<MeshRenderer>()).Where(m => m != null).ToArray();
            __tailMeshRenderers = tailAnimators.Select(t => t.gameObject.Children().Select(c => c.GetComponent<SkinnedMeshRenderer>()).First(r => r != null)).ToArray();

            foreach (var r in __eyeMeshRenderes) { r.enabled = false; }
            foreach (var r in __tailMeshRenderers) { r.enabled = false; r.transform.localScale = Vector3.zero; }
            foreach (var t in tailAnimators) t.enabled = false;
        }

        void Start()
        {
            Debug.Assert(springMassSystem.coreAttachPoint != null);

            var sourceCubeRenderer = meshBuilder.meshRenderer.transform.GetChild(0).GetComponent<MeshRenderer>();
            for (int i = 0; i < springMassSystem.points.Length; i++)
            {
                var spreadOffset = cubeSpreadRadius * springMassSystem.points[i].linkedBone.localPosition.normalized - springMassSystem.points[i].linkedBone.localPosition;
                __cubeMeshRenderers.Add(i, (i == 0 ? sourceCubeRenderer : Instantiate(sourceCubeRenderer.gameObject, meshBuilder.meshRenderer.transform).GetComponent<MeshRenderer>(), spreadOffset));
            }
            foreach (var c in springMassSystem.connections)
                __edgeCubeMeshRenderers.Add((c.pointA.index, c.pointB.index), Instantiate(sourceCubeRenderer.gameObject, meshBuilder.meshRenderer.transform).GetComponent<MeshRenderer>());

            __eyeLocalScaleCached = eyeAnimator.transform.localScale.x;
        }

        public void FadeIn(float duration)
        {
            var fadeStartTimeStamp = Time.time;

            // springMassSystem.coreAttachPoint.position = jellyBrain.coreColliderHelper.GetWorldCenter(); 
            eyeAnimator.transform.localScale = Vector3.zero;
            eyeAnimator.MinOpenValue = 0f;
            meshBuilder.meshRenderer.enabled = false;

            foreach (var r in __eyeMeshRenderes) r.enabled = true;
            foreach (var p in __cubeMeshRenderers) p.Value.Item1.enabled = true;
            foreach (var p in __edgeCubeMeshRenderers) p.Value.enabled = true;
            foreach (var r in __tailMeshRenderers) r.enabled = true;
            foreach (var t in tailAnimators) t.enabled = true;

            Observable.Timer(TimeSpan.FromSeconds(duration)).Subscribe(_ =>
            {
                springMassSystem.core.gameObject.layer = LayerMask.NameToLayer("HitBox");
            }).AddTo(this);

            __fadeUpdateDisposable?.Dispose();
            __fadeUpdateDisposable = Observable.EveryLateUpdate().Subscribe(_ =>
            {
                UpdateCoreAttachPointLookAt();

                foreach (var p in __cubeMeshRenderers)
                {
                    var cubeFadeAlpha = Mathf.Clamp01((Time.time - fadeStartTimeStamp - p.Key * 0.05f) / duration);
                    p.Value.Item1.transform.localScale = cubeFadeAlpha * Mathf.Lerp(cubeScaleMin, cubeScaleMax, Mathf.PerlinNoise(Time.time + p.Key, Time.time + p.Key * p.Key)) * Vector3.one;
                    p.Value.Item1.transform.position = Easing.Ease(EaseType.BounceOut, springMassSystem.core.position, springMassSystem.points[p.Key].linkedBone.localToWorldMatrix.MultiplyPoint(p.Value.Item2), cubeFadeAlpha);
                    p.Value.Item1.material.SetVector("_CenterPosition", springMassSystem.core.position);
                    p.Value.Item1.material.SetFloat("_FadeStartLength", Mathf.Lerp(2f, 0f, cubeFadeAlpha));
                    // __cubeMeshRenderers[i].material.SetFloat("_AlphaMultiplier", cubeFadeAlpha);
                }
                foreach (var p in __edgeCubeMeshRenderers)
                {
                    var key = (p.Key.Item1 + p.Key.Item2) * 0.5f;
                    var cubeFadeAlpha = Mathf.Clamp01((Time.time - fadeStartTimeStamp - key * 0.05f) / duration);
                    p.Value.transform.localScale = cubeFadeAlpha * Mathf.Lerp(cubeScaleMin, cubeScaleMax, Mathf.PerlinNoise(Time.time + key, Time.time + key * key)) * Vector3.one;
                    p.Value.transform.position = Easing.Ease(EaseType.BounceOut, springMassSystem.core.position, 0.5f * (__cubeMeshRenderers[p.Key.Item1].Item1.transform.position + __cubeMeshRenderers[p.Key.Item2].Item1.transform.position), cubeFadeAlpha);
                    p.Value.material.SetVector("_CenterPosition", springMassSystem.core.position);
                    p.Value.material.SetFloat("_FadeStartLength", Mathf.Lerp(2f, 0f, cubeFadeAlpha));
                }

                var eyeFadeAlpha = Mathf.Clamp01((Time.time - fadeStartTimeStamp - __cubeMeshRenderers.Count * 0.05f) / duration);
                if (eyeFadeAlpha < 1f)
                {
                    eyeAnimator.transform.position = Easing.Ease(EaseType.BounceOut, springMassSystem.core.position, eyeAnimator.transform.parent.localToWorldMatrix.GetPosition(), eyeFadeAlpha);
                    eyeAnimator.transform.localScale = Vector3.Lerp(Vector3.zero, __eyeLocalScaleCached * Vector3.one, eyeFadeAlpha);
                }

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
            springMassSystem.core.gameObject.layer = LayerMask.NameToLayer("Default");

            __fadeUpdateDisposable?.Dispose();
            __fadeUpdateDisposable = Observable.EveryLateUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(duration +  __cubeMeshRenderers.Count * 0.1f)))
                .DoOnCompleted(() =>
                {
                    foreach (var r in __eyeMeshRenderes) r.enabled = false;
                    foreach (var p in __cubeMeshRenderers) p.Value.Item1.enabled = false;
                    foreach (var p in __edgeCubeMeshRenderers) p.Value.enabled = false;
                    foreach (var r in __tailMeshRenderers) r.enabled = false;
                    foreach (var t in tailAnimators) t.enabled = false;
                })
                .Subscribe(_ =>
                {
                    UpdateCoreAttachPointLookAt();

                    foreach (var p in __cubeMeshRenderers)
                    {
                        var cubeFadeAlpha = Mathf.Clamp01((Time.time - fadeStartTimeStamp - (p.Key + 1f) * 0.05f) / duration);
                        p.Value.Item1.transform.localScale = (1f - cubeFadeAlpha) * Mathf.Lerp(cubeScaleMin, cubeScaleMax, Mathf.PerlinNoise(Time.time + p.Key, Time.time + p.Key * p.Key)) * Vector3.one;
                        p.Value.Item1.transform.position = Easing.Ease(EaseType.CubicIn, springMassSystem.points[p.Key].linkedBone.localToWorldMatrix.MultiplyPoint(p.Value.Item2), springMassSystem.core.position, cubeFadeAlpha);
                        p.Value.Item1.material.SetVector("_CenterPosition", springMassSystem.core.position);
                        // __cubeMeshRenderers[i].material.SetFloat("_AlphaMultiplier", 1f - cubeFadeAlpha);
                    }
                    foreach (var p in __edgeCubeMeshRenderers)
                    {
                        var key = (p.Key.Item1 + p.Key.Item2) * 0.5f;
                        var cubeFadeAlpha = Mathf.Clamp01((Time.time - fadeStartTimeStamp - (key + 1f) * 0.05f) / duration);
                        p.Value.transform.localScale = (1f - cubeFadeAlpha) * Mathf.Lerp(cubeScaleMin, cubeScaleMax, Mathf.PerlinNoise(Time.time + key, Time.time + key * key)) * Vector3.one;
                        p.Value.transform.position = Easing.Ease(EaseType.BounceOut, springMassSystem.core.position, 0.5f * (__cubeMeshRenderers[p.Key.Item1].Item1.transform.position + __cubeMeshRenderers[p.Key.Item2].Item1.transform.position), cubeFadeAlpha);
                        p.Value.material.SetVector("_CenterPosition", springMassSystem.core.position);
                    }

                    var eyeFadeAlpha = Mathf.Clamp01((Time.time - fadeStartTimeStamp) / duration);
                    if (eyeFadeAlpha < 1f)
                    {
                        eyeAnimator.transform.position = Easing.Ease(EaseType.CubicIn, eyeAnimator.transform.parent.localToWorldMatrix.GetPosition(), springMassSystem.core.position, eyeFadeAlpha);
                        eyeAnimator.transform.localScale = Vector3.Lerp(__eyeLocalScaleCached * Vector3.one, Vector3.zero, eyeFadeAlpha);
                    }

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
            springMassSystem.coreAttachPointOffset = 0.2f * Perlin.Noise(Time.time) * Vector3.up;

            var lookAtCameraVec = -GameContext.Instance.mainCameraCtrler.viewCamera.transform.forward;
            // springMassSystem.coreAttachPoint.transform.localPosition = __coreAttachPointLocalPositionCached;
            // springMassSystem.coreAttachPoint.transform.position += lookAtCameraVec;
            springMassSystem.coreAttachPoint.transform.LookAt(springMassSystem.coreAttachPoint.position + lookAtCameraVec);
        }

        Dictionary<int, Material> __defaultCubeMeshMaterials;
        Dictionary<ValueTuple<int, int>, Material> __defaultEdgeCubeMeshMaterials;
        Material[] __defaultTailMeshMaterials;
        Material[] __defaultEyeMeshMaterials;
        Material __defaultBodyMeshMaterial;
        Tweener __tweener;
        IDisposable __hookingPointFxDisposable;

        public void ShowHitColor(float duration)
        {
            if (__defaultCubeMeshMaterials == null)
            {
                __defaultCubeMeshMaterials = new();
                foreach (var p in __cubeMeshRenderers) __defaultCubeMeshMaterials.Add(p.Key, p.Value.Item1.material);
            }
            if (__defaultEdgeCubeMeshMaterials == null)
            {
                __defaultEdgeCubeMeshMaterials = new();
                foreach (var p in __edgeCubeMeshRenderers) __defaultEdgeCubeMeshMaterials.Add(p.Key, p.Value.material);
            }
            __defaultTailMeshMaterials ??= __tailMeshRenderers.Select(r => r.material).ToArray();
            __defaultEyeMeshMaterials ??= __eyeMeshRenderes.Select(r => r.material).ToArray();
            __defaultBodyMeshMaterial ??= meshBuilder.meshRenderer.material;

            foreach (var p in __cubeMeshRenderers) p.Value.Item1.material = cubeHitColorMaterial;
            foreach (var p in __edgeCubeMeshRenderers) p.Value.material = cubeHitColorMaterial;
            for (int i = 0; i < __tailMeshRenderers.Length; i++) __tailMeshRenderers[i].material = tailHitColorMaterial;
            for (int i = 0; i < __eyeMeshRenderes.Length; i++) if (i >= 2) __eyeMeshRenderes[i].material = bodyHitColorMaterial;
            meshBuilder.meshRenderer.material = bodyHitColorMaterial;

            __hitColorDisposable?.Dispose();
            __hitColorDisposable = Observable.Timer(TimeSpan.FromSeconds(duration)).Subscribe(_ =>
            {
                foreach (var p in __cubeMeshRenderers) p.Value.Item1.material = __defaultCubeMeshMaterials[p.Key];
                foreach (var p in __edgeCubeMeshRenderers) p.Value.material = __defaultEdgeCubeMeshMaterials[p.Key];
                for (int i = 0; i < __tailMeshRenderers.Length; i++) __tailMeshRenderers[i].material = __defaultTailMeshMaterials[i];
                for (int i = 0; i < __eyeMeshRenderes.Length; i++) __eyeMeshRenderes[i].material = __defaultEyeMeshMaterials[i];
                meshBuilder.meshRenderer.material = __defaultBodyMeshMaterial;
            }).AddTo(this);

            __tweener?.Complete();
            __tweener = springMassSystem.coreAttachPoint.DOShakePosition(duration, 0.8f);

            EffectManager.Instance.Show(onHitFx, springMassSystem.core.position, Quaternion.identity, Vector3.one);
        }

        public void StartHook()
        {
            ropeHookCtrler.LaunchHook(jellyBrain.GetHookingColliderHelper().pawnCollider);
            return;
            var hookingPointFx = EffectManager.Instance.ShowLooping(hoookingPointFx, jellyBrain.GetHookingColliderHelper().transform.position, Quaternion.identity, Vector3.one);
            __hookingPointFxDisposable = Observable.EveryLateUpdate()
                .DoOnCancel(() => hookingPointFx.Stop())
                .DoOnCompleted(() => hookingPointFx.Stop())
                .Subscribe(_ =>
                {
                    Debug.Assert(ropeHookCtrler.hookingCollider != null);
                    hookingPointFx.transform.position = ropeHookCtrler.hookingCollider.transform.position = (jellyBrain as SoldierBrain).BB.attachment.spine02.position;
                    //* 카메라 쪽으로 위치를 당겨서 겹쳐서 안보이게 되는 경우를 완화함
                    hookingPointFx.transform.position -= GameContext.Instance.mainCameraCtrler.viewCamera.transform.forward;
                }).AddTo(this);
        }

        public void FinishHook()
        {
            __hookingPointFxDisposable?.Dispose();
            __hookingPointFxDisposable = null;
            ropeHookCtrler.DetachHook();
        }
    }
}