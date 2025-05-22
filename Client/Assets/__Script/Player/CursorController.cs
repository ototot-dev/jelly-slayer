using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using TMPro;
using System;

namespace Game
{
    /// <summary>
    /// 플레이어 조작 커서 컨트롤러
    /// </summary>
    public class CursorController : MonoBehaviour
    {
        public bool drawForward;
        public bool drawAttackPoint;
        public bool drawTargetInfo;
        public bool drawActionInfo;
        public bool drawSensorInfo;
        public bool drawPawnStatus;
        public Transform sourceCubeMesh;
        public Transform sourceConeMesh;
        public Transform sourceLineRenderer;
        public TextMeshPro sourceTextMesh;
        public PawnBrainController hostBrain;

        const string __FACE_ARROW = "__FACE_ARROW";
        const string __ATTACK_ARROW = "__ATTACK_ARROW";
        const string __SOURCE_POINT = "__SOURCE_POINT";
        const string __TARGET_POINT = "__TARGET_POINT";
        const string __TARGET_DIST = "__TARGET_DIST";
        const string __PAWN_STATUS = "__PAWN_STATUS";
        readonly string[] __ACTION_START = new string[] { "__ACTION_START_1", "__ACTION_START_2", "__ACTION_START_3", "__ACTION_START_4", "__ACTION_START_5", "__ACTION_START_6", "__ACTION_START_7", "__ACTION_START_8" };
        readonly string[] __ACTION_FINISH = new string[] { "__ACTION_FINISH_1", "__ACTION_FINISH_2", "__ACTION_FINISH_3", "__ACTION_FINISH_4", "__ACTION_FINISH_5", "__ACTION_FINISH_6", "__ACTION_FINISH_7", "__ACTION_FINISH_8" };
        readonly string[] __ACTION_DIST = new string[] { "__ACTION_DIST_1", "__ACTION_DIST_2", "__ACTION_DIST_3", "__ACTION_DIST_4", "__ACTION_DIST_5", "__ACTION_DIST_6", "__ACTION_DIST_7", "__ACTION_DIST_8" };
        readonly string[] __SENSOR_INFO = new string[] { "__SENSOR_INFO_1", "__SENSOR_INFO_2", "__SENSOR_INFO_3", "__SENSOR_INFO_4", "__SENSOR_INFO_5", "__SENSOR_INFO_6", "__SENSOR_INFO_7", "__SENSOR_INFO_8", "__SENSOR_INFO_9", "__SENSOR_INFO_10" };

#region Helper-Function
        Dictionary<string, MeshRenderer> __coneMeshes = new();
        Dictionary<string, MeshRenderer> __cubeMeshes = new();
        Dictionary<string, TextMeshPro> __textMeshes = new();
        Dictionary<string, LineRenderer> __lineRenderers = new();
        MeshRenderer GetCubeMesh(string meshAlias)
        {
            if (__cubeMeshes.Count == 0)
            {
                __cubeMeshes.Add(meshAlias, sourceCubeMesh.transform.GetChild(0).GetComponent<MeshRenderer>());
                __cubeMeshes[meshAlias].transform.parent.name = "Cube" + meshAlias;
            }
            else if (!__cubeMeshes.ContainsKey(meshAlias))
            {
                __cubeMeshes.Add(meshAlias, Instantiate(sourceCubeMesh.gameObject, transform).transform.GetChild(0).GetComponent<MeshRenderer>());
                __cubeMeshes[meshAlias].transform.parent.name = "Cube" + meshAlias;
            }

            return __cubeMeshes[meshAlias];
        }
        MeshRenderer GetConeMesh(string meshAlias)
        {
            if (__coneMeshes.Count == 0)
            {
                __coneMeshes.Add(meshAlias, sourceConeMesh.transform.GetChild(0).GetComponent<MeshRenderer>());
                __coneMeshes[meshAlias].transform.parent.name = "Cube" + meshAlias;
            }
            else if (!__coneMeshes.ContainsKey(meshAlias))
            {
                __coneMeshes.Add(meshAlias, Instantiate(sourceConeMesh.gameObject, transform).transform.GetChild(0).GetComponent<MeshRenderer>());
                __coneMeshes[meshAlias].transform.parent.name = "Cube" + meshAlias;
            }

            return __coneMeshes[meshAlias];
        }
        TextMeshPro GetTextMesh(string textAlias)
        {
            if (__textMeshes.Count == 0)
            {
                __textMeshes.Add(textAlias, sourceTextMesh);
                __textMeshes[textAlias].name = "Text" + textAlias;
            }
            else if (!__textMeshes.ContainsKey(textAlias))
            {
                __textMeshes.Add(textAlias, Instantiate(sourceTextMesh.gameObject, transform).GetComponent<TextMeshPro>());
                __textMeshes[textAlias].name = "Text" + textAlias;
            }

            return __textMeshes[textAlias];
        }
        LineRenderer GetLineRenderer(string lineAlias)
        {
            if (__lineRenderers.Count == 0)
            {
                __lineRenderers.Add(lineAlias, sourceLineRenderer.GetComponent<LineRenderer>());
                __lineRenderers[lineAlias].name = "Line" + lineAlias;
            }
            else if (!__lineRenderers.ContainsKey(lineAlias))
            {
                __lineRenderers.Add(lineAlias, Instantiate(sourceLineRenderer.gameObject, transform).GetComponent<LineRenderer>());
                __lineRenderers[lineAlias].name = "Line" + lineAlias;
            }

            return __lineRenderers[lineAlias];
        }

