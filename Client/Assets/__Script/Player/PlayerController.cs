using System;
using System.Buffers;
using System.Linq;
using UGUI.Rx;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using ZLinq;

namespace Game
{
    public class PlayerController : MonoBehaviour, IPawnEventListener
    {
        [Header("Input")]
        public float attackPointAssistRange = 1f;
        public ReactiveProperty<Vector2> inputMoveVec = new();
        public ReactiveProperty<Vector3> inputLookVec = new();
        public ReactiveProperty<JellyMeshController> boundJellyMesh = new();
        public SpecialKeyController specialKeyCtrler;

        [Header("Possess")]
        public StringReactiveProperty playerName = new();
        public SlayerBrain possessedBrain;
        public GameObject PossessedPawn => possessedBrain != null ? possessedBrain.gameObject : null;
        public Action<SlayerBrain> onPossessed;
        public Action<SlayerBrain> onUnpossessed;

        [Header("Enable")]
        public bool _isEnable_Move = true;
        public bool _isEnable_Look = true;
        public bool _isEnable_NormalAttack = true;
        public bool _isEnable_SpecialAttack = true;
        public bool _isEnable_Roll = true;
        public bool _isEnable_Jump = true;
        public bool _isEnable_Guard = true;
        public bool _isEnable_Parry = true;

#region IPawnEventListener 구현
        void IPawnEventListener.OnReceivePawnActionStart(PawnBrainController sender, string actionName) 
        {
            if (actionName == "OnJellyOut")
            {
                Debug.Assert(boundJellyMesh.Value == null);

                boundJellyMesh.Value = (sender as JellyBrain).jellyMeshCtrler;
                __Logger.LogR2(gameObject, nameof(IPawnEventListener.OnReceivePawnActionStart), "jellyBrain", sender, "OnJellyOut", "boundJellyMesh", boundJellyMesh.Value);

                specialKeyCtrler = new SpecialKeyController(sender, "GroggyAttack", "Groggy").Load().Show(GameContext.Instance.CanvasManager.body.transform as RectTransform);
            }
            else if (actionName == "OnJellyOff")
            {
                Debug.Assert(boundJellyMesh.Value.hostBrain == sender);

                boundJellyMesh.Value = null;
                __Logger.LogR2(gameObject, nameof(IPawnEventListener.OnReceivePawnActionStart), "OnJellOff", "jellyBrain", sender);

                specialKeyCtrler.HideAsObservable().Subscribe(c =>  c.Unload()).AddTo(this);
                specialKeyCtrler = null;
            }
        }

        void IPawnEventListener.OnReceivePawnStatusChanged(PawnBrainController sender, PawnStatus status, float strength, float duration) 
        {
            if (sender is JellyBrain && status == PawnStatus.Groggy && strength > 0f)
            {
                Debug.Assert(specialKeyCtrler == null);
                specialKeyCtrler = new SpecialKeyController(sender, "Assault", "Encounter").Load().Show(GameContext.Instance.CanvasManager.body.transform as RectTransform);
            }
        }
        void IPawnEventListener.OnReceivePawnDamageContext(PawnBrainController sender, PawnHeartPointDispatcher.DamageContext damageContext) 
        {
            if (damageContext.senderActionSpecialTag == "Encounter")
            {
                Debug.Assert(sender == possessedBrain);
                possessedBrain.BB.action.encounterBrain.Value = damageContext.receiverBrain;

                __Logger.LogR2(gameObject, "OnReceivePawnDamageContext", "Groggy-Encounter", "encounterBrain", damageContext.receiverBrain);
            }
            else if (damageContext.groggyBreakHit)
            {
                Debug.Assert(sender == possessedBrain);
                possessedBrain.BB.action.encounterBrain.Value = null;

                __Logger.LogR2(gameObject, "OnReceivePawnDamageContext", "Groggy-Break");
            }
        }
        void IPawnEventListener.OnReceivePawnSpawningStateChanged(PawnBrainController sender, PawnSpawnStates state) {}
#endregion

