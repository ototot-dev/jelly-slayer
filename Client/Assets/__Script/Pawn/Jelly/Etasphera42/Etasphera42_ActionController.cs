using System;
using System.Collections;
using System.Collections.Generic;
using FIMSpace.Generating.Rules.Modelling;
using UniRx;
using UnityEngine;
using static FIMSpace.FProceduralAnimation.LegsAnimator;

namespace Game
{
    public class Etasphera42_ActionController : JellyQuadWalkActionController
    {
        [Header("Component")]
        public PawnColliderHelper dashActionColliderHelper;
        public PawnColliderHelper hookingPointColliderHelper;

        public override IDisposable StartOnHitAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.receiverBrain == __brain);

            if (damageContext.actionResult == ActionResults.Damaged)
            {
                var hitColliderHelper = damageContext.hitCollider.GetComponent<PawnColliderHelper>();
                ShowHitColor(hitColliderHelper);

                if (damageContext.senderActionData.actionName.StartsWith("Kick"))
                    EffectManager.Instance.Show(__brain.BB.graphics.onKickHitFx, hitColliderHelper.GetWorldCenter(), Quaternion.identity, Vector3.one, 1f);
                else if (damageContext.senderActionData.actionName.StartsWith("Heavy"))
                    EffectManager.Instance.Show(__brain.BB.graphics.onBigHitFx, hitColliderHelper.GetWorldCenter(), Quaternion.LookRotation(damageContext.hitPoint - __brain.bodyHitColliderHelper.GetWorldCenter()) * Quaternion.Euler(90f, 0f, 0f), Vector3.one, 1f);
                else
                    EffectManager.Instance.Show(__brain.BB.graphics.onHitFx, hitColliderHelper.GetWorldCenter(), Quaternion.LookRotation(damageContext.hitPoint - __brain.bodyHitColliderHelper.GetWorldCenter()) * Quaternion.Euler(90f, 0f, 0f), Vector3.one, 1f);

                SoundManager.Instance.Play(SoundID.HIT_FLESH);
            }
            else if (damageContext.actionResult == ActionResults.Missed)
            {
                SoundManager.Instance.Play(SoundID.HIT_BLOCK);
                // EffectManager.Instance.Show("@Hit 4 yellow arrow", __brain.AnimCtrler.shieldMeshSlot.position, Quaternion.identity, Vector3.one, 1f);
            }
            else if (damageContext.actionResult == ActionResults.Blocked)
            {
                __brain.AnimCtrler.mainAnimator.SetBool("IsGuarding", true);
                __brain.AnimCtrler.mainAnimator.SetTrigger("OnGuard");
                
                Observable.Timer(TimeSpan.FromSeconds(0.5f)).Subscribe(_ => __brain.AnimCtrler.mainAnimator.SetBool("IsGuarding", false)).AddTo(this);
                // Observable.NextFrame(FrameCountType.EndOfFrame).Subscribe(_ => EffectManager.Instance.Show(__brain.BB.graphics.onBlockedFx, GetShieldCenter(), Quaternion.identity, Vector3.one, 1f)).AddTo(this);
                SoundManager.Instance.Play(SoundID.HIT_BLOCK);

            }
            else if (damageContext.actionResult == ActionResults.GuardBreak) 
            {
                // Observable.NextFrame(FrameCountType.EndOfFrame).Subscribe(_ => EffectManager.Instance.Show(__brain.BB.graphics.onGuardBreakFx, GetShieldCenter(), Quaternion.identity, Vector3.one, 1f)).AddTo(this);
                SoundManager.Instance.Play(SoundID.GUARD_BREAK);
            }

