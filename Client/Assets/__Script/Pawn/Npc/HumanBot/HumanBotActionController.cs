using System;
using System.Collections;
using UniRx;
using UnityEngine;

namespace Game
{
    public class HumanBotActionController : NpcHumanoidActionController
    {
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
                if (__brain.BB.IsGroggy)
                {
                    if (damageContext.groggyBreakHit || damageContext.senderActionData.actionName == "Assault")
                    {
                        Observable.Interval(TimeSpan.FromSeconds(0.1f)).Take(3).Subscribe(_ =>
                        {
                            EffectManager.Instance.Show(__brain.BB.resource.onBleedingFx, __brain.bodyHitColliderHelper.GetWorldCenter(), Quaternion.LookRotation(__brain.coreColliderHelper.transform.forward), 1.5f * Vector3.one)
                                .transform.SetParent(__brain.bodyHitColliderHelper.transform, true);
                        }).AddTo(this);
                    }

                    if (damageContext.senderActionData.actionName == "Assault")
                    {
                        __brain.jellyMeshCtrler.springMassSystem.coreAttachPoint.parent.position = __brain.coreColliderHelper.GetWorldCenter();
                        __brain.jellyMeshCtrler.FadeIn(0.5f);
                        // __brain.jellyMeshCtrler.StartHook();
                    }
                    else
                    {
                        __brain.jellyMeshCtrler?.ShowHitColor(0.2f);
                        // SoundManager.Instance.PlayWithClip(__brain.BB.audios.onHitFleshClip);
                    }

                    SoundManager.Instance.PlayWithClip(__brain.BB.resource.onHitAudioClip);
                    SoundManager.Instance.PlayWithClip(__brain.BB.resource.onHitAudioClip2);
                }
                // else if (damageContext.senderActionData.actionName.StartsWith("Kick"))
                // {
                //     EffectManager.Instance.Show(__brain.BB.graphics.onKickHitFx, __brain.bodyHitColliderHelper.GetWorldCenter(), Quaternion.identity, Vector3.one, 1f);
                //     SoundManager.Instance.PlayWithClip(__brain.BB.audios.onKickHitAudioClip);
                // }
                // else if (damageContext.senderActionData.actionName.StartsWith("Heavy"))
                // {
                //     EffectManager.Instance.Show(__brain.BB.graphics.onBigHitFx, __brain.bodyHitColliderHelper.GetWorldCenter(), Quaternion.LookRotation(damageContext.hitPoint - __brain.bodyHitColliderHelper.GetWorldCenter()) * Quaternion.Euler(90f, 0f, 0f), Vector3.one, 1f);
                //     SoundManager.Instance.PlayWithClip(__brain.BB.audios.onBigHitAudioClip);
                //     SoundManager.Instance.PlayWithClip(__brain.BB.audios.onHitAudioClip2);
                // }
                else
                {
                    EffectManager.Instance.Show(__brain.BB.resource.onHitFx, __brain.bodyHitColliderHelper.GetWorldCenter(), Quaternion.LookRotation(damageContext.hitPoint - __brain.bodyHitColliderHelper.GetWorldCenter()) * Quaternion.Euler(90f, 0f, 0f), Vector3.one, 1f);
                    SoundManager.Instance.PlayWithClip(__brain.BB.resource.onHitAudioClip);
                    SoundManager.Instance.PlayWithClip(__brain.BB.resource.onHitAudioClip2);
                }

                ShowHitColor(__brain.bodyHitColliderHelper);
            }
            else if (damageContext.actionResult == ActionResults.Missed)
            {
                Observable.NextFrame(FrameCountType.EndOfFrame).Subscribe(_ => EffectManager.Instance.Show(__brain.BB.resource.onMissedFx, __brain.BB.children.blockingFxAttachPoint.position, Quaternion.identity, 0.8f * Vector3.one, 1f)).AddTo(this);
                SoundManager.Instance.PlayWithClip(__brain.BB.resource.onMissedAudioClip);
            }
            else if (damageContext.actionResult == ActionResults.Blocked)
            {
                __brain.AnimCtrler.mainAnimator.SetBool("IsGuarding", true);
                __brain.AnimCtrler.mainAnimator.SetTrigger("OnGuard");

                Observable.Timer(TimeSpan.FromSeconds(0.5f)).Subscribe(_ => __brain.AnimCtrler.mainAnimator.SetBool("IsGuarding", false)).AddTo(this);
                Observable.NextFrame(FrameCountType.EndOfFrame).Subscribe(_ => EffectManager.Instance.Show(__brain.BB.resource.onBlockedFx, __brain.BB.children.blockingFxAttachPoint.position, Quaternion.identity, 0.8f * Vector3.one, 1f)).AddTo(this);
                SoundManager.Instance.PlayWithClip(__brain.BB.resource.onBlockedAudioClip);

                ShowHitColor(__brain.shieldHitColliderHelper);
            }
            else if (damageContext.actionResult == ActionResults.GuardBreak)
            {
                Observable.NextFrame(FrameCountType.EndOfFrame).Subscribe(_ => EffectManager.Instance.Show(__brain.BB.resource.onGuardBreakFx, __brain.BB.children.blockingFxAttachPoint.position, Quaternion.identity, Vector3.one, 1f)).AddTo(this);
                SoundManager.Instance.PlayWithClip(__brain.BB.resource.onGuardBreakAudioClip);
            }

