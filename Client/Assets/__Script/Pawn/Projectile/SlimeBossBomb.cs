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
        public SlimeMiniBrain miniBrain;
        public ActionData actionData;

        protected override void StartInternal()
        {
            base.StartInternal();

            emitterBrain.Where(v => v != null).Subscribe(v => 
            {
                miniBrain = v.GetComponent<SlimeMiniBrain>();
                actionData = miniBrain.ActionCtrler.currActionContext.actionData;
            }).AddTo(this);

            //* Terrain 디텍션 코드
            bodyCollider.OnCollisionEnterAsObservable().Subscribe(c =>
            {
                if ((c.gameObject.layer & LayerMask.NameToLayer("Terrain")) > 0)
                    __isGrounded = true;
            }).AddTo(this);

            sensorCollider.OnTriggerEnterAsObservable().Subscribe(t => 
            {
                if (t.gameObject != __rigidBody.gameObject)
                    Explode();
            }).AddTo(this);

            onLifeTimeOut += () => { Explode(); };
        }

        bool __isGrounded;

        void Explode()
        {
            Debug.Assert(base.emitterBrain.Value != null);

            foreach (var c in Physics.OverlapSphere(__rigidBody.transform.position, explosionRange, LayerMask.GetMask("HitBox")))
            {
                if (c == null || c == base.emitterBrain.Value)
                    continue;

                if (c.TryGetComponent<PawnColliderHelper>(out var hitColliderHelper) && hitColliderHelper.pawnBrain != null && 
                    hitColliderHelper.pawnBrain != miniBrain && hitColliderHelper.pawnBrain.PawnBB.common.pawnName == "Hero")
                    miniBrain.PawnHP.Send(new PawnHeartPointDispatcher.DamageContext(this, base.emitterBrain.Value, hitColliderHelper.pawnBrain, actionData, hitColliderHelper.pawnCollider, false));
            }

            EffectManager.Instance.Show("CFXR Explosion 2", bodyCollider.transform.position + Vector3.up * 0.2f, Quaternion.identity, 0.5f * Vector3.one);

            Stop(true);
        }

        protected override void OnUpdateHandler()
        {
            base.OnUpdateHandler();

            if ((Time.time - __moveStartTimeStamp) > sensorEnabledTime && !bodyCollider.enabled)
                bodyCollider.enabled = true;

            if (!IsDespawnPending && bodyCollider.enabled && __isGrounded && __rigidBody.linearVelocity.sqrMagnitude < 0.1f)
                Explode();
        }
    }
}