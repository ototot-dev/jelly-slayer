using UniRx;
using UniRx.Triggers;
using Unity.Burst.Intrinsics;
using UnityEditor;
using UnityEngine;

namespace Game
{
    public class HeroAnimController : PawnAnimController
    {
        public Transform shieldMeshSlot;
        public Transform weaponMeshSlot;
        public Transform HeadLookAt;
        public Transform hipBone;
        public float hipBoneOffset;
        public float animLayerBlendSpeed = 1;
        public float legAnimGlueBlendSpeed = 1;
        public AnimationClip[] blockAdditiveAnimClips;
        public RuntimeAnimatorController[] _animControllers;

        void Awake()
        {
            __brain = GetComponent<HeroBrain>();

            //* Block 애님의 Additive Ref-Pose를 셋팅
            foreach (var c in blockAdditiveAnimClips)
                AnimationUtility.SetAdditiveReferencePose(c, blockAdditiveAnimClips[0], 0);
        }

        HeroBrain __brain;

        void Start()
        {
            //* Charging 예비 모션은 IsGuarding 애님을 공용으로 쓴다.
            __brain.BB.action.isCharging.Subscribe(v => mainAnimator.SetBool("IsGuarding", v)).AddTo(this);
            __brain.BB.action.isGuarding.Subscribe(v => mainAnimator.SetBool("IsGuarding", v)).AddTo(this);

            __brain.onUpdate += () =>
            {
                if (__brain.ActionCtrler.CheckActionRunning())
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
                mainAnimator.SetLayerWeight(1, __brain.BB.IsJumping ? 0f : 1f);
                mainAnimator.SetLayerWeight(2, __brain.BB.IsJumping ? 0f : 1f);
                mainAnimator.SetLayerWeight(3, Mathf.Clamp01(mainAnimator.GetLayerWeight(3) + (__brain.BB.IsRolling || (__brain.ActionCtrler.CheckActionRunning() && __brain.ActionCtrler.CurrActionName != "!OnHit" && __brain.ActionCtrler.CurrActionName != "ActiveParry") ? animLayerBlendSpeed : -animLayerBlendSpeed) * Time.deltaTime));

                mainAnimator.SetFloat("MoveSpeed", __brain.Movement.freezeRotation ? -1 : __brain.Movement.CurrVelocity.Vector2D().magnitude / __brain.BB.body.moveSpeed);
                mainAnimator.SetFloat("MoveAnimSpeed", __brain.Movement.freezeRotation ? (__brain.BB.IsGuarding ? 0.8f : 1.2f) : 1);
                mainAnimator.SetBool("IsMoving", __brain.Movement.CurrVelocity.sqrMagnitude > 0);

                var animMoveVec = __brain.coreColliderHelper.transform.InverseTransformDirection(__brain.Movement.CurrVelocity).Vector2D();
                mainAnimator.SetFloat("MoveX", animMoveVec.x / __brain.Movement.moveSpeed);
                mainAnimator.SetFloat("MoveY", animMoveVec.z / __brain.Movement.moveSpeed);

                legAnimator.User_SetIsMoving(__brain.Movement.CurrVelocity.sqrMagnitude > 0);
                legAnimator.User_SetIsGrounded( __brain.Movement.IsOnGround && !__brain.BB.IsRolling && !__brain.BB.IsDown && !__brain.BB.IsDead);
                legAnimator.MainGlueBlend = Mathf.Clamp(legAnimator.MainGlueBlend + (__brain.Movement.CurrVelocity.sqrMagnitude > 0 ? -1 : 1) * legAnimGlueBlendSpeed * Time.deltaTime, __brain.Movement.freezeRotation ? 0.5f : 0.4f, 1);
            };
        }

        public bool Jump() 
        {
            mainAnimator.SetTrigger("Jump");
            mainAnimator.SetBool("IsJumping", true);

            return true;
        }

        public void Dash()
        {
            mainAnimator.SetTrigger("Rolling");
        }

        public void OnEventLand() 
        {
            mainAnimator.SetBool("IsJumping", false);
        }
        /*
        public void ChangeWeapon(WEAPONSLOT weaponSlot)
        {
            switch (weaponSlot) {
                case WEAPONSLOT.MAINSLOT:
                    mainAnimator.runtimeAnimatorController = _animControllers[0];
                    break;
                case WEAPONSLOT.SUBSLOT:
                    mainAnimator.runtimeAnimatorController = _animControllers[1];
                    break;
            }
        }
        */
    }

}