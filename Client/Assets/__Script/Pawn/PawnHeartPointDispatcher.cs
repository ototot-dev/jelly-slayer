using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using Unity.VisualScripting;
using UnityEditor.Timeline.Actions;
using UnityEngine;

namespace Game
{
    public class PawnHeartPointDispatcher : MonoBehaviour
    {
        public FloatReactiveProperty heartPoint = new();
        public PawnBrainController PawnBrain { get; private set; }
        public float LastDamageTimeStamp { get; private set; }
        public float LastParriedTimeStamp { get; private set; }

        void Awake()
        {
            PawnBrain = GetComponent<PawnBrainController>();
        }

        void Start()
        {
            //* Groggy 종료 후 groggyHitCount 초기화
            PawnBrain.PawnBB.common.isGroggy.Skip(1).Where(v => !v).Subscribe(_ =>
            {
                PawnBrain.PawnBB.stat.groggyHitCount.Value = 0;
            }).AddTo(this);
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
            public ProjectileMovement projectile;
            public string senderActionSpecialTag;
            public string receiverActionSpecialTag;
            public MainTable.ActionData senderActionData;
            public MainTable.ActionData receiverActionData;
            public PawnBrainController senderBrain;
            public PawnBrainController receiverBrain;
            public readonly FloatReactiveProperty SenderCurrHeartPoint => senderBrain.PawnHP.heartPoint;
            public readonly FloatReactiveProperty ReceiverCurrHeartPoint => receiverBrain.PawnHP.heartPoint;
            public Tuple<PawnStatus, float> senderPenalty;
            public Tuple<PawnStatus, float> receiverPenalty;
            public ActionResults actionResult;
            public float finalDamage;
            public bool groggyBreakHit;

#if UNITY_EDITOR
            public DamageContext(Collider hitCollider)
            {
                timeStamp = Time.time;
                hitPoint = hitCollider != null ? hitCollider.transform.position : Vector3.zero;
                this.hitCollider = hitCollider;
                insufficientStamina = false;
                projectile = null;
                senderActionSpecialTag = string.Empty;
                receiverActionSpecialTag = string.Empty;
                senderActionData = null;
                receiverActionData = null;
                senderBrain = null;
                receiverBrain = null;
                senderPenalty = new(PawnStatus.None, -1);
                receiverPenalty = new(PawnStatus.None, -1);
                actionResult = ActionResults.None;
                groggyBreakHit = false;
                finalDamage = -1;
            }
#endif

            public DamageContext(PawnBrainController senderBrain, PawnBrainController receiverBrain, MainTable.ActionData actionData, string specialTag, Collider hitCollider, bool insufficientStamina)
            {
                timeStamp = Time.time;
                hitPoint = hitCollider.TryGetComponent<PawnColliderHelper>(out var hitCollierHelper) ? hitCollierHelper.GetHitPoint(senderBrain.GetWorldPosition()) : receiverBrain.bodyHitColliderHelper.GetHitPoint(senderBrain.GetWorldPosition());
                this.hitCollider = hitCollider;
                this.insufficientStamina = insufficientStamina;
                projectile = null;
                senderActionSpecialTag = specialTag;
                receiverActionSpecialTag = string.Empty;
                senderActionData = actionData;
                receiverActionData = null;
                this.senderBrain = senderBrain;
                this.receiverBrain = receiverBrain;
                senderPenalty = new(PawnStatus.None, -1);
                receiverPenalty = new(PawnStatus.None, -1);
                actionResult = ActionResults.None;
                groggyBreakHit = false;
                finalDamage = -1;
            }

            public DamageContext(PawnBrainController senderBrain, string specialTag, float finalDamage, PawnStatus penalty = PawnStatus.None, float penaltyDuration = -1)
            {
                timeStamp = Time.time;
                hitPoint = senderBrain.bodyHitColliderHelper.GetWorldCenter();
                hitCollider = null;
                insufficientStamina = false;
                projectile = null;
                senderActionSpecialTag = specialTag;
                receiverActionSpecialTag = string.Empty;
                senderActionData = null;
                receiverActionData = null;
                this.senderBrain = senderBrain;
                receiverBrain = null;
                senderPenalty = new(PawnStatus.None, -1);
                receiverPenalty = new(PawnStatus.None, -1);
                actionResult = ActionResults.None;
                groggyBreakHit = false;
                this.finalDamage = finalDamage;
            }

            public DamageContext(ProjectileMovement projectile, PawnBrainController senderBrain, PawnBrainController receiverBrain, MainTable.ActionData actionData, string specialTag, Collider hitCollider, bool insufficientStamina)
            {
                timeStamp = Time.time;
                hitPoint = projectile.transform.position;
                this.hitCollider = hitCollider;
                this.insufficientStamina = insufficientStamina;
                this.projectile = projectile;
                senderActionSpecialTag = specialTag;
                receiverActionSpecialTag = string.Empty;
                this.senderActionData = actionData;
                receiverActionData = null;
                this.senderBrain = senderBrain;
                this.receiverBrain = receiverBrain;
                senderPenalty = new(PawnStatus.None, -1);
                receiverPenalty = new(PawnStatus.None, -1);
                actionResult = ActionResults.None;
                groggyBreakHit = false;
                finalDamage = -1;
            }
        }

