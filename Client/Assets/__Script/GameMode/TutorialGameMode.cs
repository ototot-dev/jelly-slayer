using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UniRx;
using UGUI.Rx;
using Yarn.Unity;
using ZLinq;
using Game.UI;
using UnityEditor.Experimental.GraphView;
using NUnit.Framework;
using System.Collections.Generic;


namespace Game
{
    public enum TutorialMode 
    {
        None,

        NormalAttack,
        PowerAttack,

        Guard,
        Evade,
        Heal,
        Parry,

        Groggy,

        FreeBattle,

        Room2_Step1,
        Room2_Step2,
        Room2_Step3,

        Room3_Step1, // 라펙스 4기
        Room3_Step2, // 물약 회복
        Room3_Step3, // 로보솔저

        Room4_Step1,

        Room5_Step1,

        End,
    }
    public enum TutorialScene 
    {
        Tutorial_1,     // 시작 씬, 전화
        Tutorial_2,     // 적 조우, Rapex
        Tutorial_3,     // RoboSoldier
        Tutorial_4,     // 원거리 적
        Tutorial_5,     // M82
    }

    public class TutorialGameMode : BaseGameMode, IPawnEventListener
    {
        public override GameModeTypes GetGameModeType() => GameModeTypes.Tutorial;
        public override DialogueDispatcher GetDialogueDispatcher() => __dialogueDispatcher;
        TutorialDialogueDispatcher __dialogueDispatcher;
        LoadingPageController __loadingPageCtrler;
        string __currSceneName;

        public TutorialScene _curScene = TutorialScene.Tutorial_1;

        [Header("Tutorial")]
        public TutorialMode _tutorialMode = TutorialMode.None;
        [SerializeField] private bool _isInCombat = false;
        [SerializeField] private int _attackCount = 0;
        [SerializeField] private int _deadCount = 0;

        private TutorialRoboSoldierBrain _roboBrain;

        private List<PawnBrainController> _pawnList = new ();

        public override bool IsInCombat() => _isInCombat;

        public override bool CanPlayerConsumeInput()
        {
            if (__loadingPageCtrler != null) return false;
            return !__dialogueDispatcher.IsDialogueRunning() || 
                GameContext.Instance.playerCtrler.interactionKeyCtrlers.AsValueEnumerable().Any(i => i.IsInteractableEnabled) ||
                _tutorialMode != TutorialMode.None;
        }

        void Awake()
        {
            __dialogueDispatcher = gameObject.AddComponent<TutorialDialogueDispatcher>();
        }

        void Start()
        {
            PawnEventManager.Instance.RegisterEventListener(this as IPawnEventListener);
        }

        public override IObservable<Unit> EnterAsObservable()
        {
            InitPlayerCharacter(transform);

            _curScene = GameContext.Instance.launcher._tutorialStartScene;
            switch (_curScene) 
            {
                case TutorialScene.Tutorial_1: InitStartRoom(); break;
                case TutorialScene.Tutorial_2: InitTurorialRoom_2(); break;
                case TutorialScene.Tutorial_3: InitTurorialRoom_3(); break;
                case TutorialScene.Tutorial_4: InitTurorialRoom_4(); break;
                case TutorialScene.Tutorial_5: InitTurorialRoom_5(); break;
            }
            //Observable.FromCoroutine(ChangeRoom_Coroutine);

            return Observable.NextFrame();
        }
        
        public override IObservable<Unit> ExitAsObservable()
        {
            return Observable.NextFrame();
        }

        public override IObservable<Unit> ChangeSceneAsObservable(string sceneName)
        {
            return Observable.FromCoroutine(ChangeRoom_Coroutine);
        }

