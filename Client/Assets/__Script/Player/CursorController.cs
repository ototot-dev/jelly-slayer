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
            cursor.localRotation *= Quaternion.Euler(0f, 180f * Time.deltaTime, 0f);

            if (GameContext.Instance.cameraCtrler != null && GameContext.Instance.cameraCtrler.viewCamera != null && __playerCtrler.MyHeroBrain != null && __playerCtrler.MyHeroBrain.BB.TargetBrain != null)
            {
                var targetCapsule = __playerCtrler.MyHeroBrain.BB.TargetBrain.coreColliderHelper.GetCapsuleCollider();
                if (targetCapsule != null)
                    cursor.position = targetCapsule.transform.position + targetCapsule.height * Vector3.up;
                // cursor.position = __playerCtrler.MyHeroBrain.core.transform.position +
                //     Quaternion.AngleAxis(GameContext.Instance.cameraCtrler.MainCamera.transform.eulerAngles.y, Vector3.up) * new Vector3(__playerCtrler.moveVec.Value.x, 0, __playerCtrler.moveVec.Value.y);
            }
        }

        /// <summary>
        /// 커서를 지면 위 Picking Point로 이동
        /// </summary>
        /// <param name="mousePoint"></param>
        /// <returns></returns>
        public bool TryMoveCursorPositionOnTerrain(Vector3 mousePoint)
        {
            if (GameContext.Instance.cameraCtrler.viewCamera != null)
            {
                var ray = GameContext.Instance.cameraCtrler.viewCamera.ScreenPointToRay(mousePoint);
                var hitPoints = Physics.RaycastAll(ray, 9999f, LayerMask.GetMask(TerrainManager.LayerName));

                if (hitPoints.Length > 0)
                {
                    cursor.position = hitPoints.OrderBy(h => h.distance).ToArray().First().point + Vector3.up * 0.25f;
                    return true;
                }
            }

            return false;
        }
    }
}