        public Action<DamageContext, string> onAvoided;
        public Action<DamageContext> onDamaged;
        public Action<DamageContext> onDead;

        public void Die(string causeOfDeath)
        {
            heartPoint.Value = 0;
            onDead?.Invoke(new DamageContext(PawnBrain, causeOfDeath, heartPoint.Value));
            PawnBrain.PawnBB.common.isDead.Value = true;
        }

        public void Send(DamageContext damageContext) { ProcessDamageContext(ref damageContext); }
        public void Send(ref DamageContext damageContext) { ProcessDamageContext(ref damageContext); }

        void ProcessDamageContext(ref DamageContext damageContext)
        {
            if (!damageContext.receiverBrain.PawnBB.IsSpawnFinished || damageContext.receiverBrain.PawnBB.IsDead || damageContext.receiverBrain.PawnBB.IsDown)
                return;

            var cannotHitOnJump = (damageContext.senderActionData?.cannotHitOnJump ?? 0) > 0;
            if (!cannotHitOnJump && damageContext.receiverBrain is IPawnMovable receiverMovable && receiverMovable.IsJumping())
            {
                damageContext.receiverBrain.PawnHP.onAvoided.Invoke(damageContext, "Jump");
                __Logger.LogR2(gameObject, nameof(DecideActionResult), "No ActionResult => receiverMovable.IsJumping()", "senderBrain", damageContext.senderBrain, "receiverBrain", damageContext.receiverBrain);
                return;
            }

            var cannotHitOnRolling = (damageContext.senderActionData?.cannotHitOnRolling ?? 0) > 0;
            if (!cannotHitOnRolling && damageContext.receiverBrain.TryGetComponent<PawnStatusController>(out var receiverBuffCtrler) && receiverBuffCtrler.CheckStatus(PawnStatus.InvincibleDodge))
            {
                damageContext.receiverBrain.PawnHP.onAvoided.Invoke(damageContext, "Dodge");
                __Logger.LogR2(gameObject, nameof(DecideActionResult), "No ActionResult => InvincibleDodge.", "senderBrain", damageContext.senderBrain, "receiver", damageContext.receiverBrain);
                return;
            }

            DecideActionResult(ref damageContext);

            if (damageContext.actionResult == ActionResults.Blocked) ProcessActionBlocked(ref damageContext);
            else if (damageContext.actionResult == ActionResults.PunchParried) ProcessActionKickParried(ref damageContext);
            else if (damageContext.actionResult == ActionResults.GuardParried) ProcessActionGuardParried(ref damageContext);
            else  if (damageContext.finalDamage > 0 || CalcFinalDamage(ref damageContext) > 0)
            {
                damageContext.actionResult = ActionResults.Damaged;
                __Logger.LogR1(gameObject, nameof(ProcessDamageContext), "actionResult", damageContext.actionResult, "senderBrain", damageContext.senderBrain, "receiverBrain", damageContext.receiverBrain);
                
                ProcessActionDamaged(ref damageContext);
            }
            else
            {
                //* 데미지 없는 공격은 Missed 처리한다.
                damageContext.actionResult = ActionResults.Missed;
                __Logger.LogR1(gameObject, nameof(ProcessDamageContext), "actionResult", damageContext.actionResult, "senderBrain", damageContext.senderBrain, "receiverBrain", damageContext.receiverBrain);

                damageContext.finalDamage = 0f;
                damageContext.receiverPenalty = new(PawnStatus.None, 0f);
            }

            if (damageContext.receiverBrain.PawnHP.heartPoint.Value <= 0 && !damageContext.receiverBrain.PawnBB.IsDead)
            {
                __Logger.LogR2(gameObject, nameof(ProcessDamageContext), "Receiver is dead", "receiverBrain", damageContext.receiverBrain);

                //* onDamaged 호출은 isDead가 true롤 셋팅되지 전에 불려야함!!
                damageContext.receiverBrain.PawnHP.onDamaged?.Invoke(damageContext);
                damageContext.receiverBrain.PawnBB.common.isDead.Value = true;
                damageContext.receiverBrain.PawnHP.onDead?.Invoke(damageContext);
            }
            else
            {
                if (damageContext.receiverPenalty.Item1 != PawnStatus.None)
                {
                    //* 'Block' 판정인 경우엔 strength 값에 0을 대입하여 구분이 될 수 있도록 함
                    damageContext.receiverBrain.PawnStatusCtrler.AddStatus(damageContext.receiverPenalty.Item1, damageContext.actionResult == ActionResults.Blocked ? 0f : 1f, damageContext.receiverPenalty.Item2);
                }

                damageContext.receiverBrain.PawnHP.onDamaged?.Invoke(damageContext);

                //* groggyHitCount에 의한 Groggy 종료 처리
                if (damageContext.groggyBreakHit)
                {
                    Debug.Assert(damageContext.receiverBrain.PawnStatusCtrler.CheckStatus(PawnStatus.Groggy));
                    damageContext.receiverBrain.PawnStatusCtrler.RemoveStatus(PawnStatus.Groggy);
                }
            }

            if (damageContext.senderBrain != damageContext.receiverBrain)
            {
                if (damageContext.senderPenalty.Item1 != PawnStatus.None)
                    damageContext.senderBrain.PawnStatusCtrler.AddStatus(damageContext.senderPenalty.Item1, 1f, damageContext.senderPenalty.Item2);

                if (damageContext.actionResult == ActionResults.GuardParried && damageContext.senderBrain.TryGetComponent<PawnHeartPointDispatcher>(out var senderPawnHp))
                    senderPawnHp.LastParriedTimeStamp = Time.time;

                damageContext.senderBrain.PawnHP.onDamaged?.Invoke(damageContext);
            }

            if (!string.IsNullOrEmpty(damageContext.senderActionSpecialTag))
                PawnEventManager.Instance.SendPawnDamageEvent(damageContext.senderBrain, damageContext);
            if (!string.IsNullOrEmpty(damageContext.receiverActionSpecialTag))
                PawnEventManager.Instance.SendPawnDamageEvent(damageContext.receiverBrain, damageContext);

            damageContext.receiverBrain.PawnHP.LastDamageTimeStamp = Time.time;
        }

