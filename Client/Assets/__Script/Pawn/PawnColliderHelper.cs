using System;
using System.Linq;
using FlowCanvas.Nodes;
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

        public Rigidbody pawnRigidbody;
        public Collider pawnCollider;
        public PawnBrainController pawnBrain;
        public SensorFilters sensorTrigger = SensorFilters.None;
        public bool IsCoreCollider => pawnBrain != null && pawnBrain.coreColliderHelper == this;
        public bool IsVisionSensorTriggerable => (sensorTrigger & SensorFilters.Vision) > 0;
        public bool IsTouchSensorTriggerable => (sensorTrigger & SensorFilters.Touch) > 0;
        public bool IsSoundSensorTriggerable => (sensorTrigger & SensorFilters.Sound) > 0;
        public Vector3 GetCenter() => __capsuleCollider != null ? __capsuleCollider.transform.localToWorldMatrix.MultiplyPoint(__capsuleCollider.center) : pawnCollider.transform.position;
        public float GetRadius() => __capsuleCollider != null ? __capsuleCollider.radius : 0f;

        //* sourcePosition에서 자신까지의 거리 (자신의 Collider Raidus 값을 뺀 거리)
        public float GetApproachDistance(Vector3 sourcePosition) => Mathf.Max(0f, (transform.position - sourcePosition).Vector2D().magnitude - GetRadius());

        //* 목표점까지 거리에서 자신과 상대 Collider의 Radius 값을 뺀 거리
        public float GetDistanceBetween(PawnColliderHelper otherColliderHelper) => Mathf.Max(0f, (otherColliderHelper.transform.position - transform.position).Vector2D().magnitude - GetRadius() - otherColliderHelper.GetRadius());

        //* deltaVec만큼 움직였을 때 거리이 변화값
        public float GetDistanceDelta(PawnColliderHelper otherColliderHelper, Vector3 deltaVec)
        {
            var distanceVec = otherColliderHelper.transform.position - transform.position;
            return (distanceVec + deltaVec).Magnitude2D() - distanceVec.Magnitude2D();
        }

        //* Center 위치에서 senderPosition을 바라보는 방향으로 pawnCollider의 표면과 교차하는 한 점을 HitPoint로 리턴함
        public Vector3 GetHitPoint(Vector3 senderPosition) => GetCenter() + GetRadius() * (senderPosition - pawnCollider.transform.position).Vector2D().normalized;

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
        }

        public CapsuleCollider GetCapsuleCollider() => __capsuleCollider;
        CapsuleCollider __capsuleCollider;
    }
}