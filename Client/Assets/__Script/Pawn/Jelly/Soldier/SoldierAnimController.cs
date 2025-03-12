using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using FIMSpace.BonesStimulation;
using FIMSpace.FEyes;
using UniRx;
using Unity.Linq;
using UnityEngine;

namespace Game
{
    public class SoldierAnimController : PawnAnimController
    {
        [Header("Component")]
        public Transform headBone;
        public BonesStimulator leftArmBoneSimulator;
        public BonesStimulator rightArmBoneSimulator;
        public BonesStimulator leftLegBoneSimulator;
        public BonesStimulator rightLegBoneSimulator;
        public JellyMeshController jellyMeshCtrler;

        [Header("Parameter")]
        public float rigBlendWeight = 1f;
        public float rigBlendSpeed = 1f;
        public float actionLayerBlendInSpeed = 1f;
        public float actionLayerBlendOutSpeed = 1f;
        public float legAnimGlueBlendSpeed = 1f;
        public float armBoneSimulatorBlendSpeed = 1f;
        public float armBoneSimulatorTargetWeight = 0f;
        public float legBoneSimulatorBlendSpeed = 1f;
        public float legBoneSimulatorTargetWeight = 0f;
        public float headBoneScaleFactor = 1f;
        public Vector3[] moveXmoveY_Table;

        //* Animator 레이어 인덱스 값 
        enum LayerIndices : int
        {
            Base = 0,
            Action,
            Lower,
            Addictive,
            Max,
        }

        public override void OnAnimatorMoveHandler()
        {
            if (__brain.BB.IsDown)
                __brain.Movement.AddRootMotion(mainAnimator.deltaPosition, mainAnimator.deltaRotation, Time.deltaTime);
            else if (__brain.ActionCtrler.CheckActionRunning() && __brain.ActionCtrler.CanRootMotion(mainAnimator.deltaPosition))
                __brain.Movement.AddRootMotion(__brain.ActionCtrler.GetRootMotionMultiplier() * mainAnimator.deltaPosition, mainAnimator.deltaRotation, Time.deltaTime);
        }

        void Awake()
        {
            __brain = GetComponent<SoldierBrain>();
        }

        SoldierBrain __brain;
        HashSet<string> __watchingStateNames = new();
        public bool CheckWatchingState(string stateName) => __watchingStateNames.Contains(stateName);