        void DecideActionResult(ref DamageContext damageContext)
        {
            var reflectiveDamage = damageContext.projectile != null && damageContext.projectile.reflectiveBrain.Value != null;
            if (reflectiveDamage)
                Debug.Assert(damageContext.projectile.reflectiveBrain.Value == damageContext.senderBrain);

            var senderActionCtrler = damageContext.senderBrain.GetComponent<PawnActionController>();
            var receiverActionCtrler = damageContext.receiverBrain.GetComponent<PawnActionController>();
            var cannotGuard = (damageContext.senderActionData?.cannotGuard ?? 0) > 0 || reflectiveDamage;
            var cannotParry = (damageContext.senderActionData?.cannotParry ?? 0) > 0 || reflectiveDamage;
            if (receiverActionCtrler != null)
            {
                if (!cannotParry && receiverActionCtrler.CanParryAction(ref damageContext))
                {
                    if (damageContext.hitCollider == damageContext.receiverBrain.parryColliderHelper.pawnCollider)
                    {
                        damageContext.actionResult = ActionResults.PunchParried;
                        damageContext.receiverActionData = DatasheetManager.Instance.GetActionData(damageContext.receiverBrain.PawnBB.common.pawnId, "Kick");
                    }
                    else
                    {
                        damageContext.actionResult = ActionResults.GuardParried;
                        damageContext.receiverActionData = DatasheetManager.Instance.GetActionData(damageContext.receiverBrain.PawnBB.common.pawnId, "GuardParry");
                    }
                }
                else if (!cannotGuard && receiverActionCtrler.CanBlockAction(ref damageContext))
                {
                    damageContext.actionResult = ActionResults.Blocked;
                    damageContext.receiverActionData = DatasheetManager.Instance.GetActionData(damageContext.receiverBrain.PawnBB.common.pawnId, "Blocking");
                }
            }

            if (damageContext.actionResult != ActionResults.None)
                __Logger.LogR1(gameObject, nameof(DecideActionResult), "actionResult", damageContext.actionResult, "senderBrain", damageContext.senderBrain, "receiverBrain", damageContext.receiverBrain);
        }

