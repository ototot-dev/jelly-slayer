using System.Linq;
using MainTable;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Game
{
    public class SlimeBossBomb : ProjectileMovement
    {
        [Header("Parameter")]
        public float brakePower = 1f;
        public float brakeEnabledTime = 0.1f;
        public float explosionRange = 1f;
        public SlimeMiniBrain emitterBrain;
        public ActionData actionData;

        protected override void StartInternal()
        {
            base.StartInternal();

            emitter.Where(v => v != null).Subscribe(v => 
            {
                emitterBrain = v.GetComponent<SlimeMiniBrain>();
                actionData = emitterBrain.ActionCtrler.currActionContext.actionData;
            }).AddTo(this);

            //* Terrain 디텍션 코드
            rigidBodyCollider.OnCollisionEnterAsObservable().Subscribe(c =>
            {
                if ((c.gameObject.layer & LayerMask.NameToLayer("Terrain")) > 0)
                    __isGrounded = true;
            }).AddTo(this);

            sensorCollider.OnTriggerEnterAsObservable().Subscribe(t => 
            {
                if (t.gameObject != rigidBody.gameObject)
                    Explode();
            }).AddTo(this);

            onLifeTimeOut += () => { Explode(); };
        }

        bool __isGrounded;

        void Explode()
        {
            Debug.Assert(emitter.Value != null);

            foreach (var c in Physics.OverlapSphere(rigidBody.transform.position, explosionRange, LayerMask.GetMask("HitBox")))
            {
                if (c == null || c == emitter.Value)
                    continue;

                if (c.TryGetComponent<PawnColliderHelper>(out var hitColliderHelper) && hitColliderHelper.pawnBrain != null && hitColliderHelper.pawnBrain != emitterBrain && hitColliderHelper.pawnBrain.PawnBB.common.pawnName == "Hero")
                    emitterBrain.PawnHP.Send(new PawnHeartPointDispatcher.DamageContext(this, emitter.Value, hitColliderHelper.pawnBrain, actionData, hitColliderHelper.pawnCollider, false));
            }

            EffectManager.Instance.Show("CFXR Explosion 2", rigidBodyCollider.transform.position + Vector3.up * 0.2f, Quaternion.identity, 0.5f * Vector3.one);

            Stop(true);
        }

        protected override void OnUpdateHandler()
        {
            base.OnUpdateHandler();

            if ((Time.time - __moveStartTimeStamp) > sensorEnabledTime && !rigidBodyCollider.enabled)
                rigidBodyCollider.enabled = true;

            if (!IsPendingDestroy && rigidBodyCollider.enabled && __isGrounded && rigidBody.linearVelocity.sqrMagnitude < 0.1f)
                Explode();
        }
    }
}