using Retween.Rx;
using UnityEngine;

namespace Game
{
    public class DroneBotAnimController : PawnAnimController
    {
        [Header("Component")]
        public Transform leftHand;
        public Transform rightHand;
        public Transform hangingPoint;
        public TweenSelector tweenSelector;
        public ParticleSystem[] steamFx;

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
                if (__brain.ActionCtrler.CheckActionRunning() && __brain.ActionCtrler.CanRootMotion(mainAnimator.deltaPosition))
                    __brain.Movement.AddRootMotion(__brain.ActionCtrler.GetRootMotionMultiplier() * mainAnimator.deltaPosition, mainAnimator.deltaRotation);
                
                // mainAnimator.transform.SetPositionAndRotation(__brain.coreColliderHelper.transform.position + __animatorOffsetY * Vector3.up, __brain.coreColliderHelper.transform.rotation);
                mainAnimator.SetLayerWeight(1, Mathf.Clamp01(mainAnimator.GetLayerWeight(1) + ((__brain.ActionCtrler.CheckActionRunning() && __brain.ActionCtrler.CurrActionName != "!OnHit") ? 10f : -10f) * Time.deltaTime));
                mainAnimator.SetLayerWeight(2, 1f);

                var animMoveVec = __brain.coreColliderHelper.transform.InverseTransformDirection(__brain.Movement.CurrVelocity).Vector2D();
                mainAnimator.SetFloat("MoveX", animMoveVec.x / __brain.Movement.moveSpeed);
                mainAnimator.SetBool("IsMoving", __brain.Movement.CurrVelocity.sqrMagnitude > 0);
                mainAnimator.SetBool("IsMovingStrafe", __brain.Movement.freezeRotation || __brain.BB.IsHanging);

                if (__brain.BB.IsHanging && !steamFx[0].isPlaying)
                {
                    foreach (var f in steamFx)
                        f.Play();
                }
                else if (!__brain.BB.IsHanging && steamFx[0].isPlaying)
                {                   
                    foreach (var f in steamFx)
                        f.Stop();
                }
            };
        }
    }
}