        void ProcessActionBlocked(ref DamageContext damageContext)
        {
            __Logger.LogR2(gameObject, nameof(ProcessActionBlocked), "ActionResults => Blocked", "senderBrain", damageContext.senderBrain, "receiverBrain", damageContext.receiverBrain);

            if (damageContext.insufficientStamina)
            {
                damageContext.receiverPenalty = new(PawnStatus.None, 0f);
            }
            else
            {
                var staminaCost = (damageContext.receiverBrain.PawnBB.stat.guardStaminaCost * damageContext.senderActionData.guardStaminaCostMultiplier + damageContext.senderActionData.guardStaminaDamage) * Mathf.Clamp01(1f - damageContext.receiverBrain.PawnBB.stat.guardEfficiency);
                damageContext.receiverBrain.PawnBB.stat.ReduceStamina(staminaCost);

                if (damageContext.receiverBrain.PawnBB.stat.guardStrength <= damageContext.senderActionData.guardBreak)
                {
                    __Logger.LogR2(gameObject, nameof(ProcessActionBlocked), "ActionResults => GuardBreak", "senderBrain", damageContext.senderBrain, "receiverBrain", damageContext.receiverBrain);
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

                        if (damageContext.senderBrain.TryGetComponent<PawnActionController>(out var senderActionCtrler) && senderActionCtrler.CheckSuperArmorLevel(SuperArmorLevels.CanNotStraggerOnBlacked))
                        {
                            __Logger.LogR2(gameObject, nameof(ProcessActionBlocked), "Sender has SuperArmorLevels.CanNotStraggerOnBlacked", "senderBrain", damageContext.senderBrain);
                        }
                        else
                        {
                            //* 'Block' 판정인 경우엔 Sender에게 경직 발생
                            damageContext.senderPenalty = new(PawnStatus.Staggered, damageContext.receiverActionData.staggerDuration);
                            __Logger.LogR2(gameObject, nameof(ProcessActionBlocked), "Sender ActionPenalty => Staggered", "staggerDuration", damageContext.receiverActionData.staggerDuration, "senderBrain", damageContext.senderBrain);
                        }
                    }

                    //* Receiver도 'Block' 반동에 의한 약한 경직 발생함
                    if (damageContext.receiverBrain.PawnBB.pawnData.guardStaggerDuration > 0f)
                    {
                        damageContext.receiverPenalty = new(PawnStatus.Staggered, damageContext.receiverBrain.PawnBB.pawnData.guardStaggerDuration);
                        __Logger.LogR2(gameObject, nameof(ProcessActionBlocked), "Receiver ActionPenalty => Staggered", "guardStaggerDuration", damageContext.receiverBrain.PawnBB.pawnData.guardStaggerDuration, "receiverBrain", damageContext.receiverBrain);
                    }
                }
            }
        }

