using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using UGUI.Rx;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity;
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
        public bool _isEnable_Drink = true;

        #region IPawnEventListener 구현
        void IPawnEventListener.OnReceivePawnActionStart(PawnBrainController sender, string actionName)
        {
            if (actionName == "OnJellyOut")
            {
                Debug.Assert(boundJellyMesh.Value == null);

                boundJellyMesh.Value = (sender as NpcBrain).jellyMeshCtrler;
                __Logger.LogR2(gameObject, nameof(IPawnEventListener.OnReceivePawnActionStart), "jellyBrain", sender, "OnJellyOut", "boundJellyMesh", boundJellyMesh.Value);

                // interactionKeyCtrler = new InteractionKeyController(sender, "GroggyAttack", "Groggy").Load().Show(GameContext.Instance.canvasManager.body.transform as RectTransform);
            }
            else if (actionName == "OnJellyOff")
            {
                Debug.Assert(boundJellyMesh.Value.hostBrain == sender);

                boundJellyMesh.Value = null;
                __Logger.LogR2(gameObject, nameof(IPawnEventListener.OnReceivePawnActionStart), "OnJellOff", "jellyBrain", sender);

                // interactionKeyCtrler.HideAsObservable().Subscribe(c =>  c.Unload()).AddTo(this);
                // interactionKeyCtrler = null;
            }
        }

        void IPawnEventListener.OnReceivePawnStatusChanged(PawnBrainController sender, PawnStatus status, float strength, float duration)
        {
            if (sender is NpcBrain && status == PawnStatus.Groggy && strength > 0f)
            {
                // Debug.Assert(interactionKeyCtrler == null);
                // interactionKeyCtrler = new InteractionKeyController(sender, "Assault", "Encounter").Load().Show(GameContext.Instance.canvasManager.body.transform as RectTransform);
            }
        }
        void IPawnEventListener.OnReceivePawnDamageContext(PawnBrainController sender, PawnHeartPointDispatcher.DamageContext damageContext)
        {
            // if (damageContext.senderActionSpecialTag == "Encounter")
            // {
            //     Debug.Assert(sender == possessedBrain);
            //     possessedBrain.BB.action.encounterBrain.Value = damageContext.receiverBrain;

            //     __Logger.LogR2(gameObject, "OnReceivePawnDamageContext", "Groggy-Encounter", "encounterBrain", damageContext.receiverBrain);
            // }
            // else if (damageContext.groggyBreakHit)
            // {
            //     Debug.Assert(sender == possessedBrain);
            //     possessedBrain.BB.action.encounterBrain.Value = null;

            //     __Logger.LogR2(gameObject, "OnReceivePawnDamageContext", "Groggy-Break");
            // }
        }
        void IPawnEventListener.OnReceivePawnSpawningStateChanged(PawnBrainController sender, PawnSpawnStates state)
        {
            //* 타겟팅 해제
            if (state == PawnSpawnStates.DespawningStart && possessedBrain.BB.TargetBrain == sender)
                possessedBrain.BB.target.targetPawnHP.Value = null;
        }
        #endregion

        public GameObject SpawnSlayerPawn(bool possessImmediately = false)
        {
            var spawnPosition = possessedBrain != null ? possessedBrain.GetWorldPosition() : transform.position;
            var spawnRotation = possessedBrain != null ? possessedBrain.GetWorldRotation() : Quaternion.LookRotation(Vector3.left + Vector3.back, Vector3.up);

            if (possessImmediately)
            {
                Possess(Instantiate(Resources.Load<GameObject>("Pawn/Player/Slayer-K"), spawnPosition, spawnRotation).GetComponent<SlayerBrain>());
                return PossessedPawn;
            }
            else
            {
                return Instantiate(Resources.Load<GameObject>("Pawn/Player/Slayer-K"), spawnPosition, spawnRotation);
            }
        }

        public GameObject SpawnDroneBot(Vector3 position, Quaternion rotation)
        {
            return Instantiate(Resources.Load<GameObject>("Pawn/Player/DroneBot"), position, rotation);
        }

        public bool Possess(SlayerBrain targetBrain)
        {
            if (targetBrain == null || targetBrain.PawnBB.IsPossessed)
                return false;

            targetBrain.owner = this;
            possessedBrain = targetBrain;

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
            PawnEventManager.Instance.RegisterEventListener(this);
        }

        void Update()
        {
            if (!CanProcessInput())
                return;

            HandleMovement();
            HandleRotation();
        }

        bool CanProcessInput() => possessedBrain != null && GameContext.Instance.launcher.currGameMode?.CanPlayerConsumeInput() == true;
        bool CheckPossessedPawnBusy() => !possessedBrain.BB.IsSpawnFinished || possessedBrain.BB.IsDead || possessedBrain.BB.IsGroggy || possessedBrain.BB.IsDown || possessedBrain.BB.IsRolling;
        bool CanExecuteAction() => (!possessedBrain.ActionCtrler.CheckActionRunning() || possessedBrain.ActionCtrler.CanInterruptAction()) && !possessedBrain.StatusCtrler.CheckStatus(PawnStatus.Staggered);

        void HandleMovement()
        {
            if (inputMoveVec.Value.sqrMagnitude > 0)
            {
                possessedBrain.Movement.moveSpeed = (possessedBrain.BB.IsGuarding || possessedBrain.BB.IsPunchCharging) ? possessedBrain.BB.body.guardSpeed : possessedBrain.BB.body.walkSpeed;
                possessedBrain.Movement.moveVec = Quaternion.AngleAxis(45, Vector3.up) * new Vector3(inputMoveVec.Value.x, 0, inputMoveVec.Value.y);

                //* Strafe 모드가 아닌 경우엔 이동 방향과 회전 방향이 동일함
                if (!possessedBrain.Movement.freezeRotation)
                    possessedBrain.Movement.faceVec = possessedBrain.Movement.moveVec;
            }
            else
            {
                possessedBrain.Movement.moveVec = Vector3.zero;
                possessedBrain.Movement.moveSpeed = possessedBrain.BB.body.walkSpeed;
            }
        }

        void HandleRotation()
        {
            possessedBrain.Movement.freezeRotation = GameContext.Instance.launcher.currGameMode.IsInCombat();

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
            if (!CanProcessInput())
                return;

            inputMoveVec.Value = value.Get<Vector2>();
        }

        public void OnLook(InputValue value)
        {
            if (!CanProcessInput())
                return;

            if (GameContext.Instance.cameraCtrler != null &&
                GameContext.Instance.cameraCtrler.TryGetPickingPointOnTerrain(value.Get<Vector2>(), out var pickingPoint))
                inputLookVec.Value = (pickingPoint - possessedBrain.Movement.capsule.transform.position).Vector2D().normalized;
        }

        public void OnGuard(InputValue value)
        {
            if (!CanProcessInput())
                return;

            possessedBrain.BB.body.isGuarding.Value = value.Get<float>() > 0;

            if (possessedBrain.BB.IsGuarding)
            {
                if (CheckPossessedPawnBusy() || !CanExecuteAction())
                    return;

                if (!possessedBrain.StatusCtrler.CheckStatus(PawnStatus.CanNotGuard))
                    possessedBrain.StatusCtrler.AddStatus(PawnStatus.GuardParrying, 1f, possessedBrain.BB.action.guardParryDuration);
            }
        }

        IDisposable __jumpHangingDisposable;
        float __jumpExecutedTimeStamp = -1f;
        float __jumpReleasedTimeStamp = -1f;

        public void OnJump()
        {
            if (!CanProcessInput())
                return;

            if (CheckPossessedPawnBusy() || !CanExecuteAction())
                return;

            if (possessedBrain.ActionCtrler.CheckActionRunning())
                possessedBrain.ActionCtrler.CancelAction(false);

            if (possessedBrain.BB.IsJumping && !possessedBrain.Movement.IsHangingReserved())
            {
                //* Catch 판정은 0.1초로 셋팅
                var droneBot = possessedBrain.droneBotFormationCtrler.PickDroneBot();
                if (!possessedBrain.BB.IsHanging && droneBot != null && droneBot.BB.CurrDecision != DroneBotBrain.Decisions.Catch && Time.time - __jumpExecutedTimeStamp > 0.1f)
                    possessedBrain.Movement.PrepareHanging(droneBot, 0.2f);
            }
            else if (possessedBrain.BB.IsHanging)
            {
                //* 점프 방향을 셋팅해줌
                possessedBrain.Movement.GetCharacterMovement().velocity = possessedBrain.BB.body.hangingBrain.Value.Movement.CurrVelocity;
                possessedBrain.Movement.FinishHanging();
                possessedBrain.Movement.StartJump(possessedBrain.BB.body.jumpHeight);
            }
            else
            {
                //! 점프 코스트 하드코딩
                float jumpStaminaCost = 10;

                possessedBrain.Movement.StartJump(possessedBrain.BB.body.jumpHeight);
                possessedBrain.BB.stat.ReduceStamina(jumpStaminaCost);
            }
        }

        public void OnNextTarget()
        {
            if (!CanProcessInput())
                return;

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
                    .Select(c => c.TryGetComponent<PawnColliderHelper>(out var colliderHelper) ? colliderHelper.pawnBrain : null)
                    .Where(b => b != null && !b.PawnBB.IsDead && b is IPawnTargetable)
                    .Distinct().ToArrayPool();

                var currTargetIndex = -1;

                for (int i = 0; i < targetableBrains.Size; i++)
                {
                    if (targetableBrains.Array[i] == possessedBrain.BB.TargetBrain)
                    {
                        currTargetIndex = i;
                        break;
                    }
                }

                currTargetIndex++;

                if (currTargetIndex >= targetableBrains.Size)
                    currTargetIndex = 0;

                possessedBrain.BB.target.targetPawnHP.Value = targetableBrains.Array[currTargetIndex].PawnHP;
                (possessedBrain.BB.TargetBrain as IPawnTargetable).StartTargeting();

                ArrayPool<PawnBrainController>.Shared.Return(targetableBrains.Array, true);
            }
        }

        public void OnPrevTarget()
        {
            if (!CanProcessInput())
                return;

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
                if ((possessedBrain.BB.TargetBrain as IPawnTargetable).PrevTarget() != null)
                    return;

                var targetableBrains = possessedBrain.PawnSensorCtrler.ListeningColliders.AsValueEnumerable()
                    .Select(c => c.TryGetComponent<PawnColliderHelper>(out var colliderHelper) ? colliderHelper.pawnBrain : null)
                    .Where(b => b != null && !b.PawnBB.IsDead && b is IPawnTargetable)
                    .Distinct().ToArrayPool();

                var currTargetIndex = -1;

                for (int i = 0; i < targetableBrains.Size; i++)
                {
                    if (targetableBrains.Array[i] == possessedBrain.BB.TargetBrain)
                    {
                        currTargetIndex = i;
                        break;
                    }
                }

                currTargetIndex--;

                if (currTargetIndex < 0)
                    currTargetIndex = targetableBrains.Size - 1;

                possessedBrain.BB.target.targetPawnHP.Value = targetableBrains.Array[currTargetIndex].PawnHP;
                (possessedBrain.BB.TargetBrain as IPawnTargetable).StartTargeting();

                ArrayPool<PawnBrainController>.Shared.Return(targetableBrains.Array, true);
            }
        }

        public void OnRoll()
        {
            if (_isEnable_Roll == false)
                return;

            if (!CanProcessInput())
                return;
            if (CheckPossessedPawnBusy())
                return;

            var canRoll1 = !possessedBrain.ActionCtrler.CheckActionRunning() || possessedBrain.ActionCtrler.CanInterruptAction();
            var canRoll2 = canRoll1 & !possessedBrain.StatusCtrler.CheckStatus(PawnStatus.CanNotRoll);

            if (!canRoll2)
                return;

            var actionData = DatasheetManager.Instance.GetActionData(possessedBrain.PawnBB.common.pawnId, "Rolling");
            Debug.Assert(actionData != null);

            if (possessedBrain.ActionCtrler.CheckActionRunning())
                possessedBrain.ActionCtrler.CancelAction(false);

            Vector3 rollingXZ;
            Vector3 rollingVec;
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
            if (!CanProcessInput() || !GameContext.Instance.launcher.currGameMode.IsInCombat())
                return;

            if (value.isPressed)
            {
                if (!CheckPossessedPawnBusy() && !possessedBrain.BB.IsHanging && CanExecuteAction() && possessedBrain.ActionCtrler.CurrActionName == "Slash#2")
                {
                    possessedBrain.ActionCtrler.CancelAction(false);
                    possessedBrain.ActionCtrler.SetPendingAction("Punch");

                    return;
                }

                __punchPresssedTimeStamp = Time.time;
                __punchChargingDisposable ??= Observable.EveryUpdate()
                    .Subscribe(_ =>
                    {
                        possessedBrain.BB.action.punchChargingLevel.Value = possessedBrain.ActionCtrler.CheckActionRunning() ? -1 : Mathf.FloorToInt(Time.time - __punchPresssedTimeStamp);

                        if (possessedBrain.BB.action.punchChargingLevel.Value < 0)
                        {
                            __punchChargingDisposable?.Dispose();
                            __punchChargingDisposable = null;
                            __punchReleasedTimeStamp = Time.time;
                        }
                    }).AddTo(this);
            }
            else
            {
                __punchChargingDisposable?.Dispose();
                __punchChargingDisposable = null;
                __punchReleasedTimeStamp = Time.time;

                if (!CheckPossessedPawnBusy() && !possessedBrain.BB.IsHanging && CanExecuteAction() && possessedBrain.BB.action.punchChargingLevel.Value > 0)
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
            if (!CanProcessInput() || !GameContext.Instance.launcher.currGameMode.IsInCombat())
                return;
            if (CheckPossessedPawnBusy() || possessedBrain.BB.IsHanging)
                return;

            if (_isEnable_NormalAttack == false)
                return;

            var isJellyActive = boundJellyMesh.Value != null;

            if (!possessedBrain.StatusCtrler.CheckStatus(PawnStatus.Staggered))
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
                        case "Slash#3":
                            possessedBrain.ActionCtrler.CancelAction(false);
                            possessedBrain.ActionCtrler.SetPendingAction("Slash#4");
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
            if (!CanProcessInput() || !GameContext.Instance.launcher.currGameMode.IsInCombat())
                return;
            if (CheckPossessedPawnBusy() || possessedBrain.BB.IsHanging)
                return;

            if (_isEnable_NormalAttack == false)
                return;

            if (value.isPressed && !possessedBrain.StatusCtrler.CheckStatus(PawnStatus.Staggered))
            {
                __punchPresssedTimeStamp = Time.time;

                var baseActionName = possessedBrain.ActionCtrler.CheckActionRunning() ? possessedBrain.ActionCtrler.CurrActionName : possessedBrain.ActionCtrler.PrevActionName;
                var canNextAction = possessedBrain.ActionCtrler.CheckActionRunning() ? possessedBrain.ActionCtrler.CanInterruptAction() : (Time.time - possessedBrain.ActionCtrler.prevActionContext.finishTimeStamp) < MainTable.PlayerData.GetList().First().actionInputTimePadding;

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

        public void OnSpecialAttack(InputValue value)
        {
            if (!CanProcessInput())
                return;
            if (!GameContext.Instance.launcher.currGameMode.IsInCombat())
                return;

            if (value.isPressed)
            {
                var pressedCtrler = GameContext.Instance.interactionKeyCtrlers.AsValueEnumerable().FirstOrDefault(i => i.PreprocessKeyDown());

                if (pressedCtrler == null)
                    return;

                if (pressedCtrler.commandName == "Chainsaw")
                {
                    if (possessedBrain.ActionCtrler.CheckActionRunning())
                        possessedBrain.ActionCtrler.CancelAction(false);

                    possessedBrain.ActionCtrler.SetPendingAction("Chainsaw");
                    pressedCtrler.HideAsObservable().Subscribe(_ => pressedCtrler.Unload()).AddTo(this);
                }
                else if (pressedCtrler.commandName == "Assault")
                {
                    if (possessedBrain.ActionCtrler.CheckActionRunning())
                        possessedBrain.ActionCtrler.CancelAction(false);

                    // possessedBrain.droneBotFormationCtrler.PickDroneBot().ActionCtrler.SetPendingAction("Assault", "Assault", string.Empty, 0f);
                    possessedBrain.droneBotFormationCtrler.PickDroneBot().ActionCtrler.SetPendingAction("Assault");
                    pressedCtrler.HideAsObservable().Subscribe(_ => pressedCtrler.Unload()).AddTo(this);
                }
                else if (pressedCtrler.commandName == "RunLine")
                {
                    pressedCtrler.PostProcessKeyDown(true);

                    Debug.Assert(GameContext.Instance.dialogueRunner.IsDialogueRunning);

                    Observable.NextFrame().Subscribe(_ =>
                    {
                        foreach (var v in GameContext.Instance.dialogueRunner.dialogueViews)
                        {
                            if (v is LineView lineView)
                                lineView.UserRequestedViewAdvancement();
                        }
                    }).AddTo(this);
                }
                else if (pressedCtrler.commandName == "NextRoom")
                {
                    pressedCtrler.PostProcessKeyDown(true);

                    GameContext.Instance.launcher.currGameMode.ChangeScene("Tutorial-1");
                }
            }
        }

        public void OnDrink()
        {
            if (!CanProcessInput() || !GameContext.Instance.launcher.currGameMode.IsInCombat())
                return;
            if (_isEnable_Drink == false)
                return;

            Debug.Log("<color=red>Heal</color>");
            // possessedBrain.ActionCtrler.SetPendingAction("DrinkPotion");

            var droneBot = possessedBrain.droneBotFormationCtrler.PickDroneBot();
            if (droneBot != null)
                droneBot.ActionCtrler.SetPendingAction("Heal");
        }

        public void OnBurst(InputValue value)
        {
            if (!CanProcessInput() || !GameContext.Instance.launcher.currGameMode.IsInCombat())
                return;

            Debug.Log("OnBurst");
            if (possessedBrain.BB.stat.burst.Value < possessedBrain.BB.stat.maxBurst.Value)
                return;

            possessedBrain.BB.stat.burst.Value = 0;
            possessedBrain.ActionCtrler.SetPendingAction("Burst");
        }

        public void OnHook()
        {
            if (possessedBrain.BB.TargetBrain != null)
                possessedBrain.droneBotFormationCtrler.PickDroneBot().ActionCtrler.SetPendingAction("Hook");
        }
    }
}