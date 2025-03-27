using System;
using System.Collections;
using UniRx;
using UnityEngine;

namespace Game
{
    public class SoldierActionController : JellyHumanoidActionController
    {
        public override bool CheckAddictiveActionRunning(string actionName) => __laserDisposable != null && actionName == "Laser";

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
                            EffectManager.Instance.Show(__brain.BB.graphics.onBleedingFx, __brain.bodyHitColliderHelper.GetWorldCenter(), Quaternion.LookRotation(__brain.coreColliderHelper.transform.forward), 1.5f * Vector3.one)
                                .transform.SetParent(__brain.bodyHitColliderHelper.transform, true);
                        }).AddTo(this);
                    }

                    if (damageContext.senderActionData.actionName == "Assault")
                    {
                        __brain.jellyMeshCtrler.springMassSystem.coreAttachPoint.parent.position = __brain.coreColliderHelper.GetWorldCenter();
                        __brain.jellyMeshCtrler.FadeIn(0.5f);
                        __brain.jellyMeshCtrler.StartHook();
                    }
                    else
                    {
                        __brain.jellyMeshCtrler.ShowHitColor(0.1f);
                        // SoundManager.Instance.PlayWithClip(__brain.BB.audios.onHitFleshClip);
                    }

                    SoundManager.Instance.PlayWithClip(__brain.BB.audios.onHitAudioClip);
                    SoundManager.Instance.PlayWithClip(__brain.BB.audios.onHitAudioClip2);
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
                    EffectManager.Instance.Show(__brain.BB.graphics.onHitFx, __brain.bodyHitColliderHelper.GetWorldCenter(), Quaternion.LookRotation(damageContext.hitPoint - __brain.bodyHitColliderHelper.GetWorldCenter()) * Quaternion.Euler(90f, 0f, 0f), Vector3.one, 1f);
                    SoundManager.Instance.PlayWithClip(__brain.BB.audios.onHitAudioClip);
                    SoundManager.Instance.PlayWithClip(__brain.BB.audios.onHitAudioClip2);
                }

                ShowHitColor(__brain.bodyHitColliderHelper);
            }
            else if (damageContext.actionResult == ActionResults.Missed)
            {
                Observable.NextFrame(FrameCountType.EndOfFrame).Subscribe(_ => EffectManager.Instance.Show(__brain.BB.graphics.onMissedFx, __brain.BB.attachment.blockingFxAttachPoint.position, Quaternion.identity, 0.8f * Vector3.one, 1f)).AddTo(this);
                SoundManager.Instance.PlayWithClip(__brain.BB.audios.onMissedAudioClip);
            }
            else if (damageContext.actionResult == ActionResults.Blocked)
            {
                __brain.AnimCtrler.mainAnimator.SetBool("IsGuarding", true);
                __brain.AnimCtrler.mainAnimator.SetTrigger("OnGuard");

                Observable.Timer(TimeSpan.FromSeconds(0.5f)).Subscribe(_ => __brain.AnimCtrler.mainAnimator.SetBool("IsGuarding", false)).AddTo(this);
                Observable.NextFrame(FrameCountType.EndOfFrame).Subscribe(_ => EffectManager.Instance.Show(__brain.BB.graphics.onBlockedFx, __brain.BB.attachment.blockingFxAttachPoint.position, Quaternion.identity, 0.8f * Vector3.one, 1f)).AddTo(this);
                SoundManager.Instance.PlayWithClip(__brain.BB.audios.onBlockedAudioClip);

                ShowHitColor(__brain.shieldHitColliderHelper);
            }
            else if (damageContext.actionResult == ActionResults.GuardBreak)
            {
                Observable.NextFrame(FrameCountType.EndOfFrame).Subscribe(_ => EffectManager.Instance.Show(__brain.BB.graphics.onGuardBreakFx, __brain.BB.attachment.blockingFxAttachPoint.position, Quaternion.identity, Vector3.one, 1f)).AddTo(this);
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
                SoundManager.Instance.PlayWithClip(__brain.BB.audios.onHitAudioClip2);
            }

            return base.StartOnKnockDownAction(ref damageContext, isAddictiveAction);
        }
        public override IDisposable StartOnGroogyAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            base.StartOnGroogyAction(ref damageContext, isAddictiveAction);

            SoundManager.Instance.PlayWithClipPos(__brain.BB.audios.onEnterGroggy, __brain.bodyHitColliderHelper.GetWorldCenter());

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

        public override void EmitActionHandler(GameObject emitPrefab, Transform emitPoint, int emitNum)
        {
            var hoveringDuration = __brain.ActionDataSelector.CurrSequence() == __brain.ActionDataSelector.GetSequence(SoldierBrain.ActionPatterns.Leap) ?
                __brain.BB.action.hoveringDurationB : __brain.BB.action.hoveringDurationA;

            if (emitPrefab == __brain.BB.MissilePrefab)
            {
                var missile = ObjectPoolingSystem.Instance.GetObject<SoldierMissile>(emitPrefab, emitPoint.position, Quaternion.LookRotation(Vector3.up));
                missile.hoveringDuration = hoveringDuration;
                missile.Go(__brain, __brain.BB.action.missileEmitSpeed);

                //* 첫번째 발사와 마지막 발사를 제외한 갯수
                var intervalCount = Math.Max(1, __brain.BB.action.missileEmitNum - 2);
                __missileEmitDisposable?.Dispose();
                __missileEmitDisposable = Observable.Interval(TimeSpan.FromSeconds(__brain.BB.action.missileEmitIntervalA)).Take(intervalCount)
                    .Do(_ =>
                    {
                        var missile = ObjectPoolingSystem.Instance.GetObject<SoldierMissile>(emitPrefab, emitPoint.position, Quaternion.LookRotation(Vector3.up));
                        missile.hoveringDuration = hoveringDuration;
                        missile.Go(__brain, __brain.BB.action.missileEmitSpeed);
                    })
                    .DoOnCompleted(() =>
                    {
                        __missileEmitDisposable = Observable.Timer(TimeSpan.FromSeconds(__brain.BB.action.missileEmitIntervalB))
                            .Subscribe(_ =>
                            {
                                var missile = ObjectPoolingSystem.Instance.GetObject<SoldierMissile>(emitPrefab, emitPoint.position, Quaternion.LookRotation(Vector3.up));
                                missile.hoveringDuration = hoveringDuration;
                                missile.Go(__brain, __brain.BB.action.missileEmitSpeed);
                            })
                            .AddTo(this);
                    }).Subscribe().AddTo(this);
            }
        }

        readonly RaycastHit[] __hitsNonAlloc = new RaycastHit[16];
        IDisposable __laserDisposable;
        IDisposable __missileEmitDisposable;
        IDisposable __hitColorDisposable;
        SoldierBrain __brain;

        public override IDisposable StartCustomAction(ref PawnHeartPointDispatcher.DamageContext damageContext, string actionName, bool isAddictiveAction = false)
        {
            if (actionName == "Laser")
            {
                Debug.Assert(isAddictiveAction);

                __laserDisposable?.Dispose();
                __laserDisposable = Observable.FromCoroutine(LaserActionCoroutine)
                    .DoOnCancel(() =>
                    {
                        __laserDisposable = null;
                        __brain.BB.attachment.laserRenderer.flashFx.Stop();
                        __brain.BB.attachment.laserRenderer.hitFx.Stop();
                        __brain.BB.attachment.laserRenderer.FadeOut(0.2f);
                    })
                    .DoOnCompleted(() => __laserDisposable = null)
                    .Subscribe().AddTo(this);

                //* 'Laser'는 독립형 액션으로 다른 액션과 무관하게 독립적으로 동시에 동작할 수 있음
                return null;
            }

            return null;
        }

        IEnumerator LaserActionCoroutine()
        {
            var laserRenderer = __brain.BB.attachment.laserRenderer;
            laserRenderer.flashFx.Play();

            //* 차징 스텝
            yield return Observable.EveryLateUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(__brain.BB.action.laserCharingDuration))).Do(_ =>
            {
                laserRenderer.hitFx.transform.position = __brain.BB.attachment.laserAimPoint.position = __brain.BB.TargetColliderHelper.GetWorldCenter();
            }).ToYieldInstruction();

            //* 레이저 Fade-In        
            laserRenderer.FadeIn(0.2f, 0.1f);
            yield return new WaitForSeconds(0.1f);

            var hitLayerMask = LayerMask.GetMask("Terrain", "PhysicsBody", "HitBox", "HitBoxBlocking", "Obstacle");
            var hitOffset = 0.1f;
            var sendDamageTimeStamp = 0f;

            //* 레이저 발사
            yield return Observable.EveryLateUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(__brain.BB.action.laserStayDuration))).Do(_ =>
            {
                //* 히트하지 전까지만 목표를 추적함
                if (!__brain.BB.TargetColliderHelper.pawnBrain.PawnBB.IsDown)
                    __brain.BB.attachment.laserAimPoint.position = __brain.BB.TargetColliderHelper.GetWorldCenter();

                var hitIndex = LaserActionTraceTarget(laserRenderer.transform.position, laserRenderer.transform.forward, laserRenderer.lineWidth, __brain.BB.action.laseMaxDistance, hitLayerMask);
                if (hitIndex >= 0 && __hitsNonAlloc[hitIndex].collider.TryGetComponent<PawnColliderHelper>(out var hitColliderHelper) && hitColliderHelper.pawnBrain == __brain.BB.TargetColliderHelper.pawnBrain)
                {
                    //* 데미지 처리 (데미지는 한번만 처리함)
                    if ((Time.time - sendDamageTimeStamp) > __brain.BB.action.laserDamageInterval)
                    {
                        var actionData = DatasheetManager.Instance.GetActionData(PawnId.Soldier, "Laser");
                        Debug.Assert(actionData != null);

                        __brain.PawnHP.Send(new PawnHeartPointDispatcher.DamageContext(__brain, hitColliderHelper.pawnBrain, actionData, currActionContext.specialTag, hitColliderHelper.pawnCollider, false));
                        sendDamageTimeStamp = Time.time;
                    }

                    if (!laserRenderer.hitFx.isPlaying) laserRenderer.hitFx.Play();
                    laserRenderer.hitFx.transform.position = __hitsNonAlloc[hitIndex].point + hitOffset * __hitsNonAlloc[hitIndex].normal;
                }
                else
                {
                    if (laserRenderer.hitFx.isPlaying) laserRenderer.hitFx.Stop();
                    laserRenderer.hitFx.transform.position = laserRenderer.hitFx.transform.position.LerpSpeed(laserRenderer.transform.position + __brain.BB.action.laseMaxDistance * laserRenderer.transform.forward, __brain.BB.action.laserForwardSpeed, Time.deltaTime);
                }
            }).ToYieldInstruction();

            //* 레이저 발사 종료
            laserRenderer.flashFx.Stop();
            laserRenderer.hitFx.Stop();
            laserRenderer.FadeOut(0.1f);

            yield return new WaitForSeconds(0.1f);
        }

        int LaserActionTraceTarget(Vector3 origin, Vector3 direction, float radius, float distance, int layerMask)
        {
            var compareSqrDistance = -1f;
            var hitIndex = -1;
            var hitCount = Physics.SphereCastNonAlloc(origin, radius, direction, __hitsNonAlloc, distance, layerMask);
            if (hitCount > 0)
            {
                for (int i = 0; i < hitCount; i++)
                {
                    if (__hitsNonAlloc[i].collider.TryGetComponent<PawnColliderHelper>(out var helper) && helper.pawnBrain != __brain.BB.TargetColliderHelper.pawnBrain)
                        continue;

                    var sqrDistance = (__hitsNonAlloc[i].collider.transform.position - origin).sqrMagnitude;
                    if (hitIndex < 0 || sqrDistance < compareSqrDistance)
                    {
                        hitIndex = i;
                        compareSqrDistance = sqrDistance;
                    }
                }
            }

            return hitIndex;
        }

        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            __brain = GetComponent<SoldierBrain>();
        }

        protected override void StartInternal()
        {
            base.StartInternal();

            __brain.JellyBB.common.isDown.Skip(1).Subscribe(v =>
            {
                if (v)
                {
                    __pawnAnimCtrler.mainAnimator.SetBool("IsDown", true);
                    __pawnAnimCtrler.mainAnimator.SetTrigger("OnDown");
                }
                else
                {
                    //* 일어나는 모션동안은 무적
                    __humanoidBrain.PawnStatusCtrler.AddStatus(PawnStatus.Invincible, 1f, 1f);
                    __pawnAnimCtrler.mainAnimator.SetBool("IsDown", false);
                    __humanoidBrain.InvalidateDecision(2f);
                }
            }).AddTo(this);

            __brain.JellyBB.common.isGroggy.Skip(1).Where(v => !v).Subscribe(_ =>
            {
                //* 일어나는 모션동안은 무적
                __humanoidBrain.PawnStatusCtrler.AddStatus(PawnStatus.Invincible, 1f, 1f);
                __humanoidBrain.PawnStatusCtrler.AddStatus(PawnStatus.CanNotMove, 1f, 2f);
                __humanoidBrain.InvalidateDecision(2f);

                //* 막타 Hit 애님이 오전히 출력되는 시간 딜레이 
                Observable.Timer(TimeSpan.FromSeconds(1.5f)).Subscribe(_ => __pawnAnimCtrler.mainAnimator.SetBool("IsGroggy", false)).AddTo(this);
            }).AddTo(this);
        }
    }
}