        void ProcessActionKickParried(ref DamageContext damageContext)
        {
            var receiverActionCtrler = damageContext.receiverBrain.GetComponent<PawnActionController>();

            //* Sender의 현재 액션이 'ActiveParry'인지 검증함
            Debug.Assert(receiverActionCtrler.currActionContext.actionData != null && receiverActionCtrler.currActionContext.actionData == damageContext.receiverActionData);

            damageContext.senderBrain.PawnBB.stat.stance.Value += damageContext.receiverActionData.groggyAccum;

            __Logger.LogR2(gameObject, nameof(ProcessActionKickParried), "Sender Stance increased", "groggyAccum", damageContext.receiverActionData.groggyAccum, "stance", damageContext.senderBrain.PawnBB.stat.stance.Value, "maxStance", damageContext.senderBrain.PawnBB.stat.maxStance.Value, "senderBrain", damageContext.senderBrain, "receiverBrain", damageContext.receiverBrain);

            if (damageContext.senderBrain.PawnBB.stat.stance.Value >= damageContext.senderBrain.PawnBB.stat.maxStance.Value)
            {
                damageContext.senderBrain.PawnBB.stat.stance.Value = 0;
                damageContext.senderBrain.PawnBB.stat.knockDown.Value = 0;
                damageContext.senderPenalty = new(PawnStatus.Groggy, damageContext.senderBrain.PawnBB.pawnData.groggyDuration);

                __Logger.LogR2(gameObject, nameof(ProcessActionKickParried), "Sender ActionPenalty => Groggy", "groggyDuration", damageContext.senderBrain.PawnBB.pawnData.groggyDuration);
            }
            else
            {
                damageContext.senderPenalty = new(PawnStatus.Staggered, damageContext.receiverActionData.staggerDuration);
                __Logger.LogR2(gameObject, nameof(ProcessActionKickParried), "Sender ActionPenalty => Staggered", "staggerDuration", damageContext.receiverActionData.staggerDuration);
            }
        }

        void ProcessActionGuardParried(ref DamageContext damageContext)
        {
            //* Receiver가 'GuardParried' ActionData가 있는지 검증
            Debug.Assert(damageContext.receiverActionData != null);

            if (damageContext.projectile != null)
            {
                damageContext.actionResult = ActionResults.ProjectileReflected;
                damageContext.projectile.onReflected?.Invoke(damageContext.receiverBrain);
            }
            else
            {
                damageContext.senderBrain.PawnBB.stat.stance.Value += damageContext.receiverActionData.groggyAccum;

                __Logger.LogR2(gameObject, nameof(ProcessActionGuardParried), "Sender Stance increased", "groggyAccum", damageContext.receiverActionData.groggyAccum, "stance", damageContext.senderBrain.PawnBB.stat.stance.Value, "maxStance", damageContext.senderBrain.PawnBB.stat.maxStance.Value, "senderBrain", damageContext.senderBrain, "receiverBrain", damageContext.receiverBrain);

                if (damageContext.senderBrain.PawnBB.stat.stance.Value >= damageContext.senderBrain.PawnBB.stat.maxStance.Value)
                {
                    damageContext.senderBrain.PawnBB.stat.stance.Value = 0;
                    damageContext.senderBrain.PawnBB.stat.knockDown.Value = 0;
                    damageContext.senderPenalty = new(PawnStatus.Groggy, damageContext.senderBrain.PawnBB.pawnData.groggyDuration);

                    __Logger.LogR2(gameObject, nameof(ProcessDamageContext), "Sender ActionPenalty - Groggy", "groggyDuration", damageContext.senderBrain.PawnBB.pawnData.groggyDuration);
                }
                else
                {
                    damageContext.senderPenalty = new(PawnStatus.Staggered, damageContext.receiverActionData.staggerDuration);
                    __Logger.LogR2(gameObject, nameof(ProcessDamageContext), "Sender ActionPenalty - Staggered", "staggerDuration", damageContext.receiverActionData.staggerDuration);
                }
            }
        }

