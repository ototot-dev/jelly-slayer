using System;
using System.Linq;
using NodeCanvas.StateMachines;
using UniRx;
using Unity.Linq;
using UnityEngine;

namespace Game
{
    public abstract class PawnBrainController : MonoBehaviour
    {
        public virtual void OnPossessedHandler() {}
        public virtual void OnUnpossessedHandler() {}
        public virtual void OnTouchTerrainBoundaryHandler(GameObject boundary) {}
        public virtual void OnDecisionFinishedHandler() {}
        public virtual void OnWatchSomethingOrDamagedHandler(PawnBrainController otherBrain, float reservedDecisionCoolTime) {}
     
        [Header("Update")]
        public float tickInterval = 0.1f;
        public bool tickEnabled = true;
        public bool updateEnabled = true;
        public bool lateUpdateEnabled = false;
        public bool fixedUpdateEnabled = false;
        public Action onInit;
        public Action onUpdate;
        public Action onLateUpdate;
        public Action onFixedUpdate;
        public Action<float> onTick;
        public Action<float> onPreTick;

        // 타겟팅 기술을 쓸 경우 셋팅 (잡기, 처형 등등)
        public PawnBrainController TargetPawn { get; set; }

        [Header("Component")]
        public PawnColliderHelper coreColliderHelper;

        [Header("Ownership")]
        public PlayerController owner;

        void Awake()
        {
            AwakeInternal();
        }

        protected virtual void AwakeInternal()
        {
            FSM = GetComponent<FSMOwner>();
            PawnBB = GetComponent<PawnBlackboard>();
            PawnHP = GetComponent<PawnHeartPointDispatcher>();
            PawnStatusCtrler = GetComponent<PawnStatusController>();
            PawnSensorCtrler = GetComponent<PawnSensorController>();
            PawnSoundSourceGen = GetComponent<PawnSoundSourceGenerator>();
        }

        public FSMOwner FSM { get; private set; }
        public PawnBlackboard PawnBB { get; private set; }
        public PawnHeartPointDispatcher PawnHP { get; private set; }
        public PawnStatusController PawnStatusCtrler { get; private set;}
        public PawnSensorController PawnSensorCtrler { get; private set;}
        public PawnSoundSourceGenerator PawnSoundSourceGen { get; private set; }

        // 실제 모델 위치
        public Transform CoreTransform {
            get { return (coreColliderHelper) ? coreColliderHelper.transform : transform; }
        }
        
        public void TargetAction(ActionType type)
        {
            if (TargetPawn == null)
                return;

            TargetPawn.DoAction(type, this);
        }

