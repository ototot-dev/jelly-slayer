using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using FIMSpace.FEyes;
using Obi;
using ParadoxNotion.Animation;
using UniRx;
using UniRx.Triggers;
using Unity.Linq;
using UnityEngine;
using ZLinq;

namespace Game
{
    public class JellyMeshController : MonoBehaviour
    {
        [Header("Component")]
        public NpcBrain hostBrain;
        public JellySpringMassSystem springMassSystem;
        public JellyMeshBuilder meshBuilder;
        public ObiSolver ropeObiSolver;
        public BoxCollider ropeGroundCollider;
        public Transform face;
        public Transform popping;

        [Header("Parameter")]
        public float maintainDistance = 1f;
        public float distanceRecoverySpeed = 1f;
        public float hitImpulseStrength = 1f;
        public float cubeSpreadRadius = 1f;
        public float cubeScaleMin = 1f;
        public float cubeScaleMax = 1f;
        public float eyeOpenSpeed = 1f;
        public float ropeGroundLevel= 0f;
        public AnimationCurve cubeDitherAlphaCurve;
        public AnimationCurve hairDitherAlphaCurve;
        public AnimationCurve hookDitherAlphaCurve;

        [Header("Graphics")]
        public GameObject onHitFx;
        public GameObject onBloodDropFx;
        public GameObject hookingPointFx;
        public Material cubeHitColorMaterial;

        [Serializable]
        public struct HookAttachment
        {
            public Transform targetPoint;
            public Transform startPin;
            public Transform endPin;
            public ObiRope obiRopeHair;
            public ObiRopeExtrudedRenderer obiRopeRenderer;
        }
        
        [Serializable]
        public struct HairAttachment
        {
            public Transform attachPoint;
            public Transform pin;
            public ObiRope obiRopeHair;
            public ObiRopeExtrudedRenderer obiRopeRenderer;
        }

        [Serializable]
        public struct EyeAttachment
        {
            public Transform attachPoint;
            public Vector3 attachOffset;
            public Vector3 localEulerRotation;
            public Vector3 localScale;
            public FEyesAnimator eyeAnimator;
            public MeshRenderer[] eyelidRenderers;
            public MeshRenderer[] eyeBallRenderers;
        }

        [Header("Attachment")]
        public HookAttachment[] hookAttachments;
        public HairAttachment[] hairAttachments;
        public EyeAttachment[] eyeAttachments;

        Dictionary<int, MeshRenderer> __cubeMeshRenderers = new();
        Dictionary<ValueTuple<int, int>, MeshRenderer> __edgeCubeMeshRenderers = new();
        HashSet<MeshRenderer> __nonRigidBodyCubeMeshRenderers = new();
        Dictionary<MeshRenderer, Rigidbody> __cubeMeshRigidBodies = new();
        Dictionary<MeshRenderer, Material> __cubeMeshMaterials = new();
        Dictionary<MeshRenderer, Material> __cubeMeshHitColorMaterials = new();
        Material __hairSharedMaterial;
        Material __hookSharedMaterial;
        Color __defaultHairEmissiveColor;
        Color __defaultHookEmissiveColor;
        Color __defaultEyeEmissiveColor;
        Color __defaultBaseColor;

        IDisposable __fadeInDisposable;
        IDisposable __fadeOutDisposable;
        IDisposable __hitColorDisposable;
        IDisposable __dieDisposable;

