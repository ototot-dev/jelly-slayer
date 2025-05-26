using System;
using System.Collections.Generic;
using UniRx;
using Unity.Linq;
using UnityEngine;

namespace Game
{
    public class RoboSoldierBlackboard : JellyHumanoidBlackboard
    {
        public override bool IsJumping => body.isJumping.Value;
        public override bool IsGliding => body.isGliding.Value;
        public override bool IsFalling => body.isFalling.Value;
        public override bool IsGuarding => body.isGuarding.Value;
        public override float SpacingInDistance => body.spacingInDistance;
        public override float SpacingOutDistance => body.spacingOutDistance;
        public override float MinSpacingDistance => body.minSpacingDistance;
        public override float MaxSpacingDistance => body.maxSpacingDistance;
        public override float MinApproachDistance => body.minApproachDistance;
        public float LeapRootMotionMultiplier => action.leapRootMotionMultiplier;
        public float BackstepRootMotionMultiplier => action.backstepRootMotionMultiplier;
        public GameObject MissilePrefab => action.missilePrefab;
        public Transform MissileEmitPoint => action.missileEmitPoint;
        public CapsuleCollider CounterActionTraceCollider => attachment.counterActionTraceColliderHelper.pawnCollider as CapsuleCollider;

        [Serializable]
        public class Body
        {
            public BoolReactiveProperty isJumping = new();
            public BoolReactiveProperty isGliding = new();
            public BoolReactiveProperty isFalling = new();
            public BoolReactiveProperty isGuarding = new();
            public float getUpWaitTime = 1f;
            public float walkSpeed = 1f;
            public float jumpHeight = 1f;
            public float glidingDuration = 1f;
            public float glidingAmplitude = 1f;
            public float glidingFrequency = 1f;
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
            [Header("ComboAttack")]
            public float comboAttackCoolTime = 1f;
            public float comboAttackDistance = 1f;

            [Header("JumpAttack")]
            public float jumpAttackCoolTime = 10f;

            [Header("ShieldAttack")]
            public float shieldAttackCoolTime = 1f;
            public float shieldAttackRigBlendInSpeed = 1f;
            public float shieldAttackRigBlendOutSpeed = 1f;
            public BoxCollider shieldTouchSensor;

            [Header("Counter")]
            public float counterCoolTime = 1f;

            [Header("Missile")]
            public float missileCoolTime = 1f;
            public float backstepTriggerDistance = 1f;
            public float backstepRootMotionMultiplier = 1f;
            public int missileEmitNum = 1;
            public float missileEmitIntervalA = 1f;
            public float missileEmitIntervalB = 1f;
            public float missileEmitSpeed = 10f;
            public float hoveringDurationA = 1f;
            public float hoveringDurationB = 1f;
            public Transform missileEmitPoint;
            public GameObject missilePrefab;

            [Header("Laser")]
            public float laserDamageInterval = 0.1f;
            public float laserStayDuration = 1f;
            public float laseMaxDistance = 1f;
            public float laserForwardSpeed = 1f;
            public float laserRotateSpeed = 1f;
            public float laserCharingDuration = 1f;

            [Header("Leap")]
            public float leapCoolTime = 1f;
            public float leapJumpHeight = 1f;
            public float leapRootMotionDistance = 1f;
            public float leapRootMotionMultiplier = 1f;
        }

        public Action action = new();

        [Serializable]
        public class Graphics
        {
            [Header("Prefab")]
            public GameObject onHitFx;
            public GameObject onBigHitFx;
            public GameObject onKickHitFx;
            public GameObject onBleedingFx;
            public GameObject onMissedFx;
            public GameObject onBlockedFx;
            public GameObject onGuardBreakFx;
            public GameObject onHomingDecalFx;

            [Header("Material")]
            public Material hitColor;
        }

        public Graphics graphics = new();

        [Serializable]
        public class Audios
        {
            public AudioClip onHitAudioClip;
            public AudioClip onHitAudioClip2;
            public AudioClip onBigHitAudioClip;
            public AudioClip onKickHitAudioClip;
            public AudioClip onBleedingAudioClip;
            public AudioClip onMissedAudioClip;
            public AudioClip onBlockedAudioClip;
            public AudioClip onGuardBreakAudioClip;

            public AudioClip onHitFleshClip;
            public AudioClip onEnterGroggy;

            public AudioClip onFootstepClip;
        }

        public Audios audios = new();


        [Serializable]
        public class Children
        {
            public Transform jellyMeshAttactPoint;
        }

        public Children children = new();


        [Serializable]
        public class Attachment
        {
            public Transform targetLookAt;
            public Transform spine02;
            public Transform  bodyMeshParent;
            public Renderer[] bodyMeshRenderers;
            public Renderer shieldMeshRenderer;
            public ParticleSystem[] jetParticleSystems;
            public Transform blockingFxAttachPoint;
            public Transform specialKeyAttachPoint;
            public Transform jellyMeshAttactPointParent;
            public Transform laserAimPoint;
            public SoldierLaserRenderer laserRenderer;
            public PawnColliderHelper counterActionTraceColliderHelper;
        }

        public Attachment attachment = new();

        protected override void AwakeInternal()
        {
            base.AwakeInternal();

            var tempRenderers = new List<Renderer>();
            foreach (var d in attachment.bodyMeshParent.gameObject.DescendantsAndSelf())
                if (d.TryGetComponent<SkinnedMeshRenderer>(out var renderer) && renderer.enabled) tempRenderers.Add(renderer);

            //* HitColor 하이라이트 bodyMeshRenderers 셋팅
            attachment.bodyMeshRenderers = tempRenderers.ToArray();
        }


    }
}