        Vector3 GetCubePosition(string meshAlias, float offsetY = 0f) { return GetCubeMesh(meshAlias).transform.parent.position + offsetY * Vector3.up; }
        Vector3 GetConePosition(string meshAlias, float offsetY = 0f) { return GetConeMesh(meshAlias).transform.parent.position + offsetY * Vector3.up; }
        Quaternion GetCubeRotation(string meshAlias, float offsetYaw = 0f) { return GetCubeMesh(meshAlias).transform.parent.rotation * Quaternion.Euler(0f, offsetYaw, 0f); }
        Quaternion GetConeRotation(string meshAlias, float offsetYaw = 0f) { return GetConeMesh(meshAlias).transform.parent.rotation * Quaternion.Euler(0f, offsetYaw, 0f);; }
        void SetCubePositionAndRotation(string meshAlias, Vector3 position, Quaternion rotation, float offsetY = 0f, float offsetYaw = 0f) { GetCubeMesh(meshAlias).transform.parent.SetPositionAndRotation(position + offsetY * Vector3.up, rotation * Quaternion.Euler(0f, offsetYaw, 0f)); }
        void SetConePositionAndRotation(string meshAlias, Vector3 position, Quaternion rotation, float offsetY = 0f, float offsetYaw = 0f) { GetConeMesh(meshAlias).transform.parent.SetPositionAndRotation(position + offsetY * Vector3.up, rotation * Quaternion.Euler(0f, offsetYaw, 0f)); }
        float GetCubeDistance(string meshAliasA, string meshAliasB) { return Vector3.Distance(GetCubePosition(meshAliasA), GetCubePosition(meshAliasB)); }
        float GetConeDistance(string meshAliasA, string meshAliasB) { return Vector3.Distance(GetConePosition(meshAliasA), GetConePosition(meshAliasB)); }
        Vector3 LerpCubePosition(string meshAliasA, string meshAliasB, float alpha, float offsetY = 0f) { return Vector3.Lerp(GetCubePosition(meshAliasA), GetCubePosition(meshAliasB), alpha) + offsetY * Vector3.up; }
        Vector3 LerpConePosition(string meshAliasA, string meshAliasB, float alpha, float offsetY = 0f) { return Vector3.Lerp(GetConePosition(meshAliasA), GetConePosition(meshAliasB), alpha) + offsetY * Vector3.up; }
        Vector3 GetHeadRoomPosition(PawnBrainController pawnBrain, float offsetY = 0f) { return pawnBrain.GetWorldPosition() + (pawnBrain.coreColliderHelper.GetScaledCapsuleRadius() + pawnBrain.coreColliderHelper.GetScaledCapsuleHeight() + offsetY) * Vector3.up; }
        Vector3 GetGroundPosition(PawnBrainController pawnBrain, float offsetY = 0f) { return pawnBrain.GetWorldPosition() + offsetY * Vector3.up; }
#endregion
        
