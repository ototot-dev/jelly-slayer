using System.Linq;
using FIMSpace.BonesStimulation;
using FIMSpace.FEyes;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Game
{
    public class AlienAnimController : PawnAnimController
    {
        [Header("Component")]
        public Transform jellyMeshSlot;
        public Transform leftWeaponSlot;
        public Transform rightWeaponSlot;
        public Transform eyeTarget;
        public FEyesAnimator eyeAnimator;
        public BonesStimulator leftShoulderBoneSimulator;
        public BonesStimulator rightShoulderBoneSimulator;
        public JellySpringMassSystem springMassSystem;

        [Header("Parameter")]
        public float rigBlendWeight = 1f;
        public float rigBlendSpeed = 1f;
        public float actionLayerBlendSpeed = 1f;
        public float legAnimGlueBlendSpeed = 1f;
        public float boneSimulatorBlendSpeed = 1f;
        public float boneSimulatorTargetWeight = 0f;
        public Vector3[] moveXmoveY_Table;
        AlienBrain __brain;
        Rig __rig;

        void Awake()
        {
            __brain = GetComponent<AlienBrain>();
            // __rig = mainAnimator.GetComponent<RigBuilder>().layers.First().rig;
            springMassSystem.coreAttachPoint = jellyMeshSlot;
        }

        void Start()
        {
            __brain.StatusCtrler.onStatusActive += (buff) =>
            {
                if (buff == PawnStatus.Staggered || buff == PawnStatus.Groggy)
                {
                    leftShoulderBoneSimulator.GravityEffectForce = rightShoulderBoneSimulator.GravityEffectForce = 9.8f * Vector3.down;
                    leftShoulderBoneSimulator.GravityHeavyness = rightShoulderBoneSimulator.GravityHeavyness = 4f;
                    leftShoulderBoneSimulator.StimulatorAmount = rightShoulderBoneSimulator.StimulatorAmount= 1f;
                    boneSimulatorTargetWeight = __brain.BB.IsDown ? 0f : 1f;
                }
                else if (buff == PawnStatus.KnockDown)
                {
                    boneSimulatorTargetWeight = 0f;
                }
            };

            __brain.StatusCtrler.onStatusDeactive += (buff) =>
            {
                if ((buff == PawnStatus.Staggered || buff == PawnStatus.Groggy) && !__brain.StatusCtrler.CheckStatus(PawnStatus.Staggered) && !__brain.StatusCtrler.CheckStatus(PawnStatus.Groggy))
                    boneSimulatorTargetWeight = 0f;
            };

        __brain.onUpdate += () =>
            {    
                if (__brain.BB.IsDown)
                {
                    __brain.Movement.AddRootMotion(mainAnimator.deltaPosition, mainAnimator.deltaRotation);
                }
                else if (__brain.ActionCtrler.CheckActionRunning())
                {
                    if (__brain.ActionCtrler.CanRootMotion(mainAnimator.deltaPosition))
                        __brain.Movement.AddRootMotion(__brain.ActionCtrler.currActionContext.rootMotionMultiplier * mainAnimator.deltaPosition, mainAnimator.deltaRotation);

                    if (__brain.ActionCtrler.currActionContext.rootMotionCurve != null)
                    {
                        var rootMotionVec = __brain.ActionCtrler.EvaluateRootMotion(Time.deltaTime) * __brain.coreColliderHelper.transform.forward.Vector2D().normalized;
                        if (__brain.ActionCtrler.CanRootMotion(rootMotionVec))
                            __brain.Movement.AddRootMotion(__brain.ActionCtrler.EvaluateRootMotion(Time.deltaTime) * __brain.coreColliderHelper.transform.forward.Vector2D().normalized, Quaternion.identity);
                    }
                }

                mainAnimator.transform.SetPositionAndRotation(__brain.coreColliderHelper.transform.position, __brain.coreColliderHelper.transform.rotation);
                mainAnimator.SetLayerWeight(1, Mathf.Clamp01(mainAnimator.GetLayerWeight(1) + ((__brain.ActionCtrler.CheckActionRunning() && __brain.ActionCtrler.CurrActionName != "!OnHit") ? actionLayerBlendSpeed : -actionLayerBlendSpeed) * Time.deltaTime));
                mainAnimator.SetLayerWeight(2, 1f);
                mainAnimator.SetBool("IsMoving", __brain.Movement.CurrVelocity.sqrMagnitude > 0);
                mainAnimator.SetBool("IsMovingStrafe", __brain.Movement.freezeRotation);
                mainAnimator.SetFloat("MoveSpeed", __brain.Movement.CurrVelocity.magnitude);
                mainAnimator.SetFloat("MoveAnimSpeed", 1f);

                var animMoveVec = __brain.coreColliderHelper.transform.InverseTransformDirection(__brain.Movement.CurrVelocity).normalized.Vector2D();
                var animMoveVecClamped = moveXmoveY_Table.OrderBy(v => Vector3.Angle(v, animMoveVec)).First().normalized;
                animMoveVecClamped = __brain.Movement.CurrVelocity.magnitude *  Vector3.Lerp(animMoveVec, animMoveVecClamped, 0.2f);
                mainAnimator.SetFloat("MoveX", animMoveVecClamped.x / __brain.Movement.moveSpeed);
                mainAnimator.SetFloat("MoveY", animMoveVecClamped.z / __brain.Movement.moveSpeed);

                if (__brain.BB.IsDead)
                {
                    eyeAnimator.MinOpenValue = Mathf.Clamp01(eyeAnimator.MinOpenValue - legAnimGlueBlendSpeed * Time.deltaTime);
                    legAnimator.LegsAnimatorBlend = Mathf.Clamp01(legAnimator.LegsAnimatorBlend - legAnimGlueBlendSpeed * Time.deltaTime);
                    legAnimator.User_SetIsMoving(false);
                    legAnimator.User_SetIsGrounded(false);
                }
                else if (__brain.BB.IsDown)
                {
                    mainAnimator.SetLayerWeight(1, 0f);
                    legAnimator.LegsAnimatorBlend = 0f;
                    legAnimator.User_SetIsMoving(false);
                    legAnimator.User_SetIsGrounded(false);
                }
                else if (__brain.BB.IsGroggy)
                {
                    mainAnimator.SetLayerWeight(1, 0f);
                    legAnimator.LegsAnimatorBlend = 1f;
                    legAnimator.User_SetIsMoving(false);
                    legAnimator.User_SetIsGrounded(true);
                }
                else
                {   
                    legAnimator.MainGlueBlend = Mathf.Clamp(legAnimator.MainGlueBlend + (__brain.Movement.CurrVelocity.sqrMagnitude  > 0 && !__brain.ActionCtrler.CheckActionRunning() ? -1 : 1) * legAnimGlueBlendSpeed * Time.deltaTime, __brain.Movement.freezeRotation ? 0.8f : 0.9f, 1f);
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