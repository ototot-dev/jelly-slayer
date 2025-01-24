using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Game
{
    public class Etasphera42_AnimController : PawnAnimController
    {
        [Header("Component")]
        public Transform hookingPoint;
        public Transform turretBody;
        public Transform leftTurret;
        public Transform rightTurret;
        public Transform centerTurret;
        public Transform topTurret1;
        public Transform topTurret2;

        [Header("Parameter")]
        public float rigBlendWeight = 1f;
        public float rigBlendSpeed = 1f;
        public float legAnimGlueBlendSpeed = 1f;
        public float ragDollAnimBlendSpeed = 1f;
        public float actionLayerBlendSpeed = 1f;
        Etasphera42_Brain __brain;
        Rig __rig;

        public enum TurretIndices : int
        {
            Top,
            Left,
            Right,
            Center,
            Max,
        }

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
            __turretBodyRotationCached = turretBody.transform.localRotation;
            __leftTurretRotationCached = leftTurret.transform.localRotation;
            __rightTurretRotationCached = rightTurret.transform.localRotation;
            __centerTurretRotationCached = centerTurret.transform.localRotation;
            __topTurret1_RotationCached = topTurret1.transform.localRotation;
            __topTurret2_RotationCached = topTurret2.transform.localRotation;
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
                    UpdateTurretRotation(__brain.BB.TargetColliderHelper.GetWorldCenter());

                // if (__brain.BB.TargetColliderHelper != null)
                // {
                //     bodyOverrideTransform.data.rotation = 
                //         new Vector3(-Vector3.SignedAngle(__brain.coreColliderHelper.transform.forward.Vector2D(), (__brain.BB.TargetColliderHelper.transform.position - __brain.coreColliderHelper.transform.position).Vector2D(), Vector3.up), 0f, 0f);
                // }
                // else
                // {
                //     bodyOverrideTransform.data.rotation = Vector3.zero;
                // }

                // UpdateTurretPitch();
            };

            __brain.PawnHP.onDead += (_) =>
            {
                mainAnimator.SetTrigger("OnDead");
            };
        }

        Quaternion __turretBodyRotationCached;
        Quaternion __leftTurretRotationCached;
        Quaternion __rightTurretRotationCached;
        Quaternion __centerTurretRotationCached;
        Quaternion __topTurret1_RotationCached;
        Quaternion __topTurret2_RotationCached;

        void UpdateTurretRotation(Vector3 aimPoint, TurretIndices primaryTurretIndex = TurretIndices.Max)
        {
            //* turretBody의 로컬 피봇으로 인해서 Right 벡터가 Up 벡터가 됨 (또한 90도 돌아가 있어서 이를 역보정해주기 위해 Vector3.down 축을 Forward 축으로 대입함) 
            var deltaAngle1 = Vector3.SignedAngle(Vector3.down, turretBody.transform.InverseTransformPoint(aimPoint).AdjustX(0f), Vector3.right);
            turretBody.transform.localRotation = __turretBodyRotationCached.LerpRefAngleSpeed(turretBody.transform.localRotation * Quaternion.Euler(deltaAngle1, 0f, 0f), __brain.BB.body.turretBodyRotateSpeed, Time.deltaTime);

            //* leftTurret와 rightTurrent의 로컬 피봇도 Right 벡터가 Up 벡터가 됨
            var deltaAngle2 = Vector3.SignedAngle(Vector3.forward, leftTurret.transform.InverseTransformPoint(aimPoint).AdjustX(0f), Vector3.right);
            leftTurret.transform.localRotation = __leftTurretRotationCached.LerpRefAngleSpeed(leftTurret.transform.localRotation * Quaternion.Euler(deltaAngle2, 0f, 0f), __brain.BB.body.leftTurretRotateSpeed, Time.deltaTime);
            rightTurret.transform.localRotation = __rightTurretRotationCached.LerpRefAngleSpeed(rightTurret.transform.localRotation * Quaternion.Euler(deltaAngle2, 0f, 0f), __brain.BB.body.rightTurretRotateSpeed, Time.deltaTime);
            // leftTurret.transform.localRotation *= Quaternion.Euler(deltaAngle2, 0f, 0f);
            // rightTurret.transform.localRotation *= Quaternion.Euler(deltaAngle2, 0f, 0f);

            //* centerTurret은 피봇 정상
            var deltaAngle3 = Vector3.SignedAngle(Vector3.forward, centerTurret.transform.InverseTransformPoint(aimPoint).AdjustY(0f), Vector3.up);
            centerTurret.transform.localRotation = __centerTurretRotationCached.LerpRefAngleSpeed(centerTurret.transform.localRotation * Quaternion.Euler(0f, deltaAngle3, 0f), __brain.BB.body.centerTurretRotateSpeed, Time.deltaTime);
            // centerTurret.transform.localRotation *= Quaternion.Euler(0f, deltaAngle3, 0f);

            //* topTurret1도 피봇 정상
            var deltaAngle4 = Vector3.SignedAngle(Vector3.forward, topTurret1.transform.InverseTransformPoint(aimPoint).AdjustY(0f), Vector3.up);
            topTurret1.transform.localRotation = __topTurret1_RotationCached.LerpRefAngleSpeed(topTurret1.transform.localRotation * Quaternion.Euler(0f, deltaAngle4, 0f), __brain.BB.body.topTurret1_RotateSpeed, Time.deltaTime);
            // topTurret1.transform.localRotation *= Quaternion.Euler(0f, deltaAngle4, 0f);

            //* topTurret2의 로컬 피봇은 Right 벡터가 Up 벡터가 됨
            var deltaAngle5 = Vector3.SignedAngle(Vector3.forward, topTurret2.transform.InverseTransformPoint(aimPoint).AdjustX(0f), Vector3.right);
            topTurret2.transform.localRotation = __topTurret2_RotationCached.LerpRefAngleSpeed(topTurret2.transform.localRotation * Quaternion.Euler(deltaAngle5, 0f, 0f), __brain.BB.body.topTurret2_RotateSpeed, Time.deltaTime);
            // topTurret2.transform.localRotation *= Quaternion.Euler(deltaAngle5, 0f, 0f);
        }
    }
}