        void DrawCube(string meshAlias, Vector3 position, Quaternion rotation, float size, Color color)
        {
            var meshRenderer = GetCubeMesh(meshAlias);
            meshRenderer.transform.parent.gameObject.SetActive(true);
            meshRenderer.transform.parent.SetPositionAndRotation(position, rotation);
            meshRenderer.transform.localScale = size * Vector3.one;
            meshRenderer.material.color = color;
        }
        void DrawCone(string meshAlias, Vector3 position, Quaternion rotation, float size, Color color)
        {
            var meshRenderer = GetConeMesh(meshAlias);
            meshRenderer.transform.parent.gameObject.SetActive(true);
            meshRenderer.transform.parent.SetPositionAndRotation(position, rotation);
            meshRenderer.transform.localScale = size * Vector3.one;
            meshRenderer.material.color = color;
        }
        void DrawLine(string lineAlias, Vector3 start, Vector3 end, Color color, float width)
        {
            DrawLine(lineAlias, start, end, color, color, width, width);
        }
        void DrawLine(string lineAlias, Vector3 start, Vector3 end, Color startColor, Color endColor, float width)
        {
            DrawLine(lineAlias, start, end, startColor, endColor, width, width);
        }
        void DrawLine(string lineAlias, Vector3 start, Vector3 end, Color startColor, Color endColor, float startWidth, float endWidth)
        {
            var lineRenderer = GetLineRenderer(lineAlias);
            lineRenderer.gameObject.SetActive(true);
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
            lineRenderer.startWidth = startWidth;
            lineRenderer.endWidth = endWidth;
            lineRenderer.startColor = startColor;
            lineRenderer.endColor = endColor;
        }
        void DrawText(string textAlias, Vector3 position, string content, float fontSize, Color color)
        {
            var textMesh = GetTextMesh(textAlias);
            textMesh.gameObject.SetActive(true);
            textMesh.transform.localScale = 0.5f * Vector3.one;
            textMesh.transform.SetPositionAndRotation(position, Quaternion.LookRotation(GameContext.Instance.cameraCtrler.gameCamera.transform.forward));
            textMesh.text = content;
            textMesh.color = color;
            textMesh.fontSize = fontSize;
        }

        void HideCube(string meshAlias) { GetCubeMesh(meshAlias).transform.parent.gameObject.SetActive(false); }
        void HideCone(string meshAlias) { GetConeMesh(meshAlias).transform.parent.gameObject.SetActive(false); }
        void HideLine(string lineAlias) { GetLineRenderer(lineAlias).gameObject.SetActive(false); }
        void HideText(string textAlias) { GetTextMesh(textAlias).gameObject.SetActive(false); }

