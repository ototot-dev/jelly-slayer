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
            __droneBotBrain = GetComponent<DroneBotBrain>();

            if (springMass != null)
                springMass.coreAttachPoint = jellySocket;
        }

        DroneBotBrain __droneBotBrain;

        void Start()
        {
            __droneBotBrain.onUpdate += () =>
            {   
                if (__droneBotBrain.ActionCtrler.CheckActionRunning())
                {
                    if (__droneBotBrain.ActionCtrler.CanRootMotion(mainAnimator.deltaPosition))
                        __droneBotBrain.Movement.AddRootMotion(mainAnimator.deltaPosition, mainAnimator.deltaRotation);

                    if (__droneBotBrain.ActionCtrler.currActionContext.rootMotionCurve != null)
                    {
                        var rootMotionVec = __droneBotBrain.ActionCtrler.EvaluateRootMotion(Time.deltaTime) * __droneBotBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
                        if (__droneBotBrain.ActionCtrler.CanRootMotion(rootMotionVec))
                            __droneBotBrain.Movement.AddRootMotion(__droneBotBrain.ActionCtrler.EvaluateRootMotion(Time.deltaTime) * __droneBotBrain.coreColliderHelper.transform.forward.Vector2D().normalized, Quaternion.identity);
                    }
                }
                
                mainAnimator.transform.SetPositionAndRotation(__droneBotBrain.coreColliderHelper.transform.position, __droneBotBrain.coreColliderHelper.transform.rotation);
                mainAnimator.SetLayerWeight(1, Mathf.Clamp01(mainAnimator.GetLayerWeight(1) + ((__droneBotBrain.ActionCtrler.CheckActionRunning() && __droneBotBrain.ActionCtrler.CurrActionName != "!OnHit") ? 10f : -10f) * Time.deltaTime));
                mainAnimator.SetLayerWeight(2, 1f);

                var animMoveVec = __droneBotBrain.coreColliderHelper.transform.InverseTransformDirection(__droneBotBrain.Movement.CurrVelocity).Vector2D();
                mainAnimator.SetFloat("MoveX", animMoveVec.x / __droneBotBrain.Movement.moveSpeed);
                mainAnimator.SetBool("IsMoving", __droneBotBrain.Movement.CurrVelocity.sqrMagnitude > 0);
                mainAnimator.SetBool("IsMovingStrafe", __droneBotBrain.Movement.freezeRotation);
                // mainAnimator.SetFloat("MoveY", animMoveVec.z / __brain.Movement.moveSpeed);

                if (__droneBotBrain.BB.IsDead)
                    eyeAnimator.MinOpenValue = Mathf.Clamp01(eyeAnimator.MinOpenValue - Time.deltaTime);
            };

            __droneBotBrain.onLateUpdate += () =>
            {
                if (eyeSocket != null)
                    eyeAnimator.transform.position = eyeSocket.position;

                if (!__droneBotBrain.BB.IsDead && __droneBotBrain.BB.TargetBrain != null)
                    lookAt.position = __droneBotBrain.BB.TargetBrain.coreColliderHelper.GetCenter();
                else
                    lookAt.position = __droneBotBrain.Movement.IsMovingToDestination ? __droneBotBrain.Movement.destination : __droneBotBrain.coreColliderHelper.transform.position + __droneBotBrain.SensorCtrler.visionLen * __droneBotBrain.coreColliderHelper.transform.forward;
            };
        }
    }
}