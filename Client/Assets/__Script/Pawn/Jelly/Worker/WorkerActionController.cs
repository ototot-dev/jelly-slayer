using System;
using System.Linq;
using UniRx;
using UnityEngine;
using XftWeapon;

namespace Game
{
    public class WorkerActionController : PawnActionController
    {
        [Header("Component")]
        public Transform counterActionCollider;
        public XWeaponTrail sworldWeaponTrailA;
        public XWeaponTrail sworldWeaponTrailB;
        
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
                SoundManager.Instance.Play(SoundID.HIT_FLESH);
                EffectManager.Instance.Show("@Hit 23 cube", damageContext.hitPoint, Quaternion.identity, Vector3.one, 1);
                EffectManager.Instance.Show("@BloodFX_impact_col", damageContext.hitPoint, Quaternion.identity, 1.5f * Vector3.one, 3);
            }
            else if (damageContext.actionResult == ActionResults.Missed || damageContext.actionResult == ActionResults.Blocked)
            {
                SoundManager.Instance.Play(SoundID.HIT_BLOCK);
                EffectManager.Instance.Show("@Hit 4 yellow arrow", __brain.AnimCtrler.shieldMeshSlot.position, Quaternion.identity, Vector3.one, 1f);
                EffectManager.Instance.Show("BlockAttack", __brain.AnimCtrler.shieldMeshSlot.position, Quaternion.identity, Vector3.one, 1f);
            }
            else if (damageContext.actionResult == ActionResults.GuardBreak) 
            {
                SoundManager.Instance.Play(SoundID.GUARD_BREAK);
                EffectManager.Instance.Show("SwordHitRed", __brain.AnimCtrler.shieldMeshSlot.position, Quaternion.identity, Vector3.one, 1f);
            }

            return base.StartOnHitAction(ref damageContext, isAddictiveAction);
        }

        public override IDisposable StartOnKnockDownAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.receiverBrain == __brain);

            if (damageContext.actionResult == ActionResults.Damaged)
            {
                SoundManager.Instance.Play(SoundID.HIT_FLESH);
                EffectManager.Instance.Show("@Hit 23 cube", damageContext.hitPoint, Quaternion.identity, Vector3.one, 1);
                EffectManager.Instance.Show("@BloodFX_impact_col", damageContext.hitPoint, Quaternion.identity, 1.5f * Vector3.one, 3);
            }

            return base.StartOnKnockDownAction(ref damageContext, isAddictiveAction);
        }

        WorkerBrain __brain;

        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            __brain = GetComponent<WorkerBrain>();
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