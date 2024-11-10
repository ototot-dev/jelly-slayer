using FIMSpace.FEyes;
using Retween.Rx;
using UnityEngine;

namespace Game
{
    public class DroneBotAnimController : PawnAnimController
    {
        [Header("Component")]
        public Transform jellySocket;
        public Transform eyeSocket;
        public Transform lookAt;
        public FEyesAnimator eyeAnimator;
        public JellySpringMassSystem springMass;
        public SphereCollider springMassCore; 
        public TweenSelector jellyTweenSelector;

        void Awake()
        {
            __brain = GetComponent<DroneBotBrain>();

            if (springMass != null)
                springMass.coreAttachPoint = jellySocket;
        }

        DroneBotBrain __brain;

        void Start()
        {
            __brain.onUpdate += () =>
            {
                if (__brain.ActionCtrler.CheckActionRunning())
                {
                    if (__brain.ActionCtrler.currActionContext.rootMotionCurve != null)
                    {
                        var rootMotionVec = Mathf.Max(0f, __brain.ActionCtrler.EvaluateRootMotion(Time.deltaTime)) * __brain.coreColliderHelper.transform.forward.Vector2D().normalized;
                        if (__brain.ActionCtrler.CanRootMotion(rootMotionVec))
                            __brain.Movement.AddRootMotion(Mathf.Max(0f, __brain.ActionCtrler.EvaluateRootMotion(Time.deltaTime)) * __brain.coreColliderHelper.transform.forward.Vector2D().normalized, Quaternion.identity);
                    }
                }

                var animMoveVec = __brain.coreColliderHelper.transform.InverseTransformDirection(__brain.Movement.CurrVelocity).Vector2D();
                mainAnimator.SetFloat("MoveX", animMoveVec.x / __brain.Movement.moveSpeed);
                mainAnimator.SetBool("IsMoving", __brain.Movement.CurrVelocity.sqrMagnitude > 0);
                mainAnimator.SetBool("IsMovingStrafe", __brain.Movement.freezeRotation);
                // mainAnimator.SetFloat("MoveY", animMoveVec.z / __brain.Movement.moveSpeed);

                if (__brain.BB.IsDead)
                    eyeAnimator.MinOpenValue = Mathf.Clamp01(eyeAnimator.MinOpenValue - Time.deltaTime);
            };

            __brain.onLateUpdate += () =>
            {
                if (eyeSocket != null)
                    eyeAnimator.transform.position = eyeSocket.position;

                if (!__brain.BB.IsDead && __brain.BB.TargetBrain != null)
                    lookAt.position = __brain.BB.TargetBrain.coreColliderHelper.GetCenter();
                else
                    lookAt.position = __brain.Movement.IsMovingToDestination ? __brain.Movement.destination : __brain.coreColliderHelper.transform.position + __brain.SensorCtrler.visionLen * __brain.coreColliderHelper.transform.forward;
            };
        }
    }
}