        void Start()
        {
            FindObservableStateMachineTriggerEx("OnParried").OnStateEnterAsObservable().Subscribe(s => __watchingStateNames.Add("OnParried")).AddTo(this);
            FindObservableStateMachineTriggerEx("OnParried").OnStateExitAsObservable().Subscribe(s => __watchingStateNames.Remove("OnParried")).AddTo(this);

            __brain.StatusCtrler.onStatusActive += (status) =>
            {
                if ((status == PawnStatus.Staggered && __brain.StatusCtrler.GetStrength(PawnStatus.Staggered) > 0f) || status == PawnStatus.Groggy)
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
                if (!__brain.StatusCtrler.CheckStatus(PawnStatus.Staggered) && !__brain.StatusCtrler.CheckStatus(PawnStatus.Groggy))
                    armBoneSimulatorTargetWeight = 0f;
            };

            __brain.BB.common.isGroggy.Skip(1).Subscribe(v =>
            {
                if (v)
                    jellyMeshCtrler.FadeIn(1f);
                else
                    jellyMeshCtrler.FadeOut(1f);
            }).AddTo(this);

            __brain.BB.body.isJumping.Subscribe(v =>
            {
                //* 발바닥에서 발사하는 Frame 이펙트 On
                if (v)
                {
                    foreach (var p in __brain.BB.attachment.jetParticleSystems)
                    {
                        p.transform.parent.GetComponent<MeshRenderer>().enabled = true;
                        p.transform.parent.DOScale(2f * Vector3.one, 0.5f).OnComplete(() => p.Play());
                    }
                }
            }).AddTo(this);

            __brain.BB.body.isGliding.Subscribe(v =>
            {
                //* 발바닥에서 발사하는 Frame 이펙트 Off
                if (!v)
                {
                    foreach (var p in __brain.BB.attachment.jetParticleSystems)
                    {
                        p.transform.parent.DOScale(Vector3.zero, 0.2f).OnComplete(() =>
                        {
                            p.transform.parent.GetComponent<MeshRenderer>().enabled = false;
                            p.Stop();
                        });
                    }
                }
            }).AddTo(this);

            __brain.onUpdate += () =>
            {
                mainAnimator.SetLayerWeight((int)LayerIndices.Action, __brain.ActionCtrler.GetAdvancedActionLayerWeight(mainAnimator.GetLayerWeight((int)LayerIndices.Action), actionLayerBlendInSpeed, actionLayerBlendOutSpeed, Time.deltaTime));
                mainAnimator.SetLayerWeight((int)LayerIndices.Addictive, 1f);
                mainAnimator.SetBool("IsMoving", __brain.Movement.CurrVelocity.sqrMagnitude > 0 && !__brain.ActionCtrler.CheckKnockBackRunning());
                mainAnimator.SetBool("IsMovingStrafe", __brain.Movement.freezeRotation);
                mainAnimator.SetFloat("MoveSpeed", __brain.Movement.CurrVelocity.magnitude);
                mainAnimator.SetFloat("MoveAnimSpeed", 1f);

                var animMoveVec = __brain.coreColliderHelper.transform.InverseTransformDirection(__brain.Movement.CurrVelocity).Vector2D();
                var animMoveVecClamped = moveXmoveY_Table.OrderBy(v => Vector3.Angle(v, animMoveVec)).First();

                animMoveVecClamped = Vector3.Lerp(animMoveVec, animMoveVecClamped, 0.5f);
                mainAnimator.SetFloat("MoveX", animMoveVecClamped.x / __brain.Movement.moveSpeed);
                mainAnimator.SetFloat("MoveY", animMoveVecClamped.z / __brain.Movement.moveSpeed);

                if (__brain.ActionCtrler.CheckActionRunning())
                    armBoneSimulatorTargetWeight = leftArmBoneSimulator.StimulatorAmount = rightArmBoneSimulator.StimulatorAmount = 0f;
                else if (CheckWatchingState("OnParried"))
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

                //* 활공 시에 다리가 자연스럽게 흔들리도록 LegBoneSimulator의 StimulatorAmount 값을 조절함
                if (__brain.BB.IsGliding)
                {
                    mainAnimator.SetLayerWeight((int)LayerIndices.Lower, 1f);

                    if (!leftLegBoneSimulator.enabled) leftLegBoneSimulator.enabled = true;
                    if (!rightLegBoneSimulator.enabled) rightLegBoneSimulator.enabled = true;
                    leftLegBoneSimulator.StimulatorAmount = Mathf.Clamp(leftLegBoneSimulator.StimulatorAmount + legBoneSimulatorBlendSpeed * Time.deltaTime, 0f, legBoneSimulatorTargetWeight);
                    rightLegBoneSimulator.StimulatorAmount = Mathf.Clamp(rightLegBoneSimulator.StimulatorAmount + legBoneSimulatorBlendSpeed * Time.deltaTime, 0f, legBoneSimulatorTargetWeight);
                }
                else
                {
                    mainAnimator.SetLayerWeight((int)LayerIndices.Lower, 0f);
                    leftLegBoneSimulator.StimulatorAmount = Mathf.Clamp01(leftLegBoneSimulator.StimulatorAmount - legBoneSimulatorBlendSpeed * Time.deltaTime);
                    rightLegBoneSimulator.StimulatorAmount = Mathf.Clamp01(rightLegBoneSimulator.StimulatorAmount - legBoneSimulatorBlendSpeed * Time.deltaTime);

                    if (leftLegBoneSimulator.enabled && leftLegBoneSimulator.StimulatorAmount <= 0f)
                        leftLegBoneSimulator.enabled = false;
                    if (rightLegBoneSimulator.enabled && rightLegBoneSimulator.StimulatorAmount <= 0f)
                        rightLegBoneSimulator.enabled = false;
                }

                if (__brain.BB.IsDead)
                {
                    // eyeAnimator.MinOpenValue = Mathf.Clamp01(eyeAnimator.MinOpenValue - legAnimGlueBlendSpeed * Time.deltaTime);
                    legAnimator.LegsAnimatorBlend = Mathf.Clamp01(legAnimator.LegsAnimatorBlend - legAnimGlueBlendSpeed * Time.deltaTime);
                    legAnimator.User_SetIsMoving(false);
                    legAnimator.User_SetIsGrounded(false);
                }
                else if (__brain.BB.IsGroggy)
                {
                    legAnimator.LegsAnimatorBlend = 1f;
                    legAnimator.User_SetIsMoving(false);
                    legAnimator.User_SetIsGrounded(true);
                }
                else if (__brain.BB.IsDown)
                {
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
                    legAnimator.LegsAnimatorBlend = 1f;
                    legAnimator.MainGlueBlend = 1f;

                    // if (__brain.BB.IsGuarding || __brain.ActionCtrler.CheckKnockBackRunning())
                    //     legAnimator.MainGlueBlend = 1f;
                    // else
                    //     legAnimator.MainGlueBlend = Mathf.Clamp(legAnimator.MainGlueBlend + (__brain.Movement.CurrVelocity.sqrMagnitude > 0 && !__brain.ActionCtrler.CheckActionRunning() ? -1 : 1) * legAnimGlueBlendSpeed * Time.deltaTime, __brain.Movement.freezeRotation ? 0.8f : 0.9f, 1);

                    legAnimator.User_SetIsMoving(__brain.Movement.CurrVelocity.sqrMagnitude > 0 && !__brain.ActionCtrler.CheckActionRunning() && !__brain.ActionCtrler.CheckKnockBackRunning());
                    legAnimator.User_SetIsGrounded(__brain.Movement.IsOnGround);
                }
            };

            __brain.onLateUpdate += () =>
            {
                mainAnimator.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

                rigSetup.weight = 1f;
                if (__brain.ActionCtrler.CheckAddictiveActionRunning("Laser"))
                    __brain.BB.attachment.targetLookAt.position = __brain.BB.attachment.laserAimPoint.position;
                else if (__brain.BB.TargetBrain != null && __brain.BB.TargetBrain.coreColliderHelper != null)
                    __brain.BB.attachment.targetLookAt.position = __brain.BB.TargetBrain.coreColliderHelper.GetWorldCenter() + 0.5f * Vector3.up;
                else
                    __brain.BB.attachment.targetLookAt.position = __brain.coreColliderHelper.GetWorldCenter() + 1000f * __brain.coreColliderHelper.transform.forward;

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