        protected virtual void StartInternal()
        {
            //* 지형 위로 y값 보정
            var hit = TerrainManager.GetTerrainHitPoint(transform.position);
            if (hit.collider != null)
                transform.position = hit.point;

            if (FSM != null)
                Observable.Timer(TimeSpan.FromSeconds(0.1f)).Subscribe(_ => FSM.enabled = true).AddTo(this);

            if (!PawnBB.IsSpawnFinished)
            {
                (this as ISpawnable)?.OnStartSpawnHandler();

                //* spawnWaitingTime 값이 0보다 클때만 자동으로 isSpawnFinished 값이 true가 되도록 타아머 에약 (spawnWaitingTime값이 음수면 수동으로 직접 값을 true로 변경해줘야 함)
                if (PawnBB.common.spawnWaitingTime > 0f)
                {
                    __Logger.Log(gameObject, "spawnWaitingTime", PawnBB.common.spawnWaitingTime);
                    Observable.Timer(TimeSpan.FromSeconds(PawnBB.common.spawnWaitingTime)).Subscribe(_ =>
                    {
                        PawnBB.common.isSpawnFinished.Value = true;
                        (this as ISpawnable)?.OnFinishSpawnHandler();
                    }).AddTo(this);
                }
            }

            PawnBB.common.isDead.Where(v => v).Subscribe(v =>
            {
                (this as ISpawnable)?.OnDeadHandler();

                //* Despawn 웨이팅 걸기
                Observable.Timer(TimeSpan.FromSeconds(PawnBB.common.despawnWaitingTime)).Where(_ => PawnBB.IsDead).Subscribe(_ =>
                {
                    (this as ISpawnable)?.OnDespawnedHandler();
                    if (FSM != null) FSM.enabled = false;
                    Destroy(gameObject, 0.1f);
                }).AddTo(this);
            }).AddTo(this);

            if (PawnStatusCtrler != null)
            {
                PawnStatusCtrler.onStatusActive += (buff) =>
                {
                    switch (buff) {
                        case Game.PawnStatus.KnockDown:
                            PawnBB.common.isDown.Value = true; break;
                        case Game.PawnStatus.Groggy:
                            PawnBB.common.isStunned.Value = true; break;
                        case Game.PawnStatus.Bind:
                            PawnBB.common.isBind.Value = true; break;
                        case Game.PawnStatus.Guardbreak:
                            PawnBB.common.isGuardbreak.Value = true; break;
                    }
                };

                PawnStatusCtrler.onStatusDeactive += (buff) =>
                {
                    switch (buff)
                    {
                        case Game.PawnStatus.KnockDown:
                             PawnBB.common.isDown.Value = false; break;
                        case Game.PawnStatus.Groggy:
                            PawnBB.common.isStunned.Value = false; break;
                        case Game.PawnStatus.Bind:
                            PawnBB.common.isBind.Value = false; break;
                        case Game.PawnStatus.Guardbreak:
                            PawnBB.common.isGuardbreak.Value = false; break;
                    }
                };
            }
        }

        void Start()
        {
            Debug.Assert(PawnBB != null);

            StartInternal();

            Observable.NextFrame().Subscribe(_ => 
            {
                Init();
                onInit?.Invoke();
            }).AddTo(this);

            if (tickEnabled)
            {
                Observable.Interval(TimeSpan.FromSeconds(tickInterval)).Subscribe(_ =>
                {
                    if (__brainInited)
                    {
                        onPreTick?.Invoke(tickInterval);
                        OnTickInternal(tickInterval);
                        onTick?.Invoke(tickInterval);
                    }
                }).AddTo(this);
            }

            if (fixedUpdateEnabled)
                Observable.EveryFixedUpdate().Where(_ => __brainInited).Subscribe(_ => onFixedUpdate?.Invoke()).AddTo(this);
            if (lateUpdateEnabled)
                Observable.EveryLateUpdate().Where(_ => __brainInited).Subscribe(_ => onLateUpdate?.Invoke()).AddTo(this);
            if (updateEnabled)
                Observable.EveryUpdate().Where(_ => __brainInited).Subscribe(_ => onUpdate?.Invoke()).AddTo(this);

            // Spawn
            GameManager.Instance.Spawn(this);
        }

        protected bool __brainInited;

        protected virtual void Init()
        {
            __brainInited = true;
        }

        protected virtual void OnTickInternal(float interval)
        {
            if (PawnBB.IsSpawnFinished && !PawnBB.IsLifeTimeOut && !PawnBB.IsLifeTimeInfinite)
            {
                if ((PawnBB.common.lifeTime.Value = Mathf.Max(0, PawnBB.common.lifeTime.Value - interval)) <= 0)
                    (this as ISpawnable)?.OnLifeTimeOutHandler();
            }
        }
        public float GetDistance(PawnBrainController pawn) 
        {
            var vDist = CoreTransform.position - pawn.CoreTransform.position;
            return vDist.magnitude;
        }
        
        public virtual void StartJump() { }
        public virtual void StartLand() { }
        public virtual void RollingGround() { }
        public virtual void ShowTrail(bool isActive, int trailIndex) { }
        public virtual void DoAction(ActionType type, PawnBrainController attacker) { }
    }
}
