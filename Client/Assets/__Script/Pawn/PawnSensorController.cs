using System;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Game
{
    public class PawnSensorController : MonoBehaviour
    {
        [Header("Length")]
        public float listenLen = 1;
        public float visionLen = 1;
        public float visionAngle = 60;
        public float nearVisionLen = 1;

        [Header("Component")]
        public BoxCollider touchSensorTrigger;
        public CapsuleCollider soundSensorTrigger;
        public HashSet<Collider> TouchingColliders { get; private set; } = new();
        public HashSet<Collider> ListeningColliders { get; private set; } = new();
        public HashSet<Collider> WatchingColliders { get; private set; } = new();

        /// <summary>
        /// Listen 영역 안에 Random-Point를 리턴
        /// </summary>
        /// <param name="rangeScale"></param>
        /// <returns></returns>
        public Vector3 GetRandomPointInListeningArea(Vector3 domainForwardVec, float randAngle, float rangeScale = 1)
        {
            var randPoint = __pawnBrain.coreColliderHelper.transform.position;
            randPoint += Quaternion.Euler(0, UnityEngine.Random.Range(-randAngle, randAngle), 0) * domainForwardVec * UnityEngine.Random.Range(listenLen * rangeScale * 0.5f, listenLen * rangeScale);

            return TerrainManager.GetTerrainPoint(randPoint);
        }

        /// <summary>
        /// 도망칠 위치 리턴
        /// </summary>
        /// <param name="hazardPoint"></param>
        /// <param name="distanceScale"></param>
        /// <returns></returns>
        public Vector3 GetFleePoint(Vector3 hazardPoint, float distanceScale = 1)
        {
            var fleePoint = __pawnBrain.coreColliderHelper.transform.position + (__pawnBrain.coreColliderHelper.transform.position - hazardPoint).Vector2D().normalized * listenLen * distanceScale;
            return TerrainManager.GetTerrainPoint(fleePoint);
        }

        void Awake()
        {
            __pawnBrain = GetComponent<PawnBrainController>();
            __cosAOV = Mathf.Cos(visionAngle * Mathf.Deg2Rad * 0.5f);
            __colliderHeight = touchSensorTrigger != null ? touchSensorTrigger.size.y : 1;

            if (soundSensorTrigger != null)
            {
                soundSensorTrigger.center = Vector3.zero;
                soundSensorTrigger.radius = listenLen;

                if (touchSensorTrigger != null)
                {
                    soundSensorTrigger.height = 2 * listenLen + __colliderHeight;
                    soundSensorTrigger.center = 0.5f * __colliderHeight * Vector3.up;
                }
            }
        }

        PawnBrainController __pawnBrain;
        float __colliderHeight = 0.5f;
        float __cosAOV = 1;

        void Start()
        {
            if (touchSensorTrigger != null) 
            {
                touchSensorTrigger.OnTriggerEnterAsObservable().Subscribe(c =>
                {
                    if (c.gameObject.CompareTag("TerrainBoundary"))
                    {
                        __pawnBrain.OnTouchTerrainBoundaryHandler(c.gameObject);
                    }
                    else if (!c.TryGetComponent<PawnColliderHelper>(out var colliderHelper) || colliderHelper.pawnBrain != __pawnBrain)
                    {
                        TouchingColliders.Add(c);
                        onTouchSomething?.Invoke(c);
                    }
                }).AddTo(this);
            }

            if (touchSensorTrigger != null) 
                touchSensorTrigger.OnTriggerExitAsObservable().Subscribe(c => TouchingColliders.Remove(c)).AddTo(this);

            if (soundSensorTrigger != null)
                soundSensorTrigger.OnTriggerEnterAsObservable().Subscribe(c => ListeningColliders.Add(c)).AddTo(this);

            if (soundSensorTrigger != null) 
                soundSensorTrigger.OnTriggerExitAsObservable().Subscribe(c => ListeningColliders.Remove(c)).AddTo(this);

            if (soundSensorTrigger != null)
                __pawnBrain.onPreTick += OnTickHandler;
        }

        public void OnTickHandler(float interval)
        {
            var anyMissingReference = false;

            foreach (var l in ListeningColliders)
            {
                if (l == null)
                {
                    anyMissingReference = true;
                }
                else if (l.TryGetComponent<PawnColliderHelper>(out var colliderHelper))
                {
                    if (colliderHelper.IsSoundSensorTriggerable && colliderHelper.pawnBrain.PawnSoundSourceGen != null && colliderHelper.pawnBrain.PawnSoundSourceGen.QuerySoundSource(0, __prevTickTimeStamp, out var result))
                        onListenSomething?.Invoke(result);

                    var distanceVec = (l.transform.position - __pawnBrain.coreColliderHelper.transform.position).Vector2D();
                    var dot = Vector3.Dot(__pawnBrain.coreColliderHelper.transform.forward, distanceVec.normalized);

                    if ((distanceVec.sqrMagnitude <= nearVisionLen * nearVisionLen && dot >= 0) || (distanceVec.sqrMagnitude <= visionLen * visionLen && dot >= __cosAOV))
                    {
                        if (colliderHelper.IsVisionSensorTriggerable && WatchingColliders.Add(l))
                        {
                            WatchingColliders.Add(l);
                            onWatchSomething?.Invoke(l);
                            __Logger.VerboseR(gameObject, "WatchingColliders.Add()", nameof(l.name), l.name);
                        }
                    }
                    else
                    {
                        if (WatchingColliders.Remove(l))
                            __Logger.VerboseR(gameObject, "WatchingColliders.Remove()", nameof(l.name), l.name);
                    }
                }
            }

            __prevTickTimeStamp = Time.time;

            if (anyMissingReference)
                ResolveMissingReference();
        }

        float __prevTickTimeStamp;
        public Action<PawnSoundSourceGenerator.SoundSource> onListenSomething;
        public Action<Collider> onTouchSomething;
        public Action<Collider> onWatchSomething;
        public Action onResolveMissingReference;

        public void ResolveMissingReference()
        {
            TouchingColliders.RemoveWhere(t => t == null);
            ListeningColliders.RemoveWhere(l => l == null);
            WatchingColliders.RemoveWhere(w => w == null);
            onResolveMissingReference?.Invoke();
        }

        public void TurnOn()
        {
            if (touchSensorTrigger != null)
                touchSensorTrigger.enabled = true;
            if (soundSensorTrigger != null)
                soundSensorTrigger.enabled = true;
        }

        public void TurnOff()
        {
            if (touchSensorTrigger != null)
                touchSensorTrigger.enabled = false;
            if (soundSensorTrigger != null)
                soundSensorTrigger.enabled = false;

            TouchingColliders.Clear();
            ListeningColliders.Clear();
            WatchingColliders.Clear();
        }

        public bool drawGizmosEnabled;

        void OnDrawGizmosSelected()
        {
            if (drawGizmosEnabled)
            {
                Gizmos.color = Color.green;
                if (soundSensorTrigger != null && soundSensorTrigger.enabled)
                    GizmosDrawExtension.DrawFanCylinder(soundSensorTrigger.transform.position + soundSensorTrigger.center, soundSensorTrigger.transform.parent.forward, visionLen, visionAngle, __colliderHeight, 12);
            }
        }
    }
}