using FIMSpace.FEyes;
using Retween.Rx;
using UnityEngine;

namespace Game
{
    public class DroneBotAnimController : PawnAnimController
    {
        [Header("Component")]
        public Transform leftHand;
        public Transform rightHand;

        void Awake()
        {
            __brain = GetComponent<DroneBotBrain>();
            __animatorOffsetY = mainAnimator.transform.localPosition.y;
        }

        DroneBotBrain __brain;
        float __animatorOffsetY;

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
                
                mainAnimator.transform.SetPositionAndRotation(__brain.coreColliderHelper.transform.position + __animatorOffsetY * Vector3.up, __brain.coreColliderHelper.transform.rotation);
                mainAnimator.SetLayerWeight(1, Mathf.Clamp01(mainAnimator.GetLayerWeight(1) + ((__brain.ActionCtrler.CheckActionRunning() && __brain.ActionCtrler.CurrActionName != "!OnHit") ? 10f : -10f) * Time.deltaTime));
                mainAnimator.SetLayerWeight(2, 1f);

                var animMoveVec = __brain.coreColliderHelper.transform.InverseTransformDirection(__brain.Movement.CurrVelocity).Vector2D();
                mainAnimator.SetFloat("MoveX", animMoveVec.x / __brain.Movement.moveSpeed);
                mainAnimator.SetBool("IsMoving", __brain.Movement.CurrVelocity.sqrMagnitude > 0);
                mainAnimator.SetBool("IsMovingStrafe", __brain.Movement.freezeRotation);
            };
        }
    }
}