using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using XftWeapon;

namespace Game
{
    public class SoldierActionController : JellyHumanoidActionController
    {
        [Header("Component")]
        public PawnColliderHelper hookingPointColliderHelper;
        public PawnColliderHelper counterActionColliderHelper;
        public BoxCollider shieldCollider;
        public XWeaponTrail sworldWeaponTrailA;
        public XWeaponTrail sworldWeaponTrailB;

        [Header("Parameter")]
        public float leapRootMotionDistance = 7f;
        public float leapRootMotionMultiplier = 1f;
        public CapsuleCollider CounterActionCollider => counterActionColliderHelper.pawnCollider as CapsuleCollider;

        public override bool CanBlockAction(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            if (__brain.BB.IsGroggy)
                return false;
            else if (__brain.ActionCtrler.CheckActionRunning() || __brain.StatusCtrler.CheckStatus(PawnStatus.Staggered) || __brain.StatusCtrler.CheckStatus(PawnStatus.CanNotGuard))
                return false;
            else if (__brain.SensorCtrler.WatchingColliders.Contains(damageContext.senderBrain.coreColliderHelper.pawnCollider) == false)
                return false;

            return true;
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
                __brain.AnimCtrler.mainAnimator.SetBool("IsGuarding", true);
                __brain.AnimCtrler.mainAnimator.SetTrigger("OnGuard");
                
                Observable.Timer(TimeSpan.FromSeconds(0.5f)).Subscribe(_ => __brain.AnimCtrler.mainAnimator.SetBool("IsGuarding", false)).AddTo(this);
                Observable.NextFrame(FrameCountType.EndOfFrame).Subscribe(_ => EffectManager.Instance.Show(__brain.BB.graphics.onBlockedFx, __brain.BB.graphics.BlockingFxAttachPoint.transform.position, Quaternion.identity, 0.8f * Vector3.one, 1f)).AddTo(this);
                SoundManager.Instance.PlayWithClip(__brain.BB.audios.onBlockedAudioClip);

                ShowHitColor(__brain.shieldHitColliderHelper);
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
            Debug.Assert(damageContext.receiverBrain == __brain);

            if (damageContext.actionResult == ActionResults.Damaged)
            {
                EffectManager.Instance.Show(__brain.BB.graphics.onBigHitFx, __brain.bodyHitColliderHelper.GetWorldCenter(), Quaternion.LookRotation(damageContext.hitPoint - __brain.bodyHitColliderHelper.GetWorldCenter()) * Quaternion.Euler(90f, 0f, 0f), Vector3.one, 1f);
                SoundManager.Instance.PlayWithClip(__brain.BB.audios.onBigHitAudioClip);
            }

            return base.StartOnKnockDownAction(ref damageContext, isAddictiveAction);
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
            else if (hitColliderHelper == __brain.shieldHitColliderHelper)
            {
                __brain.BB.attachment.shieldMeshRenderer.materials = new Material[] { __brain.BB.attachment.shieldMeshRenderer.material, new(__brain.BB.graphics.hitColor) };

                __hitColorDisposable?.Dispose();
                __hitColorDisposable = Observable.Timer(TimeSpan.FromMilliseconds(100)).Subscribe(_ => 
                {
                    __hitColorDisposable = null;
                    __brain.BB.attachment.shieldMeshRenderer.materials = new Material[] { __brain.BB.attachment.shieldMeshRenderer.material };
                }).AddTo(this);
            }
            else
            {
                Debug.Assert(false);
            }
        }

        IDisposable __hitColorDisposable;
        SoldierBrain __brain;

        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            __brain = GetComponent<SoldierBrain>();
        }

        protected override void StartInternal()
        {
            base.StartInternal();

            __brain.BB.action.isGuarding.Subscribe(v =>
            {
                __brain.AnimCtrler.mainAnimator.SetBool("IsGuarding", v);
            }).AddTo(this);
        }
    }
}