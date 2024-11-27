using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Game
{
    public class PawnBlackboard : MonoBehaviour
    {
        public bool IsPossessed => __pawnBrain.owner != null;
        public bool IsSpawnFinished => common.isSpawnFinished.Value;
        public bool IsLifeTimeInfinite => LifeTime < 0;
        public bool IsLifeTimeOut => LifeTime == 0;
        public bool IsInvincible => common.isInvincible.Value;
        public bool IsGroggy => common.isGroggy.Value;
        public bool IsDown => common.isDown.Value;
        public bool IsDead => common.isDead.Value;
        public bool IsRagdoll => common.isRagdoll.Value;
        public bool IsThrowing => common.isThrowing.Value;
        public bool IsGrabbed => common.isGrabbed.Value;
        public bool IsBind => common.isBind.Value;
        public bool IsGuardbreak => common.isGuardbreak.Value;
        public float LifeTime => common.lifeTime.Value;

        /// <summary>
        /// Common 데이터 섹션
        /// </summary>
        [Serializable]
        public class Common
        {
            public PawnId pawnId;
            public string pawnName;
            public float spawnWaitingTime = 1;
            public float despawnWaitingTime = 1;
            public BoolReactiveProperty isSpawnFinished = new();
            public BoolReactiveProperty isInvincible = new();
            public BoolReactiveProperty isGroggy = new();
            public BoolReactiveProperty isDown = new();
            public BoolReactiveProperty isDead = new();
            public BoolReactiveProperty isRagdoll = new();
            public BoolReactiveProperty isThrowing = new();     // 잡기
            public BoolReactiveProperty isGrabbed = new();      // 잡힌 상태
            public BoolReactiveProperty isBind = new();         // 묶인 상태
            public BoolReactiveProperty isGuardbreak = new();   // 가드 깨진 상태
            public FloatReactiveProperty lifeTime = new(60);
        }

        public Common common = new();

        /// <summary>
        /// Stat 데이터 섹션
        /// </summary>
        [Serializable]
        public class Stat
        {
            public FloatReactiveProperty maxHeartPoint = new(1);
            public FloatReactiveProperty maxMagicPoint = new(1);
            public FloatReactiveProperty maxStamina = new(1);
            public FloatReactiveProperty maxStance = new(1);
            public FloatReactiveProperty maxKnockDown = new(1);
            public FloatReactiveProperty heartPoint = null;
            public FloatReactiveProperty magicPoint = new(1);
            public FloatReactiveProperty stamina = new(1);
            public FloatReactiveProperty stance = new();
            public FloatReactiveProperty knockDown = new();
            public float poise;
            public float guardStrength;
            public float guardEfficiency;
            public float guardStaminaCost;
            public float guardStaggerDuration;
            public float physAttack;
            public float magicAttack;
            public float physDefence;
            public float magicDefense;
            public float staminaRecoverSpeed;
            public float staminaRecoverTimeThreshold;
            public float stanceReduceSpeed;
            public float stanceReduceTimeThreshold;
        }

        public Stat stat = new();
        
        void Awake()
        {
#if UNITY_EDITOR
            DatasheetManager.Instance.Load();
#endif
            AwakeInternal();
        }

        protected virtual void AwakeInternal() 
        {
            pawnData = MainTable.PawnData.PawnDataList.First(d => d.pawnId == common.pawnId);
            __pawnBrain = GetComponent<PawnBrainController>();
            stat.heartPoint = GetComponent<PawnHeartPointDispatcher>().heartPoint;
            stat.maxHeartPoint.Value = stat.heartPoint.Value = pawnData.health;
            stat.maxMagicPoint.Value = stat.magicPoint.Value = pawnData.magic;
            stat.maxStamina.Value = stat.stamina.Value = pawnData.stamina;
            stat.maxStance.Value = pawnData.stance;
            stat.maxKnockDown.Value = pawnData.knockDown;
            stat.poise = pawnData.poise;
            stat.physAttack = pawnData.physAttack;
            stat.magicAttack = pawnData.magicAttack;
            stat.physDefence = pawnData.physDefence;
            stat.magicDefense = pawnData.magicAttack;
            stat.guardStrength = pawnData.guardStrength;
            stat.guardEfficiency = pawnData.guardEfficiency;
            stat.guardStaminaCost = pawnData.guardStaminaCost;
            stat.guardStaggerDuration = pawnData.guardStaggerDuration;
            stat.staminaRecoverSpeed = pawnData.staminaRecoverSpeed;
        }

        public MainTable.PawnData pawnData;
        protected PawnBrainController __pawnBrain;
    }
}