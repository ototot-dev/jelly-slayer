using System;
using System.Collections.Generic;
using System.Linq;
using FIMSpace.BonesStimulation;
using FIMSpace.FProceduralAnimation;
using UniRx;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using ZLinq;

namespace Game
{
    public class TherionideAnimController : PawnAnimController
    {
        [Header("Component")]
        public Transform headBone;
        public MultiAimConstraint headAim;
        public OverrideTransform leftHandOverride;
        public BonesStimulator leftArmBoneSimulator;
        public BonesStimulator rightArmBoneSimulator;

        [Header("Properties")]
        public float rigBlendWeight = 1f;
        public float rigBlendSpeed = 1f;
        public float actionLayerBlendInSpeed = 1f;
        public float actionLayerBlendOutSpeed = 1f;
        public float lowerLayerBlendSpeed = 1f;
        public float legAnimGlueBlendSpeed = 1f;
        public float armBoneSimulatorBlendSpeed = 1f;
        public float armBoneSimulatorTargetWeight = 0f;

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
            if (IsRootMotionForced())
                __brain.Movement.AddRootMotion(GetForecedRootMotionMultiplier() * mainAnimator.deltaPosition, mainAnimator.deltaRotation, Time.deltaTime);
            else if (__brain.ActionCtrler.CheckActionRunning() && __brain.ActionCtrler.CanRootMotion(mainAnimator.deltaPosition))
                __brain.Movement.AddRootMotion(__brain.ActionCtrler.GetRootMotionMultiplier() * mainAnimator.deltaPosition, mainAnimator.deltaRotation, Time.deltaTime);
        }

        public override void OnAnimatorFootHandler(bool isRight)
        {
            SoundManager.Instance.PlayWithClipPos(__brain.BB.resource.onFootstepClip, __brain.BB.TargetColliderHelper.GetWorldCenter(), false, true, 0.2f);
        }

        void Awake()
        {
            __brain = GetComponent<TherionideBrain>();
        }

        TherionideBrain __brain;
        Vector3[] __8waysBlendTreePosXY;

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
                        __Logger.LogR1(gameObject, "Physics.IgnoreCollision()", "ignoreCollider", c, "ragdollCollider", ragdollCollider);
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

            __brain.StatusCtrler.onStatusActive += (status) =>
            {
                if (status == PawnStatus.Staggered && __brain.StatusCtrler.GetStrength(PawnStatus.Staggered) > 0f)
                {
                    leftArmBoneSimulator.GravityEffectForce = rightArmBoneSimulator.GravityEffectForce = 9.8f * Vector3.down;
                    leftArmBoneSimulator.GravityHeavyness = 4f;
                    leftArmBoneSimulator.StimulatorAmount = 1f;
                    armBoneSimulatorTargetWeight = 1f;
                }
            };

            __brain.StatusCtrler.onStatusDeactive += (buff) =>
            {
                if (buff == PawnStatus.Staggered && !__brain.StatusCtrler.CheckStatus(PawnStatus.Staggered))
                    armBoneSimulatorTargetWeight = 0f;
            };

