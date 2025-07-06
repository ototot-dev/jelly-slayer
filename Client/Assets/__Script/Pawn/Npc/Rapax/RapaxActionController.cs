using System;
using UniRx;
using UnityEngine;
using ZLinq;

namespace Game
{
    public class RapaxActionController : NpcHumanoidActionController
    {
        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            __brain = GetComponent<RapaxBrain>();
        }

        IDisposable __hitColorDisposable;
        RapaxBrain __brain;

        void ShowHitColor(PawnColliderHelper hitColliderHelper)
        {
            Debug.Assert(hitColliderHelper == __brain.bodyHitColliderHelper);

            foreach (var r in __brain.BB.children.hitColorRenderers)
                r.materials = new Material[] { r.material, new(__brain.BB.resource.hitColor) };

            __hitColorDisposable?.Dispose();
            __hitColorDisposable = Observable.Timer(TimeSpan.FromMilliseconds(100)).Subscribe(_ =>
            {
                __hitColorDisposable = null;
                foreach (var r in __brain.BB.children.hitColorRenderers)
                    r.materials = new Material[] { r.materials[0] };
            }).AddTo(this);
        }

        public override IDisposable StartOnHitAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.receiverBrain == __brain);

            if (damageContext.actionResult == ActionResults.Damaged)
            {
                EffectManager.Instance.Show(__brain.BB.resource.onHitFx, __brain.bodyHitColliderHelper.GetWorldCenter(), Quaternion.LookRotation(damageContext.hitPoint - __brain.bodyHitColliderHelper.GetWorldCenter()) * Quaternion.Euler(90f, 0f, 0f), Vector3.one, 1f);
                SoundManager.Instance.PlayWithClip(__brain.BB.resource.onHitAudioClip);
                SoundManager.Instance.PlayWithClip(__brain.BB.resource.onHitAudioClip2);

                ShowHitColor(__brain.bodyHitColliderHelper);
            }
            else if (damageContext.actionResult == ActionResults.Missed)
            {
                Observable.NextFrame(FrameCountType.EndOfFrame).Subscribe(_ => EffectManager.Instance.Show(__brain.BB.resource.onMissedFx, __brain.bodyHitColliderHelper.GetWorldCenter(), Quaternion.identity, 0.8f * Vector3.one, 1f)).AddTo(this);
                SoundManager.Instance.PlayWithClip(__brain.BB.resource.onMissedAudioClip);
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
                var __knockBackVec = __brain.BB.body.knockDownImpulse.x * -__brain.coreColliderHelper.transform.forward.Vector2D().normalized + __brain.BB.body.knockDownImpulse.y * Vector3.up;

                Observable.EveryFixedUpdate().Take(2).DoOnCompleted(() =>
                {
                    if (__brain.AnimCtrler.ragdollAnimator.Handler.AnimatingMode != FIMSpace.FProceduralAnimation.RagdollHandler.EAnimatingMode.Falling)
                        __Logger.WarningR2(gameObject, nameof(StartOnKnockDownAction), "ragdollAnimator.Handler.AnimatingMode is invalid.", "AnimatingMode", __brain.AnimCtrler.ragdollAnimator.Handler.AnimatingMode);

                    var spineRigidBody = __brain.AnimCtrler.ragdollAnimator.Handler.Dummy_Container.Descendants().First(d => d.name == "Rapax_ Spine1").GetComponent<Rigidbody>();
                    spineRigidBody.AddForce(20f * __knockBackVec, ForceMode.VelocityChange);
                }).Subscribe().AddTo(this);

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
    }
}