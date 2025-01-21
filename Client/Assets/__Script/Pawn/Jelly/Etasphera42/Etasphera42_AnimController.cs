using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Game
{
    public class Etasphera42_AnimController : PawnAnimController
    {
        [Header("Component")]
        public Transform hookingPoint;
        public Transform leftTurret;
        public Transform rightTurret;
        public OverrideTransform bodyOverrideTransform;

        [Header("Parameter")]
        public float rigBlendWeight = 1f;
        public float rigBlendSpeed = 1f;
        public float legAnimGlueBlendSpeed = 1f;
        public float ragDollAnimBlendSpeed = 1f;
        public float actionLayerBlendSpeed = 1f;
        Etasphera42_Brain __brain;
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
            __brain = GetComponent<Etasphera42_Brain>();
        }

        void Start()
        {
            __brain.onUpdate += () =>
            {
                if (__brain.BB.IsDown)
                    __brain.Movement.AddRootMotion(mainAnimator.deltaPosition, mainAnimator.deltaRotation);
                else if (__brain.ActionCtrler.CheckActionRunning() && __brain.ActionCtrler.CanRootMotion(mainAnimator.deltaPosition))
                    __brain.Movement.AddRootMotion(__brain.ActionCtrler.GetRootMotionMultiplier() * mainAnimator.deltaPosition, mainAnimator.deltaRotation);

                mainAnimator.transform.SetPositionAndRotation(__brain.coreColliderHelper.transform.position, __brain.coreColliderHelper.transform.rotation);
                // mainAnimator.SetLayerWeight((int)LayerIndices.Action, Mathf.Clamp01(mainAnimator.GetLayerWeight((int)LayerIndices.Action) + (__brain.ActionCtrler.CheckActionRunning() ? actionLayerBlendSpeed : -actionLayerBlendSpeed) * Time.deltaTime));
                // mainAnimator.SetLayerWeight((int)LayerIndices.Addictive, 1f);
                // mainAnimator.SetBool("IsMoving", __brain.Movement.CurrVelocity.sqrMagnitude > 0 && !__brain.ActionCtrler.CheckKnockBackRunning());
                // mainAnimator.SetBool("IsMovingStrafe", __brain.Movement.freezeRotation);
                // mainAnimator.SetFloat("MoveSpeed", __brain.Movement.CurrVelocity.magnitude);
                // mainAnimator.SetFloat("MoveAnimSpeed", 1f);

                var animMoveVec = __brain.coreColliderHelper.transform.InverseTransformDirection(__brain.Movement.CurrVelocity).Vector2D();
                // mainAnimator.SetFloat("MoveX", animMoveVec.x / __brain.Movement.moveSpeed);
                // mainAnimator.SetFloat("MoveY", animMoveVec.z / __brain.Movement.moveSpeed);

                // mainAnimator.SetLayerWeight((int)LayerIndices.Lower, 0f);

                if (__brain.BB.IsDead)
                {
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
                else if (__brain.BB.IsDown || __brain.BB.IsJumping || __brain.BB.IsFalling)
                {
                    legAnimator.LegsAnimatorBlend = 0f;
                    legAnimator.User_SetIsMoving(false);
                    legAnimator.User_SetIsGrounded(false);
                }
                else
                {
                    legAnimator.LegsAnimatorBlend = 1f;
                    legAnimator.MainGlueBlend = 1f;

                    // legAnimator.User_SetIsMoving(__brain.Movement.CurrVelocity.sqrMagnitude > 0 && !__brain.ActionCtrler.CheckActionRunning() && !__brain.ActionCtrler.CheckKnockBackRunning());
                    // legAnimator.User_SetIsMoving(__brain.Movement.CurrVelocity.sqrMagnitude > 0 && !__brain.ActionCtrler.CheckActionRunning() && !__brain.ActionCtrler.CheckKnockBackRunning());
                    legAnimator.User_SetIsGrounded(__brain.Movement.IsOnGround);
                }

                ragdollAnimator.RagdollBlend = Mathf.Max(0f, ragdollAnimator.RagdollBlend - ragDollAnimBlendSpeed * Time.time);
            };

            __brain.onLateUpdate += () =>
            {
                __brain.ActionCtrler.hookingPointColliderHelper.transform.position = hookingPoint.transform.position;

                if (__brain.BB.TargetColliderHelper != null)
                {
                    bodyOverrideTransform.data.rotation = 
                        new Vector3(-Vector3.SignedAngle(__brain.coreColliderHelper.transform.forward.Vector2D(), (__brain.BB.TargetColliderHelper.transform.position - __brain.coreColliderHelper.transform.position).Vector2D(), Vector3.up), 0f, 0f);
                }
                else
                {
                    bodyOverrideTransform.data.rotation = Vector3.zero;
                }

                UpdateTurretPitch();
            };

            __brain.PawnHP.onDead += (_) =>
            {
                mainAnimator.SetTrigger("OnDead");
            };
        }

        float __currTurretPitxh;

        void UpdateTurretPitch()
        {
            if (__brain.BB.TargetBrain != null && __brain.ActionCtrler.CheckActionRunning())
            {   
                var aimVec = __brain.BB.TargetColliderHelper.GetWorldCenter() - 0.5f * (leftTurret.transform.position + rightTurret.transform.position);
                var turretPitch = Vector3.SignedAngle(-hookingPoint.transform.up, aimVec, leftTurret.transform.right);
                turretPitch = Mathf.Clamp(turretPitch, __brain.BB.action.turretHighPitch, __brain.BB.action.turretLowPitch);
                __currTurretPitxh = __currTurretPitxh.LerpSpeed( turretPitch, __brain.BB.action.turretPitchSpeed, Time.deltaTime);
            }
            else
            {
                __currTurretPitxh = __currTurretPitxh.LerpSpeed( 0f, __brain.BB.action.turretPitchRecoverSpeed, Time.deltaTime);
            }

            leftTurret.transform.localRotation = Quaternion.Euler(90f + __currTurretPitxh, -90f, 0f);
            rightTurret.transform.localRotation = Quaternion.Euler(-90f + __currTurretPitxh, -90f, 0f);
        }
    }
}