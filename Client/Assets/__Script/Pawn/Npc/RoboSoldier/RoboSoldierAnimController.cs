using System.Collections.Generic;
using DG.Tweening;
using FIMSpace.BonesStimulation;
using NodeCanvas.Framework.Internal;
using UniRx;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using ZLinq;

namespace Game
{
    public class RoboSoldierAnimController : PawnAnimController
    {
        [Header("Component")]
        public Transform headBone;
        public MultiAimConstraint headAim;
        public OverrideTransform leftHandOverride;
        public BonesStimulator leftArmBoneSimulator;
        public BonesStimulator rightArmBoneSimulator;
        public BonesStimulator leftLegBoneSimulator;
        public BonesStimulator rightLegBoneSimulator;

        [Header("Parameter")]
        public float rigBlendWeight = 1f;
        public float rigBlendSpeed = 1f;
        public float actionLayerBlendInSpeed = 1f;
        public float actionLayerBlendOutSpeed = 1f;
        public float lowerLayerBlendSpeed = 1f;
        public float legAnimGlueBlendSpeed = 1f;
        public float armBoneSimulatorBlendSpeed = 1f;
        public float armBoneSimulatorTargetWeight = 0f;
        public float legBoneSimulatorBlendSpeed = 1f;
        public float legBoneSimulatorTargetWeight = 0f;
        public float headBoneScaleFactor = 1f;
        Vector3[] __8waysBlendTreePosXY;

        //* Animator 레이어 인덱스 값 
        enum LayerIndices : int
        {
            Base = 0,
            Action,
            LeftArm,
            Lower,
            Addictive,
            Max,
        }

        public override void OnAnimatorMoveHandler()
        {
            if (IsRootMotionForced())
                __brain.Movement.AddRootMotion(mainAnimator.deltaPosition, mainAnimator.deltaRotation, Time.deltaTime);
            else if (__brain.ActionCtrler.CheckActionRunning() && __brain.ActionCtrler.CanRootMotion(mainAnimator.deltaPosition))
                __brain.Movement.AddRootMotion(__brain.ActionCtrler.GetRootMotionMultiplier() * mainAnimator.deltaPosition, mainAnimator.deltaRotation, Time.deltaTime);
        }

        public override void OnAnimatorFootHandler(bool isRight) 
        {
            var pos = __brain.BB.TargetColliderHelper.GetWorldCenter();
            SoundManager.Instance.PlayWithClipPos(__brain.BB.resource.onFootstepClip, pos, false, true, 0.2f);
        }

        void Awake()
        {
            __brain = GetComponent<RoboSoldierBrain>();
        }

        RoboSoldierBrain __brain;

