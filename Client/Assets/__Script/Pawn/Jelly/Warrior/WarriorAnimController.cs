using System.Linq;
using FIMSpace.FEyes;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Game
{
    public class WarriorAnimController : PawnAnimController
    {
        [Header("Component")]
        public Transform jellySocket;
        public Transform shieldSocket;
        public Transform eyeTarget;
        public Transform leftArmIK_Target;
        public FEyesAnimator eyeAnimator;
        public JellySpringMassSystem springMassSystem;

        [Header("Parameter")]
        public float bodyOffsetYaw = 0;
        public float rigBlendWeight = 1;
        public float rigBlendSpeed = 1;
        public float actionLayerBlendSpeed = 1;
        public float legAnimGlueBlendSpeed = 1;
        WarriorBrain __brain;
        Rig __rig;

        void Awake()
        {
            __brain = GetComponent<WarriorBrain>();
            // __rig = mainAnimator.GetComponent<RigBuilder>().layers.First().rig;
            springMassSystem.coreAttachPoint = jellySocket;
        }

        void Start()
        {
            __brain.onUpdate += () =>
            {    
                if (__brain.ActionCtrler.CheckActionRunning())
                {
                    if (__brain.ActionCtrler.CanRootMotion(mainAnimator.deltaPosition))
                        __brain.Movement.AddRootMotion(mainAnimator.deltaPosition, mainAnimator.deltaRotation);

                    if (__brain.ActionCtrler.currActionContext.rootMotionCurve != null)
                    {
                        var rootMotionVec = __brain.ActionCtrler.EvaluateRootMotion(Time.deltaTime) * __brain.coreColliderHelper.transform.forward.Vector2D().normalized;
                        if (__brain.ActionCtrler.CanRootMotion(rootMotionVec))
                            __brain.Movement.AddRootMotion(__brain.ActionCtrler.EvaluateRootMotion(Time.deltaTime) * __brain.coreColliderHelper.transform.forward.Vector2D().normalized, Quaternion.identity);
                    }
                }

                mainAnimator.transform.SetPositionAndRotation(__brain.coreColliderHelper.transform.position, __brain.coreColliderHelper.transform.rotation * Quaternion.Euler(0, bodyOffsetYaw, 0));
                mainAnimator.SetLayerWeight(1, Mathf.Clamp01(mainAnimator.GetLayerWeight(1) + ((__brain.ActionCtrler.CheckActionRunning() && __brain.ActionCtrler.CurrActionName != "!OnHit") ? actionLayerBlendSpeed : -actionLayerBlendSpeed) * Time.deltaTime));
                mainAnimator.SetLayerWeight(2, 1f);
                mainAnimator.SetBool("IsMoving", __brain.Movement.CurrVelocity.sqrMagnitude > 0);
                mainAnimator.SetBool("IsMovingStrafe", __brain.Movement.freezeRotation);
                mainAnimator.SetFloat("MoveSpeed", __brain.Movement.CurrVelocity.magnitude);
                mainAnimator.SetFloat("MoveAnimSpeed", 1f);

                var animMoveVec = __brain.coreColliderHelper.transform.InverseTransformDirection(__brain.Movement.CurrVelocity).Vector2D();
                mainAnimator.SetFloat("MoveX", animMoveVec.x / __brain.Movement.moveSpeed);
                mainAnimator.SetFloat("MoveY", animMoveVec.z / __brain.Movement.moveSpeed);

                // __rig.weight = Mathf.Clamp(__rig.weight + (__brain.BB.IsGuarding ? rigBlendSpeed : -rigBlendSpeed) * Time.deltaTime, 0, rigBlendWeight);

                if (__brain.BB.IsDead)
                {
                    eyeAnimator.MinOpenValue = Mathf.Clamp01(eyeAnimator.MinOpenValue - legAnimGlueBlendSpeed * Time.deltaTime);
                    legAnimator.LegsAnimatorBlend = Mathf.Clamp01(legAnimator.LegsAnimatorBlend - legAnimGlueBlendSpeed * Time.deltaTime);
                    legAnimator.User_SetIsMoving(false);
                    legAnimator.User_SetIsGrounded(false);
                }
                else if (__brain.BB.IsDown)
                {
                    mainAnimator.SetLayerWeight(1, 1f);
                    legAnimator.LegsAnimatorBlend = 0f;
                    legAnimator.User_SetIsMoving(false);
                    legAnimator.User_SetIsGrounded(false);
                }
                else if (__brain.BB.IsStunned)
                {
                    if (__brain.ActionCtrler.CurrActionName != "!OnHit")
                        mainAnimator.SetLayerWeight(1, 0f);
                    legAnimator.LegsAnimatorBlend = 1f;
                    legAnimator.User_SetIsMoving(false);
                    legAnimator.User_SetIsGrounded(true);
                }
                else
                {   
                    if (__brain.BB.IsGuarding)
                        legAnimator.MainGlueBlend = 1f;
                    else
                        legAnimator.MainGlueBlend = Mathf.Clamp(legAnimator.MainGlueBlend + (__brain.Movement.CurrVelocity.sqrMagnitude  > 0 && !__brain.ActionCtrler.CheckActionRunning() ? -1 : 1) * legAnimGlueBlendSpeed * Time.deltaTime, __brain.Movement.freezeRotation ? 0.8f : 0.9f, 1);

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