        void InitPlayerCharacter(Transform spawnPoint)
        {
            if (GameContext.Instance.playerCtrler.possessedBrain == null)
            {
                //* 슬레이어 스폰
                GameContext.Instance.playerCtrler.SpawnSlayerPawn(true);
                GameContext.Instance.playerCtrler.possessedBrain.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
                GameContext.Instance.playerCtrler.possessedBrain.TryGetComponent<SlayerAnimController>(out var animCtrler);

                GameContext.Instance.playerCtrler.possessedBrain.PawnHP.onDamaged += ((damageContex) => {
                    int kk = 0;
                });
                GameContext.Instance.playerCtrler.possessedBrain.StatusCtrler.onStatusActive += ((pawnStatus) => {

                    if (pawnStatus == PawnStatus.HPRegen && _tutorialMode == TutorialMode.Room3_Step2) {

                        EndMode();
                        InitBubbleDialogue("Tutorial3_step3");
                    }
                });

                InitSlayerBrainHandler();

                new PlayerStatusBarController().Load(GameObject.FindFirstObjectByType<PlayerStatusBarTemplate>()).Show(GameContext.Instance.canvasManager.body.transform as RectTransform);
            }
        }

        void RefreshPlayerCharacter(Transform spawnPoint)
        { 
            (GameContext.Instance.playerCtrler.possessedBrain as IPawnMovable).Teleport(spawnPoint.position, spawnPoint.rotation);

            //* LegAnimator 다시 활성화
            Observable.Timer(TimeSpan.FromSeconds(0.1f)).Subscribe(_ => GameContext.Instance.playerCtrler.possessedBrain.AnimCtrler.legAnimator.enabled = true);
        }

        void InitBubbleDialogue(string nodeName) 
        {
            if (__dialogueDispatcher != null)
            {
                __dialogueDispatcher.onRunLine = null;
                __dialogueDispatcher.onDialoqueComplete = null;

                __dialogueDispatcher.StopDialogue();
                __dialogueDispatcher.StartDialogue(nodeName);
            }
            var obj = GameObject.Find("3d-bubble-dialogue");
            BubbleDialoqueController controller;
            if (obj != null)
                controller = new BubbleDialoqueController().Load(obj.GetComponent<Template>());
            else
                controller = new BubbleDialoqueController().Load();

            controller.Show(GameContext.Instance.canvasManager.body.transform as RectTransform);
        }

        void InitLoadingPageCtrler(string nodeName, Action action = null) 
        {
            //* 로딩 화면 종료
            __loadingPageCtrler.HideAsObservable().Subscribe(__ =>
            {
                __loadingPageCtrler.Unload();
                __loadingPageCtrler = null;

                InitBubbleDialogue(nodeName);

                action?.Invoke();
            });
        }

        void InitCamera()
        {
            //* 카메라 타겟 셋팅
            GameContext.Instance.cameraCtrler.virtualCamera.LookAt = GameContext.Instance.playerCtrler.possessedBrain.coreColliderHelper.transform;
            GameContext.Instance.cameraCtrler.virtualCamera.Follow = GameContext.Instance.playerCtrler.possessedBrain.coreColliderHelper.transform;

            //* 카메라 이동 영역 셋팅
            var confinerBoundingBox = TaggerSystem.FindGameObjectWithTag("ConfinerBoundingBox").GetComponent<BoxCollider>();
            if (confinerBoundingBox != null)
                GameContext.Instance.cameraCtrler.RefreshConfinerVolume(confinerBoundingBox, Quaternion.Euler(45f, 45f, 0f), 10f);
        }

        void InitStartRoom() 
        { 
            GameContext.Instance.canvasManager.FadeInImmediately(Color.black);

            __currSceneName = "Tutorial-PhoneRoom";
            _curScene = TutorialScene.Tutorial_1;

            //* 로딩 시작
            __loadingPageCtrler = new LoadingPageController(new string[] { "Pawn/Player/Slayer-K", "Pawn/Player/DrontBot" }, 
                new string[] { __currSceneName });
            __loadingPageCtrler.Load().Show(GameContext.Instance.canvasManager.dimmed.transform as RectTransform);
            __loadingPageCtrler.onLoadingCompleted += () =>
            {
                //* 슬레이어 초기 위치 
                var spawnPoint = TaggerSystem.FindGameObjectWithTag("PlayerSpawnPoint").transform;
                RefreshPlayerCharacter(spawnPoint);

                InitCamera();

                InitLoadingPageCtrler("Tutorial-1", () => { });
            };
        }

