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
        public BoolReactiveProperty firePressed = new();

        public Action<PawnBrainController> onPossessed;
        public Action<PawnBrainController> onUnpossessed;

        public GameObject MyHero => MyHeroBrain != null ? MyHeroBrain.gameObject : null;
        public HeroBrain MyHeroBrain { get; private set; }

        private bool _fireKeyPressed = false;
        private float _fireKeyTime = 0;

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

        IDisposable __moveDisposable;

        public void OnMove(InputValue value)
        {
            if (__moveDisposable == null)
            {
                var moveCommand = moveVec.Select(m => m.sqrMagnitude > 0)
                    .DistinctUntilChanged()
                    .StartWith(false);

                __moveDisposable = moveCommand.Where(m => m).Subscribe(_ =>
                {
                    Observable.EveryUpdate().TakeWhile(__ => moveVec.Value.sqrMagnitude > 0)
                        .DoOnCompleted(() =>
                        {
                            if (MyHeroBrain != null)
                            {
                                MyHeroBrain.Movement.moveVec = Vector3.zero;
                                MyHeroBrain.Movement.moveSpeed = 0;
                            }
                        })
                        .Subscribe(__ =>
                        {
                            MyHeroBrain.Movement.moveSpeed = MyHeroBrain.BB.action.isGuarding.Value ? MyHeroBrain.BB.body.guardSpeed : MyHeroBrain.BB.body.moveSpeed;
                            MyHeroBrain.Movement.faceVec = MyHeroBrain.Movement.moveVec = Quaternion.AngleAxis(45, Vector3.up) * new Vector3(moveVec.Value.x, 0, moveVec.Value.y);
                        });
                }).AddTo(this);
            }

            moveVec.Value = value.Get<Vector2>();
        }

        public void OnTarget()
        {
            if (MyHeroBrain.BB.TargetPawn == null)
            {
                var newTarget = MyHeroBrain.SensorCtrler.ListeningColliders
                    .Select(l => l.GetComponent<PawnColliderHelper>())
                    .Where(p => p != null && p.pawnBrain != null)
                    .OrderBy(p => (p.transform.position - MyHeroBrain.CoreTransform.position).sqrMagnitude).FirstOrDefault();

                if (newTarget != null)
                {
                    MyHeroBrain.BB.action.targetPawnHP.Value = newTarget.pawnBrain.PawnHP;
                    MyHeroBrain.Movement.freezeRotation = true;
                }
            }
            else
            {
                MyHeroBrain.BB.action.targetPawnHP.Value = null;
                MyHeroBrain.Movement.freezeRotation = false;
            }
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

        public void OnFire()
        {
            Debug.Log("<color=cyan>OnFire</color>");

            if (MyHeroBrain == null)
                return;

            // 경직 되었는가?
            if (MyHeroBrain.BuffCtrler.CheckBuff(BuffTypes.Staggered))
                Debug.Log("onFire, stagger");
                
            // 잡거나 잡혔는가?
            if (MyHeroBrain.PawnBB.IsThrowing || MyHeroBrain.PawnBB.IsGrabbed == true) 
            {
                Debug.Log("onFire, isThrow");
                return;
            }

            // 대쉬 가능 체크
            var canAction1 = MyHeroBrain.BB.IsSpawnFinished && !MyHeroBrain.BB.IsDead && 
                !MyHeroBrain.BB.IsStunned && !MyHeroBrain.BB.IsRolling;
            var canAction2 = canAction1 && (!MyHeroBrain.ActionCtrler.CheckActionRunning() || MyHeroBrain.ActionCtrler.CanInterruptAction()) && !MyHeroBrain.BuffCtrler.CheckBuff(BuffTypes.Staggered);
            // 마우스 커서 외치로 회전
            // if (MyHeroBrain.BB.TargetBrain == null)
            //     cursorCtrler.cursor.position = GameContext.Instance.cameraCtrler.GetPickingResult(Input.mousePosition, "Terrain").point;

            if (!canAction2)
                return;

            var newActionName = GetNewActionName();            
            if (string.IsNullOrEmpty(newActionName) == false)
            {
                // // 스테미나 체크
                // var actionData = DatasheetManager.Instance.GetActionData(PawnId.Hero, newActionName);
                // float staminaCost = (actionData != null) ? actionData.staminaCost : 30;
                // if (MyHeroBrain.PawnBB.stat.stamina.Value < staminaCost)
                // {
                //     Debug.Log("onFire, stamina empty");
                //     SoundManager.Instance.Play(SoundID.EMPTY_STAMINA); // 스테미너 Empty 사운드
                //     return;
                // }
                if (MyHeroBrain.ActionCtrler.CheckActionRunning())
                    MyHeroBrain.ActionCtrler.CancelAction(false);

                MyHeroBrain.Movement.FaceAt(cursorCtrler.CurrPosition);
                MyHeroBrain.ActionCtrler.SetPendingAction(newActionName);
            }
            else 
            {
                Debug.Log("onFire, action name empty");
            }
        }

        public void OnGuard(InputValue value)
        {
            if (MyHeroBrain == null)
                return;
                
            MyHeroBrain.BB.action.isGuarding.Value = value.Get<float>() > 0;
            if (MyHeroBrain.BB.IsGuarding)
            {
                var canParry1 = MyHeroBrain.BB.IsSpawnFinished && !MyHeroBrain.BB.IsDead && !MyHeroBrain.BB.IsStunned && !MyHeroBrain.BB.IsJumping && !MyHeroBrain.BB.IsRolling;
                var canParry2 = canParry1 && (!MyHeroBrain.ActionCtrler.CheckActionRunning() || MyHeroBrain.ActionCtrler.CanInterruptAction()) && !MyHeroBrain.BuffCtrler.CheckBuff(BuffTypes.Staggered);
                if (canParry2)
                    MyHeroBrain.BuffCtrler.AddBuff(BuffTypes.PassiveParrying, 1f, 0.1f);
            }
        }

        public void OnJump()
        {
            float jumpStaminaCost = 10;

            // 이동 가능 체크
            var canJump1 = MyHeroBrain.BB.IsSpawnFinished && !MyHeroBrain.BB.IsDead && !MyHeroBrain.BB.IsStunned && !MyHeroBrain.BB.IsJumping && !MyHeroBrain.BB.IsRolling;
            var canJump2 = canJump1 && (!MyHeroBrain.ActionCtrler.CheckActionRunning() || MyHeroBrain.ActionCtrler.CanInterruptAction()) && !MyHeroBrain.BuffCtrler.CheckBuff(BuffTypes.Staggered);
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
        public void OnRoll()
        {
            var actionData = DatasheetManager.Instance.GetActionData(MyHeroBrain.PawnBB.common.pawnId, "Rolling");
            Debug.Assert(actionData != null);

            // 대쉬 가능 체크
            var canRolling1 = MyHeroBrain.BB.IsSpawnFinished && !MyHeroBrain.BB.IsDead && !MyHeroBrain.BB.IsStunned && !MyHeroBrain.BB.IsJumping && !MyHeroBrain.BB.IsRolling;
            var canRolling2 = canRolling1 && (!MyHeroBrain.ActionCtrler.CheckActionRunning() || MyHeroBrain.ActionCtrler.CanInterruptAction()) && !MyHeroBrain.BuffCtrler.CheckBuff(BuffTypes.Staggered);

            if (canRolling2)
            {
                if (MyHeroBrain.ActionCtrler.CheckActionRunning())
                    MyHeroBrain.ActionCtrler.CancelAction(false);

                MyHeroBrain.BuffCtrler.AddBuff(BuffTypes.InvincibleDodge, 1f, 0.2f);
                MyHeroBrain.Movement.StartRolling(MyHeroBrain.BB.body.rollingDuration);
            }
        }
        public void OnParry() 
        {
            var actionData = DatasheetManager.Instance.GetActionData(MyHeroBrain.PawnBB.common.pawnId, "ActiveParry");
            Debug.Assert(actionData != null);

            MyHeroBrain.ActionCtrler.SetPendingAction("ActiveParry");
        }
        public void OnAction(InputValue value) 
        {
            if (MyHeroBrain._chainCtrl == null)
                return;

            bool isPress = value.Get<float>() > 0;
            if (isPress == true)
            {
                if (MyHeroBrain._chainCtrl.IsBind == true)
                {
                    MyHeroBrain._chainCtrl.ResetChain();
                }
                else
                {
                    MyHeroBrain.ChainRolling(true);
                }
            }
            else if (MyHeroBrain._chainCtrl.IsRolling == true) 
            {
                //MyHeroBrain.ChainRolling(false);
                MyHeroBrain.ActionCtrler.SetPendingAction("ChainShot");
            }
        }
        void ChargeEnd() 
        {
            if (_fireKeyPressed == false) return;

            _fireKeyPressed = false;
            MyHeroBrain.ActionCtrler.CancelAction(false);
            MyHeroBrain.ActionCtrler.SetPendingAction("SlashHeavy#1");
        }

        public void OnLongFire(InputValue value)
        {
            float inputValue = value.Get<float>();
            bool isPress = value.Get<float>() > 0;
            Debug.Log("<color=red>OnLongFire</color> : " + inputValue);

            if (isPress == true) 
            {
                if (MyHeroBrain.ActionCtrler.CheckActionRunning() &&
                   MyHeroBrain.ActionCtrler.CurrActionName.Contains("SlashHeavy")) 
                {
                    return;
                }
                _fireKeyPressed = true;
                _fireKeyTime = Time.fixedTime;
                MyHeroBrain.ActionCtrler.SetPendingAction("ChargeHeavy#1");

                MyHeroBrain.ChangeWeapon(WeaponSetType.TWOHAND_WEAPON);

                //Invoke("ChargeEnd", 3);
            }
            else if (isPress == false && _fireKeyPressed == true) 
            {
                _fireKeyPressed = false;
                MyHeroBrain.ActionCtrler.CancelAction(false);

                // 일정 시간 이상 모으면 모으기 공격
                var timeDelta = Time.fixedTime - _fireKeyTime;
                Debug.Log("Slash Heavy : " + timeDelta);
                if (timeDelta >= 0.5f)
                {
                    MyHeroBrain.ActionCtrler.SetPendingAction("SlashHeavy#1");
                }
                else 
                {
                    MyHeroBrain.ChangeWeapon(WeaponSetType.ONEHAND_WEAPONSHIELD);
                }
            }
        }
    }
}