        public GameObject SpawnHeroPawn(GameObject heroPrefab, bool possessImmediately = false)
        {
            var spawnPosition = possessedBrain != null ? possessedBrain.GetWorldPosition() : transform.position;
            var spawnRotation = possessedBrain != null ? possessedBrain.GetWorldRotation() : Quaternion.LookRotation(Vector3.left + Vector3.back, Vector3.up);

            if (possessImmediately)
            {
                Possess(Instantiate(heroPrefab, spawnPosition, spawnRotation).GetComponent<SlayerBrain>());
                return PossessedPawn;
            }
            else
            {
                return Instantiate(heroPrefab, spawnPosition, spawnRotation);
            }
        }

        public bool Possess(SlayerBrain targetBrain)
        {
            if (targetBrain == null || targetBrain.PawnBB.IsPossessed)
                return false;

            targetBrain.owner = this;
            possessedBrain = targetBrain as SlayerBrain;

            targetBrain.transform.SetParent(transform, true);
            targetBrain.OnPossessedHandler();
            onPossessed?.Invoke(targetBrain);

            __Logger.LogR1(gameObject, nameof(Possess), nameof(targetBrain), targetBrain.name);
            return true;
        }

        public void Unpossess()
        {
            __Logger.LogR1(gameObject, nameof(Unpossess), "possessedBrain", possessedBrain);

            possessedBrain.transform.SetParent(null, true);
            onUnpossessed?.Invoke(possessedBrain);

            possessedBrain = null;
        }

        void Start()
        {
            PawnEventManager.Instance.RegisterEventListener(this as IPawnEventListener);
        }

        void Update()
        {
            if (possessedBrain == null)
                return;

            if (inputMoveVec.Value.sqrMagnitude > 0)
            {
                var isAction = possessedBrain.BB.IsGuarding || possessedBrain.BB.IsPunchCharging;
                possessedBrain.Movement.moveSpeed = isAction ? possessedBrain.BB.body.guardSpeed : possessedBrain.BB.body.walkSpeed;
                possessedBrain.Movement.moveVec = Quaternion.AngleAxis(45, Vector3.up) * new Vector3(inputMoveVec.Value.x, 0, inputMoveVec.Value.y);

                //* Strafe 모드가 아닌 경우엔 이동 방향과 회전 방향이 동일함
                if (!possessedBrain.Movement.freezeRotation)
                    possessedBrain.Movement.faceVec = possessedBrain.Movement.moveVec;
            }
            else
            {
                possessedBrain.Movement.moveVec = Vector3.zero;
                possessedBrain.Movement.moveSpeed = 0;
            }

            if (possessedBrain.Movement.freezeRotation)
            {
                if (possessedBrain.BB.TargetBrain != null)
                {
                    possessedBrain.AnimCtrler.HeadLookAt.position = possessedBrain.BB.TargetColliderHelper.GetWorldCenter();
                    possessedBrain.Movement.faceVec = (possessedBrain.AnimCtrler.HeadLookAt.position - possessedBrain.Movement.capsule.position).Vector2D().normalized;
                }
                else
                {
                    possessedBrain.Movement.faceVec = inputLookVec.Value;
                }
            }
        }

        public void OnMove(InputValue value)
        {
            if (_isEnable_Move == false)
                return;
            inputMoveVec.Value = value.Get<Vector2>();
        }

        public void OnLook(InputValue value)
        {
            if (_isEnable_Look == false || possessedBrain == null)
                return;
            if (GameContext.Instance.cameraCtrler != null && 
                GameContext.Instance.cameraCtrler.TryGetPickingPointOnTerrain(value.Get<Vector2>(), out var pickingPoint))
                inputLookVec.Value = (pickingPoint - possessedBrain.Movement.capsule.transform.position).Vector2D().normalized;
        }
        public void OnGuard(InputValue value)
        {
            if (_isEnable_Guard == false)
                return;
            if (possessedBrain == null)
                return;

            possessedBrain.ChangeWeapon(WeaponSetType.ONEHAND_WEAPONSHIELD);

            possessedBrain.BB.body.isGuarding.Value = value.Get<float>() > 0;
            if (possessedBrain.BB.IsGuarding)
            {
                var canParry1 = possessedBrain.BB.IsSpawnFinished && !possessedBrain.BB.IsDead && !possessedBrain.BB.IsGroggy && !possessedBrain.BB.IsDown && !possessedBrain.BB.IsJumping && !possessedBrain.BB.IsRolling;
                var canParry2 = canParry1 && (!possessedBrain.ActionCtrler.CheckActionRunning() || possessedBrain.ActionCtrler.CanInterruptAction());
                var canParry3 = canParry2 && !possessedBrain.StatusCtrler.CheckStatus(PawnStatus.Staggered) && !possessedBrain.StatusCtrler.CheckStatus(PawnStatus.CanNotGuard);
                if (canParry3)
                    possessedBrain.StatusCtrler.AddStatus(PawnStatus.GuardParrying, 1f, possessedBrain.BB.action.guardParryDuration);
            }
        }

