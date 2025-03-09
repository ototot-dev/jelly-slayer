using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    /// <summary>
    /// 플레이어 조작 커서 컨트롤러
    /// </summary>
    public class CursorController : MonoBehaviour
    {
        [Header("Component")]
        public Transform cursor;
        public Vector3 CurrPosition => cursor.position;
        PlayerController __playerCtrler;

        void Awake()
        {
            cursor = Instantiate(Resources.Load<GameObject>("Player/Cursor")).transform;
            __playerCtrler = GetComponent<PlayerController>();
        }

        void Update()
        {
            //* 자전 효과
            cursor.localRotation *= Quaternion.Euler(0f, 180f * Time.deltaTime, 0f);
            cursor.localScale = (0.1f * Mathf.Sin(Time.time * 10f) + 1f) * Vector3.one;

            // if (GameContext.Instance.cameraCtrler != null && GameContext.Instance.cameraCtrler.viewCamera != null && __playerCtrler.MyHeroBrain != null && __playerCtrler.MyHeroBrain.BB.TargetBrain != null)
            // {
            //     var targetCapsule = __playerCtrler.MyHeroBrain.BB.TargetBrain.coreColliderHelper.GetCapsuleCollider();
            //     if (targetCapsule != null)
            //         cursor.position = targetCapsule.transform.position + targetCapsule.height * Vector3.up;
            // }
        }

        /// <summary>
        /// 커서를 지면 위 Picking Point로 이동
        /// </summary>
        /// <param name="mousePoint"></param>
        /// <returns></returns>
        public bool TryMoveCursorPositionOnTerrain(Vector3 mousePoint)
        {
            if (GameContext.Instance.mainCameraCtrler.viewCamera != null)
            {
                var ray = GameContext.Instance.mainCameraCtrler.viewCamera.ScreenPointToRay(mousePoint);
                var hitPoints = Physics.RaycastAll(ray, 9999f, LayerMask.GetMask(TerrainManager.LayerName));

                if (hitPoints.Length > 0)
                {
                    cursor.position = hitPoints.OrderBy(h => h.distance).ToArray().First().point + Vector3.up * 0.25f;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetPickingPointOnTerrain(Vector3 mousePoint, out Vector3 result)
        {
            if (GameContext.Instance.mainCameraCtrler.viewCamera != null)
            {
                var ray = GameContext.Instance.mainCameraCtrler.viewCamera.ScreenPointToRay(mousePoint);
                if (Physics.Raycast(ray, out var hit, 9999f, LayerMask.GetMask(TerrainManager.LayerName)))
                {
                    result = hit.point;
                    return true;
                }
            }

            result = Vector3.zero;
            return false;
        }
    }
}