        void Start()
        {
            __brain.BB.common.isSpawnFinished.Subscribe(v =>
            {
                if (v)
                    __brain.AnimCtrler.mainAnimator.SetBool("IsSpawnFinished", true);
            }).AddTo(this);

            //* Ragdoll이 사용하는 PhysicsBody 레이어는 
            foreach (var d in ragdollAnimator.Handler.TargetParentForRagdollDummy.gameObject.Descendants())
            {
                if (d.TryGetComponent<Rigidbody>(out var rigidBody))
                {
                    rigidBody.excludeLayers |= LayerMask.GetMask("HitBoxBlocking");
                    if (rigidBody.TryGetComponent<Collider>(out var collider))
                        collider.excludeLayers |= LayerMask.GetMask("HitBoxBlocking");

                    __Logger.LogR1(gameObject, "Add 'HitBoxBlocking' to excludeLayers", "gameObject", rigidBody);
                }
            }

            __brain.StatusCtrler.onStatusActive += (status) =>
            {
                if (status == PawnStatus.Staggered && __brain.StatusCtrler.GetStrength(PawnStatus.Staggered) > 0f)
                {
                    leftArmBoneSimulator.GravityEffectForce = rightArmBoneSimulator.GravityEffectForce = 9.8f * Vector3.down;
                    leftArmBoneSimulator.GravityHeavyness = 4f;
                    leftArmBoneSimulator.StimulatorAmount = 1f;
                    armBoneSimulatorTargetWeight = 1f;
                }
                else if (status == PawnStatus.KnockDown)
                {
                    armBoneSimulatorTargetWeight = 0f;
                }
            };

            __brain.StatusCtrler.onStatusDeactive += (buff) =>
            {
                if (buff == PawnStatus.Staggered && !__brain.StatusCtrler.CheckStatus(PawnStatus.Staggered))
                    armBoneSimulatorTargetWeight = 0f;
            };

            __brain.BB.body.isGuarding.Subscribe(v => __brain.AnimCtrler.mainAnimator.SetBool("IsGuarding", v)).AddTo(this);
            __brain.BB.body.isJumping.Subscribe(v =>
            {
                //* 발바닥에서 발사하는 Frame 이펙트 On
                if (v && __brain.BB.IsSpawnFinished)
                {
                    foreach (var r in __brain.BB.children.jetFlameRenderers)
                    {
                        if (!r.enabled)
                        {
                            r.enabled = true;
                            r.transform.DOScale(2f * Vector3.one, 0.5f).OnComplete(() => r.transform.GetChild(0).GetComponent<ParticleSystem>().Play());
                        }
                    }
                }
            }).AddTo(this);

            __brain.BB.body.isGliding.Subscribe(v =>
            {
                //* 발바닥에서 발사하는 Frame 이펙트 Off
                if (!v && __brain.BB.IsSpawnFinished)
                {
                    foreach (var r in __brain.BB.children.jetFlameRenderers)
                    {
                        r.transform.DOScale(Vector3.zero, 0.2f).OnComplete(() =>
                        {
                            r.enabled = false;
                            r.transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
                        });
                    }
                }
            }).AddTo(this);

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
                
                animMoveVec = animMoveVec.magnitude * Vector3.Lerp(animMoveVec.normalized, animMoveVecClamped.normalized, 0.5f);

                mainAnimator.SetFloat("MoveX", animMoveVec.x / __brain.Movement.moveSpeed);
                mainAnimator.SetFloat("MoveY", animMoveVec.z / __brain.Movement.moveSpeed);

                if (__brain.ActionCtrler.CheckActionRunning())
                {
                    armBoneSimulatorTargetWeight = leftArmBoneSimulator.StimulatorAmount = rightArmBoneSimulator.StimulatorAmount = 0f;
                    if (__brain.ActionCtrler.CurrActionName == "ShieldAttack") leftHandOverride.weight = Mathf.Clamp01(leftHandOverride.weight + __brain.BB.action.shieldAttackRigBlendInSpeed * Time.deltaTime);
                }
                else if (CheckAnimStateTriggered("OnParried") || CheckAnimStateTriggered("OnGroggy (Wait)"))
                {
                    armBoneSimulatorTargetWeight = 1f;
                    if (__brain.ActionCtrler.CurrActionName != "ShieldAttack") leftHandOverride.weight = Mathf.Clamp01(leftHandOverride.weight - __brain.BB.action.shieldAttackRigBlendOutSpeed * Time.deltaTime);
                }
                else
                {
                    armBoneSimulatorTargetWeight = 0f;
                    if (__brain.ActionCtrler.CurrActionName != "ShieldAttack") leftHandOverride.weight = Mathf.Clamp01(leftHandOverride.weight - __brain.BB.action.shieldAttackRigBlendOutSpeed * Time.deltaTime);
                }
                    
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

                mainAnimator.SetLayerWeight((int)LayerIndices.LeftArm, __brain.ActionCtrler.CheckActionRunning() ? 1f : 0f);

                if ((__brain.BB.IsJumping || __brain.BB.IsGliding || __brain.BB.IsFalling) && !__brain.Movement.IsOnGround)
                    mainAnimator.SetLayerWeight((int)LayerIndices.Lower, Mathf.Clamp01(mainAnimator.GetLayerWeight((int)LayerIndices.Lower) + lowerLayerBlendSpeed * Time.deltaTime));
                else
                    mainAnimator.SetLayerWeight((int)LayerIndices.Lower, 0f);

                //* 활공 시에 다리가 자연스럽게 흔들리도록 LegBoneSimulator의 StimulatorAmount 값을 조절함
                if (__brain.BB.IsGliding)
                {
                    if (!leftLegBoneSimulator.enabled) leftLegBoneSimulator.enabled = true;
                    if (!rightLegBoneSimulator.enabled) rightLegBoneSimulator.enabled = true;
                    leftLegBoneSimulator.StimulatorAmount = Mathf.Clamp(leftLegBoneSimulator.StimulatorAmount + legBoneSimulatorBlendSpeed * Time.deltaTime, 0f, legBoneSimulatorTargetWeight);
                    rightLegBoneSimulator.StimulatorAmount = Mathf.Clamp(rightLegBoneSimulator.StimulatorAmount + legBoneSimulatorBlendSpeed * Time.deltaTime, 0f, legBoneSimulatorTargetWeight);
                }
                else
                {
                    leftLegBoneSimulator.StimulatorAmount = Mathf.Clamp01(leftLegBoneSimulator.StimulatorAmount - legBoneSimulatorBlendSpeed * Time.deltaTime);
                    rightLegBoneSimulator.StimulatorAmount = Mathf.Clamp01(rightLegBoneSimulator.StimulatorAmount - legBoneSimulatorBlendSpeed * Time.deltaTime);

                    if (leftLegBoneSimulator.enabled && leftLegBoneSimulator.StimulatorAmount <= 0f)
                        leftLegBoneSimulator.enabled = false;
                    if (rightLegBoneSimulator.enabled && rightLegBoneSimulator.StimulatorAmount <= 0f)
                        rightLegBoneSimulator.enabled = false;
                }

                if (__brain.BB.IsDead)
                {
                    headAim.weight = 0f;
                    legAnimator.LegsAnimatorBlend = Mathf.Clamp01(legAnimator.LegsAnimatorBlend - legAnimGlueBlendSpeed * Time.deltaTime);
                    legAnimator.User_SetIsMoving(false);
                    legAnimator.User_SetIsGrounded(false);
                }
                else if (__brain.BB.IsGroggy)
                {
                    headAim.weight = 0f;
                    legAnimator.LegsAnimatorBlend = 1f;
                    legAnimator.User_SetIsMoving(false);
                    legAnimator.User_SetIsGrounded(true);
                }
                else if (__brain.BB.IsDown)
                {
                    headAim.weight = 0f;
                    legAnimator.LegsAnimatorBlend = 0f;
                    legAnimator.User_SetIsMoving(false);
                    legAnimator.User_SetIsGrounded(false);
                }
                else if (__brain.BB.IsJumping || __brain.BB.IsGliding || __brain.BB.IsFalling)
                {
                    legAnimator.LegsAnimatorBlend = 0f;
                    legAnimator.User_SetIsMoving(false);
                    legAnimator.User_SetIsGrounded(false);
                }
                else
                {
                    headAim.weight = 1f;
                    legAnimator.LegsAnimatorBlend = 1f;
                    legAnimator.MainGlueBlend = 1f;
                    legAnimator.User_SetIsMoving(__brain.Movement.CurrVelocity.sqrMagnitude > 0 && !__brain.ActionCtrler.CheckActionRunning() && !__brain.ActionCtrler.CheckKnockBackRunning());
                    legAnimator.User_SetIsGrounded(__brain.Movement.IsOnGround);
                }
            };

            __brain.onLateUpdate += () =>
            {
                mainAnimator.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

                rigSetup.weight = 1f;

                if (__brain.ActionCtrler.CheckAddictiveActionRunning("Laser"))
                    __brain.BB.children.lookAtPoint.position = __brain.BB.children.laserAimPoint.position;
                else if (__brain.BB.TargetBrain != null && __brain.BB.TargetBrain.coreColliderHelper != null)
                    __brain.BB.children.lookAtPoint.position = __brain.BB.TargetBrain.coreColliderHelper.GetWorldCenter() + 0.5f * Vector3.up;
                else
                    __brain.BB.children.lookAtPoint.position = __brain.coreColliderHelper.GetWorldCenter() + 1000f * __brain.coreColliderHelper.transform.forward;

                //* 머리 사이즈 키우기
                headBone.transform.localScale = headBoneScaleFactor * Vector3.one;
                // __brain.ActionCtrler.hookingPointColliderHelper.transform.position = hookingPoint.transform.position;
            };

            __brain.PawnHP.onDead += (_) =>
            {
                mainAnimator.SetInteger("AnimId", UnityEngine.Random.Range(1, 4));
                mainAnimator.SetTrigger("OnDead");
            };
        }
    }
}