        void LateUpdate()
        {
            if (hostBrain == null) return;

            if (drawForward)
            {
                DrawCone(__FACE_ARROW, GetGroundPosition(hostBrain, 0.05f) + hostBrain.GetWorldForward(), Quaternion.LookRotation(hostBrain.GetWorldForward()), 0.15f, Color.cyan);
                DrawLine(__FACE_ARROW, GetGroundPosition(hostBrain, 0.05f), GetConePosition(__FACE_ARROW), Color.cyan.AdjustAlpha(0f), Color.cyan, 0.05f);
            }

            if (drawAttackPoint && FindAttackPoint(4f, out var attackPoint))
            {
                var attackForward = (attackPoint - hostBrain.GetWorldPosition()).Vector2D().normalized;
                DrawCone(__ATTACK_ARROW, 0.5f * (GetGroundPosition(hostBrain, 0.05f) + (attackPoint + 0.05f * Vector3.up)), Quaternion.LookRotation(attackForward), 0.2f, Color.red);
                DrawLine(__ATTACK_ARROW, hostBrain.GetWorldPosition(), attackPoint, Color.red, Color.red.AdjustAlpha(0f), 0.1f);
            }
            else
            {
                HideCone(__ATTACK_ARROW);
                HideLine(__ATTACK_ARROW);
            }

            if (drawTargetInfo && hostBrain.PawnBB.TargetBrain != null)
            {
                DrawCube(__SOURCE_POINT, GetHeadRoomPosition(hostBrain), GetCubeRotation(__SOURCE_POINT) * Quaternion.Euler(0f, 180f * Time.deltaTime, 0f), 0.1f, Color.white);
                DrawCube(__TARGET_POINT, GetHeadRoomPosition(hostBrain.PawnBB.TargetBrain), GetCubeRotation(__TARGET_POINT, 180f * Time.deltaTime), 0.15f, Color.white);
                DrawLine(__TARGET_POINT, GetHeadRoomPosition(hostBrain), GetCubePosition(__TARGET_POINT), Color.yellow.AdjustAlpha(0f), Color.white, 0.05f);
                DrawText(__TARGET_DIST, LerpCubePosition(__SOURCE_POINT, __TARGET_POINT, 0.5f, 0.4f), $"{GetCubeDistance(__SOURCE_POINT, __TARGET_POINT):F2}", 8f, Color.white);
            }
            else
            {
                HideCube(__SOURCE_POINT);
                HideCube(__TARGET_POINT);
                HideLine(__TARGET_POINT);
                HideLine(__TARGET_DIST);
            }

            // if (drawSensorInfo)
            // {
            //     int currStatusIndex = 0;
            //     foreach (var c in hostBrain.PawnSensorCtrler.ListeningColliders)
            //     {
            //         if (c.TryGetComponent<PawnColliderHelper>(out var colliderHelper))
            //         {
            //             if (hostBrain.PawnSensorCtrler.WatchingColliders.Contains(c))
            //             {                         
            //                 DrawCube(__SENSOR_INFO[currStatusIndex], GetHeadRoomPosition(hostBrain.PawnBB.TargetBrain), GetCubeRotation(__TARGET_POINT, 180f * Time.deltaTime), 0.15f, Color.yellow);
            //             }
            //             else
            //             {
            //                 DrawCube(__SENSOR_INFO[currStatusIndex], GetHeadRoomPosition(hostBrain.PawnBB.TargetBrain), GetCubeRotation(__TARGET_POINT, 180f * Time.deltaTime), 0.15f, Color.yellow);
            //             }
            //         }

            //         if (++currStatusIndex >= __SENSOR_INFO.Length)
            //             break;
            //     }

            //     for (int i = currStatusIndex; i < __SENSOR_INFO.Length; i++)
            //         HideCube(__SENSOR_INFO[i]);
            // }
            // else
            // {
            //     for (int i = 0; i < __SENSOR_INFO.Length; i++)
            //         HideCube(__SENSOR_INFO[i]);
            // }

            if (drawActionInfo)
            {
                if (!__onActionHandlerInited && hostBrain != null)
                {
                    __onActionHandlerInited = true;
                    hostBrain.GetComponent<PawnActionController>().onActionStart += OnActionStartHandle;
                    hostBrain.GetComponent<PawnActionController>().onActionFinished += OnActionFinishedHandler;
                }

                if (hostBrain.GetComponent<PawnActionController>().CheckActionRunning())
                {
                    DrawCube(__ACTION_START[__currActionIndex], GetCubePosition(__ACTION_START[__currActionIndex]), Quaternion.identity, 0.1f, Color.yellow);
                    DrawCube(__ACTION_FINISH[__currActionIndex], hostBrain.GetWorldPosition() +  0.2f * Vector3.up, Quaternion.identity, 0.2f, Color.yellow);
                    DrawLine(__ACTION_FINISH[__currActionIndex], GetCubePosition(__ACTION_START[__currActionIndex]), GetCubePosition(__ACTION_FINISH[__currActionIndex]), Color.yellow.AdjustAlpha(0f), Color.yellow, 0.05f);
                    DrawText(__ACTION_DIST[__currActionIndex], LerpCubePosition(__ACTION_START[__currActionIndex], __ACTION_FINISH[__currActionIndex], 0.5f, 0.2f), $"{GetCubeDistance(__ACTION_START[__currActionIndex], __ACTION_FINISH[__currActionIndex]):F2}", 8f, Color.yellow);
                }
            }

            if (drawPawnStatus)
            {
                if (hostBrain.PawnStatusCtrler.CheckStatus(PawnStatus.Staggered))
                    DrawText(__PAWN_STATUS, GetHeadRoomPosition(hostBrain, 0.2f), $"STGG-{hostBrain.PawnStatusCtrler.GetDuration(PawnStatus.Staggered):F1}", 7f, Color.green);
                else if (hostBrain.PawnStatusCtrler.CheckStatus(PawnStatus.KnockDown))
                    DrawText(__PAWN_STATUS, GetHeadRoomPosition(hostBrain, 0.2f), $"DOWN-{hostBrain.PawnStatusCtrler.GetDuration(PawnStatus.KnockDown):F1}", 7f, Color.green);
                else
                    HideText(__PAWN_STATUS);
            }

        }

