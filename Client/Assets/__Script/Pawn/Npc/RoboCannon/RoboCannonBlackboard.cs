using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using ZLinq;

namespace Game
{
    public class RoboCannonBlackboard : NpcHumanoidBlackboard
    {
        public override float SpacingInDistance => body.spacingInDistance;
        public override float SpacingOutDistance => body.spacingOutDistance;
        public override float MinSpacingDistance => body.minSpacingDistance;
        public override float MaxSpacingDistance => body.maxSpacingDistance;
        public override float MinApproachDistance => body.minApproachDistance;
        public Transform MissileEmitPoint => children.missileEmitPoint;

        [Serializable]
        public class Body
        {
            public float moveSpeed = 1f;
            public float aimRotateSpeed = 1f;
            public Vector2 knockDownImpulse;
            public float spacingInDistance = 1f;
            public float spacingOutDistance = 1f;
            public float minSpacingDistance = 1f;
            public float maxSpacingDistance = 1f;
            public float minApproachDistance = 1f;
        }

        public Body body = new();

        [Serializable]
        public class Action
        {
            [Header("Fire")]
            public float missileSpeed = 1f;
            public float fireCoomTime = 1f;

            // [Header("ComboAttack")]
            // public float comboAttackCoolTime = 1f;
            // public float comboAttackDistance = 1f;

            // [Header("JumpAttack")]
            // public float jumpAttackCoolTime = 10f;
            // public float jumpAttackDistance = 1f;
        }

        public Action action = new();

        [Serializable]
        public class Children
        {
            public Renderer[] hitColorRenderers;
            public Transform bodyMeshParent;
            public Transform bodyBone;
            public Transform barrelBone;
            public Transform lookAtPoint;
            public Transform missileEmitPoint;
            public Transform specialKeyAttachPoint;
        }

        public Children children = new();

        [Serializable]
        public class Resource
        {
            [Header("Material")]
            public Material hitColor;

            [Header("Fx")]
            public GameObject onHitFx;
            public GameObject onBleedingFx;
            public GameObject onMissedFx;

            [Header("Audio")]
            public AudioClip onHitAudioClip;
            public AudioClip onHitAudioClip2;
            public AudioClip onBleedingAudioClip;
            public AudioClip onMissedAudioClip;
            public AudioClip onHitFleshClip;
            public AudioClip onFootstepClip;
        }

        public Resource resource = new();

        protected override void AwakeInternal()
        {
            base.AwakeInternal();

            if (children.bodyMeshParent != null)
            {
                var tempRenderers = new List<Renderer>();

                foreach (var d in children.bodyMeshParent.DescendantsAndSelf())
                {
                    if (d.TryGetComponent<SkinnedMeshRenderer>(out var renderer) && renderer.enabled)
                        tempRenderers.Add(renderer);
                }

                //* HitColor 하이라이트 bodyMeshRenderers 셋팅
                children.hitColorRenderers = tempRenderers.ToArray();
            }
        }
    }
}