        // 긴 복도 + 적 출현
        void InitTurorialRoom_2() 
        {
            GameContext.Instance.canvasManager.FadeInImmediately(Color.black);

            __currSceneName = "Tutorial-ShortCorridor";
            _curScene = TutorialScene.Tutorial_2;

            //* 로딩 시작
            __loadingPageCtrler = new LoadingPageController(new string[] { "Pawn/Npc/Rapex" }, 
                new string[] { __currSceneName });
            __loadingPageCtrler.Load().Show(GameContext.Instance.canvasManager.dimmed.transform as RectTransform);
            __loadingPageCtrler.onLoadingCompleted += () =>
            {
                //* 슬레이어 초기 위치 
                var spawnPoint = TaggerSystem.FindGameObjectWithTag("PlayerSpawnPoint").transform;
                RefreshPlayerCharacter(spawnPoint);

                InitCamera();
                // Tutorial1
                InitLoadingPageCtrler("Tutorial-2", () => { });
            };
        }

        // 로보 솔저
        void InitTurorialRoom_3()
        {
            GameContext.Instance.canvasManager.FadeInImmediately(Color.black);

            __currSceneName = "Tutorial-ShortCorridor";
            _curScene = TutorialScene.Tutorial_3;

            //* 로딩 시작
            __loadingPageCtrler = new LoadingPageController(new string[] { "Pawn/Npc/Rapex", "Pawn/Npc/RoboSoldier-Tutorial" }, 
                new string[] { __currSceneName });
            __loadingPageCtrler.Load().Show(GameContext.Instance.canvasManager.dimmed.transform as RectTransform);
            __loadingPageCtrler.onLoadingCompleted += () =>
            {
                //* 슬레이어 초기 위치 
                var spawnPoint = TaggerSystem.FindGameObjectWithTag("PlayerSpawnPoint").transform;
                RefreshPlayerCharacter(spawnPoint);

                InitCamera();
                // Tutorial1
                InitLoadingPageCtrler("Tutorial-3", () => { });
            };
        }

        // 원거리 적
        void InitTurorialRoom_4()
        {
            GameContext.Instance.canvasManager.FadeInImmediately(Color.black);

            __currSceneName = "Tutorial-ShortCorridor";
            _curScene = TutorialScene.Tutorial_4;

            //* 로딩 시작
            __loadingPageCtrler = new LoadingPageController(new string[] { "Pawn/Npc/RoboCannon" }, 
                new string[] { __currSceneName });
            __loadingPageCtrler.Load().Show(GameContext.Instance.canvasManager.dimmed.transform as RectTransform);
            __loadingPageCtrler.onLoadingCompleted += () =>
            {
                //* 슬레이어 초기 위치 
                var spawnPoint = TaggerSystem.FindGameObjectWithTag("PlayerSpawnPoint").transform;
                RefreshPlayerCharacter(spawnPoint);

                InitCamera();
                // Tutorial1
                InitLoadingPageCtrler("Tutorial-4", () => { });
            };
        }

        // M82
        void InitTurorialRoom_5()
        {
            GameContext.Instance.canvasManager.FadeInImmediately(Color.black);

            __currSceneName = "Tutorial-BattleRoom";
            _curScene = TutorialScene.Tutorial_5;

            //* 로딩 시작
            __loadingPageCtrler = new LoadingPageController(new string[] { "Pawn/Npc/Etasphera42" }, 
                new string[] { __currSceneName });
            __loadingPageCtrler.Load().Show(GameContext.Instance.canvasManager.dimmed.transform as RectTransform);
            __loadingPageCtrler.onLoadingCompleted += () =>
            {
                //* 슬레이어 초기 위치 
                var spawnPoint = TaggerSystem.FindGameObjectWithTag("PlayerSpawnPoint").transform;
                RefreshPlayerCharacter(spawnPoint);

                InitCamera();
                // Tutorial1
                InitLoadingPageCtrler("Tutorial-5", () => { });
            };
        }