        void Awake()
        {
            meshBuilder.meshRenderer.enabled = false;
            if (meshBuilder.meshRenderer.transform.GetChild(0).TryGetComponent<MeshRenderer>(out var cubeMeshRenderer))
            {
                cubeMeshRenderer.enabled = false;
                __defaultBaseColor = cubeMeshRenderer.material.GetColor("_BaseColor");

            }

            __hairSharedMaterial = new Material(hairAttachments.First().obiRopeRenderer.material);
            __hookSharedMaterial = new Material(hookAttachments.First().obiRopeRenderer.material);
            foreach (var a in hookAttachments) 
            {
                __defaultHookEmissiveColor = __hookSharedMaterial.GetColor("_EmissiveColor");
                a.obiRopeRenderer.material = __hookSharedMaterial;
                a.obiRopeRenderer.enabled = false;
            }
            foreach (var a in hairAttachments)
            {
                __defaultHairEmissiveColor = __hairSharedMaterial.GetColor("_EmissiveColor");
                a.obiRopeRenderer.material = __hairSharedMaterial;
                a.obiRopeRenderer.enabled = false;
            }

            foreach (var a in eyeAttachments)
            {
                __defaultEyeEmissiveColor = a.eyelidRenderers.First().material.GetColor("_EmissiveColor");
                foreach (var r in a.eyelidRenderers) r.enabled = false;
                foreach (var r in a.eyeBallRenderers) r.enabled = false;
            }
        }

        void Start()
        {
            //* JellySpringMassSystem Start() 함수 선호출되고 나서 실행되도록 1 프레임 늦춤
            Observable.NextFrame().Subscribe(_ =>
            {                
                var sourceCubeRenderer = meshBuilder.meshRenderer.transform.GetChild(0).GetComponent<MeshRenderer>();
                for (int i = 0; i < springMassSystem.points.Length; i++)
                {
                    __cubeMeshRenderers.Add(i, i == 0 ? sourceCubeRenderer : Instantiate(sourceCubeRenderer.gameObject, meshBuilder.meshRenderer.transform, false).GetComponent<MeshRenderer>());
                    __cubeMeshMaterials.Add(__cubeMeshRenderers[i], __cubeMeshRenderers[i].material);
                    __nonRigidBodyCubeMeshRenderers.Add(__cubeMeshRenderers[i]);
                }
                foreach (var c in springMassSystem.connections)
                {
                    var key = (c.pointA.index, c.pointB.index);
                    __edgeCubeMeshRenderers.Add(key, Instantiate(sourceCubeRenderer.gameObject, meshBuilder.meshRenderer.transform, false).GetComponent<MeshRenderer>());
                    __cubeMeshMaterials.Add(__edgeCubeMeshRenderers[key], __edgeCubeMeshRenderers[key].material);
                    __nonRigidBodyCubeMeshRenderers.Add(__edgeCubeMeshRenderers[key]);
                }
            }).AddTo(this);

            //* 초기화를 위해서 첫번째 프레임은 실행한 후 꺼준다.
            Observable.NextFrame(FrameCountType.FixedUpdate).Subscribe(_ =>
            {
                ropeObiSolver.enabled = false;
            }).AddTo(this);
        }

        void UpdateCorePositionAndBodyOffset()
        {
            if (hostBrain != null)
            {
                var distance = (springMassSystem.core.position - hostBrain.coreColliderHelper.GetWorldCenter()).Vector2D().magnitude;
                if (distance > maintainDistance)
                    springMassSystem.core.position = springMassSystem.core.position.LerpSpeed(hostBrain.coreColliderHelper.GetWorldCenter(), distanceRecoverySpeed * (distance - maintainDistance), Time.deltaTime);
            }

            springMassSystem.bodyOffsetPoint.rotation = Quaternion.LookRotation(-GameContext.Instance.cameraCtrler.gameCamera.transform.forward);

            if (__tweener?.IsComplete() ?? true)
                springMassSystem.bodyOffsetPoint.localPosition = 0.2f * Perlin.Noise(Time.time) * Vector3.up;
        }

