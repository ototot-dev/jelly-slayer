﻿using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Component")]
        public CursorController cursorCtrler;
        public KeyboardController keyboardCtrler;

        [Header("Property")]
        public StringReactiveProperty playerName = new();
        public ReactiveProperty<Vector2> moveVec = new();
        public ReactiveProperty<Vector3> lookVec = new();
        public Action<PawnBrainController> onPossessed;
        public Action<PawnBrainController> onUnpossessed;
        public HeroBrain ConrolledBrain { get; private set; }
        public GameObject ControlledPawn => ConrolledBrain != null ? ConrolledBrain.gameObject : null;

        public GameObject SpawnHero(GameObject heroPrefab, bool possessImmediately = false)
        {
            var spawnPosition = ControlledPawn != null ? ControlledPawn.transform.position : transform.position;
            var spawnRotation = ControlledPawn != null ? ControlledPawn.transform.rotation : Quaternion.LookRotation(Vector3.left + Vector3.back, Vector3.up);

            if (possessImmediately)
            {
                Possess(Instantiate(heroPrefab, spawnPosition, spawnRotation).GetComponent<HeroBrain>());
                return ControlledPawn;
            }
            else
            {
                return Instantiate(heroPrefab, spawnPosition, spawnRotation);
            }
        }

        public bool Possess(PawnBrainController targetBrain)
        {
            if (targetBrain == null || targetBrain.PawnBB.IsPossessed)
                return false;

            targetBrain.owner = this;
            ConrolledBrain = targetBrain as HeroBrain;

            transform.SetParent((Transform)targetBrain.transform, false);
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            targetBrain.OnPossessedHandler();
            onPossessed?.Invoke(targetBrain);

            __Logger.LogR(gameObject, nameof(Possess), nameof(targetBrain), targetBrain.name);
            return true;
        }

        public void UnpossessPawn(bool recoverBrain = false)
        {
            transform.SetParent(null, true);
            onUnpossessed?.Invoke(ConrolledBrain);

            Debug.Log($"1?? {ControlledPawn.name} is unpossessed by {gameObject.name}.");

            ConrolledBrain = null;
        }

        void Update()
        {
            if (ConrolledBrain == null)
                return;

            if (moveVec.Value.sqrMagnitude > 0)
            {
                var isAction = ConrolledBrain.BB.IsGuarding || ConrolledBrain.BB.IsCharging;
                ConrolledBrain.Movement.moveSpeed = isAction ? ConrolledBrain.BB.body.guardSpeed : ConrolledBrain.BB.body.walkSpeed;
                ConrolledBrain.Movement.moveVec = Quaternion.AngleAxis(45, Vector3.up) * new Vector3(moveVec.Value.x, 0, moveVec.Value.y);

                //* Strafe 모드가 아닌 경우엔 이동 방향과 회전 방향이 동일함
                if (!ConrolledBrain.Movement.freezeRotation)
                    ConrolledBrain.Movement.faceVec = ConrolledBrain.Movement.moveVec;
            }
            else
            {
                ConrolledBrain.Movement.moveVec = Vector3.zero;
                ConrolledBrain.Movement.moveSpeed = 0;
            }

            if (ConrolledBrain.Movement.freezeRotation)
            {
                if (ConrolledBrain.BB.TargetBrain != null)
                {
                    ConrolledBrain.Movement.faceVec = (ConrolledBrain.BB.TargetBrain.GetWorldPosition() - ConrolledBrain.Movement.capsule.position).Vector2D().normalized;
                    ConrolledBrain.AnimCtrler.HeadLookAt.position = ConrolledBrain.BB.TargetBrain.coreColliderHelper.transform.position + Vector3.up;

                    var targetCapsule = ConrolledBrain.BB.TargetBrain.coreColliderHelper.GetCapsuleCollider();
                    if (targetCapsule != null)
                        cursorCtrler.cursor.position = targetCapsule.transform.position + (targetCapsule.radius * 2f + targetCapsule.height) * Vector3.up;
                }
                else
                {
                    ConrolledBrain.Movement.faceVec = lookVec.Value;
                    cursorCtrler.cursor.position = ConrolledBrain.Movement.capsule.position + lookVec.Value;

                    // if (MyHeroBrain.SensorCtrler.ListeningColliders.Count > 0 && MyHeroBrain.SensorCtrler.ListeningColliders.FirstOrDefault() != null)
                    //     MyHeroBrain.AnimCtrler.HeadLookAt.position = MyHeroBrain.SensorCtrler.ListeningColliders.FirstOrDefault().transform.position + Vector3.up;
                }
            }
        }

        public void OnMove(InputValue value)
        {
            moveVec.Value = value.Get<Vector2>();
        }

        public void OnLook(InputValue value)
        {
            if (GameContext.Instance.cameraCtrler != null && GameContext.Instance.cameraCtrler.TryGetPickingPointOnTerrain(value.Get<Vector2>(), out var pickingPoint))
                lookVec.Value = (pickingPoint - ConrolledBrain.Movement.capsule.transform.position).Vector2D().normalized;
        }
        public void OnGuard(InputValue value)
        {
            if (ConrolledBrain == null)
                return;

            ConrolledBrain.ChangeWeapon(WeaponSetType.ONEHAND_WEAPONSHIELD);

            ConrolledBrain.BB.action.isGuarding.Value = value.Get<float>() > 0;
            if (ConrolledBrain.BB.IsGuarding)
            {
                var canParry1 = ConrolledBrain.BB.IsSpawnFinished && !ConrolledBrain.BB.IsDead && !ConrolledBrain.BB.IsGroggy && !ConrolledBrain.BB.IsJumping && !ConrolledBrain.BB.IsRolling;
                var canParry2 = canParry1 && (!ConrolledBrain.ActionCtrler.CheckActionRunning() || ConrolledBrain.ActionCtrler.CanInterruptAction());
                var canParry3 = canParry2 && !ConrolledBrain.StatusCtrler.CheckStatus(PawnStatus.Staggered) && !ConrolledBrain.StatusCtrler.CheckStatus(PawnStatus.CanNotGuard);
                if (canParry3)
                    ConrolledBrain.StatusCtrler.AddStatus(PawnStatus.GuardParrying, 1f, 0.1f);
            }
        }

        public void OnJump()
        {
            float jumpStaminaCost = 10;

            // 이동 가능 체크
            var canJump1 = ConrolledBrain.BB.IsSpawnFinished && !ConrolledBrain.BB.IsDead && !ConrolledBrain.BB.IsGroggy && !ConrolledBrain.BB.IsJumping && !ConrolledBrain.BB.IsRolling;
            var canJump2 = canJump1 && (!ConrolledBrain.ActionCtrler.CheckActionRunning() || ConrolledBrain.ActionCtrler.CanInterruptAction()) && !ConrolledBrain.StatusCtrler.CheckStatus(PawnStatus.Staggered);
            var canJump3 = canJump2;// && MyHeroBrain.PawnBB.stat.stamina.Value >= jumpStaminaCost;

            if (canJump3)
            {
                if (ConrolledBrain.ActionCtrler.CheckActionRunning())
                    ConrolledBrain.ActionCtrler.CancelAction(false);

                ConrolledBrain.Movement.StartJumping();
                ConrolledBrain.BB.action.isJumping.Value = true;
                ConrolledBrain.BB.stat.ReduceStamina(jumpStaminaCost);
            }
        }

        /*
        public void OnSwap() 
        {
            Debug.Log("OnSwap");
            WEAPONSLOT weaponSlot;
            if (MyHeroBrain._weaponSlot == WEAPONSLOT.MAINSLOT)
            {
                weaponSlot = WEAPONSLOT.SUBSLOT;
            }
            else
            {
                weaponSlot = WEAPONSLOT.MAINSLOT;
            }
            MyHeroBrain.ChangeWeapon(weaponSlot);
        }
        */

        public void OnNextTarget()
        {
            if (ConrolledBrain.BB.TargetPawn == null)
            {
                var newTarget = ConrolledBrain.PawnSensorCtrler.ListeningColliders.Select(c => c.GetComponent<PawnColliderHelper>())
                    .Where(h => h != null && h.pawnBrain != null && !h.pawnBrain.PawnBB.IsDead)
                    .OrderBy(p => (p.transform.position - ConrolledBrain.GetWorldPosition()).sqrMagnitude)
                    .FirstOrDefault();

                if (newTarget != null)
                {
                    ConrolledBrain.BB.target.targetPawnHP.Value = newTarget.pawnBrain.PawnHP;
                    ConrolledBrain.Movement.freezeRotation = true;
                }
            }
            else
            {
                var colliderHelpers = ConrolledBrain.PawnSensorCtrler.ListeningColliders.Select(c => c.GetComponent<PawnColliderHelper>())
                        .Where(h => h != null && h.pawnBrain != null && !h.pawnBrain.PawnBB.IsDead)
                        .ToArray();

                for (int i = 0; i < colliderHelpers.Length; i++)
                {
                    if (colliderHelpers[i].pawnBrain == ConrolledBrain.BB.TargetBrain)
                    {
                        ConrolledBrain.BB.target.targetPawnHP.Value = (i + 1 < colliderHelpers.Length ? colliderHelpers[i + 1] : colliderHelpers[0]).pawnBrain.PawnHP;
                        return;
                    }
                }
            }
        }

        public void OnPrevTarget()
        {
            if (ConrolledBrain.BB.TargetPawn == null)
            {
                var newTarget = ConrolledBrain.PawnSensorCtrler.ListeningColliders.Select(c => c.GetComponent<PawnColliderHelper>())
                    .Where(h => h != null && h.pawnBrain != null && !h.pawnBrain.PawnBB.IsDead)
                    .OrderBy(p => (p.transform.position - ConrolledBrain.GetWorldPosition()).sqrMagnitude)
                    .FirstOrDefault();

                if (newTarget != null)
                {
                    ConrolledBrain.BB.target.targetPawnHP.Value = newTarget.pawnBrain.PawnHP;
                    ConrolledBrain.Movement.freezeRotation = true;
                }
            }
            else
            {
                var colliderHelpers = ConrolledBrain.PawnSensorCtrler.ListeningColliders.Select(c => c.GetComponent<PawnColliderHelper>())
                        .Where(h => h != null && h.pawnBrain != null && !h.pawnBrain.PawnBB.IsDead)
                        .ToArray();

                for (int i = colliderHelpers.Length - 1; i >= 0; i--)
                {
                    if (colliderHelpers[i].pawnBrain == ConrolledBrain.BB.TargetBrain)
                    {
                        ConrolledBrain.BB.target.targetPawnHP.Value = (i - 1 >= 0 ? colliderHelpers[i - 1] : colliderHelpers[colliderHelpers.Length - 1]).pawnBrain.PawnHP;
                        return;
                    }
                }
            }
        }

        public void OnRoll()
        {
            var actionData = DatasheetManager.Instance.GetActionData(ConrolledBrain.PawnBB.common.pawnId, "Rolling");
            Debug.Assert(actionData != null);

            // 대쉬 가능 체크
            var canRolling1 = ConrolledBrain.BB.IsSpawnFinished && !ConrolledBrain.BB.IsDead && !ConrolledBrain.BB.IsGroggy && !ConrolledBrain.BB.IsJumping && !ConrolledBrain.BB.IsRolling;
            var canRolling2 = canRolling1 && (!ConrolledBrain.ActionCtrler.CheckActionRunning() || ConrolledBrain.ActionCtrler.CanInterruptAction()) && !ConrolledBrain.StatusCtrler.CheckStatus(PawnStatus.CanNotRoll);

            if (canRolling2 == false)
                return;

            if (ConrolledBrain.ActionCtrler.CheckActionRunning())
                ConrolledBrain.ActionCtrler.CancelAction(false);

            var rollingXZ = Vector3.zero;
            var rollingVec = Vector3.zero;
            if (ConrolledBrain.Movement.moveVec == Vector3.zero)
            {
                rollingXZ = Vector3.back;
                rollingVec = -ConrolledBrain.Movement.capsule.forward.Vector2D().normalized;
            }
            else
            {
                rollingXZ = ConrolledBrain.Movement.capsule.InverseTransformDirection(ConrolledBrain.Movement.moveVec);
                rollingVec = ConrolledBrain.Movement.moveVec.Vector2D().normalized;
            }

            if (Mathf.Abs(rollingXZ.x) > Mathf.Abs(rollingXZ.z))
            {
                ConrolledBrain.AnimCtrler.mainAnimator.SetInteger("RollingX", rollingXZ.x > 0f ? 1 : -1);
                ConrolledBrain.AnimCtrler.mainAnimator.SetInteger("RollingZ", 0);
                ConrolledBrain.Movement.FaceTo(Quaternion.Euler(0f, rollingXZ.x > 0f ? -90f : 90f, 0f) * rollingVec);
            }
            else
            {
                ConrolledBrain.AnimCtrler.mainAnimator.SetInteger("RollingX", 0);
                ConrolledBrain.AnimCtrler.mainAnimator.SetInteger("RollingZ", rollingXZ.z > 0f ? 1 : -1);
                ConrolledBrain.Movement.FaceTo(rollingXZ.z > 0f ? rollingVec : -rollingVec);
            }

            ConrolledBrain.ActionCtrler.SetPendingAction("Rolling");

            // Roll Sound
            EffectManager.Instance.Show("FX_Cartoony_Jump_Up_01", ConrolledBrain.GetWorldPosition(),
                Quaternion.identity, Vector3.one, 1f);
            SoundManager.Instance.Play(SoundID.JUMP);
        }

        public void OnParry()
        {
            if (ConrolledBrain == null)
                return;

            var canAction1 = ConrolledBrain.BB.IsSpawnFinished && !ConrolledBrain.BB.IsDead && !ConrolledBrain.BB.IsGroggy && !ConrolledBrain.BB.IsRolling;
            var canAction2 = canAction1 && (!ConrolledBrain.ActionCtrler.CheckActionRunning() || ConrolledBrain.ActionCtrler.CanInterruptAction()) && !ConrolledBrain.StatusCtrler.CheckStatus(PawnStatus.Staggered);

            if (canAction2)
            {

                if (ConrolledBrain.ActionCtrler.CheckActionRunning())
                    ConrolledBrain.ActionCtrler.CancelAction(false);

                ConrolledBrain.ActionCtrler.SetPendingAction("Kick");
            }
        }

        IDisposable __chargingAttackDisposable;
        float __attackPresssedTimeStamp = -1f;
        float __attackReleasedTimeStamp = -1f;

        public void OnAttack(InputValue value)
        {
            if (ConrolledBrain == null)
                return;

            __chargingAttackDisposable ??= Observable.EveryUpdate().Where(_ => __attackPresssedTimeStamp > __attackReleasedTimeStamp).Subscribe(_ =>
            {
                if (ConrolledBrain.ActionCtrler.CheckActionRunning())
                {
                    __attackReleasedTimeStamp = Time.time;
                    ConrolledBrain.BB.action.isCharging.Value = false;
                    ConrolledBrain.BB.action.chargingLevel.Value = 0;

                    __Logger.LogF(gameObject, nameof(OnAttack), "Charging canceled.", "CurrActionName", ConrolledBrain.ActionCtrler.CurrActionName);
                }
                else
                {
                    var chargingTime = Time.time - __attackPresssedTimeStamp;

                    //* 챠징 판정 시간은 1초
                    if (chargingTime > 1f)
                    {
                        if (ConrolledBrain.BB.action.isCharging.Value == false)
                        {
                            Observable.Timer(TimeSpan.FromSeconds(0.2f)).Subscribe(_ 
                                => EffectManager.Instance.Show("ChonkExplosionBlue", 
                                ConrolledBrain.GetWorldPosition() + Vector3.up,
                                Quaternion.identity, 0.8f * Vector3.one, 1f)).AddTo(this);
                        }
                        ConrolledBrain.BB.action.isCharging.Value = true;
                        ConrolledBrain.BB.action.chargingLevel.Value = Mathf.FloorToInt(Time.time - __attackPresssedTimeStamp) + 1;

                        ConrolledBrain.ChangeWeapon(WeaponSetType.TWOHAND_WEAPON);
                    }
                    else
                    {
                        ConrolledBrain.BB.action.isCharging.Value = false;
                        ConrolledBrain.BB.action.chargingLevel.Value = 0;
                    }
                }
            }).AddTo(this);

            var canAction1 = ConrolledBrain.BB.IsSpawnFinished && !ConrolledBrain.BB.IsDead && !ConrolledBrain.BB.IsGroggy && !ConrolledBrain.BB.IsRolling;
            // var canAction2 = canAction1 && !MyHeroBrain.PawnBB.IsThrowing && !MyHeroBrain.PawnBB.IsGrabbed;
            var canAction3 = canAction1 && !ConrolledBrain.StatusCtrler.CheckStatus(PawnStatus.Staggered);

            if (value.isPressed)
            {
                __attackPresssedTimeStamp = Time.time;

                var baseActionName = ConrolledBrain.ActionCtrler.CheckActionRunning() ? ConrolledBrain.ActionCtrler.CurrActionName : ConrolledBrain.ActionCtrler.PrevActionName;
                var canNextAction = ConrolledBrain.ActionCtrler.CheckActionRunning() ? ConrolledBrain.ActionCtrler.CanInterruptAction() : (Time.time - ConrolledBrain.ActionCtrler.prevActionContext.finishTimeStamp) < MainTable.PlayerData.GetList().First().actionInputTimePadding;
                if (canAction3 && canNextAction)
                {
                    switch (baseActionName)
                    {
                        case "Slash#1":
                            ConrolledBrain.ActionCtrler.CancelAction(false);
                            ConrolledBrain.ActionCtrler.SetPendingAction("Slash#2");
                            break;
                        case "Slash#2":
                            ConrolledBrain.ActionCtrler.CancelAction(false);
                            ConrolledBrain.ActionCtrler.SetPendingAction("Slash#3");
                            break;
                        case "HeavySlash#1":
                            ConrolledBrain.ActionCtrler.CancelAction(false);
                            ConrolledBrain.ActionCtrler.SetPendingAction("HeavySlash#2");
                            break;
                        case "HeavySlash#2":
                            ConrolledBrain.ActionCtrler.CancelAction(false);
                            ConrolledBrain.ActionCtrler.SetPendingAction("HeavySlash#3");
                            break;
                    }

                    //* 타겟이 없을 경우에도 조준 보정을 해줌
                    if (ConrolledBrain.BB.TargetBrain == null && ConrolledBrain.SensorCtrler.ListeningColliders.Count > 0)
                    {
                        var attackPoint = ConrolledBrain.SensorCtrler.ListeningColliders.Select(c => c.transform.position)
                            .OrderBy(p => Vector3.Angle(ConrolledBrain.coreColliderHelper.transform.forward.Vector2D(), (p - ConrolledBrain.coreColliderHelper.transform.position).Vector2D()))
                            .FirstOrDefault();

                        ConrolledBrain.Movement.FaceAt(attackPoint);
                    }
                }
            }
            else
            {
                __attackReleasedTimeStamp = Time.time;

                if (canAction3 && !ConrolledBrain.ActionCtrler.CheckActionRunning())
                {
                    if (ConrolledBrain.BB.IsJumping)
                    {
                        ConrolledBrain.ActionCtrler.SetPendingAction("JumpAttack");
                        ConrolledBrain.ChangeWeapon(WeaponSetType.TWOHAND_WEAPON);
                    }
                    else if (ConrolledBrain.BB.IsCharging)
                    {
                        ConrolledBrain.ActionCtrler.SetPendingAction("HeavySlash#1");
                        ConrolledBrain.ChangeWeapon(WeaponSetType.TWOHAND_WEAPON);
                    }
                    else
                    {
                        ConrolledBrain.ActionCtrler.SetPendingAction("Slash#1");
                        ConrolledBrain.ChangeWeapon(WeaponSetType.ONEHAND_WEAPONSHIELD);
                    }

                    //* 타겟이 없을 경우에도 조준 보정을 해줌
                    if (ConrolledBrain.BB.TargetBrain == null && ConrolledBrain.SensorCtrler.ListeningColliders.Count > 0)
                    {
                        var attackPoint = ConrolledBrain.SensorCtrler.ListeningColliders.Select(c => c.transform.position)
                            .OrderBy(p => Vector3.Angle(ConrolledBrain.coreColliderHelper.transform.forward.Vector2D(), (p - ConrolledBrain.coreColliderHelper.transform.position).Vector2D()))
                            .FirstOrDefault();

                        ConrolledBrain.Movement.FaceAt(attackPoint);
                    }
                }

                //* 챠징 어택 판별을 위해서 'isCharging' 값은 제일 마지막에 리셋
                ConrolledBrain.BB.action.isCharging.Value = false;
            }
        }

        public void OnSpecialAttack(InputValue value)
        {
            if (ConrolledBrain == null)
                return;

            if (value.isPressed)
            {
                var canAction1 = ConrolledBrain.BB.IsSpawnFinished && !ConrolledBrain.BB.IsDead && !ConrolledBrain.BB.IsGroggy && !ConrolledBrain.BB.IsRolling;
                var canAction2 = canAction1 && (!ConrolledBrain.ActionCtrler.CheckActionRunning() || ConrolledBrain.ActionCtrler.CanInterruptAction()) && !ConrolledBrain.StatusCtrler.CheckStatus(PawnStatus.Staggered);

                if (canAction2)
                {

                    if (ConrolledBrain.ActionCtrler.CheckActionRunning())
                        ConrolledBrain.ActionCtrler.CancelAction(false);

                    ConrolledBrain.ActionCtrler.SetPendingAction("SpecialKick");
                }
            }
        }
        public void OnDrink() 
        {
            Debug.Log("<color=red>OnDrink</color>");
            ConrolledBrain.ActionCtrler.SetPendingAction("DrinkPotion");
        }
    }
}