using System;
using System.Collections.Generic;
using System.Linq;
using NodeCanvas.StateMachines;
using UniRx;
using UnityEngine;

namespace Game
{
    public abstract class PawnBrainController : MonoBehaviour, IInteractionKeyAttachable, IBubbleDialogueAttachable
    {
        public virtual void OnPossessedHandler() { }
        public virtual void OnUnpossessedHandler() { }
        public virtual void OnTouchTerrainBoundaryHandler(GameObject boundary) { }
        public virtual void OnWatchSomethingOrDamagedHandler(PawnBrainController otherBrain, float reservedDecisionCoolTime) { }
        public virtual void OnDecisionFinishedHandler() { }
        public virtual void InvalidateDecision(float decisionCoolTime = 0) { }
        public virtual void ChangeDecision(int newDecision) { }
        public virtual Vector3 GetWorldForward() => coreColliderHelper != null ? coreColliderHelper.transform.forward : transform.forward;
        public virtual Vector3 GetWorldPosition() => coreColliderHelper != null ? coreColliderHelper.transform.position : transform.position;
        public virtual Quaternion GetWorldRotation() => coreColliderHelper != null ? coreColliderHelper.transform.rotation : transform.rotation;
        public virtual Transform GetWorldTransform() => coreColliderHelper != null ? coreColliderHelper.transform : transform;
        public virtual PawnColliderHelper GetHookingColliderHelper() => hookingPointColliderHelper;
        public virtual Vector3 GetStatusBarAttachPoint() => coreColliderHelper != null ? coreColliderHelper.transform.position + coreColliderHelper.GetCapsuleHeight() * Vector3.up : transform.position;
        public virtual Vector3 GetBubbleDialogueAttachPoint() => coreColliderHelper != null ? coreColliderHelper.transform.position + coreColliderHelper.GetCapsuleHeight() * Vector3.up : transform.position;

#region IInteractionKeyAttachable 구현
        public virtual bool GetInteractionEnanbled() => true;
        public virtual float GetInteractionVisibleRadius() => -1f;
        public virtual Vector3 GetInteractionKeyAttachPoint() => coreColliderHelper != null ? coreColliderHelper.transform.position + coreColliderHelper.GetCapsuleHeight() * Vector3.up : transform.position;
        public virtual void OnInteractionKeyAttached() { }
        public virtual void OnInteractionKeyDetached() { }
#endregion

        [Header("Component")]
        public PawnColliderHelper coreColliderHelper;
        public PawnColliderHelper bodyHitColliderHelper;
        public PawnColliderHelper parryHitColliderHelper;
        public PawnColliderHelper shieldHitColliderHelper;
        public PawnColliderHelper hookingPointColliderHelper;
        public SphereCollider visibilityChecker;
        public PlayerController owner;

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
        public PawnStatusController PawnStatusCtrler { get; private set; }
        public PawnSensorController PawnSensorCtrler { get; private set; }
        public PawnSoundSourceGenerator PawnSoundSourceGen { get; private set; }
        public readonly HashSet<PawnColliderHelper> pawnColliderHelpers = new();

        protected virtual void StartInternal()
        {
            // if (visibilityChecker != null)
            //     LevelVisibilityManager.Instance.RegisterChecker(visibilityChecker);

            //* 지형 위로 y값 보정
            // var hit = TerrainManager.GetTerrainHitPoint(transform.position);
            // if (hit.collider != null)
            //     transform.position = hit.point;

            if (FSM != null)
                Observable.Timer(TimeSpan.FromSeconds(0.1f)).Subscribe(_ => FSM.enabled = true).AddTo(this);

            if (!PawnBB.IsSpawnFinished)
            {
                (this as IPawnSpawnable)?.OnStartSpawnHandler();

                //* spawnWaitingTime 값이 0보다 클때만 자동으로 isSpawnFinished 값이 true가 되도록 타아머 에약 (spawnWaitingTime값이 음수면 수동으로 직접 값을 true로 변경해줘야 함)
                if (PawnBB.common.spawnWaitingTime > 0f)
                {
                    __Logger.Log(gameObject, "spawnWaitingTime", PawnBB.common.spawnWaitingTime);
                    Observable.Timer(TimeSpan.FromSeconds(PawnBB.common.spawnWaitingTime)).Subscribe(_ =>
                    {
                        PawnBB.common.isSpawnFinished.Value = true;
                        (this as IPawnSpawnable)?.OnFinishSpawnHandler();
                    }).AddTo(this);
                }
            }

            PawnBB.common.isDead.Where(v => v).Subscribe(v =>
            {
                (this as IPawnSpawnable)?.OnDeadHandler();

                //* Despawn 웨이팅 걸기
                Observable.Timer(TimeSpan.FromSeconds(PawnBB.common.despawnWaitingTime)).Where(_ => PawnBB.IsDead).Subscribe(_ =>
                {
                    (this as IPawnSpawnable)?.OnDespawnedHandler();

                    if (FSM != null)
                        FSM.enabled = false;
                    if (visibilityChecker != null)
                        LevelVisibilityManager.Instance.UnregisterChecker(visibilityChecker);

                    Destroy(gameObject, 0.1f);
                }).AddTo(this);
            }).AddTo(this);

            if (PawnStatusCtrler != null)
            {
                PawnStatusCtrler.onStatusActive += (status) =>
                {
                    switch (status)
                    {
                        case PawnStatus.KnockDown:
                            PawnBB.common.isDown.Value = true; break;
                        case PawnStatus.Groggy:
                            PawnBB.common.isGroggy.Value = true; break;
                    }
                };

                PawnStatusCtrler.onStatusDeactive += (status) =>
                {
                    switch (status)
                    {
                        case PawnStatus.KnockDown:
                            PawnBB.common.isDown.Value = false; break;
                        case PawnStatus.Groggy:
                            PawnBB.common.isGroggy.Value = false; break;
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
                Observable.Timer(TimeSpan.FromSeconds(UnityEngine.Random.Range(0f, 1f))).Subscribe(_ =>
                {
                    Observable.Interval(TimeSpan.FromSeconds(tickInterval)).Subscribe(_ =>
                    {
                        if (!__isBrainInited)
                            return;

                        __tickCount++;
                        
                        onPreTick?.Invoke(tickInterval);
                        OnTickInternal(tickInterval);
                        onTick?.Invoke(tickInterval);
                    }).AddTo(this);
                }).AddTo(this);
            }

            if (fixedUpdateEnabled)
                Observable.EveryFixedUpdate().Where(_ => __isBrainInited).Subscribe(_ => onFixedUpdate?.Invoke()).AddTo(this);
            if (lateUpdateEnabled)
                Observable.EveryLateUpdate().Where(_ => __isBrainInited).Subscribe(_ => onLateUpdate?.Invoke()).AddTo(this);
            if (updateEnabled)
                Observable.EveryUpdate().Where(_ => __isBrainInited).Subscribe(_ => onUpdate?.Invoke()).AddTo(this);

            // Spawn
            GameManager.Instance.OnSpawned(this);
        }

        protected bool __isBrainInited;
        protected int __tickCount;

        protected virtual void Init()
        {
            __isBrainInited = true;
        }

        protected virtual void OnTickInternal(float interval)
        {
            if (PawnBB.IsSpawnFinished && !PawnBB.IsLifeTimeOut && !PawnBB.IsLifeTimeInfinite)
            {
                if ((PawnBB.common.lifeTime.Value = Mathf.Max(0, PawnBB.common.lifeTime.Value - interval)) <= 0)
                    (this as IPawnSpawnable)?.OnLifeTimeOutHandler();
            }
        }
    }
}
