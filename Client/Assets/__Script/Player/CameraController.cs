using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using PixelCamera;
using UniRx;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Game
{
    public class CameraController : MonoBehaviour
    {
        [Header("Component")]
        public Camera gameCamera;
        public PixelCameraManager pixelCamera;
        public CinemachineBrain cinemachineBrain;
        public CinemachineVirtualCamera virtualCamera;
        public CinemachineConfiner cinemachineConfiner;

        [Header("Parameter")]
        [Range(0.1f, 2f)]
        public float zoom = 1f;
        public float zoomSpeed = 1f;
        public float yawAngleOnGrouned = 40f;
        public float yawAngleOnHanging = 50f;
        public float yawAngleSpeed = 360f;
        public Quaternion SpriteLookRotation => Quaternion.LookRotation(pixelCamera.transform.forward, Vector3.up);
        public Quaternion BillboardRotation => Quaternion.LookRotation(-pixelCamera.transform.forward, Vector3.up);

        [Header("Volume")]
        public VolumeProfile volumeProfile;

        [Header("Vignette")]
        public float vignetteIntensity = 0.3f;
        public float vignetteSmoothness = 0.2f;
        Vignette __vignette;

        void Awake()
        {
            if (cinemachineBrain != null)
                cinemachineBrain.m_UpdateMethod = CinemachineBrain.UpdateMethod.ManualUpdate;

            Debug.Assert(volumeProfile != null);

            if (volumeProfile.TryGet(out __vignette))
            {
                __vignette.intensity.value = vignetteIntensity;
                __vignette.smoothness.value = vignetteSmoothness;
            }
        }

        public void Shake(float strength, float duration)
        {
            __shakeStrength = strength;
            __shakeDuration = duration;
            __shakeTimeStamp = Time.time;
        }

        float __shakeStrength;
        float __shakeDuration;
        float __shakeTimeStamp;
        IDisposable __zoomDisposable;

        public void InterpolateZoom(float targetZoom, float duration, float waitingTime = 0)
        {
            Debug.Assert(targetZoom > 0);
            Debug.Assert(duration > 0);

            if (__zoomDisposable != null)
            {
                __zoomDisposable.Dispose();
                __zoomDisposable = null;
            }

            __zoomDisposable = waitingTime > 0 ?
                Observable.Timer(TimeSpan.FromSeconds(1)).ContinueWith(_ => InterpolateZoomInternal(targetZoom, duration)).Subscribe().AddTo(this) :
                InterpolateZoomInternal(targetZoom, duration).Subscribe().AddTo(this);
        }

        IObservable<long> InterpolateZoomInternal(float targetZoom, float duration)
        {
            var startZoom = pixelCamera.ViewCameraZoom;
            var alpha = 0f;

            return Observable.EveryLateUpdate()
                .TakeWhile(_ => !Mathf.Approximately(pixelCamera.ViewCameraZoom, targetZoom))
                .Do(_ =>
                {
                    alpha += Time.deltaTime / duration;
                    var val = alpha * alpha;
                    pixelCamera.ViewCameraZoom = Mathf.Lerp(startZoom, targetZoom, val);
                    //Debug.Log($"zoom : {val}, {pixelCameraManager.ViewCameraZoom}");
                });
        }

        bool __prevIsHanging;
        float __currYawInterpSpeed;
        Vector3 __currFocusPoint;
        Vector3 __currTargetPoint;
        HashSet<PawnBrainController> __listeningBrains = new();

        void Update()
        {
            if (!Mathf.Approximately(__vignette.intensity.value, vignetteIntensity))
                __vignette.intensity.value = vignetteIntensity;
            if (!Mathf.Approximately(__vignette.smoothness.value, vignetteSmoothness))
                __vignette.smoothness.value = vignetteSmoothness;
        }

        void LateUpdate()
        {
            if (cinemachineBrain != null)
            {
                cinemachineBrain.ManualUpdate();
                pixelCamera.FollowedTransform.SetPositionAndRotation(cinemachineBrain.CurrentCameraState.FinalPosition, cinemachineBrain.CurrentCameraState.FinalOrientation);
            }


            // if (cameraTransform == null || GameContext.Instance.playerCtrler == null || GameContext.Instance.playerCtrler.possessedBrain == null)
            //     return;

            // //* 줌 처리
            // pixelCamera.ViewCameraZoom = pixelCamera.ViewCameraZoom.LerpSpeed(zoom, zoomSpeed, Time.deltaTime);

            // //* 점프 동작과 같이 y축 변화량이 큰 경우엔 카메라가 대상을 따라가는 y축 속도를 살짝 줄여줌
            // var interpY = __currFocusPoint.y.LerpSpeed(GameContext.Instance.playerCtrler.possessedBrain.GetWorldPosition().y, 4f, Time.deltaTime);
            // __currFocusPoint = GameContext.Instance.playerCtrler.possessedBrain.GetWorldPosition();
            // if (GameContext.Instance.playerCtrler.possessedBrain.BB.TargetBrain != null)
            // {
            //     __currFocusPoint = 0.5f * (__currFocusPoint + GameContext.Instance.playerCtrler.possessedBrain.BB.TargetBrain.GetWorldPosition());
            // }
            // else 
            // {
            //     __listeningBrains.Clear();
            //     foreach (var c in GameContext.Instance.playerCtrler.possessedBrain.PawnSensorCtrler.ListeningColliders)
            //     {
            //         if (c.TryGetComponent<PawnColliderHelper>(out var colliderHelper) && colliderHelper.pawnBrain != null && 
            //             colliderHelper.pawnBrain.CompareTag("Jelly") && !__listeningBrains.Contains(colliderHelper.pawnBrain))
            //             __listeningBrains.Add(colliderHelper.pawnBrain);
            //     }
            //     var positionSum = GameContext.Instance.playerCtrler.possessedBrain.GetWorldPosition();
            //     foreach (var b in __listeningBrains) positionSum += b.GetWorldPosition();

            //     __currFocusPoint = positionSum / (__listeningBrains.Count + 1);
            // }
            // __currFocusPoint.y = interpY;

            // //* 고도 처리
            // var currEulerAngles = cameraTransform.rotation.eulerAngles;
            // if (GameContext.Instance.playerCtrler.possessedBrain.Movement.ReservedHangingBrain != null || GameContext.Instance.playerCtrler.possessedBrain.BB.IsHanging)
            // {
            //     if (!__prevIsHanging)
            //     {
            //         __prevIsHanging = true;
            //         __currYawInterpSpeed = 0f;
            //     }

            //     __currYawInterpSpeed = __currYawInterpSpeed.LerpSpeed(yawAngleSpeed, 100f, Time.deltaTime);
            //     cameraTransform.rotation = Quaternion.Euler(currEulerAngles.x.LerpSpeed(yawAngleOnHanging, __currYawInterpSpeed, Time.deltaTime), currEulerAngles.y, currEulerAngles.z);
            // }
            // else if (GameContext.Instance.playerCtrler.possessedBrain.Movement.IsOnGround)
            // {
            //     if (__prevIsHanging)
            //     {
            //         __prevIsHanging = false;
            //         __currYawInterpSpeed = 0f;
            //     }

            //     __currYawInterpSpeed = __currYawInterpSpeed.LerpSpeed(yawAngleSpeed, 100f, Time.deltaTime);
            //     cameraTransform.rotation = Quaternion.Euler(currEulerAngles.x.LerpSpeed(yawAngleOnGrouned, __currYawInterpSpeed, Time.deltaTime), currEulerAngles.y, currEulerAngles.z);
            // }
            // //__currTargetPoint += 0.1f * (__currFocusPoint - __currTargetPoint);
            // cameraTransform.position = __currFocusPoint - 10f * cameraTransform.transform.forward;

            // if ((Time.time - __shakeTimeStamp) < __shakeDuration)
            //     cameraTransform.transform.position = cameraTransform.transform.position + (Vector3)(__shakeStrength * UnityEngine.Random.insideUnitCircle);
        }

        public bool TryGetPickingPointOnTerrain(Vector3 mousePoint, out Vector3 result)
        {
            if (gameCamera != null)
            {
                var ray = gameCamera.ScreenPointToRay(mousePoint);
                if (Physics.Raycast(ray, out var hit, 9999f, LayerMask.GetMask(TerrainManager.LayerName)))
                {
                    result = hit.point;
                    return true;
                }
            }

            result = Vector3.zero;
            return false;
        }

        public RaycastHit GetPickingResult(Vector3 screenPoint, params string[] layerNames)
        {
            Debug.Assert((layerNames?.Length ?? 0) != 0);

            var ray = Camera.main.ScreenPointToRay(screenPoint);
            var ret = new RaycastHit();

            Physics.Raycast(ray.origin, ray.direction, out ret, 9999, LayerMask.GetMask(layerNames));

            return ret;
        }

        public RaycastHit GetPickingResultEx(Vector3 screenPoint, string[] layerNames, string[] pawnNames)
        {
            if ((pawnNames?.Length ?? 0) > 0)
            {
                var ray = Camera.main.ScreenPointToRay(screenPoint);
                var hits = Physics.RaycastAll(ray.origin, ray.direction, 9999, LayerMask.GetMask(layerNames));

                if (hits.Length == 0)
                {
                    return new RaycastHit();
                }
                else
                {
                    var result = hits.Select(h => new Tuple<RaycastHit, PawnBlackboard>(h, h.collider.GetComponent<PawnBlackboard>()))
                        .Where(t => pawnNames.Contains(t.Item2?.common.pawnName ?? string.Empty))
                        .OrderBy(t => t.Item1.distance)
                        .Select(t => t.Item1).ToArray();

                    return result.Length > 0 ? result[0] : new RaycastHit();
                }
            }
            else
            {
                return GetPickingResult(screenPoint, layerNames);
            }
        }

        public void RefreshConfinerVolume(BoxCollider confinerBoundingBox, Quaternion viewRotation, float viewDistance)
        {
            var halfSize = confinerBoundingBox.size * 0.5f;

            // 로컬 공간 기준 꼭짓점 8개
            var corners = new Vector3[8]
            {
                confinerBoundingBox.center + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z), // 0
                confinerBoundingBox.center + new Vector3(-halfSize.x, -halfSize.y,  halfSize.z), // 1
                confinerBoundingBox.center + new Vector3(-halfSize.x,  halfSize.y, -halfSize.z), // 2
                confinerBoundingBox.center + new Vector3(-halfSize.x,  halfSize.y,  halfSize.z), // 3
                confinerBoundingBox.center + new Vector3( halfSize.x, -halfSize.y, -halfSize.z), // 4
                confinerBoundingBox.center + new Vector3( halfSize.x, -halfSize.y,  halfSize.z), // 5
                confinerBoundingBox.center + new Vector3( halfSize.x,  halfSize.y, -halfSize.z), // 6
                confinerBoundingBox.center + new Vector3( halfSize.x,  halfSize.y,  halfSize.z), // 7
            };

            // 월드 공간으로 변환
            for (int i = 0; i < 8; i++)
                corners[i] = confinerBoundingBox.transform.TransformPoint(corners[i]);

            var viewVec = viewRotation * Vector3.forward;
            var viewPosition = confinerBoundingBox.transform.position - viewDistance * viewVec;
            var viewMatrix = Matrix4x4.TRS(viewPosition, viewRotation, Vector3.one);
            var inverseViewMatrix = viewMatrix.inverse;

            for (int i = 0; i < 8; i++)
                corners[i] = inverseViewMatrix.MultiplyPoint(corners[i]);

            var maxCorner = new Vector3(corners.Max(c => c.x), corners.Max(c => c.y), corners.Max(c => c.z));
            var minCorner = new Vector3(corners.Min(c => c.x), corners.Min(c => c.y), corners.Min(c => c.z));

            var confinerVolume = cinemachineConfiner.m_BoundingVolume as BoxCollider;
            confinerVolume.size = (maxCorner - minCorner).AdjustZ(viewDistance * 2f);
            confinerVolume.center = Vector3.zero;

            var newCenter = 0.5f * (minCorner + maxCorner);
            var newPosition = viewMatrix.MultiplyPoint(newCenter);
            newPosition += Vector3.Distance(newPosition, viewPosition) * -viewVec;
            confinerVolume.transform.SetPositionAndRotation(newPosition, viewRotation);
        }
    }
}
