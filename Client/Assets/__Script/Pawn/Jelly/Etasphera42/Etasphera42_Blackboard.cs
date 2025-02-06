using System;
using FIMSpace.Generating;
using UniRx;
using UnityEngine;

namespace Game
{
    public class Etasphera42_Blackboard : JellyQuadWalkBlackboard
    {
        public bool IsDriving => action.isDriving.Value;
        public override bool IsJumping => action.isJumping.Value;
        public override float SpacingInDistance => body.spacingInDistance;
        public override float SpacingOutDistance => body.spacingOutDistance;
        public override float MinSpacingDistance => body.minSpacingDistance;
        public override float MaxSpacingDistance => body.maxSpacingDistance;
        public override float MinApproachDistance => body.minApproachDistance;
        public float HoldPositionRate => body.holdPositionRate;
        public float MoveAroundRate => body.moveAroundRate;
        public GameObject BulletPrefab => action.bulletPrefab;
        public GameObject FlamePrefab => action.framePrefab;
        public GameObject BombPrefab => action.bombPrefab;

        [Serializable]
        public class Body
        {
            public float walkSpeed = 1f;
            public float jumpHeight = 1f;
            public float glidingDuration = 1f;
            public float glidingAmplitude = 1f;
            public float glidingFrequency = 1f;
            public float turretRotateSpeed = 90f;
            public float spacingInDistance = 1f;
            public float spacingOutDistance = 1f;
            public float minSpacingDistance = 1f;
            public float maxSpacingDistance = 1f;
            public float minApproachDistance = 1f;
            public float holdPositionRate = 1f;
            public float moveAroundRate = 1f;
        }

        public Body body = new();

        [Serializable]
        public class Action
        {
            public BoolReactiveProperty isDriving = new();
            public BoolReactiveProperty isJumping = new();

            [Header("Bullet")]
            public GameObject bulletPrefab;
            public float bulletTurretRotateSpeed = 1f;
            public float bulletMaxShakeAngle = 1f;

            [Header("Torch")]
            public GameObject framePrefab;
            public float torchTurretRotateSpeed = 1f;

            [Header("Bomb")]
            public GameObject bombPrefab;

            [Header("LaserA")]
            public float laserA_damageInterval = 0.1f;
            public float laserA_maxDistance = 1f;
            public float laserA_turretRotateSpeed = 1f;
            public float laserA_charingDuration = 1f;
            public float laserA_approachSpeed = 1f;
            public float laserA_approachDuration = 1f;
            public float laserA_sweepDuration = 1f;

            [Header("LaserB")]
            public float laserB_stayDuration = 1f;
            public float laserB_maxDistance = 1f;
            public float laserB_turretRotateSpeed = 1f;
            public float laserB_charingDuration = 1f;
        }

        public Action action = new();

        [Serializable]
        public class Graphics
        {
            public Material hitColor;
            public GameObject onHitFx;
            public GameObject onBigHitFx;
            public GameObject onKickHitFx;
            public GameObject onBleedingFx;
            public GameObject onMissedFx;
            public GameObject onBlockedFx;
            public GameObject onGuardBreakFx;
            public GameObject onJumpSlamFx1;
            public GameObject onJumpSlamFx2;
        }

        public Graphics graphics = new();

        [Serializable]
        public class Attachment
        {
            public SkinnedMeshRenderer[] body_meshRenderers;
            public SkinnedMeshRenderer[] leftLeg1_meshRenderes;
            public SkinnedMeshRenderer[] leftLeg2_meshRenderes;
            public SkinnedMeshRenderer[] rightLeg1_meshRenderes;
            public SkinnedMeshRenderer[] rightLeg2_meshRenderes;
            public Transform laserA_aimPoint;
            public Etasphera42_LaserRenderer laserA_Renderer;
            public Etasphera42_LaserRenderer laserB_Renderer;
        }

        public Attachment attachment = new();
    }
}