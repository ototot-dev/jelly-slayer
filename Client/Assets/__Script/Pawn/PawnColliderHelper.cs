using System;
using System.Linq;
using Unity.Linq;
using UnityEngine;

namespace Game
{
    public class PawnColliderHelper : MonoBehaviour
    {
        public enum SensorFilters : UInt16
        {
            None = 0,
            Touch = 0x01,
            Sound = 0x02,
            Vision = 0x04,
            All = 0xFF,
        }

        public enum DistanceMeasureTypes
        {
            Simple,
            ApproachDistance,
            DistanceBetween,
        }

        public Rigidbody pawnRigidbody;
        public Collider pawnCollider;
        public PawnBrainController pawnBrain;
        public SensorFilters sensorTrigger = SensorFilters.None;
        public bool IsCoreCollider => pawnBrain != null && pawnBrain.coreColliderHelper == this;
        public bool IsParryHitCollider => pawnBrain != null && pawnBrain.parryHitColliderHelper == this;
        public bool IsVisionSensorTriggerable => (sensorTrigger & SensorFilters.Vision) > 0;
        public bool IsTouchSensorTriggerable => (sensorTrigger & SensorFilters.Touch) > 0;
        public bool IsSoundSensorTriggerable => (sensorTrigger & SensorFilters.Sound) > 0;
        public Vector3 GetWorldCenter() => __capsuleCollider != null ? __capsuleCollider.transform.localToWorldMatrix.MultiplyPoint(__capsuleCollider.center) : pawnCollider.transform.position;
        public float GetCapsuleRadius() => __capsuleCollider != null ? __capsuleCollider.radius : 0f;
        public float GetScaledCapsuleRadius() => GetCapsuleRadius() * pawnCollider.transform.lossyScale.MaxAbsElem();
        public float GetCapsuleHeight() => __capsuleCollider != null ? __capsuleCollider.height : 0f;
        public float GetScaledCapsuleHeight() => GetCapsuleHeight() * pawnCollider.transform.lossyScale.MaxAbsElem();
        public float GetDistanceSimple(PawnColliderHelper other) => Mathf.Max(0f, (other.transform.position - transform.position).Vector2D().magnitude);

        //* sourcePosition에서 자신까지의 거리 (자신의 Collider Raidus 값을 뺀 거리)
        public float GetApproachDistance(PawnColliderHelper other) => Mathf.Max(0f, (other.transform.position - transform.position).Vector2D().magnitude - other.GetCapsuleRadius());

        //* 목표점까지 거리에서 자신과 상대 Collider의 Radius 값을 뺀 거리
        public float GetDistanceBetween(PawnColliderHelper other) => Mathf.Max(0f, (other.transform.position - transform.position).Vector2D().magnitude - GetCapsuleRadius() - other.GetCapsuleRadius());

        //* DistanceMeasureTypes을 인자값으로 받는 함수
        public float GetDistance(PawnColliderHelper other, DistanceMeasureTypes measureType)
        {
            return measureType switch
            {
                DistanceMeasureTypes.Simple => GetDistanceSimple(other),
                DistanceMeasureTypes.ApproachDistance => GetApproachDistance(other),
                DistanceMeasureTypes.DistanceBetween => GetDistanceBetween(other),
                _ => 0f,
            };
        }

        //* deltaVec만큼 움직였을 때 거리이 변화값
        public float GetDistanceDelta(PawnColliderHelper other, Vector3 deltaVec)
        {
            var currDistance = (other.transform.position - transform.position).Magnitude2D();
            var newDistance = (other.transform.position - (transform.position + deltaVec)).Magnitude2D();
            return newDistance - currDistance;
        }

        //* Center 위치에서 senderPosition을 바라보는 방향으로 pawnCollider의 표면과 교차하는 한 점을 HitPoint로 리턴함
        public Vector3 GetHitPoint(Vector3 senderPosition) => GetWorldCenter() + GetScaledCapsuleRadius() * (senderPosition - pawnCollider.transform.position).Vector2D().normalized;

        void Awake()
        {
            if (pawnCollider == null)
                pawnCollider = GetComponent<Collider>();
            if (__capsuleCollider == null && pawnCollider != null)
                __capsuleCollider = pawnCollider as CapsuleCollider;
            if (pawnRigidbody == null)
                pawnRigidbody = GetComponent<Rigidbody>();
            if (pawnBrain == null)
                pawnBrain = gameObject.AncestorsAndSelf().First(a => a.GetComponent<PawnBrainController>() != null).GetComponent<PawnBrainController>();

            Debug.Assert(pawnBrain != null);
            pawnBrain.pawnColliderHelpers.Add(this);
        }

        public CapsuleCollider GetCapsuleCollider() => __capsuleCollider;
        CapsuleCollider __capsuleCollider;
    }
}