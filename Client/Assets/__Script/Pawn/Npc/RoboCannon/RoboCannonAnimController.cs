using System;
using System.Collections.Generic;
using System.Linq;
using FIMSpace.BonesStimulation;
using FIMSpace.FProceduralAnimation;
using UniRx;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using ZLinq;

namespace Game
{
    public class RoboCannonAnimController : PawnAnimController
    {
        [Header("Properties")]
        public float rigBlendWeight = 1f;
        public float rigBlendSpeed = 1f;
        public float actionLayerBlendInSpeed = 1f;
        public float actionLayerBlendOutSpeed = 1f;
        public float lowerLayerBlendSpeed = 1f;
        public float legAnimGlueBlendSpeed = 1f;

        //* Animator 레이어 인덱스 값 
        enum LayerIndices : int
        {
            Base = 0,
            Action,
            Addictive,
            Max,
        }

        public override void OnAnimatorMoveHandler()
        {
            if (__brain.ActionCtrler.CheckActionRunning() && __brain.ActionCtrler.CanRootMotion(mainAnimator.deltaPosition))
                __brain.Movement.AddRootMotion(__brain.ActionCtrler.GetRootMotionMultiplier() * mainAnimator.deltaPosition, mainAnimator.deltaRotation, Time.deltaTime);
        }

        public override void OnAnimatorStateEnterHandler(AnimatorStateInfo stateInfo, int layerIndex)
        {
            __runningAnimStateNames.Add(stateInfo.shortNameHash);
            base.OnAnimatorStateEnterHandler(stateInfo, layerIndex);
        }

        public override void OnAniamtorStateExitHandler(AnimatorStateInfo stateInfo, int layerIndex)
        {
            __runningAnimStateNames.Remove(stateInfo.shortNameHash);
            base.OnAniamtorStateExitHandler(stateInfo, layerIndex);
        }

        public override void OnAnimatorFootHandler(bool isRight)
        {
            SoundManager.Instance.PlayWithClipPos(__brain.BB.resource.onFootstepClip, __brain.BB.TargetColliderHelper.GetWorldCenter(), false, true, 0.2f);
        }

        public bool CheckAnimStateRunning(string stateName) => __runningAnimStateNames.Contains(Animator.StringToHash(stateName));

        void Awake()
        {
            __brain = GetComponent<RoboCannonBrain>();
            __bodyRotationCached = GetComponent<RoboCannonBlackboard>().children.bodyBone.transform.localRotation;
            __barrelRotationCached = GetComponent<RoboCannonBlackboard>().children.barrelBone.transform.localRotation;
        }

        RoboCannonBrain __brain;
        Quaternion __bodyRotationCached;
        Quaternion __barrelRotationCached;
        readonly HashSet<int> __runningAnimStateNames = new();

