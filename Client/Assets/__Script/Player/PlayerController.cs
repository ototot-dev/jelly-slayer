using System;
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
        public HeroBrain heroBrain;
        public DroneBotBrain droneBotBrain;
        public GameObject HeroPawn => heroBrain != null ? heroBrain.gameObject : null;
        public GameObject DroneBot => droneBotBrain != null ? droneBotBrain.gameObject : null;

        [Header("Property")]
        public StringReactiveProperty playerName = new();
        public ReactiveProperty<Vector2> moveVec = new();
        public ReactiveProperty<Vector3> lookVec = new();
        public Action<PawnBrainController> onPossessed;
        public Action<PawnBrainController> onUnpossessed;

        public GameObject SpawnHero(GameObject heroPrefab, bool possessImmediately = false)
        {
            var spawnPosition = heroBrain != null ? heroBrain.GetWorldPosition() : transform.position;
            var spawnRotation = heroBrain != null ? heroBrain.GetWorldRotation() : Quaternion.LookRotation(Vector3.left + Vector3.back, Vector3.up);

            if (possessImmediately)
            {
                Possess(Instantiate(heroPrefab, spawnPosition, spawnRotation).GetComponent<HeroBrain>());
                return HeroPawn;
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
            heroBrain = targetBrain as HeroBrain;

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
            onUnpossessed?.Invoke(heroBrain);

            Debug.Log($"1?? {HeroPawn.name} is unpossessed by {gameObject.name}.");

            heroBrain = null;
        }

        void Update()
        {
            if (heroBrain == null)
                return;

            if (moveVec.Value.sqrMagnitude > 0)
            {
                var isAction = heroBrain.BB.IsGuarding || heroBrain.BB.IsCharging;
                heroBrain.Movement.moveSpeed = isAction ? heroBrain.BB.body.guardSpeed : heroBrain.BB.body.walkSpeed;
                heroBrain.Movement.moveVec = Quaternion.AngleAxis(45, Vector3.up) * new Vector3(moveVec.Value.x, 0, moveVec.Value.y);

                //* Strafe 모드가 아닌 경우엔 이동 방향과 회전 방향이 동일함
                if (!heroBrain.Movement.freezeRotation)
                    heroBrain.Movement.faceVec = heroBrain.Movement.moveVec;
            }
            else
            {
                heroBrain.Movement.moveVec = Vector3.zero;
                heroBrain.Movement.moveSpeed = 0;
            }

            if (heroBrain.Movement.freezeRotation)
            {
                if (heroBrain.BB.TargetBrain != null)
                {
                    heroBrain.Movement.faceVec = (heroBrain.BB.TargetBrain.GetWorldPosition() - heroBrain.Movement.capsule.position).Vector2D().normalized;
                    heroBrain.AnimCtrler.HeadLookAt.position = heroBrain.BB.TargetBrain.coreColliderHelper.transform.position + Vector3.up;

                    var targetCapsule = heroBrain.BB.TargetBrain.coreColliderHelper.GetCapsuleCollider();
                    if (targetCapsule != null)
                        cursorCtrler.cursor.position = targetCapsule.transform.position + (targetCapsule.radius * 2f + targetCapsule.height) * Vector3.up;
                }
                else
                {
                    heroBrain.Movement.faceVec = lookVec.Value;
                    cursorCtrler.cursor.position = heroBrain.Movement.capsule.position + lookVec.Value;

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
                lookVec.Value = (pickingPoint - heroBrain.Movement.capsule.transform.position).Vector2D().normalized;
        }
        public void OnGuard(InputValue value)
        {
            if (heroBrain == null)
                return;

            heroBrain.ChangeWeapon(WeaponSetType.ONEHAND_WEAPONSHIELD);

            heroBrain.BB.action.isGuarding.Value = value.Get<float>() > 0;
            if (heroBrain.BB.IsGuarding)
            {
                var canParry1 = heroBrain.BB.IsSpawnFinished && !heroBrain.BB.IsDead && !heroBrain.BB.IsGroggy && !heroBrain.BB.IsJumping && !heroBrain.BB.IsRolling;
                var canParry2 = canParry1 && (!heroBrain.ActionCtrler.CheckActionRunning() || heroBrain.ActionCtrler.CanInterruptAction());
                var canParry3 = canParry2 && !heroBrain.StatusCtrler.CheckStatus(PawnStatus.Staggered) && !heroBrain.StatusCtrler.CheckStatus(PawnStatus.CanNotGuard);
                if (canParry3)
                    heroBrain.StatusCtrler.AddStatus(PawnStatus.GuardParrying, 1f, 0.1f);
            }
        }

        IDisposable __jumpHangingDisposable;
        float __jumpExecutedTimeStamp = -1f;
        float __jumpReleasedTimeStamp = -1f;

        public void OnJump(InputValue value)
        {
            __jumpHangingDisposable ??= Observable.EveryUpdate().Where(_ => __jumpExecutedTimeStamp > __jumpReleasedTimeStamp).Subscribe(_ =>
            {
                //* Catch 판정은 0.1초로 셋팅
                if (droneBotBrain != null && droneBotBrain.BB.CurrDecision != DroneBotBrain.Decisions.Catch && droneBotBrain.BB.CurrDecision != DroneBotBrain.Decisions.Hanging && Time.time - __jumpExecutedTimeStamp > 0.1f)
                    heroBrain.Movement.PrepareHanging(droneBotBrain);
            }).AddTo(this);

            float jumpStaminaCost = 10;

            if (value.isPressed)
            {   
                var canJump1 = heroBrain.BB.IsSpawnFinished && !heroBrain.BB.IsDead && !heroBrain.BB.IsGroggy && !heroBrain.BB.IsJumping && !heroBrain.BB.IsRolling;
                var canJump2 = canJump1 && (!heroBrain.ActionCtrler.CheckActionRunning() || heroBrain.ActionCtrler.CanInterruptAction()) && !heroBrain.StatusCtrler.CheckStatus(PawnStatus.Staggered);

                if (canJump2)
                {
                    if (heroBrain.ActionCtrler.CheckActionRunning())
                        heroBrain.ActionCtrler.CancelAction(false);
                        
                    if (heroBrain.BB.IsHanging)
                    {
                        heroBrain.Movement.FinishHanging();
                        heroBrain.InvalidateDecision(0f);
                    }
                    else
                    {
                        __jumpExecutedTimeStamp = Time.time;
                    }

                    heroBrain.Movement.StartJumping();
                    heroBrain.BB.action.isJumping.Value = true;
                    heroBrain.BB.stat.ReduceStamina(jumpStaminaCost);
                }
            }
            else
            {
                __jumpReleasedTimeStamp = Time.time;
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
            if (heroBrain.BB.TargetPawn == null)
            {
                var newTarget = heroBrain.PawnSensorCtrler.ListeningColliders.Select(c => c.GetComponent<PawnColliderHelper>())
                    .Where(h => h != null && h.pawnBrain != null && !h.pawnBrain.PawnBB.IsDead)
                    .OrderBy(p => (p.transform.position - heroBrain.GetWorldPosition()).sqrMagnitude)
                    .FirstOrDefault();

                if (newTarget != null)
                {
                    heroBrain.BB.target.targetPawnHP.Value = newTarget.pawnBrain.PawnHP;
                    heroBrain.Movement.freezeRotation = true;
                }
            }
            else
            {
                var colliderHelpers = heroBrain.PawnSensorCtrler.ListeningColliders.Select(c => c.GetComponent<PawnColliderHelper>())
                        .Where(h => h != null && h.pawnBrain != null && !h.pawnBrain.PawnBB.IsDead)
                        .ToArray();

                for (int i = 0; i < colliderHelpers.Length; i++)
                {
                    if (colliderHelpers[i].pawnBrain == heroBrain.BB.TargetBrain)
                    {
                        heroBrain.BB.target.targetPawnHP.Value = (i + 1 < colliderHelpers.Length ? colliderHelpers[i + 1] : colliderHelpers[0]).pawnBrain.PawnHP;
                        return;
                    }
                }
            }
        }

        public void OnPrevTarget()
        {
            if (heroBrain.BB.TargetPawn == null)
            {
                var newTarget = heroBrain.PawnSensorCtrler.ListeningColliders.Select(c => c.GetComponent<PawnColliderHelper>())
                    .Where(h => h != null && h.pawnBrain != null && !h.pawnBrain.PawnBB.IsDead)
                    .OrderBy(p => (p.transform.position - heroBrain.GetWorldPosition()).sqrMagnitude)
                    .FirstOrDefault();

                if (newTarget != null)
                {
                    heroBrain.BB.target.targetPawnHP.Value = newTarget.pawnBrain.PawnHP;
                    heroBrain.Movement.freezeRotation = true;
                }
            }
            else
            {
                var colliderHelpers = heroBrain.PawnSensorCtrler.ListeningColliders.Select(c => c.GetComponent<PawnColliderHelper>())
                        .Where(h => h != null && h.pawnBrain != null && !h.pawnBrain.PawnBB.IsDead)
                        .ToArray();

                for (int i = colliderHelpers.Length - 1; i >= 0; i--)
                {
                    if (colliderHelpers[i].pawnBrain == heroBrain.BB.TargetBrain)
                    {
                        heroBrain.BB.target.targetPawnHP.Value = (i - 1 >= 0 ? colliderHelpers[i - 1] : colliderHelpers[colliderHelpers.Length - 1]).pawnBrain.PawnHP;
                        return;
                    }
                }
            }
        }

        public void OnRoll()
        {
            var actionData = DatasheetManager.Instance.GetActionData(heroBrain.PawnBB.common.pawnId, "Rolling");
            Debug.Assert(actionData != null);

            // 대쉬 가능 체크
            var canRolling1 = heroBrain.BB.IsSpawnFinished && !heroBrain.BB.IsDead && !heroBrain.BB.IsGroggy && !heroBrain.BB.IsJumping && !heroBrain.BB.IsRolling;
            var canRolling2 = canRolling1 && (!heroBrain.ActionCtrler.CheckActionRunning() || heroBrain.ActionCtrler.CanInterruptAction()) && !heroBrain.StatusCtrler.CheckStatus(PawnStatus.CanNotRoll);

            if (canRolling2 == false)
                return;

            if (heroBrain.ActionCtrler.CheckActionRunning())
                heroBrain.ActionCtrler.CancelAction(false);

            var rollingXZ = Vector3.zero;
            var rollingVec = Vector3.zero;
            if (heroBrain.Movement.moveVec == Vector3.zero)
            {
                rollingXZ = Vector3.back;
                rollingVec = -heroBrain.Movement.capsule.forward.Vector2D().normalized;
            }
            else
            {
                rollingXZ = heroBrain.Movement.capsule.InverseTransformDirection(heroBrain.Movement.moveVec);
                rollingVec = heroBrain.Movement.moveVec.Vector2D().normalized;
            }

            if (Mathf.Abs(rollingXZ.x) > Mathf.Abs(rollingXZ.z))
            {
                heroBrain.AnimCtrler.mainAnimator.SetInteger("RollingX", rollingXZ.x > 0f ? 1 : -1);
                heroBrain.AnimCtrler.mainAnimator.SetInteger("RollingZ", 0);
                heroBrain.Movement.FaceTo(Quaternion.Euler(0f, rollingXZ.x > 0f ? -90f : 90f, 0f) * rollingVec);
            }
            else
            {
                heroBrain.AnimCtrler.mainAnimator.SetInteger("RollingX", 0);
                heroBrain.AnimCtrler.mainAnimator.SetInteger("RollingZ", rollingXZ.z > 0f ? 1 : -1);
                heroBrain.Movement.FaceTo(rollingXZ.z > 0f ? rollingVec : -rollingVec);
            }

            heroBrain.ActionCtrler.SetPendingAction("Rolling");

            // Roll Sound
            EffectManager.Instance.Show("FX_Cartoony_Jump_Up_01", heroBrain.GetWorldPosition(),
                Quaternion.identity, Vector3.one, 1f);
            SoundManager.Instance.Play(SoundID.JUMP);
        }

        public void OnParry()
        {
            if (heroBrain == null)
                return;

            var canAction1 = heroBrain.BB.IsSpawnFinished && !heroBrain.BB.IsDead && !heroBrain.BB.IsGroggy && !heroBrain.BB.IsRolling;
            var canAction2 = canAction1 && (!heroBrain.ActionCtrler.CheckActionRunning() || heroBrain.ActionCtrler.CanInterruptAction()) && !heroBrain.StatusCtrler.CheckStatus(PawnStatus.Staggered);

            if (canAction2)
            {

                if (heroBrain.ActionCtrler.CheckActionRunning())
                    heroBrain.ActionCtrler.CancelAction(false);

                heroBrain.ActionCtrler.SetPendingAction("Kick");
            }
        }

        IDisposable __chargingAttackDisposable;
        float __attackPresssedTimeStamp = -1f;
        float __attackReleasedTimeStamp = -1f;

        public void OnAttack(InputValue value)
        {
            if (heroBrain == null)
                return;

            __chargingAttackDisposable ??= Observable.EveryUpdate().Where(_ => __attackPresssedTimeStamp > __attackReleasedTimeStamp).Subscribe(_ =>
            {
                if (heroBrain.ActionCtrler.CheckActionRunning())
                {
                    __attackReleasedTimeStamp = Time.time;
                    heroBrain.BB.action.isCharging.Value = false;
                    heroBrain.BB.action.chargingLevel.Value = 0;

                    __Logger.LogF(gameObject, nameof(OnAttack), "Charging canceled.", "CurrActionName", heroBrain.ActionCtrler.CurrActionName);
                }
                else
                {
                    var chargingTime = Time.time - __attackPresssedTimeStamp;

                    //* 챠징 판정 시간은 1초
                    if (chargingTime > 1f)
                    {
                        if (heroBrain.BB.action.isCharging.Value == false)
                        {
                            Observable.Timer(TimeSpan.FromSeconds(0.2f)).Subscribe(_ 
                                => EffectManager.Instance.Show("ChonkExplosionBlue", 
                                heroBrain.GetWorldPosition() + Vector3.up,
                                Quaternion.identity, 0.8f * Vector3.one, 1f)).AddTo(this);
                        }
                        heroBrain.BB.action.isCharging.Value = true;
                        heroBrain.BB.action.chargingLevel.Value = Mathf.FloorToInt(Time.time - __attackPresssedTimeStamp) + 1;

                        heroBrain.ChangeWeapon(WeaponSetType.TWOHAND_WEAPON);
                    }
                    else
                    {
                        heroBrain.BB.action.isCharging.Value = false;
                        heroBrain.BB.action.chargingLevel.Value = 0;
                    }
                }
            }).AddTo(this);

            var canAction1 = heroBrain.BB.IsSpawnFinished && !heroBrain.BB.IsDead && !heroBrain.BB.IsGroggy && !heroBrain.BB.IsRolling;
            // var canAction2 = canAction1 && !MyHeroBrain.PawnBB.IsThrowing && !MyHeroBrain.PawnBB.IsGrabbed;
            var canAction3 = canAction1 && !heroBrain.StatusCtrler.CheckStatus(PawnStatus.Staggered);

            if (value.isPressed)
            {
                __attackPresssedTimeStamp = Time.time;

                var baseActionName = heroBrain.ActionCtrler.CheckActionRunning() ? heroBrain.ActionCtrler.CurrActionName : heroBrain.ActionCtrler.PrevActionName;
                var canNextAction = heroBrain.ActionCtrler.CheckActionRunning() ? heroBrain.ActionCtrler.CanInterruptAction() : (Time.time - heroBrain.ActionCtrler.prevActionContext.finishTimeStamp) < MainTable.PlayerData.GetList().First().actionInputTimePadding;
                if (canAction3 && canNextAction)
                {
                    switch (baseActionName)
                    {
                        case "Slash#1":
                            heroBrain.ActionCtrler.CancelAction(false);
                            heroBrain.ActionCtrler.SetPendingAction("Slash#2");
                            break;
                        case "Slash#2":
                            heroBrain.ActionCtrler.CancelAction(false);
                            heroBrain.ActionCtrler.SetPendingAction("Slash#3");
                            break;
                        case "HeavySlash#1":
                            heroBrain.ActionCtrler.CancelAction(false);
                            heroBrain.ActionCtrler.SetPendingAction("HeavySlash#2");
                            break;
                        case "HeavySlash#2":
                            heroBrain.ActionCtrler.CancelAction(false);
                            heroBrain.ActionCtrler.SetPendingAction("HeavySlash#3");
                            break;
                    }

                    //* 타겟이 없을 경우에도 조준 보정을 해줌
                    if (heroBrain.BB.TargetBrain == null && heroBrain.SensorCtrler.ListeningColliders.Count > 0)
                    {
                        var attackPoint = heroBrain.SensorCtrler.ListeningColliders.Select(c => c.transform.position)
                            .OrderBy(p => Vector3.Angle(heroBrain.coreColliderHelper.transform.forward.Vector2D(), (p - heroBrain.coreColliderHelper.transform.position).Vector2D()))
                            .FirstOrDefault();

                        heroBrain.Movement.FaceAt(attackPoint);
                    }
                }
            }
            else
            {
                __attackReleasedTimeStamp = Time.time;

                if (canAction3 && !heroBrain.ActionCtrler.CheckActionRunning())
                {
                    if (heroBrain.BB.IsJumping)
                    {
                        heroBrain.ActionCtrler.SetPendingAction("JumpAttack");
                        heroBrain.ChangeWeapon(WeaponSetType.TWOHAND_WEAPON);
                    }
                    else if (heroBrain.BB.IsCharging)
                    {
                        heroBrain.ActionCtrler.SetPendingAction("HeavySlash#1");
                        heroBrain.ChangeWeapon(WeaponSetType.TWOHAND_WEAPON);
                    }
                    else
                    {
                        heroBrain.ActionCtrler.SetPendingAction("Slash#1");
                        heroBrain.ChangeWeapon(WeaponSetType.ONEHAND_WEAPONSHIELD);
                    }

                    //* 타겟이 없을 경우에도 조준 보정을 해줌
                    if (heroBrain.BB.TargetBrain == null && heroBrain.SensorCtrler.ListeningColliders.Count > 0)
                    {
                        var attackPoint = heroBrain.SensorCtrler.ListeningColliders.Select(c => c.transform.position)
                            .OrderBy(p => Vector3.Angle(heroBrain.coreColliderHelper.transform.forward.Vector2D(), (p - heroBrain.coreColliderHelper.transform.position).Vector2D()))
                            .FirstOrDefault();

                        heroBrain.Movement.FaceAt(attackPoint);
                    }
                }

                //* 챠징 어택 판별을 위해서 'isCharging' 값은 제일 마지막에 리셋
                heroBrain.BB.action.isCharging.Value = false;
            }
        }

        public void OnSpecialAttack(InputValue value)
        {
            if (heroBrain == null)
                return;

            if (value.isPressed)
            {
                var canAction1 = heroBrain.BB.IsSpawnFinished && !heroBrain.BB.IsDead && !heroBrain.BB.IsGroggy && !heroBrain.BB.IsRolling;
                var canAction2 = canAction1 && (!heroBrain.ActionCtrler.CheckActionRunning() || heroBrain.ActionCtrler.CanInterruptAction()) && !heroBrain.StatusCtrler.CheckStatus(PawnStatus.Staggered);

                if (canAction2)
                {
                    if (heroBrain.ActionCtrler.CheckActionRunning())
                        heroBrain.ActionCtrler.CancelAction(false);

                    heroBrain.ActionCtrler.SetPendingAction("SpecialKick");
                }
            }
        }
        public void OnDrink() 
        {
            Debug.Log("<color=red>OnDrink</color>");
            heroBrain.ActionCtrler.SetPendingAction("DrinkPotion");
        }
    }
}