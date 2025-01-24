using System;
using UnityEngine;

namespace Game
{
    public static class UnityMathExtension
    {

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

        public static Vector3 Abs(this Vector3 vec)
        {
            return new Vector3(Mathf.Abs(vec.x), Mathf.Abs(vec.y), Mathf.Abs(vec.z));
        }

        public static Vector3 AbsRef(this ref Vector3 vec)
        {
            vec = new Vector3(Mathf.Abs(vec.x), Mathf.Abs(vec.y), Mathf.Abs(vec.z));

            return vec;
        }

        public static Vector3 Vector2D(this Vector3 vec)
        {
            return new Vector3(vec.x, 0, vec.z);
        }

        public static Vector3 VectorRef2D(this ref Vector3 vec)
        {
            vec = new Vector3(vec.x, 0, vec.z);

            return vec;
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

        public static float LerpSpeed(this float start, float end, float speed, float deltaTime)
        {
            if (end > start)
                return Mathf.Min(end, start + speed * deltaTime);
            else
                return Mathf.Max(end, start - speed * deltaTime);
        }

        public static Vector3 LerpSpeed(this Vector3 vec, Vector3 target, float speed, float deltaTime)
        {
            return vec.LerpRefSpeed(target, speed, deltaTime);
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

        public static Vector3 Random(this Vector3 vec)
        {
            var rangeVec = vec.Abs();
            return new Vector3(UnityEngine.Random.Range(-rangeVec.x, rangeVec.x), UnityEngine.Random.Range(-rangeVec.y, rangeVec.y), UnityEngine.Random.Range(-rangeVec.z, rangeVec.z));
        }

        public static Vector3 Random(this Vector3 vec, float min, float max)
        {
            return new Vector3(UnityEngine.Random.Range(min, max), UnityEngine.Random.Range(min, max), UnityEngine.Random.Range(min, max));
        }

        public static Vector3 RandomRef(this ref Vector3 vec, float min, float max)
        {
            vec = new Vector3(UnityEngine.Random.Range(min, max), UnityEngine.Random.Range(min, max), UnityEngine.Random.Range(min, max));

            return vec;
        }

        public static Vector3 RandomX(this Vector3 vec, float min, float max)
        {
            return new Vector3(UnityEngine.Random.Range(min, max), vec.y, vec.z);
        }

        public static Vector3 RandomY(this Vector3 vec, float min, float max)
        {
            return new Vector3(vec.x, UnityEngine.Random.Range(min, max), vec.z);
        }

        public static Vector3 RandomZ(this Vector3 vec, float min, float max)
        {
            return new Vector3(vec.x, vec.y, UnityEngine.Random.Range(min, max));
        }

        public static Vector3 RandomRefX(this ref Vector3 vec, float min, float max)
        {
            vec = new Vector3(UnityEngine.Random.Range(min, max), vec.y, vec.z);

            return vec;
        }

        public static Vector3 RandomRefY(this ref Vector3 vec, float min, float max)
        {
            vec = new Vector3(vec.x, UnityEngine.Random.Range(min, max), vec.z);

            return vec;
        }

        /// <summary>
        public static Vector3 RandomRefZ(this ref Vector3 vec, float min, float max)
        {
            vec = new Vector3(vec.x, vec.y, UnityEngine.Random.Range(min, max));

            return vec;
        }

        public static bool CheckOverlappedWithFan(this BoxCollider collider, float fanAngle, float fanRadius, float fanHeight, Matrix4x4 fanWorldToLocal)
        {
            return false;
        }

        public static bool CheckOverlappedWithFan(this CapsuleCollider collider, float fanAngle, float fanRadius, float fanHeight, Matrix4x4 fanWorldToLocal)
        {
            var capsuleAxis  = collider.transform.up;
            var capsuleCenter = collider.transform.position + collider.center;
            var cylinderHeight = Mathf.Max(0, collider.height - 2 * collider.radius);
            if (cylinderHeight == 0)
                return CheckOverlappedWithFan(capsuleCenter, collider.radius, fanAngle, fanRadius, fanHeight, fanWorldToLocal);

            if (CheckOverlappedWithFan(capsuleCenter + 0.5f * cylinderHeight * capsuleAxis, collider.radius, fanAngle, fanRadius, fanHeight, fanWorldToLocal))
                return true;
            if (CheckOverlappedWithFan(capsuleCenter - 0.5f * cylinderHeight * capsuleAxis, collider.radius, fanAngle, fanRadius, fanHeight, fanWorldToLocal))
                return true;

            var segmentStep = cylinderHeight / (int)(cylinderHeight / collider.radius);
            var segmentLen = segmentStep;
            while (segmentLen < cylinderHeight)
            {
                if (CheckOverlappedWithFan(capsuleCenter + (segmentLen - 0.5f * cylinderHeight) * capsuleAxis, collider.radius, fanAngle, fanRadius, fanHeight, fanWorldToLocal))
                    return true;
                segmentLen += segmentStep;
            }
            return false;
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

        public static bool CheckOverlappedWithFan(this SphereCollider collider, float fanAngle, float fanRadius, float fanHeight, Matrix4x4 fanWorldToLocal)
        {
            return CheckOverlappedWithFan(collider.transform.position + collider.center, collider.radius * Mathf.Max(collider.transform.lossyScale.x, collider.transform.lossyScale.y, collider.transform.lossyScale.z), fanAngle, fanRadius, fanHeight, fanWorldToLocal);
        }
    }
}