        public void FadeIn(float duration)
        {
            var fadeStartTimeStamp = Time.time;

            ropeObiSolver.enabled = true;
            ropeGroundCollider.enabled = true;
            meshBuilder.meshRenderer.enabled = false;
            foreach (var p in __cubeMeshRenderers) p.Value.enabled = true;
            foreach (var p in __edgeCubeMeshRenderers) p.Value.enabled = true;
            foreach (var a in hairAttachments) a.obiRopeRenderer.enabled = true;
            foreach (var a in hookAttachments) a.obiRopeRenderer.enabled = true;
            foreach (var a in eyeAttachments)
            {
                a.eyeAnimator.MinOpenValue = 0f;
                a.eyeAnimator.enabled = true;
                foreach (var r in a.eyelidRenderers) r.enabled = true;
                foreach (var r in a.eyeBallRenderers) r.enabled = true;
            }

            Observable.Timer(TimeSpan.FromSeconds(duration)).Subscribe(_ =>
            {
                springMassSystem.core.gameObject.layer = LayerMask.NameToLayer("HitBox");
            }).AddTo(this);

            var easingStartPosition = hostBrain != null ? hostBrain.coreColliderHelper.GetWorldCenter() : springMassSystem.core.position - 2f * Vector3.right;

            __hitColorDisposable?.Dispose();
            __fadeOutDisposable?.Dispose();
            __fadeOutDisposable = null;
            __fadeInDisposable = Observable.EveryLateUpdate().Subscribe(_ =>
            {
                //* __fadeInDisposable?.Dispose()가 호출되어도 한번은 Subscribe()가 호출되고 종료되기 때문에 아래와 같이 방어 코드 추가함
                if (__fadeInDisposable == null) return;

                UpdateCorePositionAndBodyOffset();

                var corePositionInViewSpace = GameContext.Instance.cameraCtrler.gameCamera.worldToCameraMatrix.MultiplyPoint(springMassSystem.core.position);
                foreach (var p in __cubeMeshRenderers)
                {
                    var cubeFadeAlpha = Mathf.Clamp01((Time.time - fadeStartTimeStamp - p.Key * 0.05f) / duration);
                    p.Value.transform.localScale = cubeFadeAlpha * Mathf.Lerp(cubeScaleMin, cubeScaleMax, Mathf.PerlinNoise(Time.time + p.Key, Time.time + p.Key * p.Key)) * Vector3.one;
                    p.Value.transform.localRotation = Quaternion.Euler(cubeFadeAlpha * Mathf.Lerp(-5f, 5f, Mathf.PerlinNoise(Time.time + p.Key, Time.time + p.Key * p.Key)) * Vector3.one);

                    var spreadVec = cubeSpreadRadius * springMassSystem.points[p.Key].linkedGrid.localPosition.normalized - springMassSystem.points[p.Key].linkedGrid.localPosition;
                    p.Value.transform.position = Easing.Ease(EaseType.BounceOut, easingStartPosition, springMassSystem.points[p.Key].linkedBone.localToWorldMatrix.MultiplyPoint(spreadVec), cubeFadeAlpha);

                    var cubePositionInViewSpace = GameContext.Instance.cameraCtrler.gameCamera.worldToCameraMatrix.MultiplyPoint(p.Value.transform.position);
                    var cubeDistanceAlpha = cubeDitherAlphaCurve.Evaluate(Vector3.Distance(corePositionInViewSpace.AdjustZ(0f), cubePositionInViewSpace.AdjustZ(0f)));

                    foreach (var m in p.Value.materials)
                    {
                        if (__hitColorDisposable != null)
                            m.SetColor("_BaseColor", Color.white.AdjustAlpha(cubeFadeAlpha < 1f ? cubeFadeAlpha : cubeFadeAlpha * cubeDistanceAlpha));
                        else
                            m.SetColor("_BaseColor", __defaultBaseColor.AdjustAlpha(cubeFadeAlpha < 1f ? cubeFadeAlpha : cubeFadeAlpha * cubeDistanceAlpha));
                    }
                }
                foreach (var p in __edgeCubeMeshRenderers)
                {
                    var key = (p.Key.Item1 + p.Key.Item2) * 0.5f;
                    var cubeFadeAlpha = Mathf.Clamp01((Time.time - fadeStartTimeStamp - key * 0.05f) / duration);
                    p.Value.transform.localScale = cubeFadeAlpha * Mathf.Lerp(cubeScaleMin, cubeScaleMax, Mathf.PerlinNoise(Time.time + key, Time.time + key * key)) * Vector3.one;
                    p.Value.transform.localRotation = Quaternion.Euler(cubeFadeAlpha * Mathf.Lerp(-15f, 15f, Mathf.PerlinNoise(Time.time + key, Time.time + key * key)) * Vector3.one);
                    p.Value.transform.position = Easing.Ease(EaseType.BounceOut, easingStartPosition, 0.5f * (__cubeMeshRenderers[p.Key.Item1].transform.position + __cubeMeshRenderers[p.Key.Item2].transform.position), cubeFadeAlpha);

                    var cubePositionInViewSpace = GameContext.Instance.cameraCtrler.gameCamera.worldToCameraMatrix.MultiplyPoint(p.Value.transform.position);
                    var cubeDistanceAlpha = cubeDitherAlphaCurve.Evaluate(Vector3.Distance(corePositionInViewSpace.AdjustZ(0f), cubePositionInViewSpace.AdjustZ(0f)));

                    foreach (var m in p.Value.materials)
                    {
                        if (__hitColorDisposable != null)
                            m.SetColor("_BaseColor", Color.white.AdjustAlpha(cubeFadeAlpha < 1f ? cubeFadeAlpha : cubeFadeAlpha * cubeDistanceAlpha));
                        else
                            m.SetColor("_BaseColor", __defaultBaseColor.AdjustAlpha(cubeFadeAlpha < 1f ? cubeFadeAlpha : cubeFadeAlpha * cubeDistanceAlpha));
                    }
                }

                foreach (var a in eyeAttachments)
                {
                    var fadeAlpha = Mathf.Clamp01((Time.time - fadeStartTimeStamp - 0.5f) / duration);
                    a.eyeAnimator.transform.localScale = fadeAlpha * a.localScale;
                    a.eyeAnimator.transform.localEulerAngles = a.localEulerRotation;
                    a.eyeAnimator.transform.position = a.attachPoint.localToWorldMatrix.MultiplyPoint(a.attachOffset);

                    if (__hitColorDisposable == null && fadeAlpha >= 1f)
                    {
                        a.eyeAnimator.MinOpenValue = Mathf.Clamp01(a.eyeAnimator.MinOpenValue + eyeOpenSpeed * Time.deltaTime);
                        foreach (var r in a.eyelidRenderers) r.material.SetColor("_BaseColor", __defaultBaseColor);
                    }
                    else
                    {
                        foreach (var r in a.eyelidRenderers) r.material.SetColor("_BaseColor", Color.white.AdjustAlpha(1f));
                    }
                }

                foreach (var a in hairAttachments)
                {
                    a.pin.position = a.attachPoint.position;

                    var fadeAlpha = Mathf.Clamp01((Time.time - fadeStartTimeStamp - 1f) / duration);
                    a.obiRopeRenderer.material.SetColor("_BaseColor", __hitColorDisposable != null ? Color.white.AdjustAlpha(fadeAlpha) : __defaultBaseColor.AdjustAlpha(fadeAlpha));
                    a.obiRopeRenderer.material.SetFloat("_AlphaDecreaseStartV", Mathf.Lerp(0f, 2f, fadeAlpha));
                }

                foreach (var a in hookAttachments)
                {
                    a.startPin.position = springMassSystem.core.position;
                    a.endPin.position = hostBrain != null ? hostBrain.GetHookingColliderHelper().transform.position : springMassSystem.core.position + springMassSystem.core.forward;

                    var fadeAlpha = Mathf.Clamp01((Time.time - fadeStartTimeStamp - 0.5f) / duration);
                    a.obiRopeRenderer.material.SetColor("_BaseColor", __hitColorDisposable != null ? Color.white.AdjustAlpha(fadeAlpha) : __defaultBaseColor.AdjustAlpha(fadeAlpha));
                }

                ropeGroundCollider.transform.position = ropeGroundCollider.transform.position.LerpSpeedY(ropeGroundLevel, 1f, Time.deltaTime);
            }).AddTo(this);

            //* Jelly 나타남 이벤트 생성
            if (hostBrain != null)
                PawnEventManager.Instance.SendPawnActionEvent(hostBrain, "OnJellyOut");
        }