        void ProcessActionDamaged(ref DamageContext damageContext)
        {
            if (damageContext.receiverBrain.PawnBB.IsInvincible)
            {
                __Logger.LogR2(gameObject, nameof(ProcessActionDamaged), "Receiver is invincible (no damage)", "senderBrain", damageContext.senderBrain, "receiverBrain", damageContext.receiverBrain);
            }
            else
            {
                damageContext.receiverBrain.PawnHP.heartPoint.Value = Mathf.Max(0, damageContext.receiverBrain.PawnHP.heartPoint.Value - damageContext.finalDamage);
                __Logger.LogR1(gameObject, nameof(ProcessActionDamaged), "finalDamage", damageContext.finalDamage, "senderBrain", damageContext.senderBrain, "receiverBrain", damageContext.receiverBrain);
            }

            if (damageContext.receiverBrain.PawnHP.heartPoint.Value <= 0)
            {
                __Logger.LogR2(gameObject, nameof(ProcessActionDamaged), "Receiver HeartPoint becomes zero", "finalDamage", damageContext.finalDamage, "senderBrain", damageContext.senderBrain, "receiverBrain", damageContext.receiverBrain);
                return;
            }

            if (damageContext.insufficientStamina)
            {
                __Logger.LogR2(gameObject, nameof(ProcessActionDamaged), "Sender has insufficient stamina", "finalDamage", damageContext.finalDamage, "senderBrain", damageContext.senderBrain, "receiverBrain", damageContext.receiverBrain);
                return;
            }

            if (!damageContext.receiverBrain.PawnBB.IsGroggy)
            {
                if (damageContext.projectile != null && damageContext.projectile.reflectiveBrain.Value != null)
                {
                    //* 반사체인 경우엔 체간을 최대치로 누적시킴
                    damageContext.receiverBrain.PawnBB.stat.stance.Value += damageContext.receiverBrain.PawnBB.stat.maxStance.Value;
                    __Logger.LogR2(gameObject, nameof(ProcessActionDamaged), "Receiver Stance increased by Reflective Projectile", "senderBrain (reflectiveBrain)", damageContext.senderBrain, "receiverBrain", damageContext.receiverBrain);
                }
                else if (damageContext.senderActionData.groggy >= damageContext.receiverBrain.PawnBB.stat.poise)
                {
                    damageContext.receiverBrain.PawnBB.stat.stance.Value += damageContext.senderActionData.groggyAccum;
                    __Logger.LogR2(gameObject, nameof(ProcessActionDamaged), "Receiver Stance increased", "groggyAccum", damageContext.senderActionData.groggyAccum, "stance", damageContext.senderBrain.PawnBB.stat.stance.Value, "maxStance", damageContext.senderBrain.PawnBB.stat.maxStance.Value, "senderBrain", damageContext.senderBrain, "receiverBrain", damageContext.receiverBrain);
                }

                if (damageContext.receiverBrain.PawnBB.stat.stance.Value >= damageContext.receiverBrain.PawnBB.stat.maxStance.Value)
                {
                    damageContext.receiverBrain.PawnBB.stat.stance.Value = 0;
                    damageContext.receiverBrain.PawnBB.stat.knockDown.Value = 0;
                    damageContext.receiverPenalty = new(PawnStatus.Groggy, damageContext.receiverBrain.PawnBB.pawnData.groggyDuration);

                    __Logger.LogR2(gameObject, nameof(ProcessActionDamaged), "Receiver ActionPenalty => Groggy", "groggyDuration", damageContext.receiverBrain.PawnBB.pawnData.groggyDuration, "senderBrain", damageContext.senderBrain, "receiverBrain", damageContext.receiverBrain);
                }
                else if (damageContext.senderActionData.knockDown >= damageContext.receiverBrain.PawnBB.stat.poise && !damageContext.receiverBrain.PawnBB.IsDown)
                {
                    damageContext.receiverBrain.PawnBB.stat.knockDown.Value += damageContext.senderActionData.knockDownAccum;

                    if (damageContext.receiverPenalty.Item1 == PawnStatus.None && damageContext.receiverBrain.PawnBB.stat.knockDown.Value >= damageContext.receiverBrain.PawnBB.stat.maxKnockDown.Value)
                    {
                        damageContext.receiverBrain.PawnBB.stat.knockDown.Value = 0;
                        damageContext.receiverPenalty = new(PawnStatus.KnockDown, damageContext.receiverBrain.PawnBB.pawnData.knockDownDuration);

                        __Logger.LogR2(gameObject, nameof(ProcessDamageContext), "Receiver ActionPenalty => KnockDown", "knockDownDuration", damageContext.receiverBrain.PawnBB.pawnData.knockDownDuration, "senderBrain", damageContext.senderBrain, "receiverBrain", damageContext.receiverBrain);
                    }
                }

                //* Hero 전용 공중 KnockDown 처리
                if (damageContext.receiverPenalty.Item1 != PawnStatus.KnockBack && damageContext.receiverBrain.TryGetComponent<HeroBlackboard>(out var heroBB) && heroBB.IsHanging)
                {
                    if (heroBB.stat.ReduceStamina(damageContext.senderActionData.hangingStaminaDamage) <= 0 && damageContext.receiverPenalty.Item1 == PawnStatus.None)
                    {
                        damageContext.receiverBrain.PawnBB.stat.knockDown.Value = 0;
                        damageContext.receiverPenalty = new(PawnStatus.KnockDown, damageContext.receiverBrain.PawnBB.pawnData.knockDownDuration);

                        __Logger.LogR2(gameObject, nameof(ProcessDamageContext), "Receiver ActionPenalty => KnockDown (from Haning)", "knockDownDuration", damageContext.receiverBrain.PawnBB.pawnData.knockDownDuration, "senderActionData.haningStaminaDamage", damageContext.senderActionData.hangingStaminaDamage, "senderBrain", damageContext.senderBrain, "receiverBrain", damageContext.receiverBrain);
                    }
                }
            }
            else
            {
                damageContext.receiverBrain.PawnBB.stat.groggyHitCount.Value += damageContext.senderActionData.groggyHit;

                //* groggyHitCount에 의한 Groggy는 종료 체크
                if (damageContext.receiverBrain.PawnBB.stat.groggyHitCount.Value >= damageContext.receiverBrain.PawnBB.stat.maxGroggyHitCount.Value)
                {
                    Debug.Assert(damageContext.receiverBrain.PawnStatusCtrler.CheckStatus(PawnStatus.Groggy));
                    damageContext.receiverBrain.PawnBB.stat.groggyHitCount.Value = 0;
                    damageContext.groggyBreakHit = true;
                }
            }

            if (damageContext.receiverPenalty.Item1 == PawnStatus.None && damageContext.receiverBrain.PawnBB.stat.poise - damageContext.senderActionData.stagger <= 0) //* 경직 처리
            {
                if (damageContext.receiverBrain.TryGetComponent<PawnActionController>(out var receiverActionCtrler) && receiverActionCtrler.CheckSuperArmorLevel(SuperArmorLevels.CanNotStarggerOnDamaged))
                {
                    __Logger.LogR2(gameObject, nameof(ProcessDamageContext), "Receiver has SuperArmorLevels.CanNotStarggerOnDamaged");
                }
                else
                {
                    damageContext.receiverPenalty = new(PawnStatus.Staggered, damageContext.senderActionData.staggerDuration);
                    __Logger.LogR2(gameObject, nameof(ProcessDamageContext), "Receiver ActionPenalty => Staggered", "staggerDuration", damageContext.senderActionData.staggerDuration);
                }
            }
        }
 
