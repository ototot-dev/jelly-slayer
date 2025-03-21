using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 플레이어 조작 커서 컨트롤러
    /// </summary>
    public class CursorController : MonoBehaviour
    {
        [Header("Component")]
        public Transform forwardCursor;
        public Transform targetCursor;
        PlayerController PlayerCtrler => transform.parent != null ? transform.parent.GetComponent<PlayerController>() : null;
        HeroBrain PossessedBrain => PlayerCtrler != null? PlayerCtrler.possessedBrain : null;

        LineRenderer GetLineRenderer(string lineName)
        {
            if (!__lineRenderers.ContainsKey(lineName))  __lineRenderers.Add(lineName, gameObject.AddComponent<LineRenderer>());
            return __lineRenderers[lineName];
        }
        Dictionary<string, LineRenderer> __lineRenderers = new();
        const string __FORWARD_LINE = "Forwaord";

        void DrawLineSimple(string lineName, Vector3 start, Vector3 end, Color color)
        {
            var lineRenderer = GetLineRenderer(lineName);
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);

            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;

            // 머터리얼 설정 (없으면 안 보임)
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }


        void Awake()
        {
            forwardCursor = Instantiate(Resources.Load<GameObject>("Player/Cursor")).transform;
            // targetCursor = Instantiate(Resources.Load<GameObject>("Player/Cursor")).transform;
        }

        void LateUpdate()
        {
            return;
            //* 자전 효과
            // forwardCursor.localRotation *= Quaternion.Euler(0f, 180f * Time.deltaTime, 0f);
            forwardCursor.localScale = (0.1f * Mathf.Sin(Time.time * 10f) + 1f) * Vector3.one;
            targetCursor.localScale = (0.1f * Mathf.Sin(Time.time * 10f) + 1f) * Vector3.one;

            forwardCursor.transform.position = PossessedBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
            DrawLineSimple(__FORWARD_LINE, PossessedBrain.GetWorldPosition(), forwardCursor.transform.position, Color.magenta);

            foreach (var c in PossessedBrain.PawnSensorCtrler.WatchingColliders)
            {
                if (c.TryGetComponent<PawnColliderHelper>(out var colliderHelper))
                    targetCursor.transform.position = colliderHelper.pawnBrain.GetWorldPosition() + colliderHelper.GetScaledCapsuleHeight() * Vector3.up;
            }

            // if (PossessedBrain == null) return;

            // if (PossessedBrain.BB.TargetBrain != null)
            // {
            //     targetCursor.transform.position = PossessedBrain.BB.TargetBrain.GetWorldPosition() + PossessedBrain.BB.TargetBrain.coreColliderHelper.GetScaledCapsuleHeight() * Vector3.up;
            // }
            // else
            // {

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
                    forwardCursor.position = hitPoints.OrderBy(h => h.distance).ToArray().First().point + Vector3.up * 0.25f;
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

        bool FindAttackPoint(float attackRange, out Vector3 attackPoint)
        {
            var jellyCollider = PossessedBrain.SensorCtrler.WatchingColliders.FirstOrDefault(c => c.TryGetComponent<PawnColliderHelper>(out var helper) && helper.gameObject.CompareTag("Jelly") && (helper.transform.position - PossessedBrain.GetWorldPosition()).Magnitude2D() < attackRange);
            if (jellyCollider != null)
            {
                attackPoint = jellyCollider.transform.position;
                return true;
            }

            var found = PossessedBrain.SensorCtrler.WatchingColliders.Select(c => c.GetComponent<PawnColliderHelper>()).Where(h => h != null && h == h.pawnBrain.coreColliderHelper)
                .OrderBy(h => Vector3.Angle(PossessedBrain.coreColliderHelper.transform.forward.Vector2D(), (h.transform.position - PossessedBrain.GetWorldPosition()).Vector2D()))
                .FirstOrDefault(h => (h.transform.position - PossessedBrain.GetWorldPosition()).Magnitude2D() < attackRange);

            if (found != null)
            {
                attackPoint = found.transform.position;
                return true;
            }
            else
            {
                attackPoint = Vector3.zero;
                return false;
            }
        }
    }
}