        public void FadeOut(float duration)
        {
            var fadeStartTimeStamp = Time.time;
            springMassSystem.core.gameObject.layer = LayerMask.NameToLayer("Default");

            var easingEndPosition = hostBrain != null ? hostBrain.coreColliderHelper.GetWorldCenter() : springMassSystem.core.position - 2f * Vector3.right;

            __hitColorDisposable?.Dispose();
            __fadeInDisposable?.Dispose();
            __fadeInDisposable = null;
            __fadeOutDisposable = Observable.EveryLateUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(duration +  __cubeMeshRenderers.Count * 0.1f)))
                .DoOnCompleted(() =>
                {
                    ropeObiSolver.enabled = false;
                    ropeGroundCollider.enabled = false;
                    meshBuilder.meshRenderer.enabled = false;
                    foreach (var p in __cubeMeshRenderers) p.Value.enabled = false;
                    foreach (var p in __edgeCubeMeshRenderers) p.Value.enabled = false;
                    foreach (var a in hairAttachments) a.obiRopeRenderer.enabled = false;
                    foreach (var a in hookAttachments) a.obiRopeRenderer.enabled = false;
                    foreach (var a in eyeAttachments)
                    {
                        a.eyeAnimator.enabled = false;
                        foreach (var r in a.eyelidRenderers) r.enabled = false;
                        foreach (var r in a.eyeBallRenderers) r.enabled = false;
                    }
                })
                .Subscribe(_ =>
                {
                    //* __fadeOutDisposable?.Dispose()가 호출되어도 한번은 Subscribe()가 호출되고 종료되기 때문에 아래와 같이 방어 코드 추가함
                    if (__fadeOutDisposable == null) return;

                    UpdateCorePositionAndBodyOffset();

                    var corePositionInViewSpace = GameContext.Instance.cameraCtrler.gameCamera.worldToCameraMatrix.MultiplyPoint(springMassSystem.core.position);
                    foreach (var p in __cubeMeshRenderers)
                    {
                        var cubeFadeAlpha = Mathf.Clamp01((Time.time - fadeStartTimeStamp - p.Key * 0.05f) / duration);
                        p.Value.transform.localScale = (1f - cubeFadeAlpha) * Mathf.Lerp(cubeScaleMin, cubeScaleMax, Mathf.PerlinNoise(Time.time + p.Key, Time.time + p.Key * p.Key)) * Vector3.one;
                        p.Value.transform.localRotation = Quaternion.Euler(cubeFadeAlpha * Mathf.Lerp(-5f, 5f, Mathf.PerlinNoise(Time.time + p.Key, Time.time + p.Key * p.Key)) * Vector3.one);

                        var spreadVec = cubeSpreadRadius * springMassSystem.points[p.Key].linkedGrid.localPosition.normalized - springMassSystem.points[p.Key].linkedGrid.localPosition;
                        p.Value.transform.position = Easing.Ease(EaseType.QuadraticIn, springMassSystem.points[p.Key].linkedBone.localToWorldMatrix.MultiplyPoint(spreadVec), easingEndPosition, cubeFadeAlpha);

                        var cubePositionInViewSpace = GameContext.Instance.cameraCtrler.gameCamera.worldToCameraMatrix.MultiplyPoint(p.Value.transform.position);
                        var cubeDistanceAlpha = cubeDitherAlphaCurve.Evaluate(Vector3.Distance(corePositionInViewSpace.AdjustZ(0f), cubePositionInViewSpace.AdjustZ(0f)));

                        foreach (var m in p.Value.materials)
                            m.SetColor("_BaseColor", __defaultBaseColor.AdjustAlpha(Easing.Ease(EaseType.QuadraticOut, cubeDistanceAlpha, cubeFadeAlpha, cubeFadeAlpha)));
                    }
                    foreach (var p in __edgeCubeMeshRenderers)
                    {
                        var key = (p.Key.Item1 + p.Key.Item2) * 0.5f;
                        var cubeFadeAlpha = Mathf.Clamp01((Time.time - fadeStartTimeStamp - key * 0.05f) / duration);
                        p.Value.transform.localScale = (1f - cubeFadeAlpha) * Mathf.Lerp(cubeScaleMin, cubeScaleMax, Mathf.PerlinNoise(Time.time + key, Time.time + key * key)) * Vector3.one;
                        p.Value.transform.localRotation = Quaternion.Euler(cubeFadeAlpha * Mathf.Lerp(-15f, 15f, Mathf.PerlinNoise(Time.time + key, Time.time + key * key)) * Vector3.one);
                        p.Value.transform.position = Easing.Ease(EaseType.QuadraticIn, 0.5f * (__cubeMeshRenderers[p.Key.Item1].transform.position + __cubeMeshRenderers[p.Key.Item2].transform.position), easingEndPosition, cubeFadeAlpha);

                        var cubePositionInViewSpace = GameContext.Instance.cameraCtrler.gameCamera.worldToCameraMatrix.MultiplyPoint(p.Value.transform.position);
                        var cubeDistanceAlpha = cubeDitherAlphaCurve.Evaluate(Vector3.Distance(corePositionInViewSpace.AdjustZ(0f), cubePositionInViewSpace.AdjustZ(0f)));

                        foreach (var m in p.Value.materials)
                            m.SetColor("_BaseColor", __defaultBaseColor.AdjustAlpha(Easing.Ease(EaseType.QuadraticOut, cubeDistanceAlpha, cubeFadeAlpha, cubeFadeAlpha)));
                    }

                    foreach (var a in eyeAttachments)
                    {
                        var fadeAlpha = Mathf.Clamp01((Time.time - fadeStartTimeStamp) / duration);
                        a.eyeAnimator.transform.localScale = (1f - fadeAlpha) * a.localScale;
                        a.eyeAnimator.transform.localEulerAngles = a.localEulerRotation;
                        a.eyeAnimator.transform.position = Easing.Ease(EaseType.CubicIn, a.attachPoint.localToWorldMatrix.MultiplyPoint(a.attachOffset), easingEndPosition, fadeAlpha);
                        a.eyeAnimator.MinOpenValue = 0f;
                    }

                    foreach (var a in hairAttachments)
                    {
                        a.pin.position = a.attachPoint.position;

                        var fadeAlpha = Mathf.Clamp01((Time.time - fadeStartTimeStamp) / duration);
                        a.obiRopeRenderer.material.SetColor("_BaseColor", __defaultBaseColor.AdjustAlpha(1f - fadeAlpha));
                    }

                    foreach (var a in hookAttachments)
                    {
                        a.startPin.position = springMassSystem.core.position;
                        a.endPin.position = hostBrain != null ? hostBrain.GetHookingColliderHelper().transform.position : springMassSystem.core.position + springMassSystem.core.forward;
                        
                        var fadeAlpha = Mathf.Clamp01((Time.time - fadeStartTimeStamp) / duration);
                        a.obiRopeRenderer.material.SetColor("_BaseColor", __defaultBaseColor.AdjustAlpha(1f - fadeAlpha));
                    }
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
            // foreach (var t in tailAnimators) t.enabled = false;

            // eyeAnimator.gameObject.AddComponent<SphereCollider>().radius = 0.35f;
            // var eyeRigidBody = eyeAnimator.gameObject.AddComponent<Rigidbody>();
            // eyeRigidBody.mass = 0.1f;
            // eyeRigidBody.linearDamping = 1f;
            // eyeAnimator.transform.parent = transform;
            // eyeAnimator.gameObject.layer = LayerMask.NameToLayer("PhysicsBody");

            // Observable.NextFrame(FrameCountType.FixedUpdate).Subscribe(_ =>
            // {
            //     eyeRigidBody.AddExplosionForce(1f, eyeRigidBody.transform.position, 1f);
            // }).AddTo(this);

            var bloodDropFxInstances = new List<ParticleSystem>();

            foreach (var p in __cubeMeshRenderers)
            {
                var rigidBody = p.Value.gameObject.AddComponent<BoxCollider>().gameObject.AddComponent<Rigidbody>();
                rigidBody.transform.parent = transform;
                rigidBody.gameObject.layer = LayerMask.NameToLayer("PhysicsBody");
                rigidBody.mass = 0.1f;

                if (Rand.Dice(4) == 1)
                {                
                    var bloodDropFx = EffectManager.Instance.ShowAndForget(onBloodDropFx, rigidBody.transform.position, Quaternion.identity, Rand.Range(1f, 2f) * Vector3.one).GetComponent<ParticleSystem>();
                    bloodDropFx.transform.parent = rigidBody.transform;
                    bloodDropFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    bloodDropFxInstances.Add(bloodDropFx);

                    rigidBody.OnCollisionEnterAsObservable().TakeWhile(_ => !bloodDropFx.isPlaying).Subscribe(c =>
                    {
                        if (c.gameObject.layer == LayerMask.NameToLayer("Terrain"))
                            bloodDropFx.Play();
                    }).AddTo(this);
                }
            }

            foreach (var p in __edgeCubeMeshRenderers)
            {
                var rigidBody = p.Value.gameObject.AddComponent<BoxCollider>().gameObject.AddComponent<Rigidbody>();
                rigidBody.transform.parent = transform;
                rigidBody.gameObject.layer = LayerMask.NameToLayer("PhysicsBody");
                rigidBody.mass = 0.1f;

                if (Rand.Dice(4) == 1)
                {                
                    var bloodDropFx = EffectManager.Instance.ShowAndForget(onBloodDropFx, rigidBody.transform.position, Quaternion.identity, Rand.Range(1f, 2f) * Vector3.one).GetComponent<ParticleSystem>();
                    bloodDropFx.transform.parent = rigidBody.transform;
                    bloodDropFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    bloodDropFxInstances.Add(bloodDropFx);

                    rigidBody.OnCollisionEnterAsObservable().TakeWhile(_ => !bloodDropFx.isPlaying).Subscribe(c =>
                    {
                        if (c.gameObject.layer == LayerMask.NameToLayer("Terrain"))
                            bloodDropFx.Play();
                    }).AddTo(this);
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
                        p.Value.transform.localScale = 0.2f * Vector3.one;

                        foreach (var m in p.Value.materials)
                        {
                            m.SetVector("_BaseColor", new Color(190f / 255f, 0f, 0f, 1f - cubeFadeAlpha));
                            // m.SetFloat("_FadeAlphaMultiplier", Mathf.Max(1f, 10f * cubeFadeAlpha));
                        }
                    }

                    foreach (var p in __edgeCubeMeshRenderers)
                    {
                        var key = (p.Key.Item1 + p.Key.Item2) * 0.5f;
                        var cubeFadeAlpha = Mathf.Clamp01((Time.time - dieStartTimeStamp - (key + 1f) * 0.05f) / duration);
                        p.Value.transform.localScale = 0.2f * Vector3.one;

                        foreach (var m in p.Value.materials)
                        {
                            m.SetVector("_BaseColor", new Color(190f / 255f, 0f, 0f, 1f - cubeFadeAlpha));
                            // m.SetFloat("_FadeAlphaMultiplier", Mathf.Max(1f, 10f * cubeFadeAlpha));
                        }
                    }

                    foreach (var b in bloodDropFxInstances)
                        b.transform.rotation = Quaternion.identity;

                    if (meshBuilder.meshRenderer.enabled) meshBuilder.meshRenderer.enabled = false;
                    // foreach (var r in __tailMeshRenderers) 
                    //     if (r.enabled) r.enabled = false;
                }).AddTo(this);
        }

