using System;
using System.Collections.Generic;
using FIMSpace;
using UnityEngine;

namespace Game
{
    public static class GizmosDrawExtension
    {
        public static void DrawBox(Vector3 center, Quaternion rotation, Vector3 halfExtent)
        {
            var up = rotation * Vector3.up;
            var right = rotation * Vector3.right;
            var forward = rotation * Vector3.forward;
            var v0 = center - halfExtent.x * right - halfExtent.y * up + halfExtent.z * forward;
            var v1 = center + halfExtent.x * right - halfExtent.y * up + halfExtent.z * forward;
            var v2 = center + halfExtent.x * right + halfExtent.y * up + halfExtent.z * forward;
            var v3 = center - halfExtent.x * right + halfExtent.y * up + halfExtent.z * forward;
            var v4 = center - halfExtent.x * right - halfExtent.y * up - halfExtent.z * forward;
            var v5 = center + halfExtent.x * right - halfExtent.y * up - halfExtent.z * forward;
            var v6 = center + halfExtent.x * right + halfExtent.y * up - halfExtent.z * forward;
            var v7 = center - halfExtent.x * right + halfExtent.y * up - halfExtent.z * forward;
            
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.DrawLine(v0, v1);
            Gizmos.DrawLine(v1, v2);
            Gizmos.DrawLine(v2, v3);
            Gizmos.DrawLine(v3, v0);
            Gizmos.DrawLine(v4, v5);
            Gizmos.DrawLine(v5, v6);
            Gizmos.DrawLine(v6, v7);
            Gizmos.DrawLine(v7, v4);
            Gizmos.DrawLine(v0, v4);
            Gizmos.DrawLine(v1, v5);
            Gizmos.DrawLine(v2, v6);
            Gizmos.DrawLine(v3, v7);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <param name="segmentNum"></param>
        public static void DrawCircle(Vector3 center, float radius, int segmentNum = 36)
        {
            var baseVec = Vector3.forward * radius;
            var stepRotator = Quaternion.Euler(0, 360f / segmentNum, 0);
            var currRotator = Quaternion.identity;

            for (int i = 0; i < segmentNum; i++)
            {
                var prevRotator = currRotator;
                currRotator *= stepRotator;

                Gizmos.DrawLine(center + prevRotator * baseVec, center + currRotator * baseVec);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <param name="height"></param>
        /// <param name="segmentNum"></param>
        public static void DrawCylinder(Vector3 center, float radius, float height, int segmentNum = 36)
        {
            var baseVec = Vector3.forward * radius;
            var heightVec = Vector3.up * height;
            var stepRotator = Quaternion.Euler(0, 360f / segmentNum, 0);
            var currRotator = Quaternion.identity;

            for (int i = 0; i < segmentNum; i++)
            {
                var prevRotator = currRotator;
                currRotator *= stepRotator;

                // 호
                Gizmos.DrawLine(center + prevRotator * baseVec, center + currRotator * baseVec);
                Gizmos.DrawLine(center + heightVec + prevRotator * baseVec, center + heightVec + currRotator * baseVec);

                // 세로 라인
                Gizmos.DrawLine(center + prevRotator * baseVec, center + heightVec + prevRotator * baseVec);
                Gizmos.DrawLine(center + currRotator * baseVec, center + heightVec + currRotator * baseVec);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="center"></param>
        /// <param name="forward"></param>
        /// <param name="radius"></param>
        /// <param name="angle"></param>
        /// <param name="segmentNum"></param>
        public static void DrawFan(Vector3 center, Vector3 forward, float radius, float angle, int segmentNum = 36)
        {
            var baseVec = forward * radius;
            var stepRotator = Quaternion.Euler(0, angle / segmentNum, 0);
            var currRotator = Quaternion.Euler(0, -angle * 0.5f, 0);

            for (int i = 0; i < segmentNum; i++)
            {
                var prevRotator = currRotator;
                currRotator *= stepRotator;

                // 우측 사이드
                if (i == 0)
                    Gizmos.DrawLine(center, center + prevRotator * baseVec);

                // 호
                Gizmos.DrawLine(center + prevRotator * baseVec, center + currRotator * baseVec);

                // 좌측 사이드
                if (i == segmentNum - 1)
                    Gizmos.DrawLine(center, center + currRotator * baseVec);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="center"></param>
        /// <param name="forward"></param>
        /// <param name="radius"></param>
        /// <param name="angle"></param>
        /// <param name="height"></param>
        /// <param name="segmentNum"></param>
        public static void DrawFanCylinder(Vector3 center, Vector3 forward, float radius, float angle, float height, int segmentNum = 36)
        {
            var baseVec = forward * radius;
            var heightVec = Vector3.up * height;
            var stepRotator = Quaternion.Euler(0, angle / segmentNum, 0);
            var currRotator = Quaternion.Euler(0, -angle * 0.5f, 0);

            Gizmos.DrawLine(center - 0.5f * heightVec, center + 0.5f * heightVec);

            for (int i = 0; i < segmentNum; i++)
            {
                var prevRotator = currRotator;
                currRotator *= stepRotator;

                // 우측 사이드
                if (i == 0)
                {
                    Gizmos.DrawLine(center - 0.5f * heightVec, center - 0.5f * heightVec + prevRotator * baseVec);
                    Gizmos.DrawLine(center + 0.5f * heightVec, center + 0.5f * heightVec + prevRotator * baseVec);
                    Gizmos.DrawLine(center + 0.5f * heightVec, center - 0.5f * heightVec + prevRotator * baseVec);
                }

                // 호
                Gizmos.DrawLine(center - 0.5f * heightVec + prevRotator * baseVec, center - 0.5f * heightVec + currRotator * baseVec);
                Gizmos.DrawLine(center + 0.5f * heightVec + prevRotator * baseVec, center + 0.5f * heightVec + currRotator * baseVec);

                // 세로 라인
                Gizmos.DrawLine(center - 0.5f * heightVec + prevRotator * baseVec, center + 0.5f * heightVec + prevRotator * baseVec);
                Gizmos.DrawLine(center - 0.5f * heightVec + currRotator * baseVec, center + 0.5f * heightVec + currRotator * baseVec);

                // 좌측 사이드
                if (i == segmentNum - 1)
                {
                    Gizmos.DrawLine(center - 0.5f * heightVec, center - 0.5f * heightVec + currRotator * baseVec);
                    Gizmos.DrawLine(center + 0.5f * heightVec, center + 0.5f * heightVec + currRotator * baseVec);
                    Gizmos.DrawLine(center - 0.5f * heightVec, center + 0.5f * heightVec + currRotator * baseVec);
                }
            }
        }

        public static void DrawFanCylinder(Matrix4x4 localToWorld, float radius, float angle, float height, int segmentNum = 36)
        {
            var baseVec = radius * Vector3.forward;
            var heightVec = Vector3.up * height;
            var stepRotator = Quaternion.Euler(0, angle / segmentNum, 0);
            var currRotator = Quaternion.Euler(0, -angle * 0.5f, 0);

            Gizmos.matrix = localToWorld;
            Gizmos.DrawLine(-0.5f * heightVec, 0.5f * heightVec);

            for (int i = 0; i < segmentNum; i++)
            {
                var prevRotator = currRotator;
                currRotator *= stepRotator;
                
                // 우측 사이드
                if (i == 0)
                {
                    Gizmos.DrawLine(-0.5f * heightVec, -0.5f * heightVec + prevRotator * baseVec);
                    Gizmos.DrawLine(0.5f * heightVec, 0.5f * heightVec + prevRotator * baseVec);
                    Gizmos.DrawLine(0.5f * heightVec, -0.5f * heightVec + prevRotator * baseVec);
                }

                // 호
                Gizmos.DrawLine(-0.5f * heightVec + prevRotator * baseVec, -0.5f * heightVec + currRotator * baseVec);
                Gizmos.DrawLine(0.5f * heightVec + prevRotator * baseVec, 0.5f * heightVec + currRotator * baseVec);

                // 세로 라인
                Gizmos.DrawLine(-0.5f * heightVec + prevRotator * baseVec, 0.5f * heightVec + prevRotator * baseVec);
                Gizmos.DrawLine(-0.5f * heightVec + currRotator * baseVec, 0.5f * heightVec + currRotator * baseVec);

                // 좌측 사이드
                if (i == segmentNum - 1)
                {
                    Gizmos.DrawLine(-0.5f * heightVec, -0.5f * heightVec + currRotator * baseVec);
                    Gizmos.DrawLine(0.5f * heightVec, 0.5f * heightVec + currRotator * baseVec);
                    Gizmos.DrawLine(-0.5f * heightVec, 0.5f * heightVec + currRotator * baseVec);
                }
            }

            Gizmos.matrix = Matrix4x4.identity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="position"></param>
        /// <param name="colour"></param>
        public static void DrawString(string text, Vector3 position, Color? colour = null)
        {
#if UNITY_EDITOR
            UnityEditor.Handles.Label(position, text);
#endif
        }
    }

    public class GizmosDrawer : MonoSingleton<GizmosDrawer>
    {
        readonly List<Tuple<float, Action>> __drawers = new List<Tuple<float, Action>>();
        
        public void Draw(float duration, Action drawer)
        {
            __drawers.Add(new Tuple<float, Action>(Time.time + duration, drawer));
        }

        void OnDrawGizmos()
        {
            __drawers.RemoveAll(d => d.Item1 <= Time.time);
            foreach (var d in __drawers)
                d.Item2.Invoke();
        }

    }

}