        public void DoEventCollider(int mode) 
        {
            switch (mode) 
            {
                case 0:
                    InitBubbleDialogue("Tutorial2_step2_2");
                    break;
            }
        }

        public IEnumerator ChangeRoom_Coroutine()
        {
            __dialogueDispatcher.StopDialogue();

            //* 현재 맵이 Unload되면 에러가 발생해서 강제 비활성화
            GameContext.Instance.playerCtrler.possessedBrain.AnimCtrler.legAnimator.enabled = false;
            GameContext.Instance.canvasManager.FadeIn(Color.black, 1f);

            yield return new WaitForSeconds(1f);

            //* 현재 Scene 제거
            yield return SceneManager.UnloadSceneAsync(__currSceneName).AsObservable().ToYieldInstruction();

            foreach (var pawn in _pawnList)
            {
                if (pawn != null && pawn.gameObject != null)
                    Destroy(pawn.gameObject);
            }
            _pawnList.Clear();


            // Next Room
            switch (_curScene) 
            {
                case TutorialScene.Tutorial_1: InitTurorialRoom_2(); break;
                case TutorialScene.Tutorial_2: InitTurorialRoom_3(); break;
                case TutorialScene.Tutorial_3: InitTurorialRoom_4(); break;
                case TutorialScene.Tutorial_4: InitTurorialRoom_5(); break;
            }
            yield return new WaitUntil(() => __loadingPageCtrler == null);

            GameContext.Instance.canvasManager.FadeOut(1f);
            yield return new WaitForSeconds(1f);
        }

        void InitSlayerBrainHandler() 
        {
            var slayerBrain = GameContext.Instance.playerCtrler.possessedBrain;
            slayerBrain.PawnHP.onDamaged += ((damageContext) =>
            {
                CheckSlayerDamage(damageContext);
            });

            // 회피 처리
            slayerBrain.PawnHP.onAvoided += ((_, reason) =>
            {
                if (reason == "Dodge")
                {
                    EvadeDamage();
                }
            });
            // 물약 처리
            slayerBrain.PawnStatusCtrler.onStatusActive += ((status) =>
            {
                if (status == PawnStatus.HPRegen && _tutorialMode == TutorialMode.Heal)
                {
                    _attackCount = 0;
                    __dialogueDispatcher._isWaitCheck = true;
                    _tutorialMode = TutorialMode.None;
                }
            });
        }