        IDisposable __jumpHangingDisposable;
        float __jumpExecutedTimeStamp = -1f;
        float __jumpReleasedTimeStamp = -1f;

        public void OnJump(InputValue value)
        {
            if (_isEnable_Jump == false)
                return;

            __jumpHangingDisposable ??= Observable.EveryUpdate().Where(_ => __jumpExecutedTimeStamp > __jumpReleasedTimeStamp).Subscribe(_ =>
            {
                //* Catch 판정은 0.1초로 셋팅
                var droneBot = possessedBrain.droneBotFormationCtrler.PickDroneBot();
                if (!possessedBrain.BB.IsHanging && droneBot != null && droneBot.BB.CurrDecision != DroneBotBrain.Decisions.Catch && Time.time - __jumpExecutedTimeStamp > 0.1f)
                    possessedBrain.Movement.PrepareHanging(droneBot, 0.2f);
            }).AddTo(this);

            float jumpStaminaCost = 10;

            if (value.isPressed)
            {   
                var canJump1 = possessedBrain.BB.IsSpawnFinished && !possessedBrain.BB.IsDead && !possessedBrain.BB.IsGroggy && !possessedBrain.BB.IsDown && !possessedBrain.BB.IsJumping && !possessedBrain.BB.IsRolling;
                var canJump2 = canJump1 && (!possessedBrain.ActionCtrler.CheckActionRunning() || possessedBrain.ActionCtrler.CanInterruptAction()) && !possessedBrain.StatusCtrler.CheckStatus(PawnStatus.Staggered);

                if (canJump2)
                {
                    if (possessedBrain.ActionCtrler.CheckActionRunning())
                        possessedBrain.ActionCtrler.CancelAction(false);
                        
                    if (possessedBrain.BB.IsHanging)
                    {
                        //* 점프 방향을 셋팅해줌
                        possessedBrain.Movement.GetCharacterMovement().velocity = possessedBrain.BB.body.hangingBrain.Value.Movement.CurrVelocity;
                        possessedBrain.Movement.FinishHanging();
                        possessedBrain.Movement.StartJump(possessedBrain.BB.body.jumpHeight);
                    }
                    else
                    {
                        //* 재귀적으로 Haning이 일어나지 않도록 'IsHanging'이 falsed인 경우에만 __jumpExecutedTimeStamp 값을 갱신해서 __jumpHangingDisposable이 동작하도록 한다
                        __jumpExecutedTimeStamp = Time.time;

                        possessedBrain.Movement.StartJump(possessedBrain.BB.body.jumpHeight);
                        possessedBrain.BB.stat.ReduceStamina(jumpStaminaCost);
                    }

                    PawnEventManager.Instance.SendPawnActionEvent(possessedBrain, "OnJump");
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
            if (possessedBrain.BB.TargetPawn == null)
            {
                var newTarget = possessedBrain.PawnSensorCtrler.ListeningColliders.AsValueEnumerable()
                    .Select(c => c.GetComponent<PawnColliderHelper>())
                    .Where(h => h != null && h.pawnBrain != null && !h.pawnBrain.PawnBB.IsDead && h.pawnBrain is IPawnTargetable)
                    .OrderBy(p => (p.transform.position - possessedBrain.GetWorldPosition()).sqrMagnitude)
                    .FirstOrDefault();

                if (newTarget != null)
                {
                    possessedBrain.BB.target.targetPawnHP.Value = newTarget.pawnBrain.PawnHP;
                    possessedBrain.Movement.freezeRotation = true;
                    (possessedBrain.BB.TargetBrain as IPawnTargetable).StartTargeting();
                }
            }
            else
            {
                //* 내부 타겟팅 순회
                if ((possessedBrain.BB.TargetBrain as IPawnTargetable).NextTarget() != null)
                    return;

                var targetableBrains = possessedBrain.PawnSensorCtrler.ListeningColliders.AsValueEnumerable()
                    .Select(c => c.GetComponent<PawnColliderHelper>())
                    .Where(h => h != null && h.pawnBrain != null && !h.pawnBrain.PawnBB.IsDead && h.pawnBrain is IPawnTargetable)
                    .Select(h => h.pawnBrain).Distinct().ToArrayPool();

                for (int i = 0; i < targetableBrains.Size; i++)
                {
                    if (targetableBrains.Array[i] == possessedBrain.BB.TargetBrain)
                    {
                        var nextTargetIndex = i + 1;
                        if (nextTargetIndex >= targetableBrains.Size) nextTargetIndex = 0;

                        possessedBrain.BB.target.targetPawnHP.Value = targetableBrains.Array[nextTargetIndex].PawnHP;
                        (possessedBrain.BB.TargetBrain as IPawnTargetable).StartTargeting();
                        break;
                    }
                }

                ArrayPool<PawnBrainController>.Shared.Return(targetableBrains.Array, true);
            }
        }

        public void OnPrevTarget()
        {
            if (possessedBrain.BB.TargetPawn == null)
            {
                var newTarget = possessedBrain.PawnSensorCtrler.ListeningColliders.AsValueEnumerable()
                    .Select(c => c.GetComponent<PawnColliderHelper>())
                    .Where(h => h != null && h.pawnBrain != null && !h.pawnBrain.PawnBB.IsDead && h.pawnBrain is IPawnTargetable)
                    .OrderBy(p => (p.transform.position - possessedBrain.GetWorldPosition()).sqrMagnitude)
                    .FirstOrDefault();

                if (newTarget != null)
                {
                    possessedBrain.BB.target.targetPawnHP.Value = newTarget.pawnBrain.PawnHP;
                    possessedBrain.Movement.freezeRotation = true;
                }
            }
            else
            {
                var colliderHelpers = possessedBrain.PawnSensorCtrler.ListeningColliders.AsValueEnumerable()
                    .Select(c => c.GetComponent<PawnColliderHelper>())
                    .Where(h => h != null && h.pawnBrain != null && !h.pawnBrain.PawnBB.IsDead && h.pawnBrain is IPawnTargetable)
                    .ToArrayPool();

                for (int i = colliderHelpers.Size - 1; i >= 0; i--)
                {
                    if (colliderHelpers.Array[i].pawnBrain == possessedBrain.BB.TargetBrain)
                    {
                        possessedBrain.BB.target.targetPawnHP.Value = (i - 1 >= 0 ? colliderHelpers.Array[i - 1] : colliderHelpers.Array[colliderHelpers.Size - 1]).pawnBrain.PawnHP;
                        break;
                    }
                }

                ArrayPool<PawnColliderHelper>.Shared.Return(colliderHelpers.Array, true);
            }
        }

        public void OnRoll()
        {
            if (possessedBrain == null) return;

            if (_isEnable_Roll == false)
                return;

            var actionData = DatasheetManager.Instance.GetActionData(possessedBrain.PawnBB.common.pawnId, "Rolling");
            Debug.Assert(actionData != null);

            // 대쉬 가능 체크
            var canRolling1 = possessedBrain.BB.IsSpawnFinished && !possessedBrain.BB.IsDead && !possessedBrain.BB.IsGroggy && !possessedBrain.BB.IsDown && !possessedBrain.BB.IsJumping && !possessedBrain.BB.IsRolling;
            var canRolling2 = canRolling1 && (!possessedBrain.ActionCtrler.CheckActionRunning() || possessedBrain.ActionCtrler.CanInterruptAction()) && !possessedBrain.StatusCtrler.CheckStatus(PawnStatus.CanNotRoll);

            if (canRolling2 == false)
                return;

            if (possessedBrain.ActionCtrler.CheckActionRunning())
                possessedBrain.ActionCtrler.CancelAction(false);

            var rollingXZ = Vector3.zero;
            var rollingVec = Vector3.zero;
            if (possessedBrain.Movement.moveVec == Vector3.zero)
            {
                rollingXZ = Vector3.back;
                rollingVec = -possessedBrain.Movement.capsule.forward.Vector2D().normalized;
            }
            else
            {
                rollingXZ = possessedBrain.Movement.capsule.InverseTransformDirection(possessedBrain.Movement.moveVec);
                rollingVec = possessedBrain.Movement.moveVec.Vector2D().normalized;
            }

            if (Mathf.Abs(rollingXZ.x) > Mathf.Abs(rollingXZ.z))
            {
                possessedBrain.AnimCtrler.mainAnimator.SetInteger("RollingX", rollingXZ.x > 0f ? 1 : -1);
                possessedBrain.AnimCtrler.mainAnimator.SetInteger("RollingZ", 0);
                possessedBrain.Movement.FaceTo(Quaternion.Euler(0f, rollingXZ.x > 0f ? -90f : 90f, 0f) * rollingVec);
            }
            else
            {
                possessedBrain.AnimCtrler.mainAnimator.SetInteger("RollingX", 0);
                possessedBrain.AnimCtrler.mainAnimator.SetInteger("RollingZ", rollingXZ.z > 0f ? 1 : -1);
                possessedBrain.Movement.FaceTo(rollingXZ.z > 0f ? rollingVec : -rollingVec);
            }

            possessedBrain.ActionCtrler.SetPendingAction("Rolling");

            // Roll Sound
            EffectManager.Instance.Show("FX/FX_Cartoony_Jump_Up_01", possessedBrain.GetWorldPosition(),
                Quaternion.identity, Vector3.one, 1f);
            SoundManager.Instance.Play(SoundID.JUMP);

            GameManager.Instance.PawnRolled();
        }

        bool FindAttackPoint(out Vector3 attackPoint)
        {
            var found = possessedBrain.SensorCtrler.WatchingColliders.AsValueEnumerable()
                .Select(c => c.GetComponent<PawnColliderHelper>()).Where(h => h != null && h == h.pawnBrain.coreColliderHelper)
                .OrderBy(h => Vector3.Angle(possessedBrain.coreColliderHelper.transform.forward.Vector2D(), (h.transform.position - possessedBrain.GetWorldPosition()).Vector2D()))
                .FirstOrDefault(h => possessedBrain.coreColliderHelper.GetApproachDistance(h) < attackPointAssistRange);

            if (found != null)
            {
                attackPoint = found.transform.position;
                return true;
            }
            else
            {
                attackPoint = Vector3.zero;
                return false;
            }
        }


        IDisposable __punchChargingDisposable;
        float __punchPresssedTimeStamp = -1f;
        float __punchReleasedTimeStamp = -1f;

        public void OnPunch(InputValue value)
        {
            if (_isEnable_Parry == false)
                return;
            if (possessedBrain == null)
                return;

            __punchChargingDisposable ??= Observable.EveryUpdate().Where(_ => __punchPresssedTimeStamp > __punchReleasedTimeStamp).Subscribe(_ =>
            {
                if (possessedBrain.ActionCtrler.CheckActionRunning())
                {
                    possessedBrain.BB.action.punchChargingLevel.Value = -1;
                    __punchReleasedTimeStamp = Time.time;
                    __Logger.LogR2(gameObject, nameof(OnAttack), "Puncg Charging canceled.", "CurrActionName", possessedBrain.ActionCtrler.CurrActionName);
                }
                else
                {
                    possessedBrain.BB.action.punchChargingLevel.Value = Mathf.FloorToInt(Time.time - __punchPresssedTimeStamp);
                }
            }).AddTo(this);

            if (value.isPressed)
            {
                __punchPresssedTimeStamp = Time.time;
            }
            else
            {
                __punchReleasedTimeStamp = Time.time;

                var canAction1 = possessedBrain.BB.IsSpawnFinished && !possessedBrain.BB.IsDead && !possessedBrain.BB.IsGroggy && !possessedBrain.BB.IsDown && !possessedBrain.BB.IsRolling && !possessedBrain.BB.IsHanging;
                var canAction2 = canAction1 && (!possessedBrain.ActionCtrler.CheckActionRunning() || possessedBrain.ActionCtrler.CanInterruptAction()) && !possessedBrain.StatusCtrler.CheckStatus(PawnStatus.Staggered);

                if (canAction2 && possessedBrain.BB.action.punchChargingLevel.Value > 0)
                {
                    if (possessedBrain.ActionCtrler.CheckActionRunning())
                        possessedBrain.ActionCtrler.CancelAction(false);

                    possessedBrain.ActionCtrler.SetPendingAction("PunchParry");
                }

                //* 챠징 어택 판별을 위해서 'punchChargeLevel' 값은 제일 마지막에 리셋
                possessedBrain.BB.action.punchChargingLevel.Value = -1;
            }
        }

        public void OnAttack(InputValue value)
        {
            if(_isEnable_NormalAttack == false)
                return;
            if (possessedBrain == null)
                return;

            var canAction1 = possessedBrain.BB.IsSpawnFinished && !possessedBrain.BB.IsDead && !possessedBrain.BB.IsGroggy && !possessedBrain.BB.IsDown && !possessedBrain.BB.IsRolling && !possessedBrain.BB.IsHanging;;
            // var canAction2 = canAction1 && !MyHeroBrain.PawnBB.IsThrowing && !MyHeroBrain.PawnBB.IsGrabbed;
            var canAction3 = canAction1 && !possessedBrain.StatusCtrler.CheckStatus(PawnStatus.Staggered);

            var isJellyActive = (boundJellyMesh.Value != null);
        
            if (canAction3)
            {
                var canNextAction = possessedBrain.ActionCtrler.CheckActionRunning() ? possessedBrain.ActionCtrler.CanInterruptAction() : (Time.time - possessedBrain.ActionCtrler.prevActionContext.finishTimeStamp) < MainTable.PlayerData.GetList().First().actionInputTimePadding;
                if (canNextAction)
                {
                    var baseActionName = possessedBrain.ActionCtrler.CheckActionRunning() ? possessedBrain.ActionCtrler.CurrActionName : possessedBrain.ActionCtrler.PrevActionName;
                    switch (baseActionName)
                    {
                        case "Punch":
                            possessedBrain.ActionCtrler.CancelAction(false);
                            possessedBrain.ActionCtrler.SetPendingAction("Slash#1");
                            break;
                        case "Slash#1":
                            possessedBrain.ActionCtrler.CancelAction(false);
                            possessedBrain.ActionCtrler.SetPendingAction("Slash#2");
                            break;
                        case "Slash#2":
                            possessedBrain.ActionCtrler.CancelAction(false);
                            possessedBrain.ActionCtrler.SetPendingAction("Slash#3");
                            break;
                        case "GroggyAttack#1":
                            if (isJellyActive == true)
                            {
                                possessedBrain.ActionCtrler.CancelAction(false);
                                possessedBrain.ActionCtrler.SetPendingAction("GroggyAttack#2");
                                possessedBrain.Movement.FaceAt(boundJellyMesh.Value.springMassSystem.core.position);
                            }
                            break;
                        case "GroggyAttack#2":
                            if (isJellyActive == true)
                            {
                                possessedBrain.ActionCtrler.CancelAction(false);
                                possessedBrain.ActionCtrler.SetPendingAction("GroggyAttack#3");
                                possessedBrain.Movement.FaceAt(boundJellyMesh.Value.springMassSystem.core.position);
                            }
                            break;
                    }
                }
                else if (!possessedBrain.ActionCtrler.CheckActionRunning())
                {
                    if (possessedBrain.BB.IsJumping)
                    {
                        possessedBrain.ActionCtrler.SetPendingAction("JumpAttack");
                        possessedBrain.ChangeWeapon(WeaponSetType.TWOHAND_WEAPON);
                    }
                    else 
                    {
                        if (isJellyActive == true)
                        {
                            possessedBrain.ActionCtrler.SetPendingAction("GroggyAttack#1");
                            possessedBrain.Movement.FaceAt(boundJellyMesh.Value.springMassSystem.core.position);
                        }
                        else
                        {
                            possessedBrain.ActionCtrler.SetPendingAction("Slash#1");
                        }
                    }
                }

                //* 타겟이 없을 경우엔 주변 타겟으로 공격 방향을 보정해줌
                if (possessedBrain.SensorCtrler.WatchingColliders.Count > 0 && FindAttackPoint(out var attackPoint))
                    possessedBrain.Movement.FaceAt(attackPoint);
            }
        }

        public void OnGroggyAttack(InputValue value)
        {
            if (_isEnable_NormalAttack == false)
                return;
            if (possessedBrain == null)
                return;

            var canAction1 = possessedBrain.BB.IsSpawnFinished && !possessedBrain.BB.IsDead && !possessedBrain.BB.IsGroggy && !possessedBrain.BB.IsDown && !possessedBrain.BB.IsRolling && !possessedBrain.BB.IsHanging; ;
            var canAction2 = canAction1 && (boundJellyMesh.Value != null);
            var canAction3 = canAction2 && !possessedBrain.StatusCtrler.CheckStatus(PawnStatus.Staggered);

            if (value.isPressed)
            {
                __punchPresssedTimeStamp = Time.time;

                var baseActionName = possessedBrain.ActionCtrler.CheckActionRunning() ? possessedBrain.ActionCtrler.CurrActionName : possessedBrain.ActionCtrler.PrevActionName;
                var canNextAction = possessedBrain.ActionCtrler.CheckActionRunning() ? possessedBrain.ActionCtrler.CanInterruptAction() : (Time.time - possessedBrain.ActionCtrler.prevActionContext.finishTimeStamp) < MainTable.PlayerData.GetList().First().actionInputTimePadding;
                if (canAction3)
                {
                    if (canNextAction)
                    {
                        switch (baseActionName)
                        {
                            case "GroggyAttack#1":
                                possessedBrain.ActionCtrler.CancelAction(false);
                                possessedBrain.ActionCtrler.SetPendingAction("GroggyAttack#2");
                                possessedBrain.Movement.FaceAt(boundJellyMesh.Value.springMassSystem.core.position);
                                break;
                            case "GroggyAttack#2":
                                possessedBrain.ActionCtrler.CancelAction(false);
                                possessedBrain.ActionCtrler.SetPendingAction("GroggyAttack#3");
                                possessedBrain.Movement.FaceAt(boundJellyMesh.Value.springMassSystem.core.position);
                                break;
                        }
                    }
                    else if (!possessedBrain.ActionCtrler.CheckActionRunning())
                    {
                        possessedBrain.ActionCtrler.SetPendingAction("GroggyAttack#1");
                        possessedBrain.Movement.FaceAt(boundJellyMesh.Value.springMassSystem.core.position);
                    }
                }
            }
        }
        
        public void OnSpecialAttack(InputValue value)
        {
            if (possessedBrain == null)
                return;

            if (value.isPressed)
            {
                var canAction1 = possessedBrain.BB.IsSpawnFinished && !possessedBrain.BB.IsDead && !possessedBrain.BB.IsGroggy && !possessedBrain.BB.IsDown && !possessedBrain.BB.IsRolling && !possessedBrain.BB.IsHanging;
                var canAction2 = canAction1 && (specialKeyCtrler?.actionName ?? string.Empty) != string.Empty;
                var canAction3 = canAction2 &&  (!possessedBrain.ActionCtrler.CheckActionRunning() || possessedBrain.ActionCtrler.CanInterruptAction()) && !possessedBrain.StatusCtrler.CheckStatus(PawnStatus.Staggered);

                if (canAction3)
                {
                    if (possessedBrain.ActionCtrler.CheckActionRunning())
                        possessedBrain.ActionCtrler.CancelAction(false);

                    if (specialKeyCtrler.actionName == "Assault")
                    {
                        possessedBrain.droneBotFormationCtrler.PickDroneBot().ActionCtrler.SetPendingAction(specialKeyCtrler.actionName, specialKeyCtrler.specialTag, string.Empty, 0f);
                        specialKeyCtrler.HideAsObservable().Subscribe(c =>  c.Unload()).AddTo(this);
                        specialKeyCtrler = null;
                    }
                    else if (specialKeyCtrler.actionName == "GroggyAttack")
                    {
                        possessedBrain.ActionCtrler.SetPendingAction($"GroggyAttack#{++specialKeyCtrler.actionCount}");
                        possessedBrain.Movement.FaceAt(boundJellyMesh.Value.springMassSystem.core.position);
                    }
                }
            }
        }

        public void OnDrink() 
        {
            Debug.Log("<color=red>OnDrink</color>");
            possessedBrain.ActionCtrler.SetPendingAction("DrinkPotion");
        }

        public void OnBurst(InputValue value)
        {
            Debug.Log("OnBurst");
            if (possessedBrain.BB.stat.rage.Value < possessedBrain.BB.stat.maxRage.Value)
                return;

            possessedBrain.BB.stat.rage.Value = 0;
            possessedBrain.ActionCtrler.SetPendingAction("Burst");
        }
    }
}