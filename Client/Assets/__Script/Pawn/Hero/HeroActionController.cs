using System;
using System.Linq;
using UniRx;
using UniRx.Triggers.Extension;
using UnityEngine;

namespace Game
{
    public class HeroActionController : PawnActionController
    {
        [Header("Component")]
        public Transform fxAttachPoint;
        public MeshRenderer forceFieldMeshRenderer;
        public Collider kickActionCollider;
        public Collider swordActionCollider;

        public override bool CanRootMotion(Vector3 rootMotionVec)
        {
            if (!base.CanRootMotion(rootMotionVec) || __brain.BB.IsRolling)
                return false;

            if (__brain.BB.TargetBrain != null && __brain.SensorCtrler.TouchingColliders.Contains(__brain.BB.TargetBrain.coreColliderHelper.pawnCollider))
            {
                //* RootMotion으로 목표물을 밀지 않도록 목묘물의 TouchingColliders와 접축할 정도로 가깝다면 rootMotionVec가 목표물에서 멀어지는 방향일때만 적용해준다.
                return __brain.coreColliderHelper.GetDistanceDelta(__brain.BB.TargetBrain.coreColliderHelper, rootMotionVec) > 0f;
            }
            else
            {
                return true;
            }
        }

        public override bool CanParryAction(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            return currActionContext.activeParryEnabled && damageContext.receiverBrain.coreColliderHelper.GetDistanceBetween(damageContext.senderBrain.coreColliderHelper) < 1f || 
                __brain.StatusCtrler.CheckStatus(PawnStatus.GuardParrying);
        }

        public override bool CanBlockAction(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            if (__brain.BB.IsCharging || __brain.BB.IsRolling || __brain.BB.IsJumping || (!__brain.BB.IsGuarding && !__brain.BB.IsAutoGuardEnabled))
                return false;
            if (__brain.ActionCtrler.CheckActionRunning() || __brain.StatusCtrler.CheckStatus(PawnStatus.Staggered) || __brain.StatusCtrler.CheckStatus(PawnStatus.CanNotGuard))
                return false;
            if (!__brain.SensorCtrler.WatchingColliders.Contains(damageContext.senderBrain.coreColliderHelper.pawnCollider))
                return false;
            if (damageContext.receiverBrain.PawnBB.stat.stamina.Value < damageContext.senderActionData.guardStaminaDamage)
                return false;

            return true;
        }

        public override IDisposable StartOnHitAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.receiverBrain == __brain);

