using System.Linq;
using FIMSpace.BonesStimulation;
using FIMSpace.FEyes;
using UniRx;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Game
{
    public class SoldierAnimController : PawnAnimController
    {
        [Header("Component")]
        public Transform jellyMeshSlot;
        public Transform shieldMeshSlot;
        public Transform eyeTarget;
        public MeshRenderer shieldMeshRenderer;
        public FEyesAnimator eyeAnimator;
        public BonesStimulator leftArmBoneSimulator;
        public BonesStimulator rightArmBoneSimulator;
        public BonesStimulator leftLegBoneSimulator;
        public BonesStimulator rightLegBoneSimulator;
        public ParticleSystem leftLegFrameFx;
        public ParticleSystem rightLegFrameFx;
        public JellySpringMassSystem springMassSystem;

        [Header("Parameter")]
        public float rigBlendWeight = 1f;
        public float rigBlendSpeed = 1f;
        public float actionLayerBlendSpeed = 1f;
        public float legAnimGlueBlendSpeed = 1f;
        public float armBoneSimulatorBlendSpeed = 1f;
        public float armBoneSimulatorTargetWeight = 0f;
        public float legBoneSimulatorBlendSpeed = 1f;
        public float legBoneSimulatorTargetWeight = 0f;
        public Vector3[] moveXmoveY_Table;
        SoldierBrain __brain;
        Rig __rig;

        //* Animator 레이어 인덱스 값 
        enum LayerIndices : int
        {
            Base = 0,
            Action,
            Lower,
            Addictive,
            Max,
        }

        void Awake()
        {
            __brain = GetComponent<SoldierBrain>();
            // __rig = mainAnimator.GetComponent<RigBuilder>().layers.First().rig;
            springMassSystem.coreAttachPoint = jellyMeshSlot;
        }

        void Start()
        {
            __brain.StatusCtrler.onStatusActive += (buff) =>
            {
                if (buff == PawnStatus.Staggered || buff == PawnStatus.Groggy)
                {
                    leftArmBoneSimulator.GravityEffectForce = rightArmBoneSimulator.GravityEffectForce = 9.8f * Vector3.down;
                    leftArmBoneSimulator.GravityHeavyness = 4f;
                    leftArmBoneSimulator.StimulatorAmount = 1f;
                    armBoneSimulatorTargetWeight = 1f;
                    shieldMeshRenderer.material.SetFloat("_Alpha", 0.03f);
                }
                else if (buff == PawnStatus.KnockDown)
                {
                    armBoneSimulatorTargetWeight = 0f;
                    shieldMeshRenderer.material.SetFloat("_Alpha", 0.3f);
                }
            };

            __brain.StatusCtrler.onStatusDeactive += (buff) =>
            {
                if ((buff == PawnStatus.Staggered || buff == PawnStatus.Groggy) && !__brain.StatusCtrler.CheckStatus(PawnStatus.Staggered) && !__brain.StatusCtrler.CheckStatus(PawnStatus.Groggy))
                {
                    armBoneSimulatorTargetWeight = 0f;
                    shieldMeshRenderer.material.SetFloat("_Alpha", 0.3f);
                }
            };
            
            __brain.BB.action.isGliding.Subscribe(v =>
            {
                //* 발바닥에서 발사하는 Frame 이펙트 출력 제어
                if (v)
                {
                    leftLegFrameFx.transform.parent.GetComponent<MeshRenderer>().enabled = true;
                    rightLegFrameFx.transform.parent.GetComponent<MeshRenderer>().enabled = true;
                    leftLegFrameFx.Play();
                    rightLegFrameFx.Play();
                }
                else
                {
                    leftLegFrameFx.transform.parent.GetComponent<MeshRenderer>().enabled = false;
                    rightLegFrameFx.transform.parent.GetComponent<MeshRenderer>().enabled = false;
                    leftLegFrameFx.Stop();
                    rightLegFrameFx.Stop();
                }
            }).AddTo(this);

            __brain.onUpdate += () =>
                {
                    if (__brain.BB.IsDown)
                        __brain.Movement.AddRootMotion(mainAnimator.deltaPosition, mainAnimator.deltaRotation);
                    else if (__brain.ActionCtrler.CheckActionRunning() && __brain.ActionCtrler.CanRootMotion(mainAnimator.deltaPosition))
                        __brain.Movement.AddRootMotion(__brain.ActionCtrler.GetRootMotionMultiplier() * mainAnimator.deltaPosition, mainAnimator.deltaRotation);

                    mainAnimator.transform.SetPositionAndRotation(__brain.coreColliderHelper.transform.position, __brain.coreColliderHelper.transform.rotation);
                    mainAnimator.SetLayerWeight((int)LayerIndices.Action, Mathf.Clamp01(mainAnimator.GetLayerWeight(1) + (__brain.ActionCtrler.CheckActionRunning() ? actionLayerBlendSpeed : -actionLayerBlendSpeed) * Time.deltaTime));
                    mainAnimator.SetLayerWeight((int)LayerIndices.Addictive, 1f);
                    mainAnimator.SetBool("IsMoving", __brain.Movement.CurrVelocity.sqrMagnitude > 0);
                    mainAnimator.SetBool("IsMovingStrafe", __brain.Movement.freezeRotation);
                    mainAnimator.SetFloat("MoveSpeed", __brain.Movement.CurrVelocity.magnitude);
                    mainAnimator.SetFloat("MoveAnimSpeed", 1f);

                    var animMoveVec = __brain.coreColliderHelper.transform.InverseTransformDirection(__brain.Movement.CurrVelocity).Vector2D();
                    var animMoveVecClamped = moveXmoveY_Table.OrderBy(v => Vector3.Angle(v, animMoveVec)).First();

                    animMoveVecClamped = Vector3.Lerp(animMoveVec, animMoveVecClamped, 0.5f);
                    mainAnimator.SetFloat("MoveX", animMoveVecClamped.x / __brain.Movement.moveSpeed);
                    mainAnimator.SetFloat("MoveY", animMoveVecClamped.z / __brain.Movement.moveSpeed);

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
                        eyeAnimator.MinOpenValue = Mathf.Clamp01(eyeAnimator.MinOpenValue - legAnimGlueBlendSpeed * Time.deltaTime);
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
                    else if (__brain.BB.IsDown || __brain.BB.IsJumping || __brain.BB.IsGliding || __brain.BB.IsFalling)
                    {
                        legAnimator.LegsAnimatorBlend = 0f;
                        legAnimator.User_SetIsMoving(false);
                        legAnimator.User_SetIsGrounded(false);
                    }
                    else
                    {
                        legAnimator.LegsAnimatorBlend = 1f;

                        if (__brain.BB.IsGuarding)
                            legAnimator.MainGlueBlend = 1f;
                        else
                            legAnimator.MainGlueBlend = Mathf.Clamp(legAnimator.MainGlueBlend + (__brain.Movement.CurrVelocity.sqrMagnitude > 0 && !__brain.ActionCtrler.CheckActionRunning() ? -1 : 1) * legAnimGlueBlendSpeed * Time.deltaTime, __brain.Movement.freezeRotation ? 0.8f : 0.9f, 1);

                        legAnimator.User_SetIsMoving(__brain.Movement.CurrVelocity.sqrMagnitude > 0 && !__brain.ActionCtrler.CheckActionRunning());
                        legAnimator.User_SetIsGrounded(__brain.Movement.IsOnGround);
                    }
                };

            __brain.onLateUpdate += () =>
            {
                if (__brain.BB.TargetBrain != null)
                    eyeTarget.position = __brain.BB.TargetBrain.coreColliderHelper.transform.position + Vector3.up;
                else
                    eyeTarget.position = __brain.coreColliderHelper.transform.position + __brain.coreColliderHelper.transform.forward + Vector3.up;
            };

            __brain.PawnHP.onDead += (_) =>
            {
                mainAnimator.SetInteger("AnimId", UnityEngine.Random.Range(1, 4));
                mainAnimator.SetTrigger("OnDead");
            };
        }
    }
}