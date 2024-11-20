using System;
using System.Linq;
using PixelCamera;
using UniRx;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game
{
    public class CameraController : MonoBehaviour
    {
        [Header("Component")]
        public Camera viewCamera;
        public Transform cameraTransform;
        public PixelCameraManager pixelCameraManager;

        [Header("Parameter")]
        [Range(0.1f, 1f)]
        public float zoom = 1f;
        public float zoomSpeed = 1f;

        public Quaternion SpriteLookRotation => cameraTransform != null ? Quaternion.LookRotation(cameraTransform.forward, Vector3.up) : Quaternion.identity;

        public void Shake(float strength, float duration)
        {
            __shakeStrength = strength;
            __shakeDuration = duration;
            __shakeTimeStamp = Time.time;
        }

        float __shakeStrength;
        float __shakeDuration;
        float __shakeTimeStamp;

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
            var startZoom = pixelCameraManager.ViewCameraZoom;
            var alpha = 0f;

            return Observable.EveryLateUpdate()
                .TakeWhile(_ => !Mathf.Approximately(pixelCameraManager.ViewCameraZoom, targetZoom))
                .Do(_ =>
                {
                    alpha += Time.deltaTime / duration;
                    var val = alpha * alpha;
                    pixelCameraManager.ViewCameraZoom = Mathf.Lerp(startZoom, targetZoom, val);
                    //Debug.Log($"zoom : {val}, {pixelCameraManager.ViewCameraZoom}");
                });
        }


        Vector3 __currFocusPoint;
        IDisposable __zoomDisposable;

        void LateUpdate()
        {
            if (cameraTransform == null || GameContext.Instance.playerCtrler == null || GameContext.Instance.playerCtrler.MyHeroBrain == null)
                return;

            __currFocusPoint = GameContext.Instance.playerCtrler.MyHeroBrain.coreColliderHelper.transform.position;
            pixelCameraManager.ViewCameraZoom = pixelCameraManager.ViewCameraZoom.LerpSpeed(zoom, zoomSpeed, Time.deltaTime);
            cameraTransform.position = __currFocusPoint - 10f * cameraTransform.transform.forward;

            if ((Time.time - __shakeTimeStamp) < __shakeDuration)
                cameraTransform.transform.position = cameraTransform.transform.position + (Vector3)(__shakeStrength * UnityEngine.Random.insideUnitCircle);
        }

        public bool TryGetPickingPointOnTerrain(Vector3 mousePoint, out Vector3 result)
        {
            if (viewCamera != null)
            {
                var ray = viewCamera.ScreenPointToRay(mousePoint);
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
    }
}