            if (damageContext.actionResult == ActionResults.Damaged || damageContext.actionResult == ActionResults.GuardBreak)
            {
                var hitVec = damageContext.receiverBrain.GetWorldPosition() - damageContext.senderBrain.GetWorldPosition();
                hitVec = damageContext.receiverBrain.GetWorldTransform().InverseTransformDirection(hitVec).Vector2D().normalized;
                if (Mathf.Abs(hitVec.x) > Mathf.Abs(hitVec.z))
                {
                    __brain.AnimCtrler.mainAnimator.SetFloat("HitX", hitVec.x > 0f ? 1f : -1f);
                    __brain.AnimCtrler.mainAnimator.SetFloat("HitY", 0f);
                }
                else
                {
                    __brain.AnimCtrler.mainAnimator.SetFloat("HitX", 0f);
                    __brain.AnimCtrler.mainAnimator.SetFloat("HitY", hitVec.z > 0f ? 1f : -1f);
                }
                __brain.AnimCtrler.mainAnimator.SetInteger("HitType", 0);
                __brain.AnimCtrler.mainAnimator.SetTrigger("OnHit");

                if (GetSuperArmorLevel() != SuperArmorLevels.CanNotStarggerOnDamaged)
                {
                    //* 경직 지속 시간과 맞춰주기 위해서 'AnimSpeed' 값을 조정함
                    __brain.AnimCtrler.mainAnimator.SetFloat("AnimSpeed", 1f / damageContext.receiverPenalty.Item2);
                }
                
                if (damageContext.actionResult == ActionResults.Damaged)
                {
                    SoundManager.Instance.Play(SoundID.HIT_FLESH);
                    //EffectManager.Instance.Show(__brain.BB.graphics.onBleedFx, damageContext.hitPoint, Quaternion.identity, Vector3.one, 10f);

                    // EffectManager.Instance.Show("@Hit 23 cube", damageContext.hitPoint, Quaternion.identity, Vector3.one, 1f);
                    // EffectManager.Instance.Show("@BloodFX_impact_col", damageContext.hitPoint, Quaternion.identity, 1.5f * Vector3.one, 3f);
                }
                else
                {
                    SoundManager.Instance.Play(SoundID.HIT_BLOCK);
                    EffectManager.Instance.Show("FX/@Hit 4 yellow arrow", __brain.BB.graphics.forceShieldRenderer.transform.position, Quaternion.identity, Vector3.one, 1f);
                }

                var knockBackVec = __brain.BB.pawnData_Movement.knockBackSpeed * damageContext.senderBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
                Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(damageContext.senderActionData.knockBackDistance / __brain.BB.pawnData_Movement.knockBackSpeed)))
                    .DoOnCancel(() => __brain.Movement.FreezeForOneFrame())
                    .DoOnCompleted(() => __brain.Movement.FreezeForOneFrame())
                    .Subscribe(_ => __brain.Movement.AddRootMotion(Time.fixedDeltaTime * knockBackVec, Quaternion.identity))
                    .AddTo(this);

                //* 구르기 불가 상태 부여
                var canNotRollDuration = Mathf.Max(0.1f, damageContext.receiverPenalty.Item2 - MainTable.PlayerData.GetList().First().earlyRollOffsetOnStarggerd);
                __brain.StatusCtrler.AddStatus(PawnStatus.CanNotRoll, 1f, canNotRollDuration);
            }
            else //* Sender의 액션을 파훼된 경우
            {
                if (damageContext.actionResult == ActionResults.Blocked)
                {
                    __brain.AnimCtrler.mainAnimator.SetInteger("HitType", 1);
                    __brain.AnimCtrler.mainAnimator.SetTrigger("OnHit");
                    __brain.AnimCtrler.mainAnimator.SetBool("IsGuarding", true);

                    Observable.Timer(TimeSpan.FromSeconds(0.5f)).Subscribe(_ => 
                    {
                        if (!__brain.BB.IsGuarding && !__brain.BB.IsCharging)
                            __brain.AnimCtrler.mainAnimator.SetBool("IsGuarding", false);
                    }).AddTo(this);

                    Observable.NextFrame(FrameCountType.EndOfFrame).Subscribe(_ =>
                    {
                        EffectManager.Instance.Show(__brain.BB.graphics.onBlockFx, __brain.BB.attachment.BlockingFxAttachPoint.transform.position, Quaternion.LookRotation(__brain.coreColliderHelper.transform.forward, Vector3.up), Vector3.one);
                        SoundManager.Instance.Play(SoundID.HIT_BLOCK);
                    });
                }
                else if (damageContext.actionResult == ActionResults.GuardParried)
                {
                    __brain.AnimCtrler.mainAnimator.SetTrigger("OnGuardParry");
                    SoundManager.Instance.Play(SoundID.HIT_PARRYING);

                    Observable.Timer(TimeSpan.FromMilliseconds(50)).Subscribe(_ =>
                    {
                        EffectManager.Instance.Show(__brain.BB.graphics.onGuardParriedFx, __brain.BB.graphics.forceShieldRenderer.transform.position + 0.5f * __brain.coreColliderHelper.transform.forward, Quaternion.identity, 0.8f * Vector3.one);
                        // SoundManager.Instance.Play(SoundID.HIT_PARRYING);
                    });
                }
                else if (damageContext.actionResult == ActionResults.KickParried)
                {
                    var hitPoint = damageContext.senderBrain.coreColliderHelper.GetWorldCenter() + 
                        damageContext.senderBrain.coreColliderHelper.GetRadius() * (__brain.coreColliderHelper.GetWorldCenter() - damageContext.senderBrain.coreColliderHelper.GetWorldCenter()).Vector2D().normalized;

                    EffectManager.Instance.Show("FX/Hit 26 blue crystal", hitPoint, Quaternion.identity, 2f * Vector3.one, 1f);
                    SoundManager.Instance.Play(SoundID.HIT_PARRYING);
                }

                if (damageContext.actionResult == ActionResults.Blocked || damageContext.actionResult == ActionResults.GuardParried)
                {
                    var knockBackVec = __brain.BB.pawnData_Movement.knockBackSpeed * damageContext.senderBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
                    Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(0.5f / __brain.BB.pawnData_Movement.knockBackSpeed)))
                        .DoOnCancel(() => __brain.Movement.FreezeForOneFrame())
                        .DoOnCompleted(() => __brain.Movement.FreezeForOneFrame())
                        .Subscribe(_ => __brain.Movement.AddRootMotion(Time.fixedDeltaTime * knockBackVec, Quaternion.identity))
                        .AddTo(this);
                }
            }

            return null;
        }

        public override IDisposable StartOnBlockedAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.senderBrain == __brain);
            Debug.Assert(damageContext.receiverActionData != null);
            Debug.Assert(!isAddictiveAction);

            __brain.AnimCtrler.mainAnimator.SetTrigger("OnHit");
            __brain.AnimCtrler.mainAnimator.SetInteger("HitType", 3);

            var knockBackVec = __brain.BB.pawnData_Movement.knockBackSpeed * damageContext.receiverBrain.GetWorldTransform().forward.Vector2D().normalized;
            Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(damageContext.receiverActionData.knockBackDistance / __brain.BB.pawnData_Movement.knockBackSpeed)))
                .DoOnCancel(() =>
                {
                    __brain.Movement.FreezeForOneFrame();
                    if (CurrActionName == "!OnBlocked")
                        FinishAction();
                })
                .DoOnCompleted(() =>
                {
                    __brain.Movement.FreezeForOneFrame();
                    if (CurrActionName == "!OnBlocked")
                        FinishAction();
                })
                .Subscribe(_ => __brain.Movement.AddRootMotion(Time.fixedDeltaTime * knockBackVec, Quaternion.identity)).AddTo(this);

            return null;
        }

        public override IDisposable StartOnKnockDownAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            if (__brain.BB.IsHanging) 
                __brain.Movement.CancelHanging();

