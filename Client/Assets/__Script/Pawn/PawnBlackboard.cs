using System;
using System.Linq;
using UniRx;
using UnityEngine;
using XftWeapon;

namespace Game
{
    public class PawnBlackboard : MonoBehaviour
    {
        public bool IsPossessed => __pawnBrain.owner != null;
        public bool IsSpawnFinished => common.isSpawnFinished.Value;
        public bool IsLifeTimeInfinite => LifeTime < 0f;
        public bool IsLifeTimeOut => LifeTime == 0f;
        public bool IsInvincible => common.isInvincible.Value;
        public bool IsGroggy => common.isGroggy.Value;
        public bool IsDown => common.isDown.Value;
        public bool IsDead => common.isDead.Value;
        public float LifeTime => common.lifeTime.Value;
        public Transform TargetCore => TargetBrain != null ? TargetBrain.coreColliderHelper.transform : null;
        public GameObject TargetPawn => target.targetPawnHP.Value != null ? target.targetPawnHP.Value.gameObject : null;
        public PawnBrainController TargetBrain => target.targetPawnHP.Value != null ? target.targetPawnHP.Value.PawnBrain : null;
        public PawnColliderHelper TargetColliderHelper => TargetBrain != null ? (TargetBrain is IPawnTargetable targetable ? targetable.CurrTarget() : TargetBrain.coreColliderHelper) : null;

        [Serializable]
        public class Common
        {
            public PawnId pawnId;
            public string pawnName;
            public string displayName;
            public float spawnWaitingTime = 1f;
            public float despawnWaitingTime = 1f;
            public BoolReactiveProperty isSpawnFinished = new();
            public BoolReactiveProperty isInvincible = new();
            public BoolReactiveProperty isDead = new();
            public BoolReactiveProperty isDown = new();
            public BoolReactiveProperty isGroggy = new();
            public FloatReactiveProperty lifeTime = new(60);
            public ReactiveProperty<PawnHeartPointDispatcher> targetPawnHP = new();
        }

        public Common common = new();

        [Serializable]
        public class Stat
        {
            public FloatReactiveProperty maxHeartPoint = new(1);
            public FloatReactiveProperty maxActionPoint = new(1);
            public FloatReactiveProperty maxStamina = new(1);
            public FloatReactiveProperty maxBurst = new(1);
            public FloatReactiveProperty maxStance = new(1);
            public FloatReactiveProperty maxKnockDown = new(1);
            public IntReactiveProperty maxGroggyHitCount = new();
            public FloatReactiveProperty heartPoint = null;
            public FloatReactiveProperty actionPoint = new(1);
            public FloatReactiveProperty stamina = new(1);
            public FloatReactiveProperty burst = new();
            public FloatReactiveProperty stance = new();
            public FloatReactiveProperty knockDown = new();
            public IntReactiveProperty groggyHitCount = new();
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

        [Serializable]
        public class Target
        {
            public ReactiveProperty<PawnHeartPointDispatcher> targetPawnHP = new();
            public XWeaponTrail[] trail = null;
        }

        public Target target = new();
        
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
            common.displayName = pawnData.name;

            __pawnBrain = GetComponent<PawnBrainController>();
            stat.heartPoint = GetComponent<PawnHeartPointDispatcher>().heartPoint;
            stat.maxHeartPoint.Value = stat.heartPoint.Value = pawnData.health;
            stat.maxActionPoint.Value = stat.actionPoint.Value = pawnData.actionPoint;
            stat.maxStamina.Value = stat.stamina.Value = pawnData.stamina;
            stat.maxBurst.Value = pawnData.rage;
            stat.maxStance.Value = pawnData.stance;
            stat.maxKnockDown.Value = pawnData.knockDown;
            stat.maxGroggyHitCount.Value = pawnData.groggyHitCount;
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