        float CalcFinalDamage(ref DamageContext damageContext)
        {
            var physDamage = Mathf.Max(0f, damageContext.senderBrain.PawnBB.stat.physAttack - damageContext.receiverBrain.PawnBB.stat.physDefence);
            var magicDamage = Mathf.Max(0f, damageContext.senderBrain.PawnBB.stat.magicAttack - damageContext.receiverBrain.PawnBB.stat.magicDefense);
            var damageMultiplier = damageContext.receiverBrain.PawnBB.IsGroggy ? (damageContext.senderActionData?.damageMultiplierOnGroggy ?? 1f) : (damageContext.senderActionData?.damageMultiplier ?? 1f);
            damageContext.finalDamage = damageMultiplier > 0f ? Mathf.Max(1f, (physDamage + magicDamage) * damageMultiplier) : 0f;

            if (damageContext.receiverBrain.TryGetComponent<PawnStatusController>(out var receiverBuffCtrler) && receiverBuffCtrler.CheckStatus(PawnStatus.Invincible))
            {
                __Logger.LogR2(gameObject, nameof(CalcFinalDamage), "Receiver is invincible => No damage", "receiverBrain", damageContext.receiverBrain);
                return 0f;
            }

            if (damageContext.insufficientStamina)
            {
                damageContext.finalDamage = Mathf.Max(1f, damageContext.finalDamage * 0.1f);
                __Logger.LogR2(gameObject, nameof(CalcFinalDamage), "Sender has insufficient stamina => Low damage", "finalDamage", damageContext.finalDamage, "receiverBrain", damageContext.receiverBrain);
            }
            else
            {
                __Logger.LogR1(gameObject, nameof(CalcFinalDamage), "finalDamage", damageContext.finalDamage, "receiverBrain", damageContext.receiverBrain);
            }

            return damageContext.finalDamage;
        }
    }
}