            return base.StartOnHitAction(ref damageContext, isAddictiveAction);
        }

        public override IDisposable StartOnKnockDownAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.receiverBrain == __brain);

            if (damageContext.actionResult == ActionResults.Damaged)
            {
                SoundManager.Instance.Play(SoundID.HIT_FLESH);
                EffectManager.Instance.Show("FX/@Hit 23 cube", damageContext.hitPoint, Quaternion.identity, Vector3.one, 1);
                EffectManager.Instance.Show("FX/@BloodFX_impact_col", damageContext.hitPoint, Quaternion.identity, 1.5f * Vector3.one, 3);
            }

            return base.StartOnKnockDownAction(ref damageContext, isAddictiveAction);
        }

        public override IDisposable StartCustomAction(ref PawnHeartPointDispatcher.DamageContext damageContext, string actionName)
        {
            if (actionName == "Jump")
            {
                return Observable.FromCoroutine(JumpActionCoroutine)
                    .DoOnCancel(() => 
                    {
                        currActionContext.actionDisposable = null;
                        __brain.AnimCtrler.legAnimator.enabled = true;
                        __brain.AnimCtrler.legAnimator.LegsAnimatorBlend = 1f;
                        foreach (var s in __brain.AnimCtrler.legBoneSimulators)
                        {
                            s.enabled = false;
                            s.StimulatorAmount = 0f;
                        }
                    })
                    .DoOnCompleted(() => currActionContext.actionDisposable = null)
                    .Subscribe().AddTo(this);
            }
            if (actionName == "LaserA")
            {
                return Observable.FromCoroutine(LaserA_ActionCoroutine)
                    .DoOnCancel(() => 
                    {
                        currActionContext.actionDisposable = null;
                        __brain.BB.attachment.laserA_Renderer.flashFx.Stop();
                        __brain.BB.attachment.laserA_Renderer.hitFx.Stop();
                        __brain.BB.attachment.laserA_Renderer.FadeOut(0.2f);
                    })
                    .DoOnCompleted(() => currActionContext.actionDisposable = null)
                    .Subscribe().AddTo(this);
            }
            else if (actionName == "LaserB")
            {
                __laserB_disposable?.Dispose();
                __laserB_disposable = Observable.FromCoroutine(LaserB_ActionCoroutine)
                    .DoOnCancel(() => 
                    {
                        __laserB_disposable = null;
                        __brain.BB.attachment.laserB_Renderer.flashFx.Stop();
                        __brain.BB.attachment.laserB_Renderer.hitFx.Stop();
                        __brain.BB.attachment.laserB_Renderer.FadeOut(0.2f);
                    })
                    .DoOnCompleted(() => __laserB_disposable = null)
                    .Subscribe().AddTo(this);

                //* 'LaserB'는 독립형 액션으로 다른 액션과 무관하게 독립적으로 동시에 동작할 수 있음
                return null;
            }

            return null;
        }

        IEnumerator JumpActionCoroutine()
        {
            __brain.AnimCtrler.legAnimator.User_AddImpulse(new ImpulseExecutor(0.4f * Vector3.down, Vector3.zero, 0.4f));
            yield return new WaitForSeconds(0.3f);

            __brain.Movement.StartJump(3f);

            yield return Observable.EveryUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(0.1f))).Do(_ =>
            {
                foreach (var s in __brain.AnimCtrler.legBoneSimulators)
                {
                    if (!s.enabled) s.enabled = true;
                    s.StimulatorAmount = Mathf.Clamp01(s.StimulatorAmount + 10f * Time.deltaTime);
                }
            }).ToYieldInstruction();

            while(__brain.Movement.GetCharacterMovement().velocity.y > 0f)
                yield return null;

            __brain.Movement.GetCharacterMovement().velocity.y = -30f;

            while(!__brain.Movement.GetCharacterMovement().isOnGround)
                yield return null;

            __brain.AnimCtrler.legAnimator.User_AddImpulse(new ImpulseExecutor(0.8f * Vector3.down, Vector3.zero, 0.2f));
            EffectManager.Instance.Show(__brain.BB.graphics.onJumpSlamFx1, __brain.coreColliderHelper.transform.position, Quaternion.identity, Vector3.one);
            EffectManager.Instance.Show(__brain.BB.graphics.onJumpSlamFx2, __brain.coreColliderHelper.transform.position, Quaternion.identity, 1.5f * Vector3.one);

            yield return Observable.EveryUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(0.1f))).Do(_ =>
            {
                foreach (var s in __brain.AnimCtrler.legBoneSimulators)
                    s.StimulatorAmount = Mathf.Clamp01(s.StimulatorAmount - 10f * Time.deltaTime);
            }).ToYieldInstruction();

            foreach (var s in __brain.AnimCtrler.legBoneSimulators)
                s.enabled = false;
        }

        IEnumerator LaserA_ActionCoroutine()
        {
            var laserRenderer = __brain.BB.attachment.laserA_Renderer;

            laserRenderer.flashFx.Play();
            laserRenderer.hitFx.Play();

            //* 2초간 차징
            yield return Observable.EveryLateUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(2f)))
                .Do(_ => 
                {
                    laserRenderer.hitFx.transform.position = laserRenderer.transform.position + 0.1f * laserRenderer.transform.forward;
                    __brain.BB.attachment.laserA_aimPoint.position = __brain.BB.TargetColliderHelper.GetWorldCenter() + 0.2f * Vector3.up;
                }).ToYieldInstruction();
                    
            laserRenderer.FadeIn(0.5f, 0.2f);
            yield return new WaitForSeconds(0.2f);

            var hitLayerMask = LayerMask.GetMask("Terrain", "PhysicsBody", "HitBox", "HitBoxBlocking", "Obstacle");
            var hitOffset = 0.1f;
            var waitApproachTimeStamp = Time.time;
            var sendDamageTimeStamp = 0f;

            //* 4초간 목표물 추적
            yield return Observable.EveryLateUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(4f))).Do(_ =>
            {
                if ((Time.time - waitApproachTimeStamp) > 0.2f)
                    __brain.BB.attachment.laserA_aimPoint.position = __brain.BB.attachment.laserA_aimPoint.position.LerpSpeed(__brain.BB.TargetColliderHelper.GetWorldCenter() + 0.2f * Vector3.up, __brain.BB.action.laserA_approachSpeed, Time.deltaTime);

                var hitIndex = LaserActionTraceTarget(laserRenderer.transform.position, laserRenderer.transform.forward, laserRenderer.lineWidth * 0.5f, __brain.BB.action.laserA_sweepDistance, hitLayerMask);
                if (hitIndex >= 0)
                {
                    laserRenderer.hitFx.transform.position = __hitsNonAlloc[hitIndex].point + hitOffset * __hitsNonAlloc[hitIndex].normal;

                    //* 데미지 처리
                    if ((Time.time - sendDamageTimeStamp) > __brain.BB.action.laserA_sendDamageInterval && __hitsNonAlloc[hitIndex].collider.TryGetComponent<PawnColliderHelper>(out var hitColliderHelper) && hitColliderHelper.pawnBrain == __brain.BB.TargetColliderHelper.pawnBrain)
                    {
                        __brain.PawnHP.Send(new PawnHeartPointDispatcher.DamageContext(__brain, hitColliderHelper.pawnBrain, currActionContext.actionData, hitColliderHelper.pawnCollider, false));
                        sendDamageTimeStamp = Time.time;
                    }
                }
                else
                {
                    laserRenderer.hitFx.transform.position = laserRenderer.transform.position + __brain.BB.action.laserA_sweepDistance * laserRenderer.transform.forward;
                }
            }).ToYieldInstruction();

            //* 레이저 굵기 증가
            laserRenderer.FadeIn(1f, 0.4f);
            yield return new WaitForSeconds(0.4f);

            var sweepAlpha = 0f;
            var sweepStartVec = laserRenderer.transform.forward; 
            var sweepEndVec = laserRenderer.transform.worldToLocalMatrix.MultiplyPoint(__brain.BB.TargetCore.position).x > 0f ? laserRenderer.transform.right.Vector2D() : -laserRenderer.transform.right.Vector2D();

            //* 1초간 매우 빠르게 현재 진행 방향으로 스위핑
            yield return Observable.EveryLateUpdate().TakeWhile(_ => sweepAlpha < 1f).Do(_ =>
            {
                var hitIndex = LaserActionTraceTarget(laserRenderer.transform.position, laserRenderer.transform.forward, laserRenderer.lineWidth * 0.5f, __brain.BB.action.laserA_sweepDistance, hitLayerMask);
                if (hitIndex >= 0)
                    laserRenderer.hitFx.transform.position = __hitsNonAlloc[hitIndex].point + hitOffset * __hitsNonAlloc[hitIndex].normal;
                else
                    laserRenderer.hitFx.transform.position = __brain.BB.attachment.laserA_aimPoint.position;

                if (hitIndex >= 0 && __hitsNonAlloc[hitIndex].collider.TryGetComponent<PawnColliderHelper>(out var hitColliderHelper) && hitColliderHelper.pawnBrain == __brain.BB.TargetColliderHelper.pawnBrain)
                {
                    sweepAlpha += 0.02f * Time.deltaTime;

                    //* 데미지 처리
                    if ((Time.time - sendDamageTimeStamp) > __brain.BB.action.laserA_sendDamageInterval)
                    {
                        __brain.PawnHP.Send(new PawnHeartPointDispatcher.DamageContext(__brain, hitColliderHelper.pawnBrain, currActionContext.actionData, hitColliderHelper.pawnCollider, false));
                        sendDamageTimeStamp = Time.time;
                    }
                }
                else
                {
                    sweepAlpha += 2f * Time.deltaTime;
                }

                var sweepVec = Vector3.Slerp(sweepStartVec, sweepEndVec, sweepAlpha);
                __brain.BB.attachment.laserA_aimPoint.position = laserRenderer.transform.position +  __brain.BB.action.laserA_sweepDistance * sweepVec;
            }).ToYieldInstruction();

            //* 레이저 발사 종료
            laserRenderer.flashFx.Stop();
            laserRenderer.hitFx.Stop();
            laserRenderer.FadeOut(0.2f);
            yield return new WaitForSeconds(0.2f);
        }

        IEnumerator LaserB_ActionCoroutine()
        {
            var laserRenderer = __brain.BB.attachment.laserB_Renderer;

            laserRenderer.flashFx.Play();
            laserRenderer.hitFx.Play();

            yield return Observable.EveryLateUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(1f))).Do(_ =>
            {
                laserRenderer.hitFx.transform.position = laserRenderer.transform.position;
            }).ToYieldInstruction();
                    
            laserRenderer.FadeIn(0.2f, 0.2f);
            yield return new WaitForSeconds(0.2f);

            var hitLayerMask = LayerMask.GetMask("Terrain", "PhysicsBody", "HitBox", "HitBoxBlocking", "Obstacle");
            var hitOffset = 0.1f;
            yield return Observable.EveryLateUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(1f))).Do(_ =>
            {
                __brain.BB.attachment.laserA_aimPoint.position = __brain.BB.TargetColliderHelper.GetWorldCenter();

                var hitIndex = LaserActionTraceTarget(laserRenderer.transform.position, laserRenderer.transform.forward, laserRenderer.lineWidth, __brain.BB.action.laserB_maxDistance, hitLayerMask);
                if (hitIndex >= 0)
                {
                    laserRenderer.hitFx.transform.position = __hitsNonAlloc[hitIndex].point + hitOffset * __hitsNonAlloc[hitIndex].normal;
                }
                else
                {
                    laserRenderer.hitFx.transform.position = laserRenderer.transform.position + __brain.BB.action.laserB_maxDistance * laserRenderer.transform.forward;
                }
            }).ToYieldInstruction();

            laserRenderer.flashFx.Stop();
            laserRenderer.hitFx.Stop();
            laserRenderer.FadeOut(0.2f);
            yield return new WaitForSeconds(0.2f);
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

        void ShowHitColor(PawnColliderHelper colliderHelper)
        {
            var hitBoxIndex = Etasphera42_Brain.HitBoxIndices.Max;
            if (colliderHelper.name.EndsWith("_pelves") || colliderHelper.name.EndsWith("_body"))
            {
                hitBoxIndex = Etasphera42_Brain.HitBoxIndices.Body;
                if (!__hitColorRenderers.ContainsKey(Etasphera42_Brain.HitBoxIndices.Body))
                {
                    __hitColorRenderers.Add(Etasphera42_Brain.HitBoxIndices.Body, __brain.BB.attachment.body_meshRenderers);
                    __hitColorDisposables.Add(Etasphera42_Brain.HitBoxIndices.Body, null);
                }
            }
            else if (colliderHelper.name.EndsWith("_1_l"))
            {
                hitBoxIndex = Etasphera42_Brain.HitBoxIndices.LeftLeg1;
                if (!__hitColorRenderers.ContainsKey(Etasphera42_Brain.HitBoxIndices.LeftLeg1))
                {
                    __hitColorRenderers.Add(Etasphera42_Brain.HitBoxIndices.LeftLeg1, __brain.BB.attachment.leftLeg1_meshRenderes);
                    __hitColorDisposables.Add(Etasphera42_Brain.HitBoxIndices.LeftLeg1, null);
                }
            }
            else if (colliderHelper.name.EndsWith("_2_l"))
            {
                hitBoxIndex = Etasphera42_Brain.HitBoxIndices.LeftLeg2;
                if (!__hitColorRenderers.ContainsKey(Etasphera42_Brain.HitBoxIndices.LeftLeg2))
                {
                    __hitColorRenderers.Add(Etasphera42_Brain.HitBoxIndices.LeftLeg2, __brain.BB.attachment.leftLeg2_meshRenderes);
                    __hitColorDisposables.Add(Etasphera42_Brain.HitBoxIndices.LeftLeg2, null);
                }
            }
            else if (colliderHelper.name.EndsWith("_1_r"))
            {
                hitBoxIndex = Etasphera42_Brain.HitBoxIndices.RightLeg1;
                if (!__hitColorRenderers.ContainsKey(Etasphera42_Brain.HitBoxIndices.RightLeg1))
                {
                    __hitColorRenderers.Add(Etasphera42_Brain.HitBoxIndices.RightLeg1, __brain.BB.attachment.rightLeg1_meshRenderes);
                    __hitColorDisposables.Add(Etasphera42_Brain.HitBoxIndices.RightLeg1, null);
                }
            }
            else if (colliderHelper.name.EndsWith("_2_r"))
            {
                hitBoxIndex = Etasphera42_Brain.HitBoxIndices.RightLeg2;
                if (!__hitColorRenderers.ContainsKey(Etasphera42_Brain.HitBoxIndices.RightLeg2))
                {
                    __hitColorRenderers.Add(Etasphera42_Brain.HitBoxIndices.RightLeg2, __brain.BB.attachment.rightLeg2_meshRenderes);
                    __hitColorDisposables.Add(Etasphera42_Brain.HitBoxIndices.RightLeg2, null);
                }
            }
            else
            {
                Debug.Assert(false);
            }

            foreach (var r in __hitColorRenderers[hitBoxIndex])
                r.materials = new Material[] { r.material, new(__brain.BB.graphics.hitColor) };

            __hitColorDisposables[hitBoxIndex]?.Dispose();
            __hitColorDisposables[hitBoxIndex] = Observable.Timer(TimeSpan.FromMilliseconds(100)).Subscribe(_ => 
            {
                __hitColorDisposables[hitBoxIndex] = null;
                foreach (var r in __hitColorRenderers[hitBoxIndex])
                    r.materials = new Material[] { r.materials[0] };
            }).AddTo(this);
        }

        public override void EmitActionHandler(GameObject emitPrefab, Transform emitPoint, int emitNum)
        {
            __nextFrameObservable ??= Observable.NextFrame(FrameCountType.EndOfFrame);
            if (emitPrefab == __brain.BB.action.bulletPrefab)
            {
                __nextFrameObservable.Subscribe(_ =>
                {
                    ObjectPoolingSystem.Instance.GetObject<Etasphera42_Bullet>(emitPrefab, emitPoint.position + UnityEngine.Random.Range(-0.2f, 0.2f) * Vector3.right, emitPoint.rotation).Go(__brain, 20f, 0.5f);
                });
            }
            else if (emitPrefab == __brain.BB.action.framePrefab)
            {
                __nextFrameObservable.Subscribe(_ =>
                {
                    ObjectPoolingSystem.Instance.GetObject<Etasphera42_Frame>(emitPrefab, emitPoint.position + UnityEngine.Random.Range(-0.2f, 0.2f) * Vector3.right, emitPoint.rotation).Go(__brain, 20f, 1f);
                });
            }
            else if (emitPrefab == __brain.BB.action.bombPrefab)
            {
                __nextFrameObservable.Subscribe(_ =>
                {
                    ObjectPoolingSystem.Instance.GetObject<Etasphera42_Bomb>(emitPrefab, emitPoint.position + UnityEngine.Random.Range(-0.2f, 0.2f) * Vector3.right, emitPoint.rotation).Pop(__brain, UnityEngine.Random.Range(2f, 10f) * Vector3.up.RandomX(-1f, 1f).RandomZ(-1f, 1f), Vector3.one);
                });
            }
        }

        readonly Dictionary<Etasphera42_Brain.HitBoxIndices, SkinnedMeshRenderer[]> __hitColorRenderers = new();
        readonly Dictionary<Etasphera42_Brain.HitBoxIndices, IDisposable> __hitColorDisposables = new();
        readonly RaycastHit[] __hitsNonAlloc = new RaycastHit[16];
        IObservable<Unit> __nextFrameObservable;
        IDisposable __laserB_disposable;
        Etasphera42_Brain __brain;

        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            __brain = GetComponent<Etasphera42_Brain>();
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