        public void PawnSpawned(GameObject obj) 
        {
            var pawn = obj.GetComponent<PawnBrainController>();
            _pawnList.Add(pawn);

            switch (pawn.PawnBB.common.pawnId) 
            {
                case PawnId.RoboSoldier:
                    {
                        _roboBrain = (TutorialRoboSoldierBrain)pawn;
                        pawn.PawnHP.onDamaged += ((damageContext) =>
                        {
                            CheckRoboSoldierDamage(damageContext);
                        });
                        pawn.PawnHP.onDead += ((damageContext) =>
                        {
                            if (_tutorialMode == TutorialMode.Room3_Step3)
                            {
                                EndMode();
                            }
                        });
                    }
                    break;
                case PawnId.Rapax:
                    pawn.PawnHP.onDead += ((damageContext) =>
                    {
                        switch (_tutorialMode) 
                        {
                            case TutorialMode.Room2_Step1:
                                {
                                    _deadCount++;
                                    if (_deadCount >= 2)
                                    {
                                        EndMode();
                                        InitBubbleDialogue("Tutorial2_step2_1");
                                    }
                                    else
                                    {
                                        __dialogueDispatcher.ShowMessagePopup("가드", "적의 공격은 자동으로 가드할 수 있습니다.", 4);
                                    }
                                }
                                break;
                            // Room3 : RoboSoldier
                            case TutorialMode.Room3_Step1:
                                {
                                    _deadCount++;
                                    if (_deadCount >= 3)
                                    {
                                        EndMode();
                                        InitBubbleDialogue("Tutorial3_step2");
                                    }
                                }
                                break;
                        }
                    });
                    break;
                case PawnId.RoboCannon:
                    {
                        pawn.PawnHP.onDead += ((damageContext) => {
                            switch (_tutorialMode)
                            {
                                case TutorialMode.Room4_Step1:
                                    {
                                        _deadCount++;
                                        if (_deadCount >= 2)
                                        {
                                            EndMode();
                                        }
                                    }
                                    break;
                            }
                        });
                    }
                    break;
                case PawnId.Etasphera42:
                    {
                        pawn.PawnHP.onDead += ((damageContext) => {
                            switch (_tutorialMode)
                            {
                                case TutorialMode.Room5_Step1:
                                    {
                                        _deadCount++;
                                        if (_deadCount >= 1)
                                        {
                                            EndMode();
                                        }
                                    }
                                    break;
                            }
                        });
                    }
                    break;
            }
        }

        void StartBattleRoboSoldier(string command)
        {
            if(command == "true")
                EnableRoboSoldier(true);
        }
        void EnableRoboSoldier(bool isEnable) 
        { 
            if (_roboBrain != null) 
            {
                _roboBrain.tutorialMoveEnabled.Value = isEnable;
                _roboBrain.tutorialGuardEnabled.Value = isEnable;
                _roboBrain.tutorialActionEnabled.Value = isEnable;
                _roboBrain.tutorialComboAttackEnbaled.Value = isEnable;
                _roboBrain.tutorialCounterAttackEnbaled.Value = isEnable;
                //_roboBrain.tutorialShieldAttackEnbaled.Value = isEnable;
                //_roboBrain.tutorialJumpActtackEnbaled.Value = isEnable;
            }
        }

        void Update()
        {
            //* 다이얼로그 진행 중에 AnyKeyDown 처리
            if (__dialogueDispatcher.IsDialogueRunning() && Input.anyKeyDown && !CanPlayerConsumeInput())
            {
                foreach (var v in GameContext.Instance.dialogueRunner.dialogueViews)
                {
                    if (v is LineView lineView)
                        lineView.UserRequestedViewAdvancement();
                }
            }
            if (Input.GetKeyDown(KeyCode.F11) == true) 
            {
                if (_roboBrain != null) {
                    var heart = _roboBrain.GetComponent<PawnHeartPointDispatcher>();
                    if (heart != null)
                        heart.Die("cheat");
                }
            }
        }

        #region TUTORIAL

        void EndMode() 
        {
            _deadCount = 0;
            _attackCount = 0;
            __dialogueDispatcher._isWaitCheck = true;
            _tutorialMode = TutorialMode.None;
        }

        public void StopAllRapax() 
        {
            /*
            foreach (var pawn in _pawnList)
            {
                if (pawn != null && pawn.PawnBB.IsDead == false) 
                {
                    var selector = pawn.GetComponent<PawnActionDataSelector>();
                    if (selector != null) {
                        selector.debugActionSelectDisabled = true;
                    }
                }
            }
            */
        }

