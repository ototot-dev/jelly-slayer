using System;
using UniRx;
using UnityEngine;

namespace Game
{
    public class DroneBotActionController : PawnActionController
    {
        [Header("Component")]
        public CapsuleCollider hitBox;
        public Transform emitPointL;
        public Transform emitPointR;
        public ParticleSystem plasmaExplosionFx;
        public ParticleSystem smallExplisionFx;
        public ParticleSystem smokeFx;

        public override bool CanRootMotion(Vector3 rootMotionVec)
        {
            Debug.Assert(CheckActionRunning());

            if (!base.CanRootMotion(rootMotionVec))
                return false;

            if (__brain.BB.TargetBrain != null && __brain.SensorCtrler.TouchingColliders.Contains(__brain.BB.TargetBrain.coreColliderHelper.pawnCollider))
            {
                //* RootMotion으로 목표물을 밀지 않도록 목묘물의 TouchingColliders와 접축할 정도로 가깝다면 rootMotionVec가 목표물에서 멀어지는 방향일때만 적용해준다.
                var newDistance = (__brain.BB.TargetBrain.coreColliderHelper.transform.position - __brain.coreColliderHelper.transform.position + rootMotionVec).Vector2D().sqrMagnitude;
                return newDistance > (__brain.BB.TargetBrain.coreColliderHelper.transform.position - __brain.coreColliderHelper.transform.position).Vector2D().sqrMagnitude;
            }
            else
            {
                return true;
            }
        }
        
        public override IDisposable StartOnHitAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.receiverBrain == __brain);

            var knockBackVec = damageContext.senderActionData.knockBackDistance / 0.2f * damageContext.senderBrain.coreColliderHelper.transform.forward.Vector2D().normalized;

            if (damageContext.actionResult == ActionResults.Damaged)
            {
                var hitVec = damageContext.senderBrain.coreColliderHelper.transform.position - damageContext.receiverBrain.coreColliderHelper.transform.position;
                hitVec = damageContext.receiverBrain.coreColliderHelper.transform.InverseTransformDirection(hitVec).Vector2D().normalized;

                __brain.AnimCtrler.mainAnimator.SetTrigger("OnHit");

                if (hitVec.x < 0f)
                {
                    __brain.AnimCtrler.mainAnimator.SetFloat("HitX", hitVec.x);
                }
                else
                {
                    var hitX = UnityEngine.Random.Range(0, 3);
                    __brain.AnimCtrler.mainAnimator.SetFloat("HitX", hitX);
                }

                SoundManager.Instance.Play(SoundID.HIT_FLESH);
                EffectManager.Instance.Show("@Hit 23 cube", damageContext.hitPoint, Quaternion.identity, Vector3.one, 1);
                EffectManager.Instance.Show("@BloodFX_impact_col", damageContext.hitPoint, Quaternion.identity, 1.5f * Vector3.one, 3);
            }

            var knockBackDisposable = Observable.EveryUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(0.2f)))
                .Subscribe(_ => __brain.Movement.AddRootMotion(Time.deltaTime * knockBackVec, Quaternion.identity))
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

        public override IDisposable StartOnParriedAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.senderBrain == __brain);
            Debug.Assert(!isAddictiveAction);

            var knockBackVec = damageContext.senderActionData.knockBackDistance / 0.2f * -damageContext.senderBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
            Observable.EveryUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(0.2f)))
                .Subscribe(_ => __brain.Movement.AddRootMotion(Time.deltaTime * knockBackVec, Quaternion.identity))
                .AddTo(this);

            return Observable.Timer(TimeSpan.FromSeconds(damageContext.receiverPenalty.Item2))
                .DoOnCancel(() =>
                {
                    if (CurrActionName == "!OnParried")
                        FinishAction();
                })
                .Subscribe(_ =>
                {
                    if (CurrActionName == "!OnParried")
                        FinishAction();
                }).AddTo(this);
        }

        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            __brain = GetComponent<DroneBotBrain>();
        }

        DroneBotBrain __brain;

        protected override void StartInternal()
        {
            base.StartInternal();

            __brain.PawnHP.onDamaged += (damageContext) =>
            {
                if (damageContext.senderBrain == this && CheckActionRunning() && CurrActionName == "Bumping")
                    currActionContext.rootMotionEnabled = false;
            };
        }

        public override void EmitProjectile(GameObject emitSource, Transform emitPoint, int emitNum)
        {
            if (Instantiate(emitSource, emitPoint.position, emitPoint.rotation).TryGetComponent<DroneBotBullet>(out var bullet))
                bullet.Go(__brain, 16f);

            Observable.Interval(TimeSpan.FromSeconds(0.2888f)).Take(emitNum - 1).Subscribe(_ =>
            {
                if (Instantiate(emitSource, emitPoint.position, emitPoint.rotation).TryGetComponent<DroneBotBullet>(out var bullet))
                    bullet.Go(__brain, 16f);
            }).AddTo(this);

            //* onEmitProjectile 호출은 제일 나중에 함
            base.EmitProjectile(emitSource, emitPoint, emitNum);
        }
    }
}  