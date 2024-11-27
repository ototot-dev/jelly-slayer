using FIMSpace.FEyes;
using UnityEngine;

namespace Game
{
    public class ZombieAnimController : PawnAnimController
    {
        [Header("Component")]
        public Transform jellyAttachPoint;
        public Transform eyeLookAt;
        public FEyesAnimator eyeAnimator;
        public JellySpringMassSystem springMassSystem;

        [Header("Parameter")]
        public float actionLayerBlendSpeed = 1;
        public float legAnimGlueBlendSpeed = 1;
        
        // [Header("Debug")]
        // public BoolReactiveProperty debug_enableRagdoll = new();
        // public Transform debug_draggingPoint;
        // public Transform debug_draggingTarget;
        ZombieBrain __brain;

        void Awake()
        {
            __brain = GetComponent<ZombieBrain>();
            springMassSystem.coreAttachPoint = jellyAttachPoint;

            // debug_enableRagdoll.Skip(1).Subscribe(v =>
            // {
            //     if (v)
            //     {
            //         StartRagdoll(true, true);

            //         Observable.EveryLateUpdate().TakeWhile(_ => debug_enableRagdoll.Value).Subscribe(_ =>
            //         {
            //             if (debug_draggingPoint != null && debug_draggingTarget != null)
            //             {
            //                 if (debug_draggingPoint.TryGetComponent<Rigidbody>(out var rigidBody)) rigidBody.isKinematic = true;
            //                 debug_draggingPoint.SetPositionAndRotation(debug_draggingTarget.position, debug_draggingTarget.rotation);
            //             }
            //         }).AddTo(this);
            //     }
            //     else
            //     {
            //         if (debug_draggingTarget != null && debug_draggingPoint != null && debug_draggingPoint.TryGetComponent<Rigidbody>(out var rigidBody))
            //         {
            //             foreach (var p in __capturedPhysicsBodyTransforms)
            //             {
            //                 p.Key.drag = 0f;
            //                 p.Key.angularDrag = 0.05f;
            //                 p.Key.useGravity = true;
            //             }

            //             rigidBody.isKinematic = false;
            //             rigidBody.AddExplosionForce(200f, debug_draggingTarget.position, 5f);

            //             Observable.Timer(TimeSpan.FromSeconds(5f)).Subscribe(_ => FinishRagdoll(1f)).AddTo(this);
            //         }
            //         else
            //         {
            //             FinishRagdoll(1f);
            //         }
            //     }
            // }).AddTo(this);
        }

        void Start()
        {
            __brain.onUpdate += () =>
            {    
                if (__brain.ActionCtrler.CheckActionRunning())
                {
                    __brain.Movement.AddRootMotion(mainAnimator.deltaPosition, mainAnimator.deltaRotation);
                    if (__brain.ActionCtrler.currActionContext.rootMotionCurve != null)
                    {
                        var rootMotionVec = Mathf.Max(0f, __brain.ActionCtrler.EvaluateRootMotion(Time.deltaTime)) * __brain.coreColliderHelper.transform.forward.Vector2D().normalized;
                        if (__brain.ActionCtrler.CanRootMotion(rootMotionVec))
                            __brain.Movement.AddRootMotion(rootMotionVec, Quaternion.identity);
                    }
                }

                mainAnimator.transform.SetPositionAndRotation(__brain.coreColliderHelper.transform.position, __brain.coreColliderHelper.transform.rotation);
                mainAnimator.SetLayerWeight(1, Mathf.Clamp01(mainAnimator.GetLayerWeight(1) + (__brain.ActionCtrler.CheckActionRunning() ? actionLayerBlendSpeed : -actionLayerBlendSpeed) * Time.deltaTime));
                mainAnimator.SetBool("IsMoving", __brain.Movement.CurrVelocity.sqrMagnitude > 0f);
                mainAnimator.SetFloat("MoveSpeed", __brain.Movement.CurrVelocity.magnitude);

                if (__brain.BB.IsDead)
                {
                    eyeAnimator.MinOpenValue = Mathf.Clamp01(eyeAnimator.MinOpenValue - legAnimGlueBlendSpeed * Time.deltaTime);
                    legAnimator.LegsAnimatorBlend = Mathf.Clamp01(legAnimator.LegsAnimatorBlend - legAnimGlueBlendSpeed * Time.deltaTime);
                    legAnimator.User_SetIsMoving(false);
                    legAnimator.User_SetIsGrounded(true);
                }
                else if (__brain.BB.IsDown)
                {
                    mainAnimator.SetLayerWeight(1, 1f);
                    legAnimator.LegsAnimatorBlend = 0f;
                    legAnimator.User_SetIsMoving(false);
                    legAnimator.User_SetIsGrounded(false);
                }
                else if (__brain.BB.IsGroggy)
                {
                    if (__brain.ActionCtrler.CurrActionName != "!OnHit")
                        mainAnimator.SetLayerWeight(1, 0f);
                    legAnimator.LegsAnimatorBlend = 1f;
                    legAnimator.User_SetIsMoving(false);
                    legAnimator.User_SetIsGrounded(true);
                }
                else if (__brain.BB.IsBind)
                {
                    //if (__brain.ActionCtrler.CurrActionName != "!OnHit")
                        //mainAnimator.SetLayerWeight(1, 0f);
                    legAnimator.LegsAnimatorBlend = 1f;
                    legAnimator.User_SetIsMoving(false);
                    legAnimator.User_SetIsGrounded(true);
                }
                else
                {   
                    // legAnimator.MainGlueBlend = Mathf.Clamp(legAnimator.MainGlueBlend + (__brain.Movement.CurrVelocity.sqrMagnitude > 0f && !__brain.ActionCtrler.CheckActionRunning() ? -1f : 1f) * legAnimGlueBlendSpeed * Time.deltaTime, __brain.Movement.freezeRotation ? 0.8f : 0.9f, 1f);
                    legAnimator.MainGlueBlend = 1f;
                    legAnimator.User_SetIsMoving(__brain.Movement.CurrVelocity.sqrMagnitude > 0f && !__brain.ActionCtrler.CheckActionRunning());
                    legAnimator.User_SetIsGrounded(__brain.Movement.IsOnGround);
                }
            };

            __brain.onLateUpdate += () =>
            {
                if (__brain.BB.TargetBrain != null)
                    eyeLookAt.position = __brain.BB.TargetBrain.coreColliderHelper.transform.position + Vector3.up;
                else
                    eyeLookAt.position = __brain.coreColliderHelper.transform.position + __brain.coreColliderHelper.transform.forward + Vector3.up;
            };

            __brain.PawnHP.onDead += (_) =>
            {
                mainAnimator.SetInteger("AnimId", UnityEngine.Random.Range(1, 4));
                mainAnimator.SetTrigger("OnDead");
            };
        }
    }
}