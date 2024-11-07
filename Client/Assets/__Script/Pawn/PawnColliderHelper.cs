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
        public float GetApproachDistance(Vector3 sourcePosition) => Mathf.Max(0f, (transform.position - sourcePosition).Vector2D().magnitude - GetRadius());

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