            return base.StartOnHitAction(ref damageContext, isAddictiveAction);
        }

        public override IDisposable StartOnKnockDownAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.receiverBrain == __brain);

            if (damageContext.actionResult == ActionResults.Damaged)
            {
                EffectManager.Instance.Show(__brain.BB.resource.onBigHitFx, __brain.bodyHitColliderHelper.GetWorldCenter(), Quaternion.LookRotation(damageContext.hitPoint - __brain.bodyHitColliderHelper.GetWorldCenter()) * Quaternion.Euler(90f, 0f, 0f), Vector3.one, 1f);
                SoundManager.Instance.PlayWithClip(__brain.BB.resource.onBigHitAudioClip);
                SoundManager.Instance.PlayWithClip(__brain.BB.resource.onHitAudioClip2);
            }

            return base.StartOnKnockDownAction(ref damageContext, isAddictiveAction);
        }
        public override IDisposable StartOnGroogyAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            base.StartOnGroogyAction(ref damageContext, isAddictiveAction);

            SoundManager.Instance.PlayWithClipPos(__brain.BB.resource.onEnterGroggy, __brain.bodyHitColliderHelper.GetWorldCenter());

            return null;
        }

        void ShowHitColor(PawnColliderHelper hitColliderHelper)
        {
            if (hitColliderHelper == __brain.bodyHitColliderHelper)
            {
                foreach (var r in __brain.BB.children.bodyMeshRenderers)
                    r.materials = new Material[] { r.material, new(__brain.BB.resource.hitColor) };

                __hitColorDisposable?.Dispose();
                __hitColorDisposable = Observable.Timer(TimeSpan.FromMilliseconds(100)).Subscribe(_ =>
                {
                    __hitColorDisposable = null;
                    foreach (var r in __brain.BB.children.bodyMeshRenderers)
                        r.materials = new Material[] { r.materials[0] };
                }).AddTo(this);
            }
            else if (hitColliderHelper == __brain.shieldHitColliderHelper)
            {
                __brain.BB.children.shieldMeshRenderer.materials = new Material[] { __brain.BB.children.shieldMeshRenderer.material, new(__brain.BB.resource.hitColor) };

                __hitColorDisposable?.Dispose();
                __hitColorDisposable = Observable.Timer(TimeSpan.FromMilliseconds(100)).Subscribe(_ =>
                {
                    __hitColorDisposable = null;
                    __brain.BB.children.shieldMeshRenderer.materials = new Material[] { __brain.BB.children.shieldMeshRenderer.material };
                }).AddTo(this);
            }
            else
            {
                Debug.Assert(false);
            }
        }

        readonly RaycastHit[] __hitsNonAlloc = new RaycastHit[16];
        IDisposable __hitColorDisposable;
        HumanBotBrain __brain;

        protected override void AwakeInternal()
        {
            base.AwakeInternal();

            __brain = GetComponent<HumanBotBrain>();
        }

        protected override void StartInternal()
        {
            base.StartInternal();

            onHomingRotationStarted += (_) =>
            {
                if (currActionContext.actionName == "Leap")
                {
                    var homingDecalFx = GameObject.Instantiate(__brain.BB.resource.onHomingDecalFx);
                    homingDecalFx.transform.position = __brain.BB.TargetBrain.GetWorldPosition() + 0.1f * Vector3.up;
                    
                    Observable.EveryUpdate().TakeWhile(_ => CheckActionRunning())
                        .DoOnCancel(() => Destroy(homingDecalFx))
                        .DoOnCompleted(() => Destroy(homingDecalFx))
                        .Subscribe(_ =>
                        {
                            if (currActionContext.homingRotationDisposable != null)
                            {
                                homingDecalFx.transform.position = __brain.BB.TargetBrain.GetWorldPosition() + 0.1f * Vector3.up;
                                __brain.BB.action.leapRootMotionMultiplier = (__brain.BB.TargetBrain.GetWorldPosition() - __brain.GetWorldPosition()).Magnitude2D() / __brain.BB.action.leapRootMotionDistance;
                            }
                        }).AddTo(this);
                }
            };
        }
    }
}