        bool __onActionHandlerInited;
        int __currActionIndex;

        void OnActionStartHandle(PawnActionController.ActionContext actionContext, PawnHeartPointDispatcher.DamageContext damageContext)
        {
            if (++__currActionIndex >= __ACTION_START.Length) __currActionIndex = 0;
            SetCubePositionAndRotation(__ACTION_START[__currActionIndex], hostBrain.GetWorldPosition(), Quaternion.identity, 0.2f);
        }

        void OnActionFinishedHandler(PawnActionController.ActionContext actionContext)
        {
            var capturedIndex = __currActionIndex;
            Observable.Timer(TimeSpan.FromSeconds(2f)).Subscribe(_ =>
            {
                HideCube(__ACTION_START[capturedIndex]);
                HideCube(__ACTION_FINISH[capturedIndex]);
                HideLine(__ACTION_FINISH[capturedIndex]);
                HideText(__ACTION_DIST[capturedIndex]);
            });
        }

        /// <summary>
        /// 커서를 지면 위 Picking Point로 이동
        /// </summary>
        /// <param name="mousePoint"></param>
        /// <returns></returns>
        public bool TryMoveCursorPositionOnTerrain(Vector3 mousePoint)
        {
            if (GameContext.Instance.cameraCtrler.gameCamera != null)
            {
                var ray = GameContext.Instance.cameraCtrler.gameCamera.ScreenPointToRay(mousePoint);
                var hitPoints = Physics.RaycastAll(ray, 9999f, LayerMask.GetMask(TerrainManager.LayerName));

                if (hitPoints.Length > 0)
                {
                    // forwardCursor.position = hitPoints.OrderBy(h => h.distance).ToArray().First().point + Vector3.up * 0.25f;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetPickingPointOnTerrain(Vector3 mousePoint, out Vector3 result)
        {
            if (GameContext.Instance.cameraCtrler.gameCamera != null)
            {
                var ray = GameContext.Instance.cameraCtrler.gameCamera.ScreenPointToRay(mousePoint);
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
            var sensorCtrler = hostBrain.GetComponent<PawnSensorController>();
            var jellyCollider = sensorCtrler.WatchingColliders.FirstOrDefault(c => c.TryGetComponent<PawnColliderHelper>(out var helper) && helper.gameObject.CompareTag("Jelly") && (helper.transform.position - hostBrain.GetWorldPosition()).Magnitude2D() < attackRange);
            if (jellyCollider != null)
            {
                attackPoint = jellyCollider.transform.position;
                return true;
            }

            var found = sensorCtrler.WatchingColliders.Select(c => c.GetComponent<PawnColliderHelper>()).Where(h => h != null && h == h.pawnBrain.coreColliderHelper)
                .OrderBy(h => Vector3.Angle(hostBrain.coreColliderHelper.transform.forward.Vector2D(), (h.transform.position - hostBrain.GetWorldPosition()).Vector2D()))
                .FirstOrDefault(h => hostBrain.coreColliderHelper.GetApproachDistance(h) < attackRange);

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