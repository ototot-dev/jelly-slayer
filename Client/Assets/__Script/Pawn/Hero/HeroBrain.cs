using System;
using System.Collections.Generic;
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

    public class HeroBrain : PawnBrainController, IPawnSpawnable, IPawnMovable
    {

#region ISpawnable/IMovable 구현
        Vector3 IPawnSpawnable.GetSpawnPosition() => Vector3.zero;
        void IPawnSpawnable.OnDespawnedHandler() {}
        void IPawnSpawnable.OnStartSpawnHandler() {}
        void IPawnSpawnable.OnFinishSpawnHandler() {}
        void IPawnSpawnable.OnDeadHandler() 
        { 
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
        void IPawnMovable.Teleport(Vector3 destination) { Movement.Teleport(destination); }
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

        public HeroBlackboard BB { get; private set; }
        public HeroMovement Movement { get; private set; }
        public HeroAnimController AnimCtrler { get; private set; }
        public HeroActionController ActionCtrler { get; private set; }
        public PawnSensorController SensorCtrler { get; private set; }
        public PawnStatusController StatusCtrler { get; private set;}

        public bool _isBind = false;
        public List<PawnBrainController> _bindLIst = new ();

        protected override void AwakeInternal()
        {
            base.AwakeInternal();

            BB = GetComponent<HeroBlackboard>();
            Movement = GetComponent<HeroMovement>();
            AnimCtrler = GetComponent<HeroAnimController>();
            ActionCtrler = GetComponent<HeroActionController>();
            SensorCtrler = GetComponent<PawnSensorController>();
            StatusCtrler = GetComponent<PawnStatusController>();
        }

        protected override void StartInternal()
        {
            base.StartInternal();

            ChangeWeapon(WeaponSetType.ONEHAND_WEAPONSHIELD);

            Observable.Timer(TimeSpan.FromSeconds(0.2f)).Subscribe(_ => GameContext.Instance.playerCtrler.Possess(this));

            ActionCtrler.onActionStart += (actionContext, _) =>
            {
                if ((actionContext.actionData?.actionName ?? string.Empty) == "DrinkPotion")
                    Movement.moveSpeed = BB.body.walkSpeed;
            };

            ActionCtrler.onActionCanceled += (actionContext, _) =>
            {
                if ((actionContext.actionData?.actionName ?? string.Empty) == "DrinkPotion")
                    Movement.moveSpeed = BB.body.walkSpeed;

                ChangeWeapon(WeaponSetType.ONEHAND_WEAPONSHIELD);
            };

            ActionCtrler.onActionFinished += (actionContext) =>
            {
                if ((actionContext.actionData?.actionName ?? string.Empty) == "DrinkPotion")
                    Movement.moveSpeed = BB.body.walkSpeed;

                ChangeWeapon(WeaponSetType.ONEHAND_WEAPONSHIELD);
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
                BB.stat.RecoverStamina(Mathf.Max(ActionCtrler.LastActionTimeStamp, PawnHP.LastDamageTimeStamp, Movement.LastJumpTimeStamp), Time.deltaTime);
                BB.stat.ReduceStance(PawnHP.LastDamageTimeStamp, Time.deltaTime);
            };

            StatusCtrler.onStatusActive += (status) =>
            {
                if (status == PawnStatus.IncreasePoise)
                    BB.stat.poise = BB.pawnData.poise + StatusCtrler.GetStrength(PawnStatus.IncreasePoise);
            };
            
            StatusCtrler.onStatusDeactive += (status) =>
            {
                if (status == PawnStatus.IncreasePoise)
                    BB.stat.poise = BB.pawnData.poise;
            };

            BB.body.isCharging.Subscribe(v => 
            {
                if (v)
                    StatusCtrler.AddStatus(PawnStatus.IncreasePoise, 50);
                else
                    StatusCtrler.RemoveStatus(PawnStatus.IncreasePoise);
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
                if (StatusCtrler.CheckStatus(PawnStatus.HPRegen))
                {
                    var rate = StatusCtrler.GetStrength(PawnStatus.HPRegen);
                    if (timeDist >= 1f)
                    {
                        var hpAdd = rate * BB.stat.maxHeartPoint.Value;
                        PawnHP.heartPoint.Value += hpAdd;
                        Debug.Log("<color=green>HP Regen : " + (100 * rate) + "%, " + hpAdd + "</green>");

                        var viewVec = GameContext.Instance.MainCamera.transform.forward;
                        EffectManager.Instance.Show("FX/HealSingle", GetWorldPosition() + Vector3.up - viewVec, Quaternion.identity, Vector3.one, 1f);
                    }
                }
            };
        }

        void DamageReceiverHandler(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            if (damageContext.receiverBrain.PawnBB.IsDead)
                return;

            if (damageContext.receiverPenalty.Item1 == Game.PawnStatus.None)
            {
                ActionCtrler.StartAddictiveAction(damageContext, "!OnHit");
            }
            else
            {
                if (ActionCtrler.CheckActionRunning())
                    ActionCtrler.CancelAction(false);

                switch (damageContext.receiverPenalty.Item1)
                {
                    case Game.PawnStatus.Staggered: ActionCtrler.StartAction(damageContext, "!OnHit", string.Empty); break;
                    case Game.PawnStatus.KnockDown: ActionCtrler.StartAction(damageContext, "!OnKnockDown", string.Empty); break;
                    case Game.PawnStatus.Groggy: ActionCtrler.StartAction(damageContext, "!OnGroggy", string.Empty); break;
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
                    ActionCtrler.StartAction(damageContext, "!OnBlocked", string.Empty); break;
                    
                case ActionResults.KickParried:
                case ActionResults.GuardParried:
                    ActionCtrler.StartAction(damageContext, damageContext.senderPenalty.Item1 == Game.PawnStatus.Groggy ? "!OnGroggy" : "!OnParried", string.Empty); break;
            }
        }

        // public override void ShowTrail(bool isActive, int trailIndex) 
        // {
        //     if (_weaponCtrlRightHand != null)
        //     {
        //         _weaponCtrlRightHand.ShowTrail(isActive);
        //     }
        // }

        // public void Bind(PawnBrainController pawn, bool isBind = true) 
        // {
        //     _isBind = isBind;
        //     if (_isBind == true)
        //     {
        //         pawn.PawnStatusCtrler.AddStatus(Game.PawnStatus.Bind);
        //         _bindLIst.Add(pawn);
        //     }
        //     else 
        //     {
        //         if (pawn != null)
        //         {
        //             pawn.PawnStatusCtrler.RemoveStatus(Game.PawnStatus.Bind);
        //         }
        //         _bindLIst.Clear();
        //     }
        // }

        // public void ChainRolling(bool isRolling) 
        // {
        //     _chainCtrl.Rolling(isRolling);
        // }

        // public void ChainShoot() 
        // {
        //     Debug.Log("ChainShoot");
        //     _chainCtrl.Shoot(TargetPawn);
        // }

        void ResetToMainWeapon() 
        {
            ChangeWeapon(WeaponSetType.ONEHAND_WEAPONSHIELD);
        }
        public void SetTarget(PawnBrainController newBrain) 
        {
            BB.target.targetPawnHP.Value = newBrain.PawnHP;
            Movement.freezeRotation = true;
        }

        WeaponController GetWeaponController(WeaponType weaponType)
        {
            return _weaponController[(int)weaponType];
        }
        Transform GetWeaponBone(WeaponBone weaponBone)
        {
            return _trWeaponBone[(int)weaponBone];
        }

        void EquipWeaponToBone(WeaponType weaponType, WeaponBone weaponBone) 
        {
            return;
            var controller = GetWeaponController(weaponType);
            var bone = GetWeaponBone(weaponBone);
            if (weaponBone == WeaponBone.RIGHTHAND)
            {
                _weaponCtrlRightHand = controller;
                AnimCtrler.weaponMeshSlot = controller.transform;
            }
            controller.EquipToBone(bone, (weaponBone == WeaponBone.RIGHTHAND));
        }

        public void ChangeWeapon(WeaponSetType weaponSetType) 
        {
            return;
            // if(_weaponSetType == weaponSetType) 
            //     return;

            _weaponSetType = weaponSetType;
            switch (weaponSetType)
            {
                case WeaponSetType.ONEHAND_WEAPONSHIELD:
                    {
                        EquipWeaponToBone(WeaponType.SWORD, WeaponBone.RIGHTHAND);
                        EquipWeaponToBone(WeaponType.SHIELD, WeaponBone.LEFTHAND);
                        EquipWeaponToBone(WeaponType.KATANA, WeaponBone.BACK);
                    }
                    break;
                case WeaponSetType.TWOHAND_WEAPON:
                    {
                        EquipWeaponToBone(WeaponType.SWORD, WeaponBone.BACK);
                        EquipWeaponToBone(WeaponType.SHIELD, WeaponBone.BACK);
                        EquipWeaponToBone(WeaponType.KATANA, WeaponBone.RIGHTHAND);
                    }
                    break;
            }
        }
    }
}