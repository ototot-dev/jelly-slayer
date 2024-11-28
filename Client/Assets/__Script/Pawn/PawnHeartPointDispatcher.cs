using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Game
{
    public class PawnHeartPointDispatcher : MonoBehaviour
    {
        public FloatReactiveProperty heartPoint = new();
        public PawnBrainController PawnBrain { get; private set; }
        public float LastDamageTimeStamp { get; private set; }

        void Awake()
        {
            PawnBrain = GetComponent<PawnBrainController>();
        }

        /// <summary>
        /// 데미지 히스토리
        /// Key => Attacker, Value => (Item1: Damage, Item2: IsDamageBlocked, Item3: ActionName, Item4: TimeStamp)
        /// </summary>
        public Dictionary<GameObject, Tuple<float, bool, string, float>> damageHistories = new();

        public void RecordDamageHistory(GameObject attacker, string actionName, float damage, bool isDamageBlocked)
        {
            LastDamageTimeStamp = Time.time;
            if (!damageHistories.ContainsKey(attacker))
                damageHistories.Add(attacker, new Tuple<float, bool, string, float>(damage, isDamageBlocked, actionName, LastDamageTimeStamp));
            else
                damageHistories[attacker] = new Tuple<float, bool, string, float>(damage, isDamageBlocked, actionName, LastDamageTimeStamp);
        }

        public bool CheckDamageHistory(GameObject attacker, float maxDeltaTime, params string[] actionNames)
        {
            if (damageHistories.TryGetValue(attacker, out var tuple))
                return actionNames.Any(n => n == tuple.Item3) && (Time.time - tuple.Item4) < maxDeltaTime;
            else
                return false;
        }

        public bool CheckDamageHistory(GameObject attacker, float maxDeltaTime)
        {
            if (damageHistories.TryGetValue(attacker, out var tuple))
                return (Time.time - tuple.Item4) < maxDeltaTime;
            else
                return false;
        }

        public bool CheckDamageBlockedHistory(GameObject attacker, float maxDeltaTime)
        {
            if (damageHistories.TryGetValue(attacker, out var tuple))
                return tuple.Item2 && (Time.time - tuple.Item4) < maxDeltaTime;
            else
                return false;
        }

        public struct DamageContext
        {
            public float timeStamp;
            public Vector3 hitPoint;
            public Collider hitCollider;
            public bool insufficientStamina;
            public MainTable.ActionData senderActionData;
            public MainTable.ActionData receiverActionData;
            public PawnBrainController senderBrain;
            public PawnBrainController receiverBrain;
            public ProjectileMovement projectile;
            public readonly FloatReactiveProperty SenderCurrHeartPoint => senderBrain.PawnHP.heartPoint;
            public readonly FloatReactiveProperty ReceiverCurrHeartPoint => receiverBrain.PawnHP.heartPoint;
            public ActionResults actionResult;
            public Tuple<PawnStatus, float> senderPenalty;
            public Tuple<PawnStatus, float> receiverPenalty;
            public float finalDamage;

            public DamageContext(PawnBrainController senderBrain, PawnBrainController receiverBrain, MainTable.ActionData senderActionData, Vector3 hitPoint, Collider hitCollider, bool insufficientStamina)
            {
                timeStamp = Time.time;
                this.hitPoint = hitPoint;
                this.hitCollider = hitCollider;
                this.insufficientStamina = insufficientStamina;
                this.senderActionData = senderActionData;
                receiverActionData = null;
                this.senderBrain = senderBrain;
                this.receiverBrain = receiverBrain;
                projectile = null;
                actionResult = ActionResults.None;
                senderPenalty = new(PawnStatus.None, -1);
                receiverPenalty = new(PawnStatus.None, -1);
                finalDamage = -1;
            }

            public DamageContext(PawnBrainController senderBrain, PawnBrainController receiverBrain, MainTable.ActionData actionData, Collider hitCollider, bool insufficientStamina)
            {
                timeStamp = Time.time;
                hitPoint = receiverBrain.coreColliderHelper.pawnCollider.bounds.center;
                this.hitCollider = hitCollider;
                this.insufficientStamina = insufficientStamina;
                this.senderActionData = actionData;
                receiverActionData = null;
                this.senderBrain = senderBrain;
                this.receiverBrain = receiverBrain;
                projectile = null;
                actionResult = ActionResults.None;
                senderPenalty = new(PawnStatus.None, -1);
                receiverPenalty = new(PawnStatus.None, -1);
                finalDamage = -1;
            }

            public DamageContext(PawnBrainController senderBrain, string actionName, float finalDamage, PawnStatus penalty = PawnStatus.None, float penaltyDuration = -1)
            {
                timeStamp = Time.time;
                hitPoint = senderBrain.coreColliderHelper.pawnCollider.bounds.center;
                hitCollider = null;
                insufficientStamina = false;
                senderActionData = null;
                receiverActionData = null;
                this.senderBrain = senderBrain;
                this.receiverBrain = null;
                projectile = null;
                actionResult = ActionResults.None;
                senderPenalty = new(PawnStatus.None, -1);
                receiverPenalty = new(PawnStatus.None, -1);
                this.finalDamage = finalDamage;
            }

            public DamageContext(ProjectileMovement projectile, PawnBrainController senderBrain, PawnBrainController receiverBrain, MainTable.ActionData senderActionData, Collider hitCollider, bool insufficientStamina)
            {
                timeStamp = Time.time;
                hitPoint = projectile.transform.position;
                this.hitCollider = hitCollider;
                this.insufficientStamina = insufficientStamina;
                this.senderActionData = senderActionData;
                receiverActionData = null;
                this.senderBrain = senderBrain;
                this.receiverBrain = receiverBrain.GetComponent<PawnBrainController>();
                this.projectile = projectile;
                actionResult = ActionResults.None;
                senderPenalty = new(PawnStatus.None, -1);
                receiverPenalty = new(PawnStatus.None, -1);
                finalDamage = -1;
            }
        }

        public Action<DamageContext> onDamaged;
        public Action<DamageContext> onDead;

        public void Die(string reasonName)
        {
            heartPoint.Value = 0;
            onDead?.Invoke(new DamageContext(PawnBrain, reasonName, heartPoint.Value));
            PawnBrain.PawnBB.common.isDead.Value = true;
        }

        public void Send(DamageContext damageContext)
        {
            ProcessDamageContext(ref damageContext);
        }

        void ProcessDamageContext(ref DamageContext damageContext)
        {
            if (!damageContext.receiverBrain.PawnBB.IsSpawnFinished || damageContext.receiverBrain.PawnBB.IsDead || damageContext.receiverBrain.PawnBB.IsDown)
                return;

            var cannotHitOnJump = (damageContext.senderActionData?.cannotHitOnJump ?? 1) > 0;
            if (cannotHitOnJump && damageContext.receiverBrain is IMovable receiverMovable && receiverMovable.IsJumping())
            {
                __Logger.LogF(gameObject, nameof(CalcFinalDamage), "No damage. IsJumping() is true.", "sender", damageContext.senderBrain, "receiverBrain", damageContext.receiverBrain);
                return;
            }

            var cannotAvoidOnRolling = (damageContext.senderActionData?.cannotAvoidOnRolling ?? 0) > 0;
            if (!cannotAvoidOnRolling && damageContext.receiverBrain.TryGetComponent<PawnStatusController>(out var receiverBuffCtrler) && receiverBuffCtrler.CheckStatus(PawnStatus.InvincibleDodge))
            {
                __Logger.LogF(gameObject, nameof(ProcessDamageContext), "No damage. CheckBuff(BuffTypes.InvincibleDodge) is true.",  "sender", damageContext.senderBrain, "receiver", damageContext.receiverBrain);
                return;
            }

            var senderActionCtrler = damageContext.senderBrain.GetComponent<PawnActionController>();
            var receiverActionCtrler = damageContext.receiverBrain.GetComponent<PawnActionController>();
            var canNotGuard = (damageContext.senderActionData?.cannotGuard ?? 0) > 0;
            var canNotParry = (damageContext.senderActionData?.cannotParry ?? 0) > 0;
            if (receiverActionCtrler != null)
            {
                if (!canNotParry && receiverActionCtrler.CanParryAction(ref damageContext))
                {
                    if (damageContext.hitCollider == receiverActionCtrler.parryHitColliderHelper.pawnCollider)
                    {
                        damageContext.actionResult = ActionResults.ActiveParried;
                        damageContext.receiverActionData = DatasheetManager.Instance.GetActionData(damageContext.receiverBrain.PawnBB.common.pawnId, "ActiveParry");
                    }
                    else
                    {
                        damageContext.actionResult = ActionResults.PassiveParried;
                        damageContext.receiverActionData = DatasheetManager.Instance.GetActionData(damageContext.receiverBrain.PawnBB.common.pawnId, "PassiveParry");
                    }
                }
                else if (!canNotGuard && receiverActionCtrler.CanBlockAction(ref damageContext))
                {
                    damageContext.actionResult = ActionResults.Blocked;
                    damageContext.receiverActionData = DatasheetManager.Instance.GetActionData(damageContext.receiverBrain.PawnBB.common.pawnId, "Block");
                }
            }

            if (damageContext.actionResult == ActionResults.ActiveParried)
            {
                //* Sender의 현재 액션이 'ActiveParry'인지 검증함
                Debug.Assert(receiverActionCtrler.currActionContext.actionData != null && receiverActionCtrler.currActionContext.actionData == damageContext.receiverActionData);

                damageContext.senderBrain.PawnBB.stat.stance.Value += damageContext.receiverActionData.groggyAccum;

                __Logger.LogF(gameObject, nameof(ProcessDamageContext), "ActionResults.ActiveParried", "stance", damageContext.senderBrain.PawnBB.stat.stance.Value, "maxStance", damageContext.senderBrain.PawnBB.stat.maxStance.Value, "sender", damageContext.senderBrain, "receiver", damageContext.receiverBrain);

                if (damageContext.senderBrain.PawnBB.stat.stance.Value >= damageContext.senderBrain.PawnBB.stat.maxStance.Value)
                {
                    damageContext.senderBrain.PawnBB.stat.stance.Value = 0;
                    damageContext.senderBrain.PawnBB.stat.knockDown.Value = 0;
                    damageContext.senderPenalty = new(PawnStatus.Groggy, damageContext.senderBrain.PawnBB.pawnData.groggyDuration);
                }
                else
                {
                    damageContext.senderPenalty = new(PawnStatus.Staggered, damageContext.receiverActionData.staggerDuration);
                }
            }
            else if (damageContext.actionResult == ActionResults.PassiveParried)
            {
                //* Receiver가 'PassiveParried' ActionData가 있는지 검증
                Debug.Assert(damageContext.receiverActionData != null);

                damageContext.senderBrain.PawnBB.stat.stance.Value += damageContext.receiverActionData.groggyAccum;

                __Logger.LogF(gameObject, nameof(ProcessDamageContext), "ActionResults.PassiveParried", "stance", damageContext.senderBrain.PawnBB.stat.stance.Value, "maxStance", damageContext.senderBrain.PawnBB.stat.maxStance.Value, "sender", damageContext.senderBrain, "receiver", damageContext.receiverBrain);

                if (damageContext.senderBrain.PawnBB.stat.stance.Value >= damageContext.senderBrain.PawnBB.stat.maxStance.Value)
                {
                    damageContext.senderBrain.PawnBB.stat.stance.Value = 0;
                    damageContext.senderBrain.PawnBB.stat.knockDown.Value = 0;
                    damageContext.senderPenalty = new(PawnStatus.Groggy, damageContext.senderBrain.PawnBB.pawnData.groggyDuration);
                }
                else
                {
                    damageContext.senderPenalty = new(PawnStatus.Staggered, damageContext.receiverActionData.staggerDuration);
                }
            }
            else if (damageContext.actionResult == ActionResults.Blocked)
            {
                __Logger.LogF(gameObject, nameof(ProcessDamageContext), "ActionResults.Blocked", "sender", damageContext.senderBrain, "receiver", damageContext.receiverBrain);

                var staminaCost = (damageContext.receiverBrain.PawnBB.stat.guardStaminaCost * damageContext.senderActionData.guardStaminaCostMultiplier + damageContext.senderActionData.guardStaminaDamage) * Mathf.Clamp01(1f - damageContext.receiverBrain.PawnBB.stat.guardEfficiency);
                damageContext.receiverBrain.PawnBB.stat.ReduceStamina(staminaCost);

                if (damageContext.receiverBrain.PawnBB.stat.guardStrength <= damageContext.senderActionData.guardBreak)
                {
                    __Logger.LogF(gameObject, nameof(ProcessDamageContext), "ActionResults.GuardBreak", "sender", damageContext.senderBrain, "receiver", damageContext.receiverBrain);
                    damageContext.actionResult = ActionResults.GuardBreak;

                    //* 'BreakGuard'인 경우 'Staggered' 디버프를 받게 되며, 'Staggered' 지속 시간은 피격 경직 시간과 동일하게 적용함
                    damageContext.receiverPenalty = new(PawnStatus.Staggered, damageContext.senderActionData.staggerDuration);
                }
                else
                {
                    if (damageContext.projectile == null)
                    {
                        //* Receiver가 'Block' ActionData가 있는지 검증
                        Debug.Assert(damageContext.receiverActionData != null);

                        //* 'Block' 판정인 경우엔 Sender는 역경직을 받게 되고, Receiver도 짧은 자체 경직 시간을 갖게됨
                        if (senderActionCtrler == null || !senderActionCtrler.IsSuperArmorEnabled )
                            damageContext.senderPenalty = new(PawnStatus.Staggered, damageContext.receiverActionData.staggerDuration);
                    }
                }
            }
            else if (damageContext.finalDamage > 0 || CalcFinalDamage(ref damageContext) > 0)
            {
                __Logger.LogF(gameObject, nameof(ProcessDamageContext), "ActionResults.Damaged", "finalDamage", damageContext.finalDamage, "sender", damageContext.senderBrain, "receiver", damageContext.receiverBrain);

                damageContext.actionResult = ActionResults.Damaged;

                if (!damageContext.receiverBrain.PawnBB.IsInvincible)
                    damageContext.receiverBrain.PawnHP.heartPoint.Value = Mathf.Max(0, damageContext.receiverBrain.PawnHP.heartPoint.Value - damageContext.finalDamage);

                if (damageContext.receiverBrain.PawnHP.heartPoint.Value > 0)
                {
                    if (damageContext.senderActionData.groggy >= damageContext.receiverBrain.PawnBB.stat.poise && !damageContext.receiverBrain.PawnBB.IsGroggy) //* Groggy 처리
                    {
                        damageContext.receiverBrain.PawnBB.stat.stance.Value += damageContext.senderActionData.groggyAccum;

                        __Logger.LogF(gameObject, nameof(ProcessDamageContext), "ActionResults.Damaged", "stat.stance", damageContext.senderBrain.PawnBB.stat.stance.Value, "stat.maxStance", damageContext.senderBrain.PawnBB.stat.maxStance.Value, "sender", damageContext.senderBrain, "receiver", damageContext.receiverBrain);

                        if (damageContext.receiverBrain.PawnBB.stat.stance.Value >= damageContext.receiverBrain.PawnBB.stat.maxStance.Value)
                        {
                            damageContext.receiverBrain.PawnBB.stat.stance.Value = 0;
                            damageContext.receiverBrain.PawnBB.stat.knockDown.Value = 0;
                            damageContext.receiverPenalty = new(PawnStatus.Groggy, damageContext.receiverBrain.PawnBB.pawnData.groggyDuration);
                        }
                    }

                    //* KnockDown 처리
                    if (damageContext.receiverBrain.PawnBB.IsGroggy)
                    {
                        damageContext.receiverBrain.PawnBB.stat.groggyHitCount.Value += damageContext.senderActionData.groggyHit;
                        Debug.Assert(damageContext.receiverPenalty.Item1 == PawnStatus.None);

                        __Logger.LogF(gameObject, nameof(ProcessDamageContext), "ActionResults.Damaged", "stat.groggyHitCount", damageContext.receiverBrain.PawnBB.stat.groggyHitCount.Value, "stat.maxGroggyHitCount", damageContext.receiverBrain.PawnBB.stat.maxGroggyHitCount.Value, "sender", damageContext.senderBrain, "receiver", damageContext.receiverBrain);

                        if (damageContext.receiverPenalty.Item1 == PawnStatus.None && damageContext.receiverBrain.PawnBB.stat.groggyHitCount.Value >= damageContext.receiverBrain.PawnBB.stat.maxGroggyHitCount.Value)
                        {
                            damageContext.receiverBrain.PawnBB.stat.groggyHitCount.Value = 0;
                            damageContext.receiverPenalty = new(PawnStatus.KnockDown, damageContext.receiverBrain.PawnBB.pawnData.knockDownDuration);
                        }
                    }
                    else if (damageContext.senderActionData.knockDown >= damageContext.receiverBrain.PawnBB.stat.poise && !damageContext.receiverBrain.PawnBB.IsDown)
                    {
                        damageContext.receiverBrain.PawnBB.stat.knockDown.Value += damageContext.senderActionData.knockDownAccum;

                        __Logger.LogF(gameObject, nameof(ProcessDamageContext), "ActionResults.Damaged", "stat.knockDown", damageContext.receiverBrain.PawnBB.stat.knockDown.Value, "stat.maxKnockDown", damageContext.receiverBrain.PawnBB.stat.maxKnockDown.Value, "sender", damageContext.senderBrain, "receiver", damageContext.receiverBrain);

                        if (damageContext.receiverPenalty.Item1 == PawnStatus.None && damageContext.receiverBrain.PawnBB.stat.knockDown.Value >= damageContext.receiverBrain.PawnBB.stat.maxKnockDown.Value)
                        {
                            damageContext.receiverBrain.PawnBB.stat.knockDown.Value = 0;
                            damageContext.receiverPenalty = new(PawnStatus.KnockDown, damageContext.receiverBrain.PawnBB.pawnData.knockDownDuration);
                        }
                    }

                    if (damageContext.receiverPenalty.Item1 == PawnStatus.None && damageContext.receiverBrain.PawnBB.stat.poise - damageContext.senderActionData.stagger <= 0) //* 경직 처리
                    {
                        if (receiverActionCtrler.IsSuperArmorEnabled)
                        {
                            __Logger.LogF(gameObject, nameof(ProcessDamageContext), "receiverActionCtrler.IsSuperArmorEnabled is true. BuffTypes.Staggered is ignored.");
                        }
                        else
                        {
                            damageContext.receiverPenalty = new(PawnStatus.Staggered, damageContext.senderActionData.staggerDuration);
                        }
                    }
                }
            }

            if (damageContext.receiverBrain.PawnHP.heartPoint.Value <= 0)
            {
                __Logger.LogF(gameObject, nameof(ProcessDamageContext), "receiverBrain.PawnHP.heartPoint is below 0. receiverBrain is now dead.");
                damageContext.receiverBrain.PawnBB.common.isDead.Value = true;
                damageContext.receiverBrain.PawnHP.onDamaged?.Invoke(damageContext);
                damageContext.receiverBrain.PawnHP.onDead?.Invoke(damageContext);
            }
            else
            {
                if (damageContext.receiverPenalty.Item1 != PawnStatus.None)
                {
                    __Logger.LogF(gameObject, nameof(ProcessDamageContext), "receiverPenalty is added.", "BuffTypes", damageContext.receiverPenalty.Item1, "duration", damageContext.receiverPenalty.Item2, "receiverBrain", damageContext.receiverBrain);
                    damageContext.receiverBrain.PawnStatusCtrler.AddStatus(damageContext.receiverPenalty.Item1, 1f, damageContext.receiverPenalty.Item2);

                    //* Groggy 상태에서 KnockDown이 발생하면 Groggy는 종료시킴
                    if (damageContext.receiverPenalty.Item1 == PawnStatus.KnockDown && damageContext.receiverBrain.PawnStatusCtrler.CheckStatus(PawnStatus.Groggy))
                        damageContext.receiverBrain.PawnStatusCtrler.RemoveStatus(PawnStatus.Groggy);
                }
                damageContext.receiverBrain.PawnHP.onDamaged?.Invoke(damageContext);
            }

            if (damageContext.senderBrain != damageContext.receiverBrain)
            {
                if (damageContext.senderPenalty.Item1 != PawnStatus.None)
                {
                    __Logger.LogF(gameObject, nameof(ProcessDamageContext), "senderPenalty is added.", "BuffTypes", damageContext.senderPenalty.Item1, "duration", damageContext.senderPenalty.Item2, "senderBrain", damageContext.senderBrain);
                    damageContext.senderBrain.PawnStatusCtrler.AddStatus(damageContext.senderPenalty.Item1, 1f, damageContext.senderPenalty.Item2);
                }
                damageContext.senderBrain.PawnHP.onDamaged?.Invoke(damageContext);
            }

            // 현재 Block 시간 기록
            damageContext.receiverBrain.PawnHP.LastDamageTimeStamp = Time.time;

            //* 히트 시엔 impulseRootMotion을 꺼줌
            if (senderActionCtrler != null)
            {
                senderActionCtrler.currActionContext.impulseRootMotionDisposable?.Dispose();
                senderActionCtrler.currActionContext.impulseRootMotionDisposable = null;
            }
        }

        float CalcFinalDamage(ref DamageContext damageContext)
        {
            var physDamage = Mathf.Max(0f, damageContext.senderBrain.PawnBB.stat.physAttack - damageContext.receiverBrain.PawnBB.stat.physDefence);
            var magicDamage = Mathf.Max(0f, damageContext.senderBrain.PawnBB.stat.magicAttack - damageContext.receiverBrain.PawnBB.stat.magicDefense);
            damageContext.finalDamage = Mathf.Max(10f, (physDamage + magicDamage) * (damageContext.senderActionData?.damageMultiplier ?? 1));

            if (damageContext.receiverBrain.TryGetComponent<PawnStatusController>(out var receiverBuffCtrler) && receiverBuffCtrler.CheckStatus(PawnStatus.Invincible))
            {
                __Logger.LogF(gameObject, nameof(CalcFinalDamage), "No damage. CheckBuff(BuffTypes.Invincible) is true.", "receiverBrain", damageContext.receiverBrain);
                return 0f;
            }
            
            if (damageContext.insufficientStamina)
            {
                damageContext.finalDamage *= 0.1f;
                __Logger.LogF(gameObject, nameof(CalcFinalDamage), "damageContext.finalDamage is reduced. (insufficientStamina is true)", "finalDamage", damageContext.finalDamage);
            }
            else
            {
                __Logger.LogF(gameObject, nameof(CalcFinalDamage), "damageContext.finalDamage calculation done.", "finalDamage", damageContext.finalDamage);
            }

            return damageContext.finalDamage;
        }
    }
}
