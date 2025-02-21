using System;
using UniRx;
using UnityEngine;

namespace Game
{
    public class SoldierBlackboard : JellyHumanoidBlackboard
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
        public GameObject MissilePrefab => action.missilePrefab;
        public Transform MissileEmitPoint => action.missileEmitPoint;

        [Serializable]
        public class Body
        {
            public BoolReactiveProperty isJumping = new();
            public BoolReactiveProperty isGliding = new();
            public BoolReactiveProperty isFalling = new();
            public BoolReactiveProperty isGuarding = new();
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
            [Header("CoolDown")]
            public float minCoolDownDuration = 1f;
            public float maxCoolDownDuration= 1f;
            public float sequenceCoolTimeLeft;

            [Header("Counter")]
            public float counterProbBoostRateOnGuard; //* 가드 후 반격 발생 확률 증가

            [Header("Missile")]
            public int missileEmitNum = 1;
            public float missileEmitIntervalA = 1f;
            public float missileEmitIntervalB = 1f;
            public float missileEmitSpeed = 10f;
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
            public float backStepTriggerDistance = 1;
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

            [Header("Material")]
            public Material hitColor;

            [Header("Attached")]
            public Transform BlockingFxAttachPoint;
        }

        public Graphics graphics = new();

        [Serializable]
        public class Audios
        {
            public AudioClip onHitAudioClip;
            public AudioClip onBigHitAudioClip;
            public AudioClip onKickHitAudioClip;
            public AudioClip onBleedingAudioClip;
            public AudioClip onMissedAudioClip;
            public AudioClip onBlockedAudioClip;
            public AudioClip onGuardBreakAudioClip;
        }

        public Audios audios = new();

        [Serializable]
        public class Attachment
        {
            public Renderer shieldMeshRenderer;
            public Renderer[] bodyMeshRenderers;
            public ParticleSystem[] jetParticleSystems;
            public Transform blockingFxAttachPoint;
            public Transform laserAimPoint;
            public SoldierLaserRenderer laserRenderer;
        }

        public Attachment attachment = new();
    }
}