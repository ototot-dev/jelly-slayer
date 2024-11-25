using NodeCanvas.Tasks.Actions;
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

        [Header("Property")]
        public StringReactiveProperty playerName = new();
        public ReactiveProperty<Vector2> moveVec = new();
        public ReactiveProperty<Vector3> lookVec = new();
        public Action<PawnBrainController> onPossessed;
        public Action<PawnBrainController> onUnpossessed;

        public GameObject MyHero => MyHeroBrain != null ? MyHeroBrain.gameObject : null;
        public HeroBrain MyHeroBrain { get; private set; }

        public GameObject SpawnHero(GameObject heroPrefab, bool possessImmediately = false)
        {
            var spawnPosition = MyHero != null ? MyHero.transform.position : transform.position;
            var spawnRotation = MyHero != null ? MyHero.transform.rotation : Quaternion.LookRotation(Vector3.left + Vector3.back, Vector3.up);

            if (possessImmediately)
            {
                Possess(Instantiate(heroPrefab, spawnPosition, spawnRotation).GetComponent<HeroBrain>());
                return MyHero;
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
            MyHeroBrain = targetBrain as HeroBrain;

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
            onUnpossessed?.Invoke(MyHeroBrain);

            Debug.Log($"1?? {MyHero.name} is unpossessed by {gameObject.name}.");

            MyHeroBrain = null;
        }

        void Update()
        {
            if (MyHeroBrain == null)
                return;
            
            if (moveVec.Value.sqrMagnitude > 0)
            {
                MyHeroBrain.Movement.moveSpeed = MyHeroBrain.BB.action.isGuarding.Value ? MyHeroBrain.BB.body.guardSpeed : MyHeroBrain.BB.body.moveSpeed;
                MyHeroBrain.Movement.moveVec = Quaternion.AngleAxis(45, Vector3.up) * new Vector3(moveVec.Value.x, 0, moveVec.Value.y);

                //* Strafe 모드가 아닌 경우엔 이동 방향과 회전 방향이 동일함
                if (!MyHeroBrain.Movement.freezeRotation)
                    MyHeroBrain.Movement.faceVec = MyHeroBrain.Movement.moveVec;
            }
            else
            {
                MyHeroBrain.Movement.moveVec = Vector3.zero;
                MyHeroBrain.Movement.moveSpeed = 0;
            }

            if (MyHeroBrain.Movement.freezeRotation)
            {
                if (MyHeroBrain.BB.TargetBrain != null)
                {
                    MyHeroBrain.Movement.faceVec = (MyHeroBrain.BB.TargetBrain.CoreTransform.position - MyHeroBrain.Movement.capsule.position).Vector2D().normalized;
                    MyHeroBrain.AnimCtrler.HeadLookAt.position = MyHeroBrain.BB.TargetBrain.coreColliderHelper.transform.position + Vector3.up;

                    var targetCapsule = MyHeroBrain.BB.TargetBrain.coreColliderHelper.GetCapsuleCollider();
                    if (targetCapsule != null)
                        cursorCtrler.cursor.position = targetCapsule.transform.position + (targetCapsule.radius * 2f + targetCapsule.height) * Vector3.up;
                }
                else
                {
                    MyHeroBrain.Movement.faceVec = lookVec.Value;
                    if (MyHeroBrain.SensorCtrler.ListeningColliders.Count > 0)
                        MyHeroBrain.AnimCtrler.HeadLookAt.position = MyHeroBrain.SensorCtrler.ListeningColliders.First().transform.position + Vector3.up;

                    cursorCtrler.cursor.position = MyHeroBrain.Movement.capsule.position + lookVec.Value;
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
                lookVec.Value = (pickingPoint - MyHeroBrain.Movement.capsule.transform.position).Vector2D().normalized;
        }

        // 다음 액션 이름을 꺼내 준다
        string GetNewActionName()
        {
            var newActionName = string.Empty;
            var actionCtrler = MyHeroBrain.ActionCtrler;
            var isCtrlDown = Input.GetKey(KeyCode.LeftControl);
            if (!actionCtrler.CheckActionRunning())
            {
                // 타겟이 있고 타겟이 스턴 상태면... 처형
                if (MyHeroBrain.BB.action.targetPawnHP != null && MyHeroBrain.BB.action.targetPawnHP.Value != null)
                {
                    var targetBrain = MyHeroBrain.BB.action.targetPawnHP.Value.PawnBrain;
                    if (targetBrain != null && targetBrain.PawnBB.IsStunned == true)
                    {
                        var dist = MyHeroBrain.GetDistance(targetBrain);
                        //Debug.Log($"dist = {dist}");
                        if (dist <= 4)
                        {
                            MyHeroBrain.TargetPawn = targetBrain;

                            // 타겟 일정 거리로 강제 이동
                            var trTarget = targetBrain.CoreTransform;
                            var trHero = MyHeroBrain.CoreTransform;
                            var vDist = trHero.position - trTarget.position;
                            trHero.position = trTarget.position + (1.0f * vDist.normalized); 

                            return "Execution";
                        }
                    }
                }
                // 콤보 중 인지 체크
                var context = MyHeroBrain.ActionCtrler.prevActionContext;
                if (actionCtrler.PrevActionName.Contains("Slash") && (Time.time - context.finishTimeStamp) <= 0.05f)   //* 'Slash'류 액션은 종료 후 0.05초 안에 입력이 들어오면, 다음 콤보 액션을 수행한다. 
                {
                    // Debug.Log($"CheckActionRunning true : {Time.time}, {context.startTimeStamp}, {context.finishTimeStamp} : {context.actionName}");
                    if (isCtrlDown)
                    {
                        switch (actionCtrler.PrevActionName)
                        {
                            case "SlashHeavy#1": newActionName = "SlashHeavy#2"; break;
                            case "SlashHeavy#2": newActionName = "SlashHeavy#1"; break;
                            default:
                                Debug.Log("onFire, getActionName is Default 1");
                                break;
                        }
                    }
                    else
                    {
                        switch (actionCtrler.PrevActionName)
                        {
                            case "Slash#1": newActionName = "Slash#2"; break;
                            case "Slash#2": newActionName = "Slash#3"; break;

                            case "THSlash#1": newActionName = "THSlash#2"; break;
                            case "THSlash#2": newActionName = "THSlash#3"; break;
                            default: break;
                        }
                    }
                }
                else
                {
                    newActionName = "Slash#1";
                    /*
                    if(MyHeroBrain._weaponSlot == WEAPONSLOT.MAINSLOT)
                        newActionName = (isCtrlDown) ? "SlashHeavy#1" : "Slash#1";
                    else
                        newActionName = (isCtrlDown) ? "THSlashHeavy#1" : "THSlash#1";
                    */
                }
            }
            else if (actionCtrler.CurrActionName.Contains("Slash") && actionCtrler.CanInterruptAction())  //* CanInterruptAction()가 true면, 현재 액션을 취소하고 다음 액션을 수행할 수 있다.
            {
                if (isCtrlDown)
                {
                    switch (actionCtrler.CurrActionName)
                    {
                        case "SlashHeavy#1": newActionName = "SlashHeavy#2"; break;
                        case "SlashHeavy#2": newActionName = "SlashHeavy#1"; break;
                        default:
                            Debug.Log("onFire, getActionName is Default 2 : " + actionCtrler.PrevActionName);
                            break;
                    }
                }
                else
                {
                    switch (actionCtrler.CurrActionName)
                    {
                        case "Slash#1": newActionName = "Slash#2"; break;
                        case "Slash#2": newActionName = "Slash#3"; break;

                        case "THSlash#1": newActionName = "THSlash#2"; break;
                        case "THSlash#2": newActionName = "THSlash#3"; break;
                        default: break;
                    }
                }
            }
            else 
            {
                var actRunning = actionCtrler.CheckActionRunning();
                var actSlash = actionCtrler.CurrActionName.StartsWith("Slash");
                var actInterrupt = actionCtrler.CanInterruptAction();
                Debug.Log("onFire, fail : " + actRunning + ", " + actSlash + ", " + actInterrupt);
            }
            return newActionName;
        }

        public void OnGuard(InputValue value)
        {
            if (MyHeroBrain == null)
                return;

            MyHeroBrain.ChangeWeapon(WeaponSetType.ONEHAND_WEAPONSHIELD);

            MyHeroBrain.BB.action.isGuarding.Value = value.Get<float>() > 0;
            if (MyHeroBrain.BB.IsGuarding)
            {
                var canParry1 = MyHeroBrain.BB.IsSpawnFinished && !MyHeroBrain.BB.IsDead && !MyHeroBrain.BB.IsStunned && !MyHeroBrain.BB.IsJumping && !MyHeroBrain.BB.IsRolling;
                var canParry2 = canParry1 && (!MyHeroBrain.ActionCtrler.CheckActionRunning() || MyHeroBrain.ActionCtrler.CanInterruptAction()) && !MyHeroBrain.BuffCtrler.CheckStatus(PawnStatus.Staggered);
                if (canParry2)
                    MyHeroBrain.BuffCtrler.AddStatus(PawnStatus.PassiveParrying, 1f, 0.1f);
            }
        }

        public void OnJump()
        {
            float jumpStaminaCost = 10;

            // 이동 가능 체크
            var canJump1 = MyHeroBrain.BB.IsSpawnFinished && !MyHeroBrain.BB.IsDead && !MyHeroBrain.BB.IsStunned && !MyHeroBrain.BB.IsJumping && !MyHeroBrain.BB.IsRolling;
            var canJump2 = canJump1 && (!MyHeroBrain.ActionCtrler.CheckActionRunning() || MyHeroBrain.ActionCtrler.CanInterruptAction()) && !MyHeroBrain.BuffCtrler.CheckStatus(PawnStatus.Staggered);
            var canJump3 = canJump2 && MyHeroBrain.PawnBB.stat.stamina.Value >= jumpStaminaCost;

            if (canJump3)
            {
                if (MyHeroBrain.ActionCtrler.CheckActionRunning())
                    MyHeroBrain.ActionCtrler.CancelAction(false);

                MyHeroBrain.Movement.StartJumping();
                MyHeroBrain.BB.action.isJumping.Value = true;
                MyHeroBrain.BB.stat.ReduceStamina(jumpStaminaCost);
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
            if (MyHeroBrain.BB.TargetPawn == null)
            {
                var newTarget = MyHeroBrain.PawnSensorCtrler.ListeningColliders.Select(c => c.GetComponent<PawnColliderHelper>())
                    .Where(h => h != null && h.pawnBrain != null && !h.pawnBrain.PawnBB.IsDead)
                    .OrderBy(p => (p.transform.position - MyHeroBrain.CoreTransform.position).sqrMagnitude)
                    .FirstOrDefault();

                if (newTarget != null)
                {
                    MyHeroBrain.BB.action.targetPawnHP.Value = newTarget.pawnBrain.PawnHP;
                    MyHeroBrain.Movement.freezeRotation = true;
                }
            }
            else
            {
                var colliderHelpers = MyHeroBrain.PawnSensorCtrler.ListeningColliders.Select(c => c.GetComponent<PawnColliderHelper>())
                        .Where(h => h != null && h.pawnBrain != null && !h.pawnBrain.PawnBB.IsDead)
                        .ToArray();
                        
                for (int i = 0 ; i < colliderHelpers.Length; i++)
                {
                    if (colliderHelpers[i].pawnBrain == MyHeroBrain.BB.TargetBrain)
                    {
                        MyHeroBrain.BB.action.targetPawnHP.Value  = (i + 1  < colliderHelpers.Length ? colliderHelpers[i + 1] : colliderHelpers[0]).pawnBrain.PawnHP;
                        return;
                    }
                }
            }
        }

        public void OnPrevTarget()
        {
            if (MyHeroBrain.BB.TargetPawn == null)
            {
                var newTarget = MyHeroBrain.PawnSensorCtrler.ListeningColliders.Select(c => c.GetComponent<PawnColliderHelper>())
                    .Where(h => h != null && h.pawnBrain != null && !h.pawnBrain.PawnBB.IsDead)
                    .OrderBy(p => (p.transform.position - MyHeroBrain.CoreTransform.position).sqrMagnitude)
                    .FirstOrDefault();

                if (newTarget != null)
                {
                    MyHeroBrain.BB.action.targetPawnHP.Value = newTarget.pawnBrain.PawnHP;
                    MyHeroBrain.Movement.freezeRotation = true;
                }
            }
            else
            {
                var colliderHelpers = MyHeroBrain.PawnSensorCtrler.ListeningColliders.Select(c => c.GetComponent<PawnColliderHelper>())
                        .Where(h => h != null && h.pawnBrain != null && !h.pawnBrain.PawnBB.IsDead)
                        .ToArray();

                for (int i = colliderHelpers.Length - 1 ; i >= 0; i--)
                {
                    if (colliderHelpers[i].pawnBrain == MyHeroBrain.BB.TargetBrain)
                    {
                        MyHeroBrain.BB.action.targetPawnHP.Value  = (i - 1 >= 0 ? colliderHelpers[i - 1] : colliderHelpers[colliderHelpers.Length - 1]).pawnBrain.PawnHP;
                        return;
                    }
                }
            }
        }

        public void OnRoll()
        {
            var actionData = DatasheetManager.Instance.GetActionData(MyHeroBrain.PawnBB.common.pawnId, "Rolling");
            Debug.Assert(actionData != null);

            // 대쉬 가능 체크
            var canRolling1 = MyHeroBrain.BB.IsSpawnFinished && !MyHeroBrain.BB.IsDead && !MyHeroBrain.BB.IsStunned && !MyHeroBrain.BB.IsJumping && !MyHeroBrain.BB.IsRolling;
            var canRolling2 = canRolling1 && (!MyHeroBrain.ActionCtrler.CheckActionRunning() || MyHeroBrain.ActionCtrler.CanInterruptAction()) && !MyHeroBrain.BuffCtrler.CheckStatus(PawnStatus.Staggered);

            if (canRolling2)
            {
                if (MyHeroBrain.ActionCtrler.CheckActionRunning())
                    MyHeroBrain.ActionCtrler.CancelAction(false);

                var rollingXZ = Vector3.zero;
                var rollingVec = Vector3.zero;
                if (MyHeroBrain.Movement.moveVec == Vector3.zero)
                {
                    rollingXZ = Vector3.back;
                    rollingVec = -MyHeroBrain.Movement.capsule.forward.Vector2D().normalized;
                }
                else
                {
                    rollingXZ = MyHeroBrain.Movement.capsule.InverseTransformDirection(MyHeroBrain.Movement.moveVec);
                    rollingVec = MyHeroBrain.Movement.moveVec.Vector2D().normalized;
                }

                if (Mathf.Abs(rollingXZ.x) > Mathf.Abs(rollingXZ.z))
                {
                    MyHeroBrain.AnimCtrler.mainAnimator.SetInteger("RollingX", rollingXZ.x > 0f ? 1 : -1);
                    MyHeroBrain.AnimCtrler.mainAnimator.SetInteger("RollingZ", 0);
                    MyHeroBrain.Movement.FaceTo(Quaternion.Euler(0f, rollingXZ.x > 0f ? -90f : 90f, 0f) * rollingVec);
                }
                else
                {
                    MyHeroBrain.AnimCtrler.mainAnimator.SetInteger("RollingX", 0);
                    MyHeroBrain.AnimCtrler.mainAnimator.SetInteger("RollingZ", rollingXZ.z > 0f ? 1 : -1);
                    MyHeroBrain.Movement.FaceTo(rollingXZ.z > 0f ? rollingVec : -rollingVec);
                }

                MyHeroBrain.ActionCtrler.SetPendingAction("Rolling");

                // MyHeroBrain.BuffCtrler.AddBuff(BuffTypes.InvincibleDodge, 1f, 0.2f);
                // MyHeroBrain.Movement.StartRolling(MyHeroBrain.BB.body.rollingDuration);
            }
        }
        public void OnParry() 
        {
            var actionData = DatasheetManager.Instance.GetActionData(MyHeroBrain.PawnBB.common.pawnId, "ActiveParry");
            Debug.Assert(actionData != null);

            MyHeroBrain.ActionCtrler.SetPendingAction("ActiveParry");
        }

        public void OnSpecialAttack(InputValue value) 
        {
            if (value.isPressed && MyHeroBrain != null)
            {
                var canAction1 = MyHeroBrain.BB.IsSpawnFinished && !MyHeroBrain.BB.IsDead && !MyHeroBrain.BB.IsStunned && !MyHeroBrain.BB.IsRolling;
                var canAction2 = canAction1 && (!MyHeroBrain.ActionCtrler.CheckActionRunning() || MyHeroBrain.ActionCtrler.CanInterruptAction()) && !MyHeroBrain.BuffCtrler.CheckStatus(PawnStatus.Staggered);

                //Debug.Log("<color=yellow>Attack Released</color> : " + MyHeroBrain.BB.IsCharging + " " + canAction3);
                if (canAction2)
                {
                    if (MyHeroBrain.ActionCtrler.CheckActionRunning())
                    {
                        switch (MyHeroBrain.ActionCtrler.CurrActionName)
                        {
                            case "HeavySlash#1":
                                MyHeroBrain.ActionCtrler.CancelAction(false);
                                MyHeroBrain.ActionCtrler.SetPendingAction("HeavySlash#2"); 
                                break;
                            case "HeavySlash#2":
                                MyHeroBrain.ActionCtrler.CancelAction(false);
                                MyHeroBrain.ActionCtrler.SetPendingAction("HeavySlash#3"); 
                                break;
                        }

                        //* 타겟이 없을 경우에도 조준 보정을 해줌
                        if (MyHeroBrain.BB.TargetBrain == null && MyHeroBrain.SensorCtrler.ListeningColliders.Count > 0)
                        {
                            var attackPoint = MyHeroBrain.SensorCtrler.ListeningColliders.Select(c => c.transform.position)
                                .OrderBy(p => Vector3.Angle(MyHeroBrain.coreColliderHelper.transform.forward.Vector2D(), (p - MyHeroBrain.coreColliderHelper.transform.position).Vector2D()))
                                .FirstOrDefault();

                            MyHeroBrain.Movement.FaceAt(attackPoint);
                        }
                    }
                    else
                    {
                        MyHeroBrain.ActionCtrler.SetPendingAction("HeavySlash#1");

                        //* 타겟이 없을 경우에도 조준 보정을 해줌
                        if (MyHeroBrain.BB.TargetBrain == null && MyHeroBrain.SensorCtrler.ListeningColliders.Count > 0)
                        {
                            var attackPoint = MyHeroBrain.SensorCtrler.ListeningColliders.Select(c => c.transform.position)
                                .OrderBy(p => Vector3.Angle(MyHeroBrain.coreColliderHelper.transform.forward.Vector2D(), (p - MyHeroBrain.coreColliderHelper.transform.position).Vector2D()))
                                .FirstOrDefault();

                            MyHeroBrain.Movement.FaceAt(attackPoint);
                        }
                    }
                }
            }
        }

        IDisposable __chargingAttackDisposable;
        float __attackPresssedTimeStamp = -1f;
        float __attackReleasedTimeStamp = -1f;

        public void OnAttack(InputValue value)
        {
            __chargingAttackDisposable ??= Observable.EveryUpdate().Where(_ => __attackPresssedTimeStamp > __attackReleasedTimeStamp).Subscribe(_ => 
            {
                if (MyHeroBrain.ActionCtrler.CheckActionRunning())
                {
                    __attackReleasedTimeStamp = Time.time;
                    MyHeroBrain.BB.action.isCharging.Value = false;
                    MyHeroBrain.BB.action.chargingLevel.Value = 0;

                    __Logger.LogF(gameObject, nameof(OnAttack), "Charging canceled.", "CurrActionName", MyHeroBrain.ActionCtrler.CurrActionName);
                }
                else
                {
                    var chargingTime = Time.time - __attackPresssedTimeStamp;

                    //* 챠징 판정 시간은 0.2초
                    if (chargingTime > 0.2f)
                    {
                        MyHeroBrain.BB.action.isCharging.Value = true;
                        MyHeroBrain.BB.action.chargingLevel.Value = Mathf.FloorToInt(Time.time - __attackPresssedTimeStamp) + 1;
                    
                        MyHeroBrain.ChangeWeapon(WeaponSetType.TWOHAND_WEAPON);
                    }
                    else
                    {
                        MyHeroBrain.BB.action.isCharging.Value = false;
                        MyHeroBrain.BB.action.chargingLevel.Value = 0;
                    }
                }
            }).AddTo(this);

            if (value.isPressed)
            {
                __attackPresssedTimeStamp = Time.time;
                //Debug.Log("<color=red>Attack Pressed</color> : " + MyHeroBrain.BB.IsCharging);
            }
            else
            {
                __attackReleasedTimeStamp = Time.time;

                if (MyHeroBrain != null)
                {
                    var canAction1 = MyHeroBrain.BB.IsSpawnFinished && !MyHeroBrain.BB.IsDead && !MyHeroBrain.BB.IsStunned && !MyHeroBrain.BB.IsRolling;
                    var canAction2 = canAction1 && !MyHeroBrain.PawnBB.IsThrowing && !MyHeroBrain.PawnBB.IsGrabbed;
                    var canAction3 = canAction2 && (!MyHeroBrain.ActionCtrler.CheckActionRunning() || MyHeroBrain.ActionCtrler.CanInterruptAction()) && !MyHeroBrain.BuffCtrler.CheckStatus(PawnStatus.Staggered);

                    //Debug.Log("<color=yellow>Attack Released</color> : " + MyHeroBrain.BB.IsCharging + " " + canAction3);
                    if (canAction3)
                    {
                        if (MyHeroBrain.ActionCtrler.CheckActionRunning())
                        {
                            switch (MyHeroBrain.ActionCtrler.CurrActionName)
                            {
                                case "Slash#1":
                                    MyHeroBrain.ActionCtrler.CancelAction(false);
                                    MyHeroBrain.ActionCtrler.SetPendingAction("Slash#2");
                                    break;
                                case "Slash#2": 
                                    MyHeroBrain.ActionCtrler.CancelAction(false); 
                                    MyHeroBrain.ActionCtrler.SetPendingAction("Slash#1"); 
                                    break;
                                case "HeavySlash#1":
                                    MyHeroBrain.ActionCtrler.CancelAction(false);
                                    MyHeroBrain.ActionCtrler.SetPendingAction("HeavySlash#2"); 
                                    break;
                                case "HeavySlash#2":
                                    MyHeroBrain.ActionCtrler.CancelAction(false);
                                    MyHeroBrain.ActionCtrler.SetPendingAction("HeavySlash#3"); 
                                    break;
                            }

                            //* 타겟이 없을 경우에도 조준 보정을 해줌
                            if (MyHeroBrain.BB.TargetBrain == null && MyHeroBrain.SensorCtrler.ListeningColliders.Count > 0)
                            {
                                var attackPoint = MyHeroBrain.SensorCtrler.ListeningColliders.Select(c => c.transform.position)
                                    .OrderBy(p => Vector3.Angle(MyHeroBrain.coreColliderHelper.transform.forward.Vector2D(), (p - MyHeroBrain.coreColliderHelper.transform.position).Vector2D()))
                                    .FirstOrDefault();

                                MyHeroBrain.Movement.FaceAt(attackPoint);
                            }
                        }
                        else
                        {
                            //Debug.Log("<color=yellow>Attack Released</color> : " + MyHeroBrain.BB.IsCharging + " " + canAction3);
                            if (MyHeroBrain.BB.IsJumping == true)
                            {
                                MyHeroBrain.ActionCtrler.CancelAction(false);
                                MyHeroBrain.ActionCtrler.SetPendingAction("JumpAttack");

                                MyHeroBrain.ChangeWeapon(WeaponSetType.TWOHAND_WEAPON);

                                MyHeroBrain.Movement.Gravity = new Vector3(0, -0, 0);
                                Observable.Timer(TimeSpan.FromSeconds(0.3f)).Subscribe(_ =>
                                {
                                    MyHeroBrain.Movement.Gravity = new Vector3(0, -30, 0);
                                }).AddTo(this);
                            }
                            else
                            {
                                if (MyHeroBrain.BB.IsCharging) 
                                { 
                                    MyHeroBrain.ActionCtrler.SetPendingAction("HeavySlash#1");
                                    MyHeroBrain.ChangeWeapon(WeaponSetType.TWOHAND_WEAPON);
                                }
                                else
                                {
                                    MyHeroBrain.ActionCtrler.SetPendingAction("Slash#1");
                                    MyHeroBrain.ChangeWeapon(WeaponSetType.ONEHAND_WEAPONSHIELD);
                                }
                            }
                            //* 타겟이 없을 경우에도 조준 보정을 해줌
                            if (MyHeroBrain.BB.TargetBrain == null && MyHeroBrain.SensorCtrler.ListeningColliders.Count > 0)
                            {
                                var attackPoint = MyHeroBrain.SensorCtrler.ListeningColliders.Select(c => c.transform.position)
                                    .OrderBy(p => Vector3.Angle(MyHeroBrain.coreColliderHelper.transform.forward.Vector2D(), (p - MyHeroBrain.coreColliderHelper.transform.position).Vector2D()))
                                    .FirstOrDefault();

                                MyHeroBrain.Movement.FaceAt(attackPoint);
                            }
                        }
                    }
                }
                
                //* 챠징 어택 판별을 위해서 'isCharging' 값은 제일 마지막에 리셋
                MyHeroBrain.BB.action.isCharging.Value = false;
            }
        }
    }
}