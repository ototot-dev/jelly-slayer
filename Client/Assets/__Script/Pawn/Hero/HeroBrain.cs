using NUnit.Framework;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using static FIMSpace.FProceduralAnimation.LegsAnimator;

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

    public class HeroBrain : PawnBrainController, ISpawnable, IMovable
    {

#region ISpawnable/IMovable 구현
        Vector3 ISpawnable.GetSpawnPosition() => Vector3.zero;
        void ISpawnable.OnDespawnedHandler() {}
        void ISpawnable.OnStartSpawnHandler() {}
        void ISpawnable.OnFinishSpawnHandler() {}
        void ISpawnable.OnDeadHandler() {}
        void ISpawnable.OnLifeTimeOutHandler() {}
        bool IMovable.IsJumping() { return BB.IsJumping; }
        bool IMovable.IsRolling() { return BB.IsRolling; }
        bool IMovable.CheckReachToDestination() { return false; }
        Vector3 IMovable.GetDestination() { return Movement.capsule.position + Movement.moveVec; }
        float IMovable.GetEstimateTimeToDestination() { return 0; }
        float IMovable.GetDefaultMinApproachDistance() { return 0; }
        bool IMovable.GetFreezeMovement() { return Movement.freezeMovement; }
        bool IMovable.GetFreezeRotation() { return Movement.freezeRotation; }
        void IMovable.AddRootMotion(Vector3 deltaPosition, Quaternion deltaRotation) { Movement.AddRootMotion(deltaPosition, deltaRotation); }
        void IMovable.ReserveDestination(Vector3 destination) {}
        float IMovable.SetDestination(UnityEngine.Vector3 destination) { return 0; }
        void IMovable.SetMinApproachDistance(float distance) {}
        void IMovable.SetFaceVector(Vector3 faceVec) { Movement.faceVec = faceVec; }
        void IMovable.FreezeMovement(bool newValue) {}
        void IMovable.FreezeRotation(bool newValue) {}
        void IMovable.Teleport(Vector3 destination) { Movement.Teleport(destination); }
        void IMovable.MoveTo(Vector3 destination) { Movement.MoveTo(destination); }
        void IMovable.FaceTo(Vector3 lookAt) { Movement.FaceTo(lookAt); }
        void IMovable.Stop() {}
        #endregion

        [Header("Weapon")]
        public WeaponController _weaponCtrlRightHand;

        public WeaponSetType _weaponSetType;
        public Transform[] _trWeaponBone;
        public WeaponController[] _weaponController;

        [Header("Chain")]
        public ChainController _chainCtrl;

        public HeroBlackboard BB { get; private set; }
        public HeroMovement Movement { get; private set; }
        public HeroAnimController AnimCtrler { get; private set; }
        public HeroActionController ActionCtrler { get; private set; }
        public PawnSensorController SensorCtrler { get; private set; }
        public PawnBuffController BuffCtrler { get; private set;}

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
            BuffCtrler = GetComponent<PawnBuffController>();
        }

        protected override void StartInternal()
        {
            base.StartInternal();

            ChangeWeapon(WeaponSetType.ONEHAND_WEAPONSHIELD);

            Observable.Timer(TimeSpan.FromSeconds(0.2f)).Subscribe(_ => GameContext.Instance.playerCtrler.Possess(this));

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
                var timeStampA = Mathf.Max(ActionCtrler.LastActionTimeStamp, PawnHP.LastDamageTimeStamp);
                var timeStampB = Mathf.Max(Movement.LastJumpTimeStamp, Movement.LastRollingTimeStamp);
                BB.stat.RecoverStamina(Mathf.Max(timeStampA, timeStampB), Time.deltaTime);
                BB.stat.ReduceStance(PawnHP.LastDamageTimeStamp, Time.deltaTime);
            };

            onTick += (_) =>
            {
                if (!BB.IsSpawnFinished || BB.IsDead)
                    return;

                //* 1초마다 SoundSource를 발생시켜 주변의 어그로를 끔
                if (Time.time - PawnSoundSourceGen.LastGenerateTimeStamp > 1f)
                    PawnSoundSourceGen.GenerateSoundSource(coreColliderHelper.pawnCollider, 1f, 1f);
            };
        }

        void DamageReceiverHandler(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            if (damageContext.receiverBrain.PawnBB.IsDead)
                return;

            if (damageContext.actionResult == ActionResults.Blocked || damageContext.actionResult == ActionResults.ActiveParried || damageContext.actionResult == ActionResults.PassiveParried)
            {
                ActionCtrler.StartAddictiveAction(damageContext, "!OnHit");
            }
            else if (damageContext.receiverPenalty.Item1 != BuffTypes.None)
            {
                if (ActionCtrler.CheckActionRunning())
                    ActionCtrler.CancelAction(false);

                switch (damageContext.receiverPenalty.Item1)
                {
                    case BuffTypes.Groggy: ActionCtrler.StartAction(damageContext, "!OnGroggy", string.Empty); break;
                    case BuffTypes.Staggered: ActionCtrler.StartAction(damageContext, "!OnHit", string.Empty); break;
                    case BuffTypes.KnockDown: ActionCtrler.StartAction(damageContext, "!OnKnockDown", string.Empty); break;
                }
            }
            
            if (damageContext.finalDamage > 0)
                GameManager.Instance.PawnDamaged(ref damageContext);
        }

        void DamageSenderHandler(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            if (damageContext.senderBrain.PawnBB.IsDead)
                return;

            if (damageContext.senderPenalty.Item1 != BuffTypes.None && ActionCtrler.CheckActionRunning())
                ActionCtrler.CancelAction(false);

            switch (damageContext.actionResult)
            {
                case ActionResults.Blocked: 
                    ActionCtrler.StartAction(damageContext, "!OnBlocked", string.Empty); break;
                    
                case ActionResults.ActiveParried:
                case ActionResults.PassiveParried: 
                    ActionCtrler.StartAction(damageContext, "!OnParried", string.Empty); break;
            }
        }

        public override void StartJump() 
        {
            // Effect
            var pos = coreColliderHelper.transform.position;
            pos.y = 0;
            EffectManager.Instance.Show("FX_Cartoony_Jump_Up_01", pos, Quaternion.identity, Vector3.one, 1f);

            SoundManager.Instance.Play(SoundID.JUMP);
        }
        public override void RollingGround()
        {
            // Effect
            var pos = coreColliderHelper.transform.position;
            pos.y = 0;
            EffectManager.Instance.Show("FX_Cartoony_Jump_Up_01", pos, Quaternion.identity, Vector3.one, 1f);

            SoundManager.Instance.Play(SoundID.ROLL);
        }
        public override void StartLand()
        {
            Debug.Log("PlayLandEffect");
            AnimCtrler.OnEventLand();

            // Effect
            var pos = coreColliderHelper.transform.position;
            pos.y = 0;
            EffectManager.Instance.Show("FX_Cartoony_Jump_01", pos, Quaternion.identity, Vector3.one, 1f);

            //SoundManager.Instance.Play(SoundID.LAND);
        }

        public override void ShowTrail(bool isActive, int trailIndex) 
        {
            if (_weaponCtrlRightHand != null)
            {
                _weaponCtrlRightHand.ShowTrail(isActive);
            }
        }

        public void Bind(PawnBrainController pawn, bool isBind = true) 
        {
            _isBind = isBind;
            if (_isBind == true)
            {
                pawn.PawnBuff.AddBuff(BuffTypes.Bind);
                _bindLIst.Add(pawn);
            }
            else 
            {
                if (pawn != null)
                {
                    pawn.PawnBuff.RemoveBuff(BuffTypes.Bind);
                }
                _bindLIst.Clear();
            }
        }

        public void ChainRolling(bool isRolling) 
        {
            _chainCtrl.Rolling(isRolling);
        }

        public void ChainShoot() 
        {
            Debug.Log("ChainShoot");
            _chainCtrl.Shoot(TargetPawn);
        }

        void ResetToMainWeapon() 
        {
            ChangeWeapon(WeaponSetType.ONEHAND_WEAPONSHIELD);
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
            var controller = GetWeaponController(weaponType);
            var bone = GetWeaponBone(weaponBone);
            controller.EquipToBone(bone);

            if (weaponBone == WeaponBone.RIGHTHAND) 
            {
                _weaponCtrlRightHand = controller;
            }
        }

        public void ChangeWeapon(WeaponSetType weaponSetType) 
        {
            if(_weaponSetType == weaponSetType) 
                return;

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