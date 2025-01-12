using System;
using UniRx;
using UnityEngine;

namespace Game
{
    public class SoldierBlackboard : JellyHumanoidBlackboard
    {
        public override bool IsJumping => action.isJumping.Value;
        public override bool IsGliding => action.isGliding.Value;
        public override bool IsFalling => action.isFalling.Value;
        public override bool IsGuarding => action.isGuarding.Value;
        public override float SpacingInDistance => action.spacingInDistance;
        public override float SpacingOutDistance => action.spacingOutDistance;
        public override float MinSpacingDistance => action.minSpacingDistance;
        public override float MaxSpacingDistance => action.maxSpacingDistance;
        public override float MinApproachDistance => action.minApproachDistance;
        public float HoldPositionRate => action.holdPositionRate;
        public float MoveAroundRate => action.moveAroundRate;

        [Serializable]
        public class Body
        {
            public float walkSpeed = 1f;
            public float jumpHeight = 1f;
            public float glidingDuration = 1f;
            public float glidingAmplitude = 1f;
            public float glidingFrequency = 1f;
        }

        public Body body = new();

        [Serializable]
        public class Action
        {
            public BoolReactiveProperty isJumping = new();
            public BoolReactiveProperty isGliding = new();
            public BoolReactiveProperty isFalling = new();
            public BoolReactiveProperty isGuarding = new();
            public float spacingInDistance = 1f;
            public float spacingOutDistance = 1f;
            public float minSpacingDistance = 1f;
            public float maxSpacingDistance = 1f;
            public float minApproachDistance = 1f;
            public float holdPositionRate = 1f;
            public float moveAroundRate = 1f;
            public float comboAttackRateBoostAfterCounterAttack;  //* 반격 후 콤보 1타 발생 확률 증가
            public float allAttackFixedRateAfterLeapHit; //* 점프 공격 히트 후 모든 공격 발생 확률 고정
            public float comboAttackRateStep;  //* Idle 상태에서 콤보 1타 발생 확률 증가
            public float counterAttackRateStep; //* 블럭 후 반격 발생 확률 증가
            public float leapRateStep; //* 타켓과 거리가 떨어졌을 때 Leap 발생 확률 증가
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
    }
}