        void Start()
        {
            var hitBoxBlockingColliders = __brain.coreColliderHelper.transform.DescendantsAndSelf().Where(d => d.gameObject.layer == LayerMask.NameToLayer("HitBoxBlocking")).Select(d => d.GetComponent<Collider>());

            //* Ragdoll이 사용하는 PhysicsBody 레이어는 'HitBoxBlocking'과 충돌하는 것을 막음
            foreach (var d in ragdollAnimator.Handler.TargetParentForRagdollDummy.Descendants())
            {
                if (!d.TryGetComponent<Collider>(out var ragdollCollider))
                    continue;

                foreach (var c in hitBoxBlockingColliders)
                {
                    Physics.IgnoreCollision(c, ragdollCollider);
                    __Logger.LogR1(gameObject, "Physics.IgnoreCollision()", "ignoreColliderA", c, "ignoreColliderB", d.GetComponent<Collider>());
                }
            }

            __brain.BB.common.isSpawnFinished.Subscribe(v =>
            {
                if (v)
                    __brain.AnimCtrler.mainAnimator.SetBool("IsSpawnFinished", true);

            }).AddTo(this);

            __brain.BB.common.isDown.Skip(1).Subscribe(v =>
            {
                if (v)
                {
                    if (ragdollAnimator.Handler.AnimatingMode != FIMSpace.FProceduralAnimation.RagdollHandler.EAnimatingMode.Standing)
                        __Logger.WarningR2(gameObject, "BB.common.isDown.Skip(1).Subscribe()", "ragdollAnimator.Handler.AnimatingMode is invalid.", "AnimatingMode", ragdollAnimator.Handler.AnimatingMode);

                    Observable.NextFrame().Subscribe(_ =>
                    {
                        ragdollAnimator.Handler.AnimatingMode = FIMSpace.FProceduralAnimation.RagdollHandler.EAnimatingMode.Falling;
                    }).AddTo(this);
                }
            }).AddTo(this);

            __brain.BB.common.isDead.Skip(1).Subscribe(v =>
            {
                if (v)
                {
                    ragdollAnimator.Handler.GetExtraFeatureHelper<RAF_AutoGetUp>().Enabled = false;

                    Observable.Timer(TimeSpan.FromSeconds(3f)).Subscribe(_ =>
                    {
                        ragdollAnimator.Handler.AnimatingMode = FIMSpace.FProceduralAnimation.RagdollHandler.EAnimatingMode.Falling;
                    }).AddTo(this);
                }
            }).AddTo(this);

            __brain.onUpdate += () =>
            {
                return;

                if (!__brain.BB.IsSpawnFinished)
                    return;

                mainAnimator.SetLayerWeight((int)LayerIndices.Action, __brain.ActionCtrler.GetAdvancedActionLayerWeight(mainAnimator.GetLayerWeight((int)LayerIndices.Action), actionLayerBlendInSpeed, actionLayerBlendOutSpeed, Time.deltaTime));
                mainAnimator.SetLayerWeight((int)LayerIndices.Addictive, 1f);
                mainAnimator.SetBool("IsMoving", __brain.Movement.CurrVelocity.sqrMagnitude > 0 && !__brain.ActionCtrler.CheckKnockBackRunning());
                mainAnimator.SetBool("IsMovingStrafe", __brain.Movement.freezeRotation);
                mainAnimator.SetFloat("MoveSpeed", __brain.Movement.CurrVelocity.magnitude);
                mainAnimator.SetFloat("MoveAnimSpeed", 1f);

                var animMoveVec = __brain.coreColliderHelper.transform.InverseTransformDirection(__brain.Movement.CurrVelocity).Vector2D();

                mainAnimator.SetFloat("MoveX", animMoveVec.x / __brain.Movement.moveSpeed);
                mainAnimator.SetFloat("MoveY", animMoveVec.z / __brain.Movement.moveSpeed);

                if (__brain.BB.IsDown || __brain.BB.IsDead)
                {
                    // eyeAnimator.MinOpenValue = Mathf.Clamp01(eyeAnimator.MinOpenValue - legAnimGlueBlendSpeed * Time.deltaTime);
                    rigSetup.weight = 0f;

                    legAnimator.LegsAnimatorBlend = Mathf.Clamp01(legAnimator.LegsAnimatorBlend - legAnimGlueBlendSpeed * Time.deltaTime);

                    if (legAnimator.LegsAnimatorBlend <= 0f && legAnimator.enabled)
                        legAnimator.enabled = false;

                    legAnimator.User_SetIsMoving(false);
                    legAnimator.User_SetIsGrounded(false);

                    ragdollAnimator.RagdollBlend = 1f;

                    mainAnimator.SetLayerWeight((int)LayerIndices.Action, __brain.ActionCtrler.GetAdvancedActionLayerWeight(mainAnimator.GetLayerWeight((int)LayerIndices.Action), actionLayerBlendInSpeed, actionLayerBlendOutSpeed, Time.deltaTime));
                    mainAnimator.SetBool("IsMoving", false);
                }
                else if (__brain.BB.IsJumping)
                {
                    legAnimator.LegsAnimatorBlend = 0f;
                    legAnimator.User_SetIsMoving(false);
                    legAnimator.User_SetIsGrounded(false);
                }
                else
                {
                    // headAim.weight = 1f;
                    legAnimator.LegsAnimatorBlend = 1f;

                    if (!legAnimator.enabled)
                        legAnimator.enabled = true;

                    legAnimator.MainGlueBlend = 1f;
                    legAnimator.User_SetIsMoving(__brain.Movement.CurrVelocity.sqrMagnitude > 0 && !__brain.ActionCtrler.CheckActionRunning() && !__brain.ActionCtrler.CheckKnockBackRunning());
                    legAnimator.User_SetIsGrounded(__brain.Movement.IsOnGround);
                    ragdollAnimator.RagdollBlend = 0.1f;
                }
            };

            __brain.onLateUpdate += () =>
            {
                mainAnimator.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

                // rigSetup.weight = 1f;

                if (__brain.BB.TargetBrain != null && __brain.BB.TargetBrain.coreColliderHelper != null)
                    __brain.BB.children.lookAtPoint.position = __brain.BB.TargetBrain.coreColliderHelper.GetWorldCenter() + 0.5f * Vector3.up;
                else
                    __brain.BB.children.lookAtPoint.position = __brain.coreColliderHelper.GetWorldCenter() + 1000f * __brain.coreColliderHelper.transform.forward;

                UpdateAimRotation(__brain.BB.children.lookAtPoint.position, __brain.BB.body.aimRotateSpeed);
            };

            __brain.PawnHP.onDead += (_) =>
            {
                mainAnimator.SetInteger("AnimId", UnityEngine.Random.Range(1, 4));
                mainAnimator.SetTrigger("OnDead");
            };
        }

        void UpdateAimRotation(Vector3 targetPoint, float rotateSpeed, float offsetAngle = 0f)
        {
            //* bodyBone은 Yaw 회전
            var deltaAngle = Vector3.SignedAngle(Vector3.forward, __brain.BB.children.bodyBone.transform.InverseTransformPoint(targetPoint).AdjustY(0f), Vector3.up);
            __brain.BB.children.bodyBone.transform.localRotation = Quaternion.Euler(offsetAngle, 0f, 0f) * __bodyRotationCached.LerpRefAngleSpeed(__brain.BB.children.bodyBone.transform.localRotation * Quaternion.Euler(0f, deltaAngle, 0f), rotateSpeed, Time.deltaTime);

            //* barrelBone은 Pitch 회전
            deltaAngle = Vector3.SignedAngle(Vector3.forward, __brain.BB.children.barrelBone.transform.InverseTransformPoint(targetPoint).AdjustX(0f), Vector3.right);
            __brain.BB.children.barrelBone.transform.localRotation = __barrelRotationCached.LerpRefAngleSpeed(__brain.BB.children.barrelBone.transform.localRotation * Quaternion.Euler(deltaAngle, 0f, 0f), rotateSpeed, Time.deltaTime);
        }
    }
}