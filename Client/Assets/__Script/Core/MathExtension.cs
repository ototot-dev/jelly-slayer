using System;
using NUnit.Framework.Constraints;
using Unity.VisualScripting;
using UnityEngine;

namespace Game
{
    public static class MathExtension
    {
        public const float EPSILON_LENGTH = 0.0001f;
        public const float EPSILON_ANGLE = 0.01f;

        public static float MaxElem(this Vector3 vec)
        {
            return Mathf.Max(Mathf.Max(vec.x, vec.y), vec.z);
        }

        public static float MaxAbsElem(this Vector3 vec)
        {
            return Mathf.Max(Mathf.Max(Mathf.Abs(vec.x), Mathf.Abs(vec.y)), Mathf.Abs(vec.z));
        }

        public static float Magnitude2D(this Vector3 vec)
        {
            return new Vector3(vec.x, 0, vec.z).magnitude;
        }

        public static float SqrMagnitude2D(this Vector3 vec)
        {
            return new Vector3(vec.x, 0, vec.z).sqrMagnitude;
        }

        public static float Distance2D(this Vector3 vec, Vector3 target)
        {
            return (target - vec).Magnitude2D();
        }

        public static float SqrDistance2D(this Vector3 vec, Vector3 target)
        {
            return (target - vec).SqrMagnitude2D();
        }

        public static Vector3 Abs(this Vector3 vec)
        {
            return new(Mathf.Abs(vec.x), Mathf.Abs(vec.y), Mathf.Abs(vec.z));
        }

        public static Vector3 AbsRef(this ref Vector3 vec)
        {
            return vec = new(Mathf.Abs(vec.x), Mathf.Abs(vec.y), Mathf.Abs(vec.z));
        }

        public static Vector2 DropX(this Vector3 vec)
        {
            return new(vec.y, vec.z);
        }

        public static Vector2 DropY(this Vector3 vec)
        {
            return new(vec.x, vec.z);
        }

        public static Vector2 DropZ(this Vector3 vec)
        {
            return new(vec.x, vec.y);
        }

        public static Vector3 DropX(this Vector4 vec)
        {
            return new(vec.y, vec.z, vec.w);
        }

        public static Vector3 DropY(this Vector4 vec)
        {
            return new(vec.x, vec.z, vec.w);
        }

        public static Vector3 DropZ(this Vector4 vec)
        {
            return new(vec.x, vec.y, vec.w);
        }

        public static Vector3 DropW(this Vector4 vec)
        {
            return new(vec.x, vec.y, vec.z);
        }

        public static Vector3 Vector2D(this Vector3 vec)
        {
            return new(vec.x, 0, vec.z);
        }

        public static Vector3 VectorRef2D(this ref Vector3 vec)
        {
            return vec = new(vec.x, 0, vec.z);
        }

        public static Vector3 AdjustX(this Vector3 vec, float x)
        {
            vec.x = x;
            return vec;
        }

        public static Vector3 AdjustY(this Vector3 vec, float y)
        {
            vec.y = y;
            return vec;
        }

        public static Vector3 AdjustZ(this Vector3 vec, float z)
        {
            vec.z = z;
            return vec;
        }

        public static Vector3 AdjustXZ(this Vector3 vec, float x, float z)
        {
            vec.x = x;
            vec.z = z;

            return vec;
        }

