using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Game
{
    public class SlayerActionController : PawnActionController
    {
        [Header("Component")]
        public Transform fxAttachPoint;
        public Collider punchActionCollider;

        public override bool CanRootMotion(Vector3 rootMotionVec)
        {
            if (!base.CanRootMotion(rootMotionVec))
                return false;

            if (__brain.BB.IsRolling || __brain.BB.TargetBrain == null || !__brain.SensorCtrler.TouchingColliders.Contains(__brain.BB.TargetBrain.coreColliderHelper.pawnCollider))
                return true;

            //* RootMotion으로 목표물을 밀지 않도록 목표물의 TouchingColliders와 접축할 정도로 가깝다면 rootMotionVec가 목표물에서 멀어지는 방향일때만 적용해준다.
            return __brain.coreColliderHelper.GetDistanceDelta(__brain.BB.TargetBrain.coreColliderHelper, rootMotionVec) > 0f;
        }

        public override bool CanParryAction(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            return (currActionContext.parryingEnabled && damageContext.hitCollider == __brain.parryHitColliderHelper.pawnCollider) || __brain.StatusCtrler.CheckStatus(PawnStatus.GuardParrying);
        }

        public override bool CanBlockAction(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            if (__brain.BB.IsPunchCharging || __brain.BB.IsRolling || __brain.BB.IsJumping || (!__brain.BB.IsGuarding && !__brain.BB.IsAutoGuardEnabled))
                return false;
            if (__brain.ActionCtrler.CheckActionRunning() || __brain.StatusCtrler.CheckStatus(PawnStatus.Staggered) || __brain.StatusCtrler.CheckStatus(PawnStatus.CanNotGuard))
                return false;
            if (!__brain.SensorCtrler.WatchingColliders.Contains(damageContext.senderBrain.coreColliderHelper.pawnCollider))
                return false;
            if (damageContext.receiverBrain.PawnBB.stat.stamina.Value <= 0)
                return false;

            return true;
        }

        public override IDisposable StartOnHitAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.receiverBrain == __brain);

            if (damageContext.actionResult == ActionResults.Damaged)
            {
                var hitVec = damageContext.receiverBrain.GetWorldPosition() - damageContext.senderBrain.GetWorldPosition();
                hitVec = damageContext.receiverBrain.GetWorldTransform().InverseTransformDirection(hitVec).Vector2D().normalized;

                if (Mathf.Abs(hitVec.x) > Mathf.Abs(hitVec.z))
                {
                    __pawnAnimCtrler.mainAnimator.SetFloat("HitX", hitVec.x > 0f ? 1f : -1f);
                    __pawnAnimCtrler.mainAnimator.SetFloat("HitY", 0f);
                }
                else
                {
                    __pawnAnimCtrler.mainAnimator.SetFloat("HitX", 0f);
                    __pawnAnimCtrler.mainAnimator.SetFloat("HitY", hitVec.z > 0f ? 1f : -1f);
                }

                //* 구르기 불가 상태 부여
                var cannotRollDuration = Mathf.Max(0.1f, damageContext.receiverPenalty.Item2 - DatasheetManager.Instance.GetPlayerData().earlyRollOffsetOnStarggerd);
                __brain.StatusCtrler.AddStatus(PawnStatus.CanNotRoll, 1f, cannotRollDuration);
                __brain.AnimCtrler.mainAnimator.SetInteger("HitType", 0);
                __brain.AnimCtrler.mainAnimator.SetTrigger("OnHit");

                //* 경직 지속 시간과 맞춰주기 위해서 'AnimSpeed' 값을 조정함
                if (GetSuperArmorLevel() != SuperArmorLevels.CanNotStarggerOnDamaged)
                    __brain.AnimCtrler.mainAnimator.SetFloat("AnimSpeed", 1f / damageContext.receiverPenalty.Item2);

                var viewMatrix = GameContext.Instance.cameraCtrler.pixelCamera.transform.worldToLocalMatrix;
                var hitPointOffsetVec = viewMatrix.MultiplyPoint(damageContext.hitPoint.AdjustY(0f)) - viewMatrix.MultiplyPoint(__brain.GetWorldPosition().AdjustY(0f));
                if (Mathf.Abs(hitPointOffsetVec.x) > Mathf.Abs(hitPointOffsetVec.y))
                {
                    EffectManager.Instance.Show(__brain.BB.graphics.onBloodBurstFx[0], damageContext.hitPoint, GameContext.Instance.cameraCtrler.BillboardRotation, 0.6f * (hitPointOffsetVec.x > 0f ? new Vector3(-1f, 1f, 1f) : Vector3.one))
                        .transform.SetParent(__brain.bodyHitColliderHelper.transform, true);
                }
                else
                {
                    EffectManager.Instance.Show(__brain.BB.graphics.onBloodBurstFx[1], damageContext.hitPoint, GameContext.Instance.cameraCtrler.BillboardRotation, Vector3.one)
                        .transform.SetParent(__brain.bodyHitColliderHelper.transform, true);
                }
                EffectManager.Instance.Show(__brain.BB.graphics.onBleedFx, __brain.bodyHitColliderHelper.GetWorldCenter(), Quaternion.LookRotation(damageContext.hitPoint - __brain.bodyHitColliderHelper.GetWorldCenter()), Vector3.one)
                    .transform.SetParent(__brain.bodyHitColliderHelper.transform, true);

                SoundManager.Instance.Play(SoundID.HIT_FLESH);
                ShowHitColor(__brain.bodyHitColliderHelper);

                var knockBackVec = __brain.BB.pawnData_Movement.knockBackSpeed * damageContext.senderBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
                Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(damageContext.senderActionData.knockBackDistance / __brain.BB.pawnData_Movement.knockBackSpeed)))
                    .Subscribe(_ => __brain.Movement.AddRootMotion(Time.fixedDeltaTime * knockBackVec, Quaternion.identity, Time.fixedDeltaTime))
                    .AddTo(this);

                // 분노 게이지
                var rage = MainTable.PlayerData.GetList().First().damageRage;
                __brain.AddRagePoint(rage);
            }
            else if (damageContext.actionResult == ActionResults.GuardBreak)
            {
                __brain.StatusCtrler.AddStatus(PawnStatus.CanNotRoll, 1f, damageContext.receiverPenalty.Item2);
                __brain.AnimCtrler.mainAnimator.SetInteger("HitType", 2);
                __brain.AnimCtrler.mainAnimator.SetTrigger("OnHit");

                //* 경직 지속 시간과 맞춰주기 위해서 'AnimSpeed' 값을 조정함
                __brain.AnimCtrler.mainAnimator.SetFloat("AnimSpeed", 1f / damageContext.receiverPenalty.Item2);

                EffectManager.Instance.Show(__brain.BB.graphics.onGuardBreakFx, __brain.PartsCtrler.leftMechHandBone.transform.position, Quaternion.LookRotation(__brain.coreColliderHelper.transform.forward, Vector3.up), Vector3.one);
                SoundManager.Instance.PlayWithClipPos(__brain.BB.audios.onGuardBreakAudioClip, __brain.PartsCtrler.leftMechHandBone.transform.position);

                var knockBackVec = __brain.BB.pawnData_Movement.knockBackSpeed * damageContext.senderBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
                Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(damageContext.senderActionData.knockBackDistance / __brain.BB.pawnData_Movement.knockBackSpeed)))
                    .Subscribe(_ => __brain.Movement.AddRootMotion(Time.fixedDeltaTime * knockBackVec, Quaternion.identity, Time.fixedDeltaTime))
                    .AddTo(this);

                // 분노 게이지
                var rage = MainTable.PlayerData.GetList().First().damageRage;
                __brain.AddRagePoint(rage);
            }
            else //* Sender의 액션을 파훼된 경우
            {
                if (damageContext.actionResult == ActionResults.Blocked)
                {
                    __brain.AnimCtrler.mainAnimator.SetInteger("HitType", 1);
                    __brain.AnimCtrler.mainAnimator.SetTrigger("OnHit");

                    //* 경직 지속 시간과 맞춰주기 위해서 'AnimSpeed' 값을 조정함
                    __brain.AnimCtrler.mainAnimator.SetFloat("AnimSpeed", 1f / damageContext.receiverPenalty.Item2);

                    __brain.AnimCtrler.mainAnimator.SetBool("IsGuarding", true);
                    Observable.Timer(TimeSpan.FromSeconds(0.5f)).Subscribe(_ => 
                    {
                        if (!__brain.BB.IsGuarding)
                            __brain.AnimCtrler.mainAnimator.SetBool("IsGuarding", false);
                    }).AddTo(this);

                    Observable.NextFrame(FrameCountType.EndOfFrame).Subscribe(_ =>
                    {
                        EffectManager.Instance.Show(__brain.BB.graphics.onBlockFx, __brain.PartsCtrler.leftMechHandBone.transform.position, Quaternion.LookRotation(__brain.coreColliderHelper.transform.forward, Vector3.up), Vector3.one);
                        SoundManager.Instance.Play(SoundID.HIT_BLOCK);
                    });

                    ShowHitColor();

                    // 분노 게이지 (가드)
                    var rage = MainTable.PlayerData.GetList().First().guardRage;
                    __brain.AddRagePoint(rage);
                }
                else if (damageContext.actionResult == ActionResults.GuardParrying)
                {
                    __brain.AnimCtrler.mainAnimator.SetTrigger("OnGuardParry");
                    SoundManager.Instance.Play(SoundID.HIT_PARRYING);

                    TimeManager.Instance.SlomoTime(this, 0.5f, 0.2f);

                    Observable.Timer(TimeSpan.FromMilliseconds(50)).Subscribe(_ =>
                    {
                        var pos = __brain.PartsCtrler.leftMechHandBone.transform.position + 0.5f * __brain.coreColliderHelper.transform.forward;
                        EffectManager.Instance.Show(__brain.BB.graphics.onGuardParriedFx, pos, Quaternion.identity, 0.8f * Vector3.one);
                        EffectManager.Instance.Show(__brain.BB.graphics.onGuardParriedFx2, pos, Quaternion.identity, 2.0f * Vector3.one);
                    });

                    ShowHitColor();

                    // 분노 게이지 (패리)
                    var rage = MainTable.PlayerData.GetList().First().parryRage;
                    __brain.AddRagePoint(rage);
                }
                else if (damageContext.actionResult == ActionResults.PunchParrying)
                {
                    var hitPoint = damageContext.senderBrain.coreColliderHelper.GetWorldCenter() + 
                        damageContext.senderBrain.coreColliderHelper.GetCapsuleRadius() * (__brain.coreColliderHelper.GetWorldCenter() - damageContext.senderBrain.coreColliderHelper.GetWorldCenter()).Vector2D().normalized;

                    EffectManager.Instance.Show("FX/Hit 26 blue crystal", hitPoint, Quaternion.identity, 2f * Vector3.one, 1f);
                    SoundManager.Instance.Play(SoundID.HIT_PARRYING);

                    // 분노 게이지 (패리)
                    var rage = MainTable.PlayerData.GetList().First().parryRage;
                    __brain.AddRagePoint(rage);
                }

                if (damageContext.actionResult == ActionResults.Blocked || damageContext.actionResult == ActionResults.GuardParrying)
                {
                    var knockBackVec = __brain.BB.pawnData_Movement.knockBackSpeed * damageContext.senderBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
                    Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(0.5f / __brain.BB.pawnData_Movement.knockBackSpeed)))
                        .Subscribe(_ => __brain.Movement.AddRootMotion(Time.fixedDeltaTime * knockBackVec, Quaternion.identity, Time.fixedDeltaTime))
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

            //* 경직 지속 시간과 맞춰주기 위해서 'AnimSpeed' 값을 조정함
            __brain.AnimCtrler.mainAnimator.SetFloat("AnimSpeed", 1f / damageContext.senderPenalty.Item2);

            var knockBackVec = __brain.BB.pawnData_Movement.knockBackSpeed * damageContext.receiverBrain.GetWorldTransform().forward.Vector2D().normalized;
            Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(damageContext.receiverActionData.knockBackDistance / __brain.BB.pawnData_Movement.knockBackSpeed)))
                .DoOnCancel(() =>
                {
                    if (CurrActionName == "!OnBlocked")
                        FinishAction();
                })
                .DoOnCompleted(() =>
                {
                    if (CurrActionName == "!OnBlocked")
                        FinishAction();
                })
                .Subscribe(_ => __brain.Movement.AddRootMotion(Time.fixedDeltaTime * knockBackVec, Quaternion.identity, Time.fixedDeltaTime)).AddTo(this);

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
                // var __knockBackVec = -__brain.BB.pawnData_Movement.knockBackSpeed * __brain.coreColliderHelper.transform.forward.Vector2D().normalized;
                var __knockBackVec = -2f * __brain.coreColliderHelper.transform.forward.Vector2D().normalized;
                Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(0.4f))).Subscribe(_ =>
                {
                    __brain.Movement.AddRootMotion(Time.fixedDeltaTime * __knockBackVec, Quaternion.identity, Time.fixedDeltaTime);
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
            var knockBakVec = __brain.BB.pawnData_Movement.knockBackSpeed * (damageContext.receiverBrain.GetWorldPosition() - damageContext.senderBrain.GetWorldPosition()).Vector2D().normalized;
            Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(0.1f))).Subscribe(_ =>
            {
                __brain.Movement.AddRootMotion(Time.fixedDeltaTime * knockBakVec, Quaternion.identity, Time.fixedDeltaTime);
            }).AddTo(this);

            return null;
        }

        Material[] __bodyMeshRenderersCached;
        Material[] __swordMeshRenderersCached;
        Material[] __hitColorBodyMeshRenderersCached;
        Material[] __hitColorSwordMeshRenderersCached;

        void ShowHitColor(PawnColliderHelper hitColliderHelper = null)
        {
            __bodyMeshRenderersCached ??= __brain.PartsCtrler.bodyMeshRenderer.materials;
            __swordMeshRenderersCached ??= __brain.PartsCtrler.swordMeshRenderer.materials;
            __hitColorBodyMeshRenderersCached ??= new Material[] { __brain.BB.graphics.hitColor, __brain.BB.graphics.hitColor, __brain.BB.graphics.hitColor, __brain.BB.graphics.hitColor };
            __hitColorSwordMeshRenderersCached ??= new Material[] { __brain.BB.graphics.hitColor, __brain.BB.graphics.hitColor };

            if (hitColliderHelper == __brain.bodyHitColliderHelper)
            {
                __brain.PartsCtrler.bodyMeshRenderer.materials = __hitColorBodyMeshRenderersCached;
                __brain.PartsCtrler.swordMeshRenderer.materials = __hitColorSwordMeshRenderersCached;

                foreach (var r in __brain.PartsCtrler.mechArmRenderers)
                    r.materials = new Material[] { r.material, new(__brain.BB.graphics.hitColor) };

                __hitColorDisposable?.Dispose();
                __hitColorDisposable = Observable.Timer(TimeSpan.FromMilliseconds(100)).Subscribe(_ => 
                {
                    __hitColorDisposable = null;
                    __brain.PartsCtrler.bodyMeshRenderer.materials = __bodyMeshRenderersCached;
                    __brain.PartsCtrler.swordMeshRenderer.materials = __swordMeshRenderersCached;

                    foreach (var r in __brain.PartsCtrler.mechArmRenderers)
                        r.materials = new Material[] { r.materials[0] };
                }).AddTo(this);
            }
            else
            {
                foreach (var r in __brain.PartsCtrler.mechArmRenderers)
                    r.materials = new Material[] { r.material, new(__brain.BB.graphics.hitColor) };

                __hitColorDisposable?.Dispose();
                __hitColorDisposable = Observable.Timer(TimeSpan.FromMilliseconds(100)).Subscribe(_ => 
                {
                    foreach (var r in __brain.PartsCtrler.mechArmRenderers)
                        r.materials = new Material[] { r.materials[0] };
                }).AddTo(this);
            }
        }

        SlayerBrain __brain;
        IDisposable __hitColorDisposable;

        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            __brain = GetComponent<SlayerBrain>();
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

            __brain.BB.body.isJumping.Skip(1).Subscribe(v =>
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

            onParryingEnabled += (_) => __brain.parryHitColliderHelper.pawnCollider.enabled = currActionContext.parryingEnabled;
            onActionStart += (_, __) => 
            {
                if ((currActionContext.actionData?.actionName ?? string.Empty) == "Rolling") 
                    __brain.BB.body.isRolling.Value = true; 
            };
            onActionCanceled += (actionContext, __) => 
            { 
                __brain.parryHitColliderHelper.pawnCollider.enabled = false; 
                if ((actionContext.actionData?.actionName ?? string.Empty) == "Rolling") 
                    __brain.BB.body.isRolling.Value = false; 
            };
            onActionFinished += (actionContext) => 
            { 
                __brain.parryHitColliderHelper.pawnCollider.enabled = false;
                if ((actionContext.actionData?.actionName ?? string.Empty) == "Rolling") 
                    __brain.BB.body.isRolling.Value = false; 
            };
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