#if UNITY_EDITOR
            if (damageContext.senderBrain == null && damageContext.receiverBrain == null)
            {
                __brain.PawnStatusCtrler.AddStatus(PawnStatus.KnockDown, 1f, __brain.BB.pawnData.knockDownDuration);

                var __knockDownTimeStamp = Time.time;
                var __knockBackVec = -__brain.coreColliderHelper.transform.forward.Vector2D().normalized;
                Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(0.1f))).Subscribe(_ =>
                {
                    __brain.Movement.AddRootMotion(Time.fixedDeltaTime * (__brain.BB.pawnData_Movement.knockBackSpeed * __knockBackVec), Quaternion.identity);
                }).AddTo(this);

                return null;
            }
#endif

            Debug.Assert(damageContext.receiverBrain == __brain);
            Debug.Assert(!isAddictiveAction);
            
            var hitVec = damageContext.receiverBrain.GetWorldPosition() - damageContext.senderBrain.GetWorldPosition();
            hitVec = damageContext.receiverBrain.GetWorldTransform().InverseTransformDirection(hitVec).Vector2D().normalized;
            if (Mathf.Abs(hitVec.x) > Mathf.Abs(hitVec.z))
            {
                __brain.AnimCtrler.mainAnimator.SetFloat("HitX", hitVec.x > 0f ? 1f : -1f);
                __brain.AnimCtrler.mainAnimator.SetFloat("HitY", 0f);
            }
            else
            {
                __brain.AnimCtrler.mainAnimator.SetFloat("HitX", 0f);
                __brain.AnimCtrler.mainAnimator.SetFloat("HitY", hitVec.z > 0f ? 1f : -1f);
            }
            __brain.AnimCtrler.mainAnimator.SetInteger("HitType", 0);
            __brain.AnimCtrler.mainAnimator.SetTrigger("OnHit");

            var knockDownTimeStamp = Time.time;
            var knockBakVec = (damageContext.receiverBrain.GetWorldPosition() - damageContext.senderBrain.GetWorldPosition()).Vector2D().normalized;
            Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(0.1f))).Subscribe(_ =>
            {
                __brain.Movement.AddRootMotion(Time.fixedDeltaTime * (__brain.BB.pawnData_Movement.knockBackSpeed * knockBakVec), Quaternion.identity);
            }).AddTo(this);

            return null;
        }

        HeroBrain __brain;

        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            __brain = GetComponent<HeroBrain>();
        }

        protected override void StartInternal()
        {
            base.StartInternal();

            __brain.BB.common.isDown.Skip(1).Subscribe(v =>
            {
                if (v)
                {
                    __brain.AnimCtrler.mainAnimator.SetBool("IsDown", true);
                    __brain.AnimCtrler.mainAnimator.SetTrigger("OnDown");
                }
                else
                {
                    //* 일어나는 모션동안은 무적
                    __brain.PawnStatusCtrler.AddStatus(PawnStatus.Invincible, 1f, 1f);
                    __brain.AnimCtrler.mainAnimator.SetBool("IsDown", false);
                }
            }).AddTo(this);

            __brain.BB.action.isJumping.Skip(1).Subscribe(v =>
            {
                if (v)
                {
                    EffectManager.Instance.Show("FX/FX_Cartoony_Jump_Up_01", __brain.GetWorldPosition(), 
                        Quaternion.identity, Vector3.one, 1f);
                    SoundManager.Instance.Play(SoundID.JUMP);
                }
                else if (__brain.Movement.IsOnGround)
                {
                    EffectManager.Instance.Show("FX/JumpCloudSmall", __brain.GetWorldPosition() + 
                        Time.deltaTime * __brain.Movement.moveSpeed * __brain.Movement.moveVec + 
                        0.1f * Vector3.up, Quaternion.identity, 0.8f * Vector3.one, 1f);
                    SoundManager.Instance.Play(SoundID.LAND);
                }
            }).AddTo(this);

            __brain.PawnStatusCtrler.onStatusActive += (status) =>
            {
                // if (status == PawnStatus.Staggered && __staggerFxInstance == null)
                // {
                //     __staggerFxInstance = EffectManager.Instance.ShowLooping(staggerFx, fxAttachPoint.position, Quaternion.identity, Vector3.one);
                //     __staggerFxInstance.transform.SetParent(fxAttachPoint);
                // }
            };

            __brain.PawnStatusCtrler.onStatusDeactive += (status) =>
            {
                //* 경직 종료 후에 짧은 시간 동안 가드 불가 부여 
                if (status == PawnStatus.Staggered)
                {
                    __brain.PawnStatusCtrler.AddStatus(PawnStatus.CanNotGuard, 1f, MainTable.PlayerData.GetList().First().postGuardDelayOnStaggered);
                    // __staggerFxInstance?.Stop();
                    // __staggerFxInstance = null;
                }
            };

            onActiveParryEnabled += (_) => __brain.parryColliderHelper.pawnCollider.enabled = currActionContext.activeParryEnabled;
            onActionCanceled += (_, __) => __brain.parryColliderHelper.pawnCollider.enabled = false;
            onActionFinished += (_) => __brain.parryColliderHelper.pawnCollider.enabled = false;

            onEmitProjectile += OnEmitProjectile;
        }

        void OnEmitProjectile(ActionContext context, ProjectileMovement proj, Transform point, int num) 
        {
            var obj = GameObject.Instantiate(proj);

            var trRoot = __brain.GetWorldTransform();

            var chainProj = obj.GetComponent<HeroChainShotProjectile>();
            var pos = (trRoot.position + Vector3.up) + trRoot.forward;
            chainProj.heroBrain = __brain;
            chainProj.Go(null, pos, 10.0f * trRoot.forward, Vector3.one);
        }

        void OnDrawGizmos()
        {
            // if (swordCollider != null)
            //     GizmosDrawExtension.DrawBox(swordCollider.transform.position + swordCollider.transform.rotation * (swordCollider as BoxCollider).center, swordCollider.transform.rotation , 0.5f * (swordCollider as BoxCollider).size);
        }
    }
}