        public static Color AdjustAlpha(this Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        public static float LerpSpeed(this float curr, float target, float speed, float deltaTime)
        {
            if (target > curr)
                return Mathf.Min(target, curr + speed * deltaTime);
            else
                return Mathf.Max(target, curr - speed * deltaTime);
        }

        public static float LerpRefSpeed(this ref float curr, float target, float speed, float deltaTime)
        {
            if (target > curr)
                curr = Mathf.Min(target, curr + speed * deltaTime);
            else
                curr = Mathf.Max(target, curr - speed * deltaTime);

            return curr;
        }

        public static Vector3 LerpSpeed(this Vector3 vec, Vector3 target, float speed, float deltaTime)
        {
            return vec.LerpRefSpeed(target, speed, deltaTime);
        }

        public static Vector3 LerpSpeedX(this Vector3 vec, float x, float speed, float deltaTime)
        {
            if (x > vec.x)
                vec.x = Mathf.Min(x, vec.x + speed * deltaTime);
            else
                vec.x = Mathf.Max(x, vec.x - speed * deltaTime);

            return vec;
        }

        public static Vector3 LerpSpeedY(this Vector3 vec, float y, float speed, float deltaTime)
        {
            if (y > vec.y)
                vec.y = Mathf.Min(y, vec.y + speed * deltaTime);
            else
                vec.y = Mathf.Max(y, vec.y - speed * deltaTime);

            return vec;
        }

        public static Vector3 LerpSpeedZ(this Vector3 vec, float z, float speed, float deltaTime)
        {
            if (z > vec.z)
                vec.z = Mathf.Min(z, vec.z + speed * deltaTime);
            else
                vec.z = Mathf.Max(z, vec.z - speed * deltaTime);

            return vec;
        }

        public static Vector3 LerpSpeedXZ(this Vector3 vec, float x, float z, float speed, float deltaTime)
        {
            if (x > vec.x)
                vec.x = Mathf.Min(x, vec.x + speed * deltaTime);
            else
                vec.x = Mathf.Max(x, vec.x - speed * deltaTime);

            if (z > vec.z)
                vec.z = Mathf.Min(z, vec.z + speed * deltaTime);
            else
                vec.z = Mathf.Max(z, vec.z - speed * deltaTime);

            return vec;
        }

        public static Vector3 LerpRefSpeed(this ref Vector3 vec, Vector3 target, float speed, float deltaTime)
        {
            var newVec = vec + (target - vec).normalized * speed * deltaTime;

            if (target.x > vec.x)
                vec.x = Mathf.Min(newVec.x, target.x);
            else
                vec.x = Mathf.Max(newVec.x, target.x);

            if (target.y > vec.y)
                vec.y = Mathf.Min(newVec.y, target.y);
            else
                vec.y = Mathf.Max(newVec.y, target.y);

            if (target.z > vec.z)
                vec.z = Mathf.Min(newVec.z, target.z);
            else
                vec.z = Mathf.Max(newVec.z, target.z);

            return vec;
        }

        public static Vector3 LerpRefSpeedX(this ref Vector3 vec, float x, float speed, float deltaTime)
        {
            if (x > vec.x)
                vec.x = Mathf.Min(x, vec.x + speed * deltaTime);
            else
                vec.x = Mathf.Max(x, vec.x - speed * deltaTime);

            return vec;
        }

        public static Vector3 LerpRefSpeedY(this ref Vector3 vec, float y, float speed, float deltaTime)
        {
            if (y > vec.y)
                vec.y = Mathf.Min(y, vec.y + speed * deltaTime);
            else
                vec.y = Mathf.Max(y, vec.y - speed * deltaTime);

            return vec;
        }

        public static Vector3 LerpRefSpeedZ(this ref Vector3 vec, float z, float speed, float deltaTime)
        {
            if (z > vec.z)
                vec.z = Mathf.Min(z, vec.z + speed * deltaTime);
            else
                vec.z = Mathf.Max(z, vec.z - speed * deltaTime);

            return vec;
        }

        public static Vector3 LerpRefSpeedXZ(this ref Vector3 vec, float x, float z, float speed, float deltaTime)
        {
            if (x > vec.x)
                vec.x = Mathf.Min(x, vec.x + speed * deltaTime);
            else
                vec.x = Mathf.Max(x, vec.x - speed * deltaTime);

            if (z > vec.z)
                vec.z = Mathf.Min(z, vec.z + speed * deltaTime);
            else
                vec.z = Mathf.Max(z, vec.z - speed * deltaTime);

            return vec;
        }

        public static Quaternion LerpAngleSpeed(this Quaternion rot, Quaternion target, float speed, float deltaTime)
        {
            var delta = Quaternion.Angle(rot, target);
            return Mathf.Approximately(delta, 0f) ? target : Quaternion.Lerp(rot, target, Mathf.Clamp01(speed * deltaTime / delta));
        }

        public static Quaternion LerpRefAngleSpeed(this ref Quaternion rot, Quaternion target, float speed, float deltaTime)
        {
            var delta = Quaternion.Angle(rot, target);
            rot = Mathf.Approximately(delta, 0f) ? target : Quaternion.Lerp(rot, target, Mathf.Clamp01(speed * deltaTime / delta));
            return rot;
        }

        public static Matrix4x4 Lerp(this Matrix4x4 matrix, Matrix4x4 target, float t)
        {
            // 1. 분해
            Vector3 posA = matrix.GetColumn(3);
            Vector3 posB = target.GetColumn(3);

            Vector3 scaleA = matrix.lossyScale;
            Vector3 scaleB = target.lossyScale;

            Quaternion rotA = Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
            Quaternion rotB = Quaternion.LookRotation(target.GetColumn(2), target.GetColumn(1));

            // 2. 보간
            Vector3 posLerp = Vector3.Lerp(posA, posB, t);
            Vector3 scaleLerp = Vector3.Lerp(scaleA, scaleB, t);
            Quaternion rotSlerp = Quaternion.Slerp(rotA, rotB, t);

            // 3. 재조합
            return Matrix4x4.TRS(posLerp, rotSlerp, scaleLerp);
        }

        public static Vector3 Random(this Vector3 vec)
        {
            var rangeVec = vec.Abs();
            return new(UnityEngine.Random.Range(-rangeVec.x, rangeVec.x), UnityEngine.Random.Range(-rangeVec.y, rangeVec.y), UnityEngine.Random.Range(-rangeVec.z, rangeVec.z));
        }

        public static Vector3 Random(this Vector3 vec, float min, float max)
        {
            return new(UnityEngine.Random.Range(min, max), UnityEngine.Random.Range(min, max), UnityEngine.Random.Range(min, max));
        }

        public static Vector3 RandomRef(this ref Vector3 vec, float min, float max)
        {
            vec = new(UnityEngine.Random.Range(min, max), UnityEngine.Random.Range(min, max), UnityEngine.Random.Range(min, max));
            return vec;
        }

        public static Vector3 RandomX(this Vector3 vec, float min, float max)
        {
            return new(UnityEngine.Random.Range(min, max), vec.y, vec.z);
        }

        public static Vector3 RandomY(this Vector3 vec, float min, float max)
        {
            return new(vec.x, UnityEngine.Random.Range(min, max), vec.z);
        }

        public static Vector3 RandomZ(this Vector3 vec, float min, float max)
        {
            return new(vec.x, vec.y, UnityEngine.Random.Range(min, max));
        }

        public static Vector3 RandomRefX(this ref Vector3 vec, float min, float max)
        {
            return vec = new(UnityEngine.Random.Range(min, max), vec.y, vec.z);
        }

        public static Vector3 RandomRefY(this ref Vector3 vec, float min, float max)
        {
            return vec = new(vec.x, UnityEngine.Random.Range(min, max), vec.z);
        }

        public static Vector3 RandomRefZ(this ref Vector3 vec, float min, float max)
        {
            return vec = new(vec.x, vec.y, UnityEngine.Random.Range(min, max));
        }

        public static bool CheckOverlappedWithFan(this BoxCollider collider, float fanAngle, float fanRadius, float fanHeight, Matrix4x4 fanWorldToLocal)
        {
            return false;
        }

        public static bool CheckOverlappedWithBox(this BoxCollider collider, Vector3 boxSize, Matrix4x4 boxWorldToLocal)
        {
            // BoxCollider의 월드 변환 정보 가져오기
            Transform t1 = collider.transform;
            // Transform t2 = boxCollider2.transform;

            // OBB의 중심 좌표
            Vector3 c1 = t1.position;
            Vector3 c2 = boxWorldToLocal.GetPosition();

            // OBB의 반경 (half-extents)
            Vector3 e1 = collider.size * 0.5f;
            Vector3 e2 = boxSize * 0.5f;

            // OBB의 로컬 좌표축 (월드 공간 기준)
            Vector3[] axes1 = {
                t1.right.normalized,   // X축
                t1.up.normalized,      // Y축
                t1.forward.normalized  // Z축
            };

            Vector3[] axes2 = {
                boxWorldToLocal.GetColumn(0).DropW().normalized,
                boxWorldToLocal.GetColumn(1).DropW().normalized,
                boxWorldToLocal.GetColumn(2).DropW().normalized
            };

            // 두 박스의 중심 간 벡터
            Vector3 centerDiff = c2 - c1;

            // 모든 축에 대해 SAT 검사
            for (int i = 0; i < 3; i++) // 첫 번째 박스의 3축 (X, Y, Z)
            {
                if (!OverlapOnAxis(axes1[i], axes1, axes2, e1, e2, centerDiff)) return false;
            }

            for (int i = 0; i < 3; i++) // 두 번째 박스의 3축 (X, Y, Z)
            {
                if (!OverlapOnAxis(axes2[i], axes1, axes2, e1, e2, centerDiff)) return false;
            }

            for (int i = 0; i < 3; i++) // 9개의 교차 축 (외적)
            {
                for (int j = 0; j < 3; j++)
                {
                    Vector3 axis = Vector3.Cross(axes1[i], axes2[j]);
                    if (axis.sqrMagnitude < 1e-6f) continue; // 축이 0이면 무시 (평행한 경우)

                    if (!OverlapOnAxis(axis.normalized, axes1, axes2, e1, e2, centerDiff)) return false;
                }
            }

            return true; // 모든 축에서 분리되지 않았으면 겹쳐 있음
        }

        private static bool OverlapOnAxis(Vector3 axis, Vector3[] axes1, Vector3[] axes2, Vector3 e1, Vector3 e2, Vector3 centerDiff)
        {
            // 축이 충분히 작은 경우(수치적으로 불안정할 경우) 무시
            if (axis.sqrMagnitude < 1e-6f) return true;

            // 두 박스를 이 축에 투영했을 때의 반경 계산
            float r1 = Mathf.Abs(Vector3.Dot(axes1[0], axis)) * e1.x +
                    Mathf.Abs(Vector3.Dot(axes1[1], axis)) * e1.y +
                    Mathf.Abs(Vector3.Dot(axes1[2], axis)) * e1.z;

            float r2 = Mathf.Abs(Vector3.Dot(axes2[0], axis)) * e2.x +
                    Mathf.Abs(Vector3.Dot(axes2[1], axis)) * e2.y +
                    Mathf.Abs(Vector3.Dot(axes2[2], axis)) * e2.z;

            // 중심 거리의 투영값
            float centerDist = Mathf.Abs(Vector3.Dot(centerDiff, axis));

            // 두 박스가 이 축에서 분리되었는지 확인
            return centerDist <= (r1 + r2);
        }

        public static bool CheckOverlappedWithFan(this CapsuleCollider collider, float fanAngle, float fanRadius, float fanHeight, Matrix4x4 fanWorldToLocal)
        {
            var (right, up, forward) = (collider.transform.right, collider.transform.up, collider.transform.forward);
            var capsuleCenter = collider.transform.position + collider.center.x * right + collider.center.y * up + collider.center.z * forward;
            var capsuleHeight = collider.height * collider.transform.lossyScale.y;
            var capsuleRadius = collider.radius * Mathf.Max(collider.transform.lossyScale.x, collider.transform.lossyScale.z);
            var cylinderHeight = Mathf.Max(0, capsuleHeight - 2f * capsuleRadius);
            
            if (CheckOverlappedWithFan(capsuleCenter, capsuleRadius, fanAngle, fanRadius, fanHeight, fanWorldToLocal))
                return true;
            if (cylinderHeight <= 0f)
                return false;

            var upperCapPosition = capsuleCenter + 0.5f * cylinderHeight * up;
            if (CheckOverlappedWithFan(upperCapPosition, capsuleRadius, fanAngle, fanRadius, fanHeight, fanWorldToLocal))
                return true;

            var lowerCapPosition = capsuleCenter - 0.5f * cylinderHeight * up;
            if (CheckOverlappedWithFan(lowerCapPosition, capsuleRadius, fanAngle, fanRadius, fanHeight, fanWorldToLocal))
                return true;

            if (CheckOverlappedWithFan(0.5f * (capsuleCenter + upperCapPosition), capsuleRadius, fanAngle, fanRadius, fanHeight, fanWorldToLocal))
                return true;

            if (CheckOverlappedWithFan(0.5f * (capsuleCenter + lowerCapPosition), capsuleRadius, fanAngle, fanRadius, fanHeight, fanWorldToLocal))
                return true;

            return false;
        }

        public static bool CheckOverlappedWithFan(this SphereCollider collider, float fanAngle, float fanRadius, float fanHeight, Matrix4x4 fanWorldToLocal)
        {
            return CheckOverlappedWithFan(collider.transform.position + collider.center, collider.radius * Mathf.Max(collider.transform.lossyScale.x, collider.transform.lossyScale.y, collider.transform.lossyScale.z), fanAngle, fanRadius, fanHeight, fanWorldToLocal);
        }

        public static bool CheckOverlappedWithFan(Vector3 sphereCenter, float sphereRadius, float fanAngle, float fanRadius, float fanHeight, Matrix4x4 fanWorldToLocal)
        {
            sphereCenter = fanWorldToLocal.MultiplyPoint(sphereCenter);
            var sphereCenter2D = sphereCenter.Vector2D();
            var sphereCenter2D_sqrMagnitude = sphereCenter.SqrMagnitude2D();
            if (sphereCenter2D_sqrMagnitude > (sphereRadius + fanRadius) * (sphereRadius + fanRadius))
                return false;

            var heightDistance = Mathf.Abs(sphereCenter.y) - 0.5f * fanHeight;
            if (heightDistance > sphereRadius)
                return false;

            if (heightDistance > 0f)
                sphereRadius = Mathf.Sqrt(sphereRadius * sphereRadius - heightDistance * heightDistance);

            var shortestPoint2D = sphereCenter2D;
            var shortestPoint2D_norm = sphereCenter2D.normalized;
            if (sphereCenter2D_sqrMagnitude > fanRadius * fanRadius)
                shortestPoint2D -= sphereRadius * shortestPoint2D_norm;

            var edgeVec0 = Quaternion.Euler(0f, 0.5f * fanAngle, 0f) * Vector3.forward;
            var projection0 = Vector3.Project(sphereCenter2D, edgeVec0);
            if (Vector3.Dot(edgeVec0, projection0) < 0f)
                projection0 = Vector3.zero;
            else if (projection0.sqrMagnitude > fanRadius * fanRadius)
                projection0 = fanRadius * edgeVec0;
            if ((projection0 - sphereCenter2D).sqrMagnitude <= sphereRadius * sphereRadius)
                return true;

            var edgeVec1 = edgeVec0.AdjustX(-edgeVec0.x);
            var projection1 = Vector3.Project(sphereCenter2D, edgeVec1);
            if (Vector3.Dot(edgeVec1, projection1) < 0f)
                projection1 = Vector3.zero;
            else if (projection1.sqrMagnitude > fanRadius * fanRadius)
                projection1 = fanRadius * edgeVec1;
            if ((projection1 - sphereCenter2D).sqrMagnitude <= sphereRadius * sphereRadius)
                return true;

            if (fanAngle >= 360f)
                return shortestPoint2D.sqrMagnitude < fanRadius * fanRadius;
            else
                return shortestPoint2D.sqrMagnitude < fanRadius * fanRadius && Vector3.Dot(Vector3.forward, shortestPoint2D_norm) >= Mathf.Cos(Mathf.Deg2Rad * 0.5f * fanAngle);
        }
    }
}