using System;
using System.Buffers;
using System.Linq;
using UGUI.Rx;
using UniRx;
using UnityEditor.Experimental.GraphView;
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
        public HeroBrain possessedBrain;
        public GameObject PossessedPawn => possessedBrain != null ? possessedBrain.gameObject : null;
        public Action<HeroBrain> onPossessed;
        public Action<HeroBrain> onUnpossessed;

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
            }
            else if (actionName == "OnJellyOff")
            {
                Debug.Assert(boundJellyMesh.Value.jellyBrain == sender);

                boundJellyMesh.Value = null;
                __Logger.LogR2(gameObject, nameof(IPawnEventListener.OnReceivePawnActionStart), "OnJellOff", "jellyBrain", sender);
            }
        }

        void IPawnEventListener.OnReceivePawnStatusChanged(PawnBrainController sender, PawnStatus status, float strength, float duration) 
        {
            if (sender is JellyBrain && status == PawnStatus.Groggy && strength > 0f)
            {
                Debug.Assert(specialKeyCtrler == null);
                specialKeyCtrler = new SpecialKeyController(sender, "Assault", "Encounter").Load().Show(GameContext.Instance.MainCanvasCtrler.body);
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
#endregion

        public GameObject SpawnHeroPawn(GameObject heroPrefab, bool possessImmediately = false)
        {
            var spawnPosition = possessedBrain != null ? possessedBrain.GetWorldPosition() : transform.position;
            var spawnRotation = possessedBrain != null ? possessedBrain.GetWorldRotation() : Quaternion.LookRotation(Vector3.left + Vector3.back, Vector3.up);

            if (possessImmediately)
            {
                Possess(Instantiate(heroPrefab, spawnPosition, spawnRotation).GetComponent<HeroBrain>());
                return PossessedPawn;
            }
            else
            {
                return Instantiate(heroPrefab, spawnPosition, spawnRotation);
            }
        }

        public bool Possess(HeroBrain targetBrain)
        {
            if (targetBrain == null || targetBrain.PawnBB.IsPossessed)
                return false;

            targetBrain.owner = this;
            possessedBrain = targetBrain as HeroBrain;

            transform.SetParent((Transform)targetBrain.transform, false);
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            targetBrain.OnPossessedHandler();
            onPossessed?.Invoke(targetBrain);

            __Logger.LogR1(gameObject, nameof(Possess), nameof(targetBrain), targetBrain.name);
            return true;
        }

        public void Unpossess()
        {
            transform.SetParent(null, true);
            onUnpossessed?.Invoke(possessedBrain);

            Debug.Log($"1?? {PossessedPawn.name} is unpossessed by {gameObject.name}.");
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

        public void OnParry()
        {
            if (_isEnable_Parry == false)
                return;
            if (possessedBrain == null)
                return;

            var canAction1 = possessedBrain.BB.IsSpawnFinished && !possessedBrain.BB.IsDead && !possessedBrain.BB.IsGroggy && !possessedBrain.BB.IsDown && !possessedBrain.BB.IsRolling && !possessedBrain.BB.IsHanging;
            var canAction2 = canAction1 && (!possessedBrain.ActionCtrler.CheckActionRunning() || possessedBrain.ActionCtrler.CanInterruptAction()) && !possessedBrain.StatusCtrler.CheckStatus(PawnStatus.Staggered);

            if (canAction2)
            {

                if (possessedBrain.ActionCtrler.CheckActionRunning())
                    possessedBrain.ActionCtrler.CancelAction(false);

                possessedBrain.ActionCtrler.SetPendingAction("Punch");
            }
        }

        IDisposable __chargingAttackDisposable;
        float __attackPresssedTimeStamp = -1f;
        float __attackReleasedTimeStamp = -1f;

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

        public void OnAttack(InputValue value)
        {
            if(_isEnable_NormalAttack == false)
                return;
            if (possessedBrain == null)
                return;

            __chargingAttackDisposable ??= Observable.EveryUpdate().Where(_ => __attackPresssedTimeStamp > __attackReleasedTimeStamp).Subscribe(_ =>
            {
                if (possessedBrain.ActionCtrler.CheckActionRunning())
                {
                    possessedBrain.BB.action.punchChargeLevel.Value = -1;
                    __attackReleasedTimeStamp = Time.time;
                    __Logger.LogR2(gameObject, nameof(OnAttack), "Charging canceled.", "CurrActionName", possessedBrain.ActionCtrler.CurrActionName);
                }
                else
                {
                    possessedBrain.BB.action.punchChargeLevel.Value = Mathf.FloorToInt(Time.time - __attackPresssedTimeStamp);
                }
            }).AddTo(this);

            var canAction1 = possessedBrain.BB.IsSpawnFinished && !possessedBrain.BB.IsDead && !possessedBrain.BB.IsGroggy && !possessedBrain.BB.IsDown && !possessedBrain.BB.IsRolling && !possessedBrain.BB.IsHanging;;
            // var canAction2 = canAction1 && !MyHeroBrain.PawnBB.IsThrowing && !MyHeroBrain.PawnBB.IsGrabbed;
            var canAction3 = canAction1 && !possessedBrain.StatusCtrler.CheckStatus(PawnStatus.Staggered);

            if (value.isPressed)
            {
                __attackPresssedTimeStamp = Time.time;

                var baseActionName = possessedBrain.ActionCtrler.CheckActionRunning() ? possessedBrain.ActionCtrler.CurrActionName : possessedBrain.ActionCtrler.PrevActionName;
                var canNextAction = possessedBrain.ActionCtrler.CheckActionRunning() ? possessedBrain.ActionCtrler.CanInterruptAction() : (Time.time - possessedBrain.ActionCtrler.prevActionContext.finishTimeStamp) < MainTable.PlayerData.GetList().First().actionInputTimePadding;
                if (canAction3 && canNextAction)
                {
                    switch (baseActionName)
                    {
                        case "Slash#1":
                            possessedBrain.ActionCtrler.CancelAction(false);
                            possessedBrain.ActionCtrler.SetPendingAction("Slash#2");
                            break;
                        case "Slash#2":
                            possessedBrain.ActionCtrler.CancelAction(false);
                            possessedBrain.ActionCtrler.SetPendingAction("Slash#3");
                            break;
                        case "GroggyAttack#1":
                            possessedBrain.ActionCtrler.CancelAction(false);
                            possessedBrain.ActionCtrler.SetPendingAction("GroggyAttack#2");
                            break;
                        case "GroggyAttack#2":
                            possessedBrain.ActionCtrler.CancelAction(false);
                            possessedBrain.ActionCtrler.SetPendingAction("GroggyAttack#3");
                            break;
                        case "Punch":
                            possessedBrain.ActionCtrler.CancelAction(false);
                            possessedBrain.ActionCtrler.SetPendingAction("Slash#1");
                            break;
                        case "HeavySlash#1":
                            possessedBrain.ActionCtrler.CancelAction(false);
                            possessedBrain.ActionCtrler.SetPendingAction("HeavySlash#2");
                            break;
                        case "HeavySlash#2":
                            possessedBrain.ActionCtrler.CancelAction(false);
                            possessedBrain.ActionCtrler.SetPendingAction("HeavySlash#3");
                            break;
                    }

                    //* 타겟이 없을 경우엔 주변 타겟으로 공격 방향을 보정해줌
                    if (possessedBrain.SensorCtrler.WatchingColliders.Count > 0 && FindAttackPoint(out var attackPoint))
                        possessedBrain.Movement.FaceAt(attackPoint);
                }
            }
            else
            {
                __attackReleasedTimeStamp = Time.time;

                if (canAction3 && !possessedBrain.ActionCtrler.CheckActionRunning())
                {
                    if (possessedBrain.BB.IsJumping)
                    {
                        possessedBrain.ActionCtrler.SetPendingAction("JumpAttack");
                        possessedBrain.ChangeWeapon(WeaponSetType.TWOHAND_WEAPON);
                    }
                    else if (possessedBrain.BB.action.punchChargeLevel.Value >= 1)
                    {
                        possessedBrain.ActionCtrler.SetPendingAction("HeavySlash#1");
                        possessedBrain.ChangeWeapon(WeaponSetType.TWOHAND_WEAPON);
                    }
                    else 
                    {
                        if (possessedBrain.BB.TargetBrain != null && possessedBrain.BB.TargetBrain.PawnBB.IsGroggy)
                            possessedBrain.ActionCtrler.SetPendingAction("GroggyAttack#1");
                        else
                            possessedBrain.ActionCtrler.SetPendingAction("Slash#1");
                        possessedBrain.ChangeWeapon(WeaponSetType.ONEHAND_WEAPONSHIELD);
                    }

                    //* 타겟이 없을 경우엔 주변 타겟으로 공격 방향을 보정해줌
                    if (possessedBrain.SensorCtrler.WatchingColliders.Count > 0 && FindAttackPoint(out var attackPoint))
                        possessedBrain.Movement.FaceAt(attackPoint);
                }

                //* 챠징 어택 판별을 위해서 'punchChargeLevel' 값은 제일 마지막에 리셋
                possessedBrain.BB.action.punchChargeLevel.Value = -1;
            }
        }
        public void OnGroggyAttack(InputValue value)
        {
            if (_isEnable_NormalAttack == false)
                return;
            if (possessedBrain == null)
                return;

            var canAction1 = possessedBrain.BB.IsSpawnFinished && !possessedBrain.BB.IsDead && !possessedBrain.BB.IsGroggy && !possessedBrain.BB.IsDown && !possessedBrain.BB.IsRolling && !possessedBrain.BB.IsHanging; ;
            // var canAction2 = canAction1 && !MyHeroBrain.PawnBB.IsThrowing && !MyHeroBrain.PawnBB.IsGrabbed;
            var canAction3 = canAction1 && !possessedBrain.StatusCtrler.CheckStatus(PawnStatus.Staggered);

            if (value.isPressed)
            {

            }
        }
        public void OnSpecialAttack(InputValue value)
        {
            if (_isEnable_SpecialAttack == false)
                return;
            if (possessedBrain == null)
                return;

            if (value.isPressed)
            {
                var canAction1 = possessedBrain.BB.IsSpawnFinished && !possessedBrain.BB.IsDead && !possessedBrain.BB.IsGroggy && !possessedBrain.BB.IsDown && !possessedBrain.BB.IsRolling && !possessedBrain.BB.IsHanging;;
                var canAction2 = canAction1 && (specialKeyCtrler?.actionName ?? string.Empty) != string.Empty;
                var canAction3 = canAction2 &&  (!possessedBrain.ActionCtrler.CheckActionRunning() || possessedBrain.ActionCtrler.CanInterruptAction()) && !possessedBrain.StatusCtrler.CheckStatus(PawnStatus.Staggered);

                if (canAction3)
                {
                    if (possessedBrain.ActionCtrler.CheckActionRunning())
                        possessedBrain.ActionCtrler.CancelAction(false);

                    possessedBrain.droneBotFormationCtrler.PickDroneBot().ActionCtrler.SetPendingAction(specialKeyCtrler.actionName, specialKeyCtrler.specialTag, string.Empty, 0f);

                    specialKeyCtrler.HideAsObservable().Subscribe(c =>  c.Unload(true)).AddTo(this);
                    specialKeyCtrler = null;

                    // if (possessedBrain.BB.IsJumping && (possessedBrain.Movement.LastFinishHangingTimeStamp - Time.time) < 0.2f)
                    //     possessedBrain.ActionCtrler.SetPendingAction("SpecialKick");
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