        Tweener __tweener;
        IDisposable __hookingPointFxDisposable;

        void SetHitColorMaterialEnabled(MeshRenderer meshRenderer, bool newEnabled)
        {
            if (newEnabled)
            {    
                if (__cubeMeshHitColorMaterials.TryGetValue(meshRenderer, out var material))
                {
                    var tempAlpha = meshRenderer.material.GetColor("_BaseColor").a;
                    meshRenderer.material = material;
                    meshRenderer.material.SetColor("_BaseColor", Color.white.AdjustAlpha(tempAlpha));
                }
                else
                {
                    var tempAlpha = meshRenderer.material.GetColor("_BaseColor").a;
                    meshRenderer.material = cubeHitColorMaterial;
                    meshRenderer.material.SetColor("_BaseColor", Color.white.AdjustAlpha(tempAlpha));
                    
                    __cubeMeshHitColorMaterials.Add(meshRenderer, meshRenderer.material);
                }
            }
            else
            {
                meshRenderer.material = __cubeMeshMaterials[meshRenderer];
            }
        }

        public void ShowHitColor(float duration)
        {
            __hitColorDisposable?.Dispose();

            foreach (var p in __cubeMeshRenderers) SetHitColorMaterialEnabled(p.Value, true);
            foreach (var p in __edgeCubeMeshRenderers) SetHitColorMaterialEnabled(p.Value, true);
            __hairSharedMaterial.SetColor("_EmissiveColor", Color.white.AdjustAlpha(1f));
            __hookSharedMaterial.SetColor("_EmissiveColor", Color.white.AdjustAlpha(1f));

            foreach (var a in eyeAttachments)
            {
                foreach (var r in a.eyelidRenderers) r.material.SetColor("_EmissiveColor", Color.gray.AdjustAlpha(1f));
                a.eyeAnimator.MinOpenValue = 0f;
            }

            __hitColorDisposable = Observable.Timer(TimeSpan.FromSeconds(duration))
                .DoOnCompleted(() => 
                {
                    __hitColorDisposable = null;
                    __hairSharedMaterial.SetColor("_EmissiveColor", __defaultHairEmissiveColor);
                    __hookSharedMaterial.SetColor("_EmissiveColor", __defaultHookEmissiveColor);
                    foreach (var p in __cubeMeshRenderers) SetHitColorMaterialEnabled(p.Value, false);
                    foreach (var p in __edgeCubeMeshRenderers) SetHitColorMaterialEnabled(p.Value, false); 
                    foreach (var a in eyeAttachments)
                        foreach (var r in a.eyelidRenderers) r.material.SetColor("_EmissiveColor", __defaultEyeEmissiveColor);
                })
                .DoOnCancel(() => 
                {
                    __hitColorDisposable = null;
                    __hairSharedMaterial.SetColor("_EmissiveColor", __defaultHairEmissiveColor);
                    __hookSharedMaterial.SetColor("_EmissiveColor", __defaultHookEmissiveColor);
                    foreach (var p in __cubeMeshRenderers) SetHitColorMaterialEnabled(p.Value, false);
                    foreach (var p in __edgeCubeMeshRenderers) SetHitColorMaterialEnabled(p.Value, false);
                    foreach (var a in eyeAttachments)
                        foreach (var r in a.eyelidRenderers) r.material.SetColor("_EmissiveColor", __defaultEyeEmissiveColor);
                })
                .Subscribe().AddTo(this);

            __tweener?.Complete();
            __tweener = springMassSystem.bodyOffsetPoint.DOShakePosition(duration, 1f);

            EffectManager.Instance.Show(onHitFx, springMassSystem.bodyOffsetPoint.position, Quaternion.identity, Vector3.one);
        }