            __brain.onUpdate += () =>
            {
                if (!__brain.BB.IsSpawnFinished)
                    return;

                mainAnimator.SetLayerWeight((int)LayerIndices.Action, __brain.ActionCtrler.GetAdvancedActionLayerWeight(mainAnimator.GetLayerWeight((int)LayerIndices.Action), actionLayerBlendInSpeed, actionLayerBlendOutSpeed, Time.deltaTime));
                mainAnimator.SetLayerWeight((int)LayerIndices.Addictive, 1f);
                mainAnimator.SetBool("IsMoving", __brain.Movement.CurrVelocity.sqrMagnitude > 0 && !__brain.ActionCtrler.CheckKnockBackRunning());
                mainAnimator.SetBool("IsMovingStrafe", __brain.Movement.freezeRotation);
                mainAnimator.SetFloat("MoveSpeed", __brain.Movement.CurrVelocity.magnitude);
                mainAnimator.SetFloat("MoveAnimSpeed", 1f);

                 __8waysBlendTreePosXY ??= new Vector3[]
                {
                    new(0f, 0f, 1f),
                    new(0f, 0f, -1f),
                    new(-1f, 0f, 1f),
                    new(-1f, 0f, 0f),
                    new(-1f, 0f, -1f),
                    new(1f, 0f, 1f),
                    new(1f, 0f, 0f),
                    new(1f, 0f, -1f),
                };

                var animMoveVec = __brain.coreColliderHelper.transform.InverseTransformDirection(__brain.Movement.CurrVelocity).Vector2D();
                var animMoveVecClamped = __8waysBlendTreePosXY.AsValueEnumerable().OrderBy(v => Vector3.Angle(v, animMoveVec)).First();

                // animMoveVec = animMoveVec.magnitude * Vector3.Lerp(animMoveVec.normalized, animMoveVecClamped.normalized, 0.2f);
                mainAnimator.SetFloat("MoveX", animMoveVec.x / __brain.Movement.moveSpeed);
                mainAnimator.SetFloat("MoveY", animMoveVec.z / __brain.Movement.moveSpeed);

                if (__brain.ActionCtrler.CheckActionRunning())
                    armBoneSimulatorTargetWeight = leftArmBoneSimulator.StimulatorAmount = rightArmBoneSimulator.StimulatorAmount = 0f;
                else
                    armBoneSimulatorTargetWeight = 0f;
                    
                if (armBoneSimulatorTargetWeight > 0f)
                {
                    if (!leftArmBoneSimulator.enabled) leftArmBoneSimulator.enabled = true;
                    if (!rightArmBoneSimulator.enabled) rightArmBoneSimulator.enabled = true;
                    leftArmBoneSimulator.StimulatorAmount = Mathf.Clamp01(leftArmBoneSimulator.StimulatorAmount + armBoneSimulatorBlendSpeed * Time.deltaTime);
                    rightArmBoneSimulator.StimulatorAmount = Mathf.Clamp01(rightArmBoneSimulator.StimulatorAmount + armBoneSimulatorBlendSpeed * Time.deltaTime);
                }
                else
                {
                    leftArmBoneSimulator.StimulatorAmount = Mathf.Clamp01(leftArmBoneSimulator.StimulatorAmount - armBoneSimulatorBlendSpeed * Time.deltaTime);
                    rightArmBoneSimulator.StimulatorAmount = Mathf.Clamp01(rightArmBoneSimulator.StimulatorAmount - armBoneSimulatorBlendSpeed * Time.deltaTime);

                    if (leftArmBoneSimulator.enabled && leftArmBoneSimulator.StimulatorAmount <= 0f)
                        leftArmBoneSimulator.enabled = false;
                    if (rightArmBoneSimulator.enabled && rightArmBoneSimulator.StimulatorAmount <= 0f)
                        rightArmBoneSimulator.enabled = false;
                }

                if (__brain.BB.IsDown || __brain.BB.IsDead)
                {
                    // eyeAnimator.MinOpenValue = Mathf.Clamp01(eyeAnimator.MinOpenValue - legAnimGlueBlendSpeed * Time.deltaTime);

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
                    legAnimator.LegsAnimatorBlend = 1f;

                    if (!legAnimator.enabled)
                        legAnimator.enabled = true;

                    legAnimator.MainGlueBlend = 1f;
                    legAnimator.User_SetIsMoving(__brain.Movement.CurrVelocity.sqrMagnitude > 0 && !__brain.ActionCtrler.CheckActionRunning() && !__brain.ActionCtrler.CheckKnockBackRunning());
                    legAnimator.User_SetIsGrounded(__brain.Movement.IsOnGround);
                    ragdollAnimator.RagdollBlend = 0.01f;
                }
            };

            __brain.onLateUpdate += () =>
            {
                mainAnimator.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

                // rigSetup.weight = 0f;
                // headAim.weight = 0f;

                if (__brain.BB.TargetBrain != null && __brain.BB.TargetBrain.coreColliderHelper != null)
                    __brain.BB.children.lookAtPoint.position = __brain.BB.TargetBrain.coreColliderHelper.GetWorldCenter() + 0.5f * Vector3.up;
                else
                    __brain.BB.children.lookAtPoint.position = __brain.coreColliderHelper.GetWorldCenter() + 1000f * __brain.coreColliderHelper.transform.forward;
            };

            __brain.PawnHP.onDead += (_) =>
            {
                mainAnimator.SetInteger("AnimId", UnityEngine.Random.Range(1, 4));
                mainAnimator.SetTrigger("OnDead");
            };
        }
    }
}