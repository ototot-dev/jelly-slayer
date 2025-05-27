using System;
using UniRx;
using UnityEngine;
using XftWeapon;

namespace Game
{
    public class AlienActionController : NpcHumanoidActionController
    {
        [Header("Component")]
        public Transform counterActionCollider;
        public XWeaponTrail leftKnifeTrailA;
        public XWeaponTrail leftKnifeTrailB;
        public XWeaponTrail rightKnifeTrailA;
        public XWeaponTrail rightKnifeTrailB;

        public override bool CanBlockAction(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            if (__brain.BB.IsGroggy)
                return false;
            else if (__brain.ActionCtrler.CheckActionRunning())
                return false;
            else if (!__brain.SensorCtrler.WatchingColliders.Contains(damageContext.senderBrain.coreColliderHelper.pawnCollider))
                return false;

            return damageContext.insufficientStamina;
        }

        public override IDisposable StartOnHitAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.receiverBrain == __brain);

            if (damageContext.actionResult == ActionResults.Damaged)
            {
                if (damageContext.senderActionData.actionName.StartsWith("Kick"))
                {
                    EffectManager.Instance.Show(__brain.BB.graphics.onKickHitFx, __brain.bodyHitColliderHelper.GetWorldCenter(), Quaternion.identity, Vector3.one, 1f);
                    SoundManager.Instance.PlayWithClip(__brain.BB.audios.onKickHitAudioClip);
                }
                else if (damageContext.senderActionData.actionName.StartsWith("Heavy"))
                {
                    EffectManager.Instance.Show(__brain.BB.graphics.onBigHitFx, __brain.bodyHitColliderHelper.GetWorldCenter(), Quaternion.LookRotation(damageContext.hitPoint - __brain.bodyHitColliderHelper.GetWorldCenter()) * Quaternion.Euler(90f, 0f, 0f), Vector3.one, 1f);
                    SoundManager.Instance.PlayWithClip(__brain.BB.audios.onBigHitAudioClip);
                }
                else
                {
                    EffectManager.Instance.Show(__brain.BB.graphics.onHitFx, __brain.bodyHitColliderHelper.GetWorldCenter(), Quaternion.LookRotation(damageContext.hitPoint - __brain.bodyHitColliderHelper.GetWorldCenter()) * Quaternion.Euler(90f, 0f, 0f), Vector3.one, 1f);
                    SoundManager.Instance.PlayWithClip(__brain.BB.audios.onHitAudioClip);
                }

                ShowHitColor(__brain.bodyHitColliderHelper);
            }
            else if (damageContext.actionResult == ActionResults.Missed)
            {
                Observable.NextFrame(FrameCountType.EndOfFrame).Subscribe(_ => EffectManager.Instance.Show(__brain.BB.graphics.onMissedFx, __brain.BB.graphics.BlockingFxAttachPoint.transform.position, Quaternion.identity, 0.8f * Vector3.one, 1f)).AddTo(this);
                SoundManager.Instance.PlayWithClip(__brain.BB.audios.onMissedAudioClip);
            }
            else if (damageContext.actionResult == ActionResults.Blocked)
            {
                Observable.NextFrame(FrameCountType.EndOfFrame).Subscribe(_ => EffectManager.Instance.Show(__brain.BB.graphics.onBlockedFx, __brain.BB.graphics.BlockingFxAttachPoint.transform.position, Quaternion.identity, 0.8f * Vector3.one, 1f)).AddTo(this);
                SoundManager.Instance.PlayWithClip(__brain.BB.audios.onBlockedAudioClip);
            }
            else if (damageContext.actionResult == ActionResults.GuardBreak) 
            {
                Observable.NextFrame(FrameCountType.EndOfFrame).Subscribe(_ => EffectManager.Instance.Show(__brain.BB.graphics.onGuardBreakFx, __brain.BB.graphics.BlockingFxAttachPoint.transform.position, Quaternion.identity, Vector3.one, 1f)).AddTo(this);
                SoundManager.Instance.PlayWithClip(__brain.BB.audios.onGuardBreakAudioClip);
            }

            return base.StartOnHitAction(ref damageContext, isAddictiveAction);
        }

        public override IDisposable StartOnKnockDownAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
#if UNITY_EDITOR
            if (damageContext.senderBrain == null && damageContext.receiverBrain == null)
            {
                __brain.PawnStatusCtrler.AddStatus(PawnStatus.KnockDown, 1f, __brain.BB.pawnData.knockDownDuration);

                var __knockDownTimeStamp = Time.time;
                var __knockBackVec = -__brain.coreColliderHelper.transform.forward.Vector2D().normalized;
                Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(0.1f))).Subscribe(_ =>
                {
                    __brain.Movement.AddRootMotion(Time.fixedDeltaTime * (__brain.BB.pawnData_Movement.knockBackSpeed * __knockBackVec), Quaternion.identity, Time.fixedDeltaTime);
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
                __brain.Movement.AddRootMotion(Time.fixedDeltaTime * (__brain.BB.pawnData_Movement.knockBackSpeed * knockBakVec), Quaternion.identity, Time.fixedDeltaTime);
            }).AddTo(this);

            return null;
        }

        void ShowHitColor(PawnColliderHelper hitColliderHelper)
        {
            if (hitColliderHelper == __brain.bodyHitColliderHelper)
            {
                foreach (var r in __brain.BB.attachment.bodyMeshRenderers)
                    r.materials = new Material[] { r.material, new(__brain.BB.graphics.hitColor) };

                __hitColorDisposable?.Dispose();
                __hitColorDisposable = Observable.Timer(TimeSpan.FromMilliseconds(100)).Subscribe(_ => 
                {
                    __hitColorDisposable = null;
                    foreach (var r in __brain.BB.attachment.bodyMeshRenderers)
                        r.materials = new Material[] { r.materials[0] };
                }).AddTo(this);
            }
            else
            {
                Debug.Assert(false);
            }
        }

        IDisposable __hitColorDisposable;
        AlienBrain __brain;

        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            __brain = GetComponent<AlienBrain>();
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

            __brain.BB.action.isGuarding.Subscribe(v =>
            {
                __brain.AnimCtrler.mainAnimator.SetBool("IsGuarding", v);
            }).AddTo(this);
        }
    }
}