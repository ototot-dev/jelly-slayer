using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using ZLinq;

namespace Game
{
    public class TherionideActionController : NpcHumanoidActionController
    {
        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            __brain = GetComponent<TherionideBrain>();
        }

        IDisposable __hitColorDisposable;
        TherionideBrain __brain;

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

        public override void EmitActionHandler(GameObject emitPrefab, Transform emitPoint, int emitIndex)
        {
            Observable.NextFrame(FrameCountType.EndOfFrame).Subscribe(_ =>
            {
                ObjectPoolingSystem.Instance.GetObject<TherionideProjectile>(emitPrefab, emitPoint.position, emitPoint.rotation).Go(__brain, emitPoint.position, __brain.BB.action.shootProjectileSpeed * emitPoint.forward, Vector3.one);
            }).AddTo(this);
        }
    }
}