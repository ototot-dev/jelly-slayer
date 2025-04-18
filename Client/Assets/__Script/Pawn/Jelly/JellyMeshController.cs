using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using FIMSpace.FEyes;
using FIMSpace.FTail;
using ParadoxNotion.Animation;
using UniRx;
using Unity.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Game
{
    public class JellyMeshController : MonoBehaviour
    {
        [Header("Component")]
        public JellyBrain hostBrain;
        public JellySpringMassSystem springMassSystem;
        public JellyMeshBuilder meshBuilder;
        public FEyesAnimator eyeAnimator;
        public TailAnimator2[] tailAnimators;
        public RopeHookController ropeHookCtrler;

        [Header("Parameter")]
        public float maintainDistance = 1f;
        public float mainDistanceRecoverySpeed = 1f;
        public float cubeSpreadRadius = 1f;
        public float cubeScaleMin = 1f;
        public float cubeScaleMax = 1f;
        public float eyeOpenSpeed = 1f;
        public float tailLength = 1f;
        public float impulseStrength = 1f;

        [Header("Graphics")]
        public GameObject hoookingPointFx;
        public GameObject onHitFx;
        public GameObject onBloodDropFx;
        public Material cubeHitColorMaterial;
        public Material tailHitColorMaterial;
        public Material bodyHitColorMaterial;

        SkinnedMeshRenderer[] __tailMeshRenderers;
        MeshRenderer[]  __eyeMeshRenderes;
        Dictionary<int, ValueTuple<MeshRenderer, Vector3>> __cubeMeshRenderers = new();
        Dictionary<ValueTuple<int, int>, MeshRenderer> __edgeCubeMeshRenderers = new();
        float __eyeLocalScaleCached;
        IDisposable __fadeInDisposable;
        IDisposable __fadeOutDisposable;
        IDisposable __hitColorDisposable;
        IDisposable __dieDisposable;


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

            __fadeOutDisposable?.Dispose();
            __fadeOutDisposable = null;
            __fadeInDisposable = Observable.EveryLateUpdate().Subscribe(_ =>
            {
                //* __fadeInDisposable?.Dispose()가 호출되어도 한번은 Subscribe()가 호출되고 종료되기 때문에 아래와 같이 방어 코드 추가함
                if (__fadeInDisposable == null) return;

                UpdateBodyOffsetPointAndAttachPoint();

                foreach (var p in __cubeMeshRenderers)
                {
                    var cubeFadeAlpha = Mathf.Clamp01((Time.time - fadeStartTimeStamp - p.Key * 0.05f) / duration);
                    p.Value.Item1.transform.localScale = cubeFadeAlpha * Mathf.Lerp(cubeScaleMin, cubeScaleMax, Mathf.PerlinNoise(Time.time + p.Key, Time.time + p.Key * p.Key)) * Vector3.one;
                    p.Value.Item1.transform.position = Easing.Ease(EaseType.BounceOut, springMassSystem.core.position, springMassSystem.points[p.Key].linkedBone.localToWorldMatrix.MultiplyPoint(p.Value.Item2), cubeFadeAlpha);
                    foreach (var m in p.Value.Item1.materials)
                    {
                        m.SetVector("_CenterPosition", springMassSystem.core.position);
                        m.SetFloat("_FadeStartLength", Mathf.Lerp(2f, 0f, cubeFadeAlpha));
                    }
                }
                foreach (var p in __edgeCubeMeshRenderers)
                {
                    var key = (p.Key.Item1 + p.Key.Item2) * 0.5f;
                    var cubeFadeAlpha = Mathf.Clamp01((Time.time - fadeStartTimeStamp - key * 0.05f) / duration);
                    p.Value.transform.localScale = cubeFadeAlpha * Mathf.Lerp(cubeScaleMin, cubeScaleMax, Mathf.PerlinNoise(Time.time + key, Time.time + key * key)) * Vector3.one;
                    p.Value.transform.position = Easing.Ease(EaseType.BounceOut, springMassSystem.core.position, 0.5f * (__cubeMeshRenderers[p.Key.Item1].Item1.transform.position + __cubeMeshRenderers[p.Key.Item2].Item1.transform.position), cubeFadeAlpha);
                    foreach (var m in p.Value.materials)
                    {
                        m.SetVector("_CenterPosition", springMassSystem.core.position);
                        m.SetFloat("_FadeStartLength", Mathf.Lerp(2f, 0f, cubeFadeAlpha));
                    }
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
                        foreach (var m in __tailMeshRenderers[i].materials)
                        {
                            m.SetVector("_CenterPosition", springMassSystem.core.position);
                            m.SetFloat("_AlphaMultiplier", eyeFadeAlpha);
                        }
                    }

                    if (eyeFadeAlpha > 0.5f)
                        eyeAnimator.MinOpenValue = Mathf.Clamp01(eyeAnimator.MinOpenValue + eyeOpenSpeed * Time.deltaTime);
                }
            }).AddTo(this);

            //* Jelly 나타남 이벤트 생성
            PawnEventManager.Instance.SendPawnActionEvent(hostBrain, "OnJellyOut");
        }

        public void FadeOut(float duration)
        {
            var fadeStartTimeStamp = Time.time;

            meshBuilder.meshRenderer.enabled = false;
            springMassSystem.core.gameObject.layer = LayerMask.NameToLayer("Default");

            __fadeInDisposable?.Dispose();
            __fadeInDisposable = null;
            __fadeOutDisposable = Observable.EveryLateUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(duration +  __cubeMeshRenderers.Count * 0.1f)))
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
                    //* __fadeOutDisposable?.Dispose()가 호출되어도 한번은 Subscribe()가 호출되고 종료되기 때문에 아래와 같이 방어 코드 추가함
                    if (__fadeOutDisposable == null) return;

                    UpdateBodyOffsetPointAndAttachPoint();

                    foreach (var p in __cubeMeshRenderers)
                    {
                        var cubeFadeAlpha = Mathf.Clamp01((Time.time - fadeStartTimeStamp - (p.Key + 1f) * 0.05f) / duration);
                        p.Value.Item1.transform.localScale = (1f - cubeFadeAlpha) * Mathf.Lerp(cubeScaleMin, cubeScaleMax, Mathf.PerlinNoise(Time.time + p.Key, Time.time + p.Key * p.Key)) * Vector3.one;
                        p.Value.Item1.transform.position = Easing.Ease(EaseType.CubicIn, springMassSystem.points[p.Key].linkedBone.localToWorldMatrix.MultiplyPoint(p.Value.Item2), springMassSystem.core.position, cubeFadeAlpha);
                        foreach (var m in p.Value.Item1.materials)
                            m.SetVector("_CenterPosition", springMassSystem.core.position);
                    }
                    foreach (var p in __edgeCubeMeshRenderers)
                    {
                        var key = (p.Key.Item1 + p.Key.Item2) * 0.5f;
                        var cubeFadeAlpha = Mathf.Clamp01((Time.time - fadeStartTimeStamp - (key + 1f) * 0.05f) / duration);
                        p.Value.transform.localScale = (1f - cubeFadeAlpha) * Mathf.Lerp(cubeScaleMin, cubeScaleMax, Mathf.PerlinNoise(Time.time + key, Time.time + key * key)) * Vector3.one;
                        p.Value.transform.position = Easing.Ease(EaseType.BounceOut, springMassSystem.core.position, 0.5f * (__cubeMeshRenderers[p.Key.Item1].Item1.transform.position + __cubeMeshRenderers[p.Key.Item2].Item1.transform.position), cubeFadeAlpha);
                        foreach (var m in p.Value.materials)
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
                        foreach (var m in __tailMeshRenderers[i].materials)
                        {
                            m.SetVector("_CenterPosition", springMassSystem.core.position);
                            m.SetFloat("_AlphaMultiplier", 1f - eyeFadeAlpha);
                        }
                    }

                    eyeAnimator.MinOpenValue = Mathf.Clamp01(eyeAnimator.MinOpenValue - eyeOpenSpeed * Time.deltaTime);
                }).AddTo(this);

            //* Jelly 사라짐 이벤트 생성
            PawnEventManager.Instance.SendPawnActionEvent(hostBrain, "OnJellyOff");
        }

        public void Die(float duration)
        {
            var dieStartTimeStamp = Time.time;

            __fadeInDisposable?.Dispose();
            __fadeOutDisposable?.Dispose();

            meshBuilder.meshRenderer.enabled = false;
            foreach (var t in tailAnimators) t.enabled = false;

            eyeAnimator.AddComponent<SphereCollider>().radius = 0.35f;
            var eyeRigidBody = eyeAnimator.AddComponent<Rigidbody>();
            eyeRigidBody.mass = 0.1f;
            eyeRigidBody.linearDamping = 1f;
            eyeAnimator.transform.parent = transform;
            eyeAnimator.gameObject.layer = LayerMask.NameToLayer("PhysicsBody");

            Observable.NextFrame(FrameCountType.FixedUpdate).Subscribe(_ =>
            {
                eyeRigidBody.AddExplosionForce(1f, eyeRigidBody.transform.position, 1f);
            }).AddTo(this);

            var bloodDropFxInstances = new List<GameObject>();

            foreach (var p in __cubeMeshRenderers)
            {
                var rigidBody = p.Value.Item1.gameObject.AddComponent<BoxCollider>().AddComponent<Rigidbody>();
                rigidBody.transform.parent = transform;
                rigidBody.gameObject.layer = LayerMask.NameToLayer("PhysicsBody");
                rigidBody.mass = 0.1f;

                if (Rand.Dice(4) == 1)
                {                
                    var bloodDropFx = EffectManager.Instance.ShowAndForget(onBloodDropFx, rigidBody.transform.position, Quaternion.identity, Rand.Range(1f, 2f) * Vector3.one);
                    bloodDropFx.transform.parent = rigidBody.transform;
                    bloodDropFxInstances.Add(bloodDropFx);
                }
            }

            foreach (var p in __edgeCubeMeshRenderers)
            {
                var rigidBody = p.Value.gameObject.AddComponent<BoxCollider>().AddComponent<Rigidbody>();
                rigidBody.transform.parent = transform;
                rigidBody.gameObject.layer = LayerMask.NameToLayer("PhysicsBody");
                rigidBody.mass = 0.1f;

                if (Rand.Dice(4) == 1)
                {                
                    var bloodDropFx = EffectManager.Instance.ShowAndForget(onBloodDropFx, rigidBody.transform.position, Quaternion.identity, Rand.Range(1f, 2f) * Vector3.one);
                    bloodDropFx.transform.parent = rigidBody.transform;
                    bloodDropFxInstances.Add(bloodDropFx);
                }
            }

            __dieDisposable = Observable.EveryUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(duration)))
                .DoOnCompleted(() =>
                {
                    foreach (var b in bloodDropFxInstances) b.GetComponent<ParticleSystem>().Stop();
                })
                .Subscribe(_ =>
                {
                    foreach (var p in __cubeMeshRenderers)
                    {
                        var cubeFadeAlpha = Mathf.Clamp01((Time.time - dieStartTimeStamp - (p.Key + 1f) * 0.05f) / duration);
                        p.Value.Item1.transform.localScale = 0.2f * (1f - cubeFadeAlpha) * Vector3.one;

                        foreach (var m in p.Value.Item1.materials)
                        {
                            m.SetVector("_CenterPosition", p.Value.Item1.transform.position);
                            // m.SetFloat("_FadeAlphaMultiplier", Mathf.Max(1f, 10f * cubeFadeAlpha));
                        }
                    }

                    foreach (var p in __edgeCubeMeshRenderers)
                    {
                        var key = (p.Key.Item1 + p.Key.Item2) * 0.5f;
                        var cubeFadeAlpha = Mathf.Clamp01((Time.time - dieStartTimeStamp - (key + 1f) * 0.05f) / duration);
                        p.Value.transform.localScale = 0.2f * (1f - cubeFadeAlpha) * Vector3.one;

                        foreach (var m in p.Value.materials)
                        {
                            m.SetVector("_CenterPosition", p.Value.transform.position);
                            // m.SetFloat("_FadeAlphaMultiplier", Mathf.Max(1f, 10f * cubeFadeAlpha));
                        }
                    }

                    foreach (var b in bloodDropFxInstances)
                        b.transform.rotation = Quaternion.identity;

                    if (meshBuilder.meshRenderer.enabled) meshBuilder.meshRenderer.enabled = false;
                    foreach (var r in __tailMeshRenderers) 
                        if (r.enabled) r.enabled = false;
                }).AddTo(this);
        }

        void UpdateBodyOffsetPointAndAttachPoint()
        {
            var lookAtCameraVec = -GameContext.Instance.cameraCtrler.viewCamera.transform.forward;
            springMassSystem.bodyOffsetPoint.transform.LookAt(springMassSystem.core.position + lookAtCameraVec);

            if (__tweener?.IsComplete() ?? true)
                springMassSystem.bodyOffsetPoint.localPosition = 0.2f * Perlin.Noise(Time.time) * Vector3.up;

            var distance = (springMassSystem.coreAttachPoint.parent.position - hostBrain.coreColliderHelper.GetWorldCenter()).Vector2D().magnitude;
            if (distance > maintainDistance)
                springMassSystem.coreAttachPoint.parent.position = springMassSystem.coreAttachPoint.parent.position.LerpSpeed(hostBrain.coreColliderHelper.GetWorldCenter(), mainDistanceRecoverySpeed * (distance - maintainDistance), Time.deltaTime);
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
            __tweener = springMassSystem.bodyOffsetPoint.DOShakePosition(duration, 1f);

            EffectManager.Instance.Show(onHitFx, springMassSystem.core.position, Quaternion.identity, Vector3.one);
        }

        public void StartHook()
        {
            ropeHookCtrler.LaunchHook(hostBrain.GetHookingColliderHelper().pawnCollider);
            return;
            var hookingPointFx = EffectManager.Instance.ShowLooping(hoookingPointFx, hostBrain.GetHookingColliderHelper().transform.position, Quaternion.identity, Vector3.one);
            __hookingPointFxDisposable = Observable.EveryLateUpdate()
                .DoOnCancel(() => hookingPointFx.Stop())
                .DoOnCompleted(() => hookingPointFx.Stop())
                .Subscribe(_ =>
                {
                    Debug.Assert(ropeHookCtrler.hookingCollider != null);
                    hookingPointFx.transform.position = ropeHookCtrler.hookingCollider.transform.position = (hostBrain as SoldierBrain).BB.attachment.spine02.position;
                    //* 카메라 쪽으로 위치를 당겨서 겹쳐서 안보이게 되는 경우를 완화함
                    hookingPointFx.transform.position -= GameContext.Instance.cameraCtrler.viewCamera.transform.forward;
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