        void CheckRoboSoldierDamage(PawnHeartPointDispatcher.DamageContext damageContext) 
        {
            var slayerBrain = GameContext.Instance.playerCtrler.possessedBrain;
            switch (_tutorialMode)
            {
                case TutorialMode.NormalAttack:
                    if (damageContext.actionResult == ActionResults.Damaged && damageContext.senderBrain == slayerBrain)
                    {
                        _attackCount++;
                        if (_attackCount >= 3)
                        {
                            EndMode();
                        }
                    }
                    break;                    
                case TutorialMode.PowerAttack:
                    //if (damageContext.actionResult == ActionResults.GuardBreak)
                    {
                        _attackCount++;
                        if (_attackCount >= 3)
                        {
                            EndMode();
                        }
                    }
                    break;         
            }
        }


        void CheckSlayerDamage(PawnHeartPointDispatcher.DamageContext damageContext)
        {
            var slayerBrain = GameContext.Instance.playerCtrler.possessedBrain;
            switch (_tutorialMode)
            {
                case TutorialMode.Guard:
                    if (damageContext.actionResult == ActionResults.Blocked && damageContext.receiverBrain == slayerBrain)
                    {
                        _attackCount++;
                        if (_attackCount >= 3)
                        {
                            EndMode();
                        }
                    }
                    break;
                case TutorialMode.Parry:
                    if (damageContext.actionResult == ActionResults.GuardParrying && damageContext.receiverBrain == slayerBrain)
                    {
                        _attackCount++;
                        if (_attackCount >= 3)
                        {
                            EndMode();
                        }
                    }
                    break;
            }
        }
        void EvadeDamage()
        {
            if (_tutorialMode != TutorialMode.Evade)
                return;

            _attackCount++;
            if (_attackCount >= 3)
            {
                EndMode();
            }
        }

        public void SetCombat()
        {
            _isInCombat = true;
        }
        
        public void ResetCombat()
        {
            _isInCombat = false;

            var slayerBrain = GameContext.Instance.playerCtrler.possessedBrain;
            if (slayerBrain != null) 
            {
                slayerBrain.Movement.Stop();
            }
        }

        void SetMode(TutorialMode mode) 
        {
            _attackCount = 0;
            _tutorialMode = mode;
        }

        void SetTutorialMode(string mode)
        {
            if (Enum.TryParse<TutorialMode>(mode, out TutorialMode result))
            {
                SetMode(result);
            }
        }

        public void SetPawnToGroggy() 
        { 
            // 강제 그로기 셋팅
            _roboBrain.StatusCtrler.AddStatus(PawnStatus.Groggy, 1, 10000);
            _roboBrain.AnimCtrler.mainAnimator.SetBool("IsGroggy", true);
            _roboBrain.AnimCtrler.mainAnimator.SetTrigger("OnGroggy");
            _roboBrain.BB.common.isInvincible.Value = false;
        }

        public void RoboSoldierStartGuard() 
        {
            if (_roboBrain == null)
                return;

            _roboBrain.tutorialActionEnabled.Value = true;
            _roboBrain.tutorialGuardEnabled.Value = false;
        }

        public void RoboSoldierStartAttack() 
        {
            if (_roboBrain == null)
                return;

            _roboBrain.tutorialActionEnabled.Value = true;
            _roboBrain.tutorialComboAttackEnbaled.Value = true;
        }
        public void RoboSoldierEndAttack()
        {
            if (_roboBrain == null)
                return;

            _roboBrain.tutorialActionEnabled.Value = false;
            _roboBrain.tutorialComboAttackEnbaled.Value = false;
        }

        public void OnReceivePawnActionStart(PawnBrainController sender, string actionName)
        {
        }

        public void OnReceivePawnStatusChanged(PawnBrainController sender, PawnStatus status, float strength, float duration)
        {
        }

        public void OnReceivePawnDamageContext(PawnBrainController sender, PawnHeartPointDispatcher.DamageContext damageContext)
        {
            if (damageContext.senderBrain == GameContext.Instance.playerCtrler.possessedBrain) 
            {
            }
        }

        public void OnReceivePawnSpawningStateChanged(PawnBrainController sender, PawnSpawnStates state)
        {
            return;
            //throw new NotImplementedException();
        }
        #endregion
    }
}