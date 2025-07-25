using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Game
{
    public enum WeaponSetType 
    {
        ONEHAND_WEAPONSHIELD,
        TWOHAND_WEAPON, 
    }
    
    public enum WeaponBone
    {
        RIGHTHAND = 0,
        LEFTHAND,
        BACK,
    }

    public enum WeaponType 
    {
        SWORD = 0,
        SHIELD,
        KATANA,
    }

    public class SlayerBrain : PawnBrainController, IPawnSpawnable, IPawnMovable
    {

#region ISpawnable/IMovable 구현
        Vector3 IPawnSpawnable.GetSpawnPosition() => Vector3.zero;
        void IPawnSpawnable.OnDespawnedHandler() {}
        void IPawnSpawnable.OnStartSpawnHandler() {}
        void IPawnSpawnable.OnFinishSpawnHandler() 
        {
            if (this is IPawnEventListener listener)
                PawnEventManager.Instance.RegisterEventListener(listener);
        }
        void IPawnSpawnable.OnDeadHandler() 
        {
            if (this is IPawnEventListener listener)
                PawnEventManager.Instance.UnregisterEventListener(listener);
                 
            var droneBotBrain = droneBotFormationCtrler.PickDroneBot();
            droneBotFormationCtrler.ReleaseDroneBot(droneBotBrain);
            Destroy(droneBotBrain.gameObject);
        }
        void IPawnSpawnable.OnLifeTimeOutHandler() {}
        bool IPawnMovable.IsJumping() { return BB.IsJumping; }
        bool IPawnMovable.IsRolling() { return BB.IsRolling; }
        bool IPawnMovable.IsOnGround() { return Movement.IsOnGround; }
        bool IPawnMovable.CheckReachToDestination() { return false; }
        Vector3 IPawnMovable.GetDestination() { return Movement.capsule.position + Movement.moveVec; }
        float IPawnMovable.GetEstimateTimeToDestination() { return 0; }
        float IPawnMovable.GetDefaultMinApproachDistance() { return 0; }
        bool IPawnMovable.GetFreezeMovement() { return Movement.freezeMovement; }
        bool IPawnMovable.GetFreezeRotation() { return Movement.freezeRotation; }
        void IPawnMovable.ReserveDestination(Vector3 destination) {}
        float IPawnMovable.SetDestination(UnityEngine.Vector3 destination) { return 0; }
        void IPawnMovable.SetMinApproachDistance(float distance) {}
        void IPawnMovable.SetFaceVector(Vector3 faceVec) { Movement.faceVec = faceVec; }
        void IPawnMovable.FreezeMovement(bool newValue) {}
        void IPawnMovable.FreezeRotation(bool newValue) {}
        void IPawnMovable.AddRootMotion(Vector3 deltaPosition, Quaternion deltaRotation, float deltaTime) { Movement.AddRootMotion(deltaPosition, deltaRotation, deltaTime); }
        void IPawnMovable.StartJump(float jumpHeight) {}
        void IPawnMovable.FinishJump() {}
        void IPawnMovable.Teleport(Vector3 destination, Quaternion rot) { Movement.Teleport(destination, rot); }
        void IPawnMovable.MoveTo(Vector3 destination) { Debug.Assert(false); }
        void IPawnMovable.FaceTo(Vector3 lookAt) { Movement.FaceTo(lookAt); }
        void IPawnMovable.Stop() {}
        #endregion

        [Header("Weapon")]
        public WeaponController _weaponCtrlRightHand;

        public WeaponSetType _weaponSetType;
        public Transform[] _trWeaponBone;
        public WeaponController[] _weaponController;

        [Header("Chain")]
        public ChainController _chainCtrl;

        [Header("Component")]
        public DroneBotFormationController droneBotFormationCtrler;

        //public Highlighters.HighlighterSettings _highlighters;

        public SlayerBlackboard BB { get; private set; }
        public SlayerMovement Movement { get; private set; }
        public SlayerAnimController AnimCtrler { get; private set; }
        public SlayerPartsController PartsCtrler { get; private set; }
        public SlayerActionController ActionCtrler { get; private set; }
        public PawnSensorController SensorCtrler { get; private set; }
        public PawnStatusController StatusCtrler { get; private set;}

        public bool _isBind = false;
        public List<PawnBrainController> _bindLIst = new ();

        protected override void AwakeInternal()
        {
            base.AwakeInternal();

            BB = GetComponent<SlayerBlackboard>();
            Movement = GetComponent<SlayerMovement>();
            AnimCtrler = GetComponent<SlayerAnimController>();
            PartsCtrler = GetComponent<SlayerPartsController>();
            ActionCtrler = GetComponent<SlayerActionController>();
            SensorCtrler = GetComponent<PawnSensorController>();
            StatusCtrler = GetComponent<PawnStatusController>();
        }

        IDisposable __regenHeartPointDisposable;
        EffectInstance __regenHeartPointFx;

        protected override void StartInternal()
        {
            base.StartInternal();

            LevelVisibilityManager.Instance.RegisterChecker(BB.children.visibilityChecker);

            // ChangeWeapon(WeaponSetType.ONEHAND_WEAPONSHIELD);

            ActionCtrler.onActionStart += (actionContext, _) =>
            {
                if ((actionContext.actionData?.actionName ?? string.Empty) == "DrinkPotion")
                    Movement.moveSpeed = BB.body.walkSpeed;
            };

            ActionCtrler.onActionCanceled += (actionContext, _) =>
            {
                if ((actionContext.actionData?.actionName ?? string.Empty) == "DrinkPotion")
                    Movement.moveSpeed = BB.body.walkSpeed;

                // ChangeWeapon(WeaponSetType.ONEHAND_WEAPONSHIELD);
            };

            ActionCtrler.onActionFinished += (actionContext) =>
            {
                if ((actionContext.actionData?.actionName ?? string.Empty) == "DrinkPotion")
                    Movement.moveSpeed = BB.body.walkSpeed;

                // ChangeWeapon(WeaponSetType.ONEHAND_WEAPONSHIELD);
            };

            PawnHP.onAvoided += (_, reason) =>
            {
                switch (reason)
                {
                    case "Jump": TimeManager.Instance.SlomoTime(this, 0.7f, 0.2f); break;
                    case "Dodge":
                        {
                            TimeManager.Instance.SlomoTime(this, 0.7f, 0.2f);

                            // 분노 게이지 (회피)
                            var rage = MainTable.PlayerData.GetList().First().evadeRage;
                            AddRagePoint(rage);

                            // 회피 사운드
                            SoundManager.Instance.PlayWithClipPos(BB.audios.onEvadeClip, GetWorldPosition());
                            break;
                        }
                }
            };

            PawnHP.onDamaged += (damageContext) =>
            {
                //! Sender와 Recevier가 동일할 수 있기 때문에 반드시 'receiverBrain'을 먼저 체크해야 함
                if (damageContext.receiverBrain == this)
                    DamageReceiverHandler(ref damageContext);
                else
                    DamageSenderHandler(ref damageContext);
            };

            onUpdate += () =>
            {
                if (Time.time - Mathf.Max(ActionCtrler.LastActionTimeStamp, PawnHP.LastDamageTimeStamp, Movement.LastJumpTimeStamp) > BB.stat.staminaRecoverTimeThreshold)
                    BB.stat.RecoverStamina(Mathf.Max(ActionCtrler.LastActionTimeStamp, PawnHP.LastDamageTimeStamp, Movement.LastJumpTimeStamp), Time.deltaTime);

                BB.stat.ReduceStance(PawnHP.LastDamageTimeStamp, Time.deltaTime);
            };

            StatusCtrler.onStatusActive += (status) =>
            {
                if (status == PawnStatus.RegenHeartPoint)
                {
                    var healInterval = 0.5f;
                    var healCount = StatusCtrler.GetDuration(PawnStatus.RegenHeartPoint) / healInterval;
                    var healAmount = StatusCtrler.GetStrength(PawnStatus.RegenHeartPoint) / healCount;

                    //* 최초 1번은 즉시 회복시켜줌
                    PawnHP.Recover(healAmount, string.Empty);

                    __regenHeartPointDisposable = Observable.Interval(TimeSpan.FromSeconds(healInterval)).Subscribe(_ =>
                    {
                        PawnHP.Recover(healAmount, string.Empty);
                    }).AddTo(this);

                    //* Regen 이펙트 출력
                    __regenHeartPointFx = EffectManager.Instance.ShowLooping(BB.graphics.onHealFx, GetWorldPosition(), Quaternion.identity, Vector3.one);
                    __regenHeartPointFx.transform.SetParent(coreColliderHelper.transform, false);
                    __regenHeartPointFx.transform.localPosition = Vector3.zero;
                }
                else if (status == PawnStatus.IncreasePoise)
                {
                    BB.stat.poise = BB.pawnData.poise + StatusCtrler.GetStrength(PawnStatus.IncreasePoise);
                }
            };

            StatusCtrler.onStatusDeactive += (status) =>
            {
                if (status == PawnStatus.RegenHeartPoint)
                {
                    __regenHeartPointDisposable?.Dispose();
                    __regenHeartPointDisposable = null;

                    //* Regen 이펙트 정지
                    __regenHeartPointFx.Stop();
                }
                else if (status == PawnStatus.IncreasePoise)
                {
                    BB.stat.poise = BB.pawnData.poise;
                }
            };

            BB.action.encounterBrain.Skip(1).Subscribe(v =>
            {
                // if (v != null)
            }).AddTo(this);

            BB.action.punchChargingLevel.Skip(1).Subscribe(v =>
            {
                if (v >= 0)
                {
                    if (StatusCtrler.GetStrength(PawnStatus.IncreasePoise) < 50f)
                        StatusCtrler.AddStatus(PawnStatus.IncreasePoise, 50f);
                }
                else
                {
                    StatusCtrler.RemoveStatus(PawnStatus.IncreasePoise);
                }
            });

            onTick += (_) =>
            {
                if (!BB.IsSpawnFinished || BB.IsDead)
                    return;

                //* 1초마다 SoundSource를 발생시켜 주변의 어그로를 끔
                var timeDist = Time.time - PawnSoundSourceGen.LastGenerateTimeStamp;
                if (timeDist >= 1f)
                    PawnSoundSourceGen.GenerateSoundSource(coreColliderHelper.pawnCollider, 1f, 1f);

                // 1초마다 체력 재생
                // if (StatusCtrler.CheckStatus(PawnStatus.RegenHeartPoint))
                // {
                //     var rate = StatusCtrler.GetStrength(PawnStatus.RegenHeartPoint);
                //     if (timeDist >= 1f)
                //     {
                //         var hpAdd = rate * BB.stat.maxHeartPoint.Value;
                //         PawnHP.heartPoint.Value += hpAdd;
                //         Debug.Log("<color=green>HP Regen : " + (100 * rate) + "%, " + hpAdd + "</green>");

                //         var viewVec = GameContext.Instance.cameraCtrler.gameCamera.transform.forward;
                //         EffectManager.Instance.Show("FX/HealSingle", GetWorldPosition() + Vector3.up - viewVec, Quaternion.identity, Vector3.one, 1f);
                //     }
                // }
            };
        }

        void DamageReceiverHandler(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            if (damageContext.receiverBrain.PawnBB.IsDead)
                return;

            if (damageContext.receiverPenalty.Item1 == Game.PawnStatus.None)
            {
                ActionCtrler.StartAddictiveAction(damageContext, "!OnHit", AnimCtrler.actionLayerBlendOutSpeed);
            }
            else
            {
                if (ActionCtrler.CheckActionRunning())
                    ActionCtrler.CancelAction(false);

                switch (damageContext.receiverPenalty.Item1)
                {
                    case Game.PawnStatus.Staggered: ActionCtrler.StartAction(damageContext, "!OnHit"); break;
                    case Game.PawnStatus.KnockDown: ActionCtrler.StartAction(damageContext, "!OnKnockDown"); break;
                    case Game.PawnStatus.Groggy: ActionCtrler.StartAction(damageContext, "!OnGroggy"); break;
                }
            }

            GameManager.Instance.PawnDamaged(ref damageContext);
        }

        void DamageSenderHandler(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            if (damageContext.senderBrain.PawnBB.IsDead)
                return;

            if (damageContext.senderPenalty.Item1 != Game.PawnStatus.None && ActionCtrler.CheckActionRunning())
                ActionCtrler.CancelAction(false);

            switch (damageContext.actionResult)
            {
                case ActionResults.Blocked: 
                    ActionCtrler.StartAction(damageContext, "!OnBlocked"); break;
                    
                case ActionResults.PunchParrying:
                case ActionResults.GuardParrying:
                    ActionCtrler.StartAction(damageContext, damageContext.senderPenalty.Item1 == Game.PawnStatus.Groggy ? "!OnGroggy" : "!OnParried"); break;
            }
        }
        public void AddRagePoint(float rage)
        {
            if (BB.stat.burst.Value >= BB.stat.maxBurst.Value)
                return;

            BB.stat.burst.Value += rage;
            if (BB.stat.burst.Value >= BB.stat.maxBurst.Value) 
            {
                BB.stat.burst.Value = BB.stat.maxBurst.Value;
            }
        }

        public void SetTarget(PawnBrainController newBrain) 
        {
            BB.target.targetPawnHP.Value = newBrain.PawnHP;
            Movement.freezeRotation = true;
        }

    }
}