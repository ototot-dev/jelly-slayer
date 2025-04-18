using System;
using UniRx;
using UnityEngine;

namespace Game
{
    public class SlimeMiniActionController : PawnActionController
    {
        [Header("Component")]
        public CapsuleCollider hitBox;
        public Transform emitPoint;
        public float emitSpeed = 1f;
        public float emitPitch = 30f;

        public override bool CanRootMotion(Vector3 rootMotionVec)
        {
            Debug.Assert(CheckActionRunning());

            if (!base.CanRootMotion(rootMotionVec))
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

        public override IDisposable StartOnHitAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.receiverBrain == __brain);
                
            SoundManager.Instance.Play(SoundID.HIT_FLESH);
            EffectManager.Instance.Show("@Hit 23 cube", damageContext.hitPoint, Quaternion.identity, Vector3.one, 1);
            EffectManager.Instance.Show("@BloodFX_impact_col", damageContext.hitPoint, Quaternion.identity, 1.5f * Vector3.one, 3);

            __brain.AnimCtrler.springMass.AddImpulseRandom(5f);

            var knockBackVec = damageContext.senderActionData.knockBackDistance / 0.2f * damageContext.senderBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
            var knockBackDisposable = Observable.EveryUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(0.2f)))
                .Subscribe(_ => __brain.Movement.AddRootMotion(Time.deltaTime * knockBackVec, Quaternion.identity, Time.deltaTime))
                .AddTo(this);
                
            if (isAddictiveAction)
            {
                return knockBackDisposable;
            }
            else
            {
                return Observable.Timer(TimeSpan.FromSeconds(damageContext.receiverPenalty.Item2))
                    .DoOnCancel(() =>
                    {
                        if (CurrActionName == "!OnHit")
                            FinishAction();
                    })
                    .Subscribe(_ =>
                    {
                        if (CurrActionName == "!OnHit")
                            FinishAction();
                    }).AddTo(this);
            }
        }

        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            __brain = GetComponent<SlimeMiniBrain>();
        }

        SlimeMiniBrain __brain;

        protected override void StartInternal()
        {
            base.StartInternal();

            __brain.PawnHP.onDamaged += (damageContext) =>
            {
                if (damageContext.senderBrain == this && CheckActionRunning() && CurrActionName == "Bumping")
                    currActionContext.rootMotionEnabled = false;
            };
        }

        public override void EmitActionHandler(GameObject emitSource, Transform emitPoint, int emitNum)
        {
            if (emitSource.TryGetComponent<SlimeBossBomb>(out var sourceBomb))
            {
                Observable.Interval(TimeSpan.FromSeconds(0.1f)).Take(emitNum).Subscribe(_ =>
                {
                    if (Instantiate(emitSource, emitPoint.position, Quaternion.Euler(-emitPitch, emitPoint.rotation.eulerAngles.y, 0f)).TryGetComponent<SlimeBossBomb>(out var bomb))
                        bomb.Pop(__brain, emitSpeed);
                }).AddTo(this);
            }

            //* onEmitProjectile 호출은 제일 나중에 함
            base.EmitActionHandler(emitSource, emitPoint, emitNum);
        }
    }
}