        public void PopRandomCube(int popCount)
        {
            while (popCount > 0)
            {
                var picked = __nonRigidBodyCubeMeshRenderers.ElementAt(UnityEngine.Random.Range(0, __nonRigidBodyCubeMeshRenderers.Count));
                var rigidBody = GameObject.Instantiate(picked.gameObject, popping).AddComponent<BoxCollider>().gameObject.AddComponent<Rigidbody>();
                rigidBody.gameObject.layer = LayerMask.NameToLayer("PhysicsBody");
                // rigidBody.transform.localScale = 0.5f * rigidBody.transform.localScale;
                rigidBody.mass = 0.1f;
                rigidBody.AddExplosionForce(20f, springMassSystem.bodyOffsetPoint.position, 5f);

                Observable.Timer(TimeSpan.FromSeconds(0.2f)).Subscribe(_ =>
                {
                    rigidBody.GetComponent<MeshRenderer>().material = __cubeMeshMaterials[picked];
                }).AddTo(picked);

                Observable.Timer(TimeSpan.FromSeconds(2f))
                    .DoOnCompleted(() => Destroy(rigidBody.gameObject))
                    .DoOnCancel(() => Destroy(rigidBody.gameObject))
                    .Subscribe().AddTo(this);

                popCount--;
            }
        }
    }
}