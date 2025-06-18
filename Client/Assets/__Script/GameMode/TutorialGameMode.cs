using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UniRx;
using UGUI.Rx;
using Yarn.Unity;
using ZLinq;
using Game.UI;


namespace Game
{
    public enum TutorialMode 
    {
        None,
        NormalAttack,
        PowerAttack,

        Guard,
        Evade,
    }
    public enum TutorialScene 
    {
        Tutorial_0,     // 시작 씬, 전화
        Tutorial_1,     // 적 조우
    }

    public class TutorialGameMode : BaseGameMode, IPawnEventListener
    {
        public override GameModeTypes GetGameModeType() => GameModeTypes.Tutorial;
        public override DialogueDispatcher GetDialogueDispatcher() => __dialogueDispatcher;
        TutorialDialogueDispatcher __dialogueDispatcher;
        LoadingPageController __loadingPageCtrler;
        string __currSceneName;

        public TutorialScene _startScene = TutorialScene.Tutorial_0;

        [Header("Tutorial")]
        public TutorialMode _tutorialMode = TutorialMode.None;
        private bool _isInCombat = false;
        private int _attackCount = 0;

        private TutorialRoboSoldierBrain _roboBrain;

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
            //InitStartRoom();
            InitTurorial1Room();

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

                GameContext.Instance.playerCtrler.possessedBrain.PawnHP.onDamaged += ((damageContex) =>
                {
                });
            }
            else
            {
                (GameContext.Instance.playerCtrler.possessedBrain as IPawnMovable).Teleport(spawnPoint.position);

                //* LegAnimator 다시 활성화
                Observable.Timer(TimeSpan.FromSeconds(0.1f)).Subscribe(_ => GameContext.Instance.playerCtrler.possessedBrain.AnimCtrler.legAnimator.enabled = true);
            }
            
            new PlayerStatusBarController().Load(GameObject.FindFirstObjectByType<PlayerStatusBarTemplate>()).Show(GameContext.Instance.canvasManager.body.transform as RectTransform);
            //new FloatingStatusBarController(GameContext.Instance.playerCtrler.possessedBrain).Load(GameObject.FindFirstObjectByType<FloatingStatusBarTemplate>()).Show(GameContext.Instance.canvasManager.body.transform as RectTransform);
        }

        void InitBubbleDialogue(string nodeName) 
        {
            if (__dialogueDispatcher != null)
            {
                __dialogueDispatcher.onRunLine = null;
                __dialogueDispatcher.onDialoqueComplete = null;
            }
            var obj = GameObject.Find("3d-bubble-dialogue");

            BubbleDialoqueController controller;
            if (obj != null)
                controller = new BubbleDialoqueController().Load(obj.GetComponent<Template>());
            else
                controller = new BubbleDialoqueController().Load();

            controller.Show(GameContext.Instance.canvasManager.body.transform as RectTransform);
            __dialogueDispatcher.StartDialogue(nodeName);
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

            __currSceneName = "Tutorial-0";

            //* 로딩 시작
            __loadingPageCtrler = new LoadingPageController(new string[] { "Pawn/Player/Slayer-K", "Pawn/Player/DrontBot" }, new string[] { "Tutorial-0" });
            __loadingPageCtrler.Load().Show(GameContext.Instance.canvasManager.dimmed.transform as RectTransform);
            __loadingPageCtrler.onLoadingCompleted += () =>
            {
                //* 슬레이어 초기 위치 
                var spawnPoint = TaggerSystem.FindGameObjectWithTag("PlayerSpawnPoint").transform;
                InitPlayerCharacter(spawnPoint);

                InitCamera();

                InitLoadingPageCtrler("Tutorial-0", () => { });
            };
        }

        void InitTurorial1Room() 
        {
            GameContext.Instance.canvasManager.FadeInImmediately(Color.black);

            __currSceneName = "Tutorial-1";

            //* 로딩 시작
            __loadingPageCtrler = new LoadingPageController(new string[] { }, new string[] { "Tutorial-1" });
            __loadingPageCtrler.Load().Show(GameContext.Instance.canvasManager.dimmed.transform as RectTransform);
            __loadingPageCtrler.onLoadingCompleted += () =>
            {
                //* 슬레이어 초기 위치 
                var spawnPoint = TaggerSystem.FindGameObjectWithTag("PlayerSpawnPoint").transform;
                InitPlayerCharacter(spawnPoint);

                InitCamera();
                // Tutorial1
                InitLoadingPageCtrler("Tutorial1", () => { });
            };
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

            InitTurorial1Room();

            yield return new WaitUntil(() => __loadingPageCtrler == null);

            GameContext.Instance.canvasManager.FadeOut(1f);
            yield return new WaitForSeconds(1f);
        }

        public void PawnSpawned(GameObject obj) 
        {
            var pawn = obj.GetComponent<PawnBrainController>();
            if (pawn.PawnBB.common.pawnId == PawnId.RoboSoldier)
            {
                _roboBrain = (TutorialRoboSoldierBrain)pawn;

                pawn.PawnHP.onDamaged += ((damageContext) =>
                {
                    CheckDamage(damageContext);
                });
                /*
                pawn.PawnHP.onAvoided += ((_, reason) => 
                {
                    if (reason == "Dodge") 
                    {
                        EvadeDamage();
                    }
                });
                //*/
            }
            var slayerBrain = GameContext.Instance.playerCtrler.possessedBrain;
            slayerBrain.PawnHP.onAvoided += ((_, reason) =>
            {
                if (reason == "Dodge")
                {
                    EvadeDamage();
                }
            });
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
        }

        #region TUTORIAL

        void CheckDamage(PawnHeartPointDispatcher.DamageContext damageContext) 
        {
            var slayerBrain = GameContext.Instance.playerCtrler.possessedBrain;
            switch (_tutorialMode)
            {
                case TutorialMode.NormalAttack:
                    _attackCount++;
                    if (_attackCount >= 3)
                    {
                        _attackCount = 0;
                        __dialogueDispatcher._isWaitCheck = true;
                        _tutorialMode = TutorialMode.None;
                    }
                    break;
                case TutorialMode.PowerAttack:
                    //if (damageContext.actionResult == ActionResults.GuardBreak)
                    {
                        _attackCount++;
                        if (_attackCount >= 3)
                        {
                            _attackCount = 0;
                            __dialogueDispatcher._isWaitCheck = true;
                            _tutorialMode = TutorialMode.None;
                        }
                    }
                    break;
                case TutorialMode.Guard:
                    if (damageContext.actionResult == ActionResults.Blocked && damageContext.receiverBrain == slayerBrain)
                    {
                        _attackCount++;
                        if (_attackCount >= 3)
                        {
                            _attackCount = 0;
                            __dialogueDispatcher._isWaitCheck = true;
                            _tutorialMode = TutorialMode.None;
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
                _attackCount = 0;
                __dialogueDispatcher._isWaitCheck = true;
                _tutorialMode = TutorialMode.None;
            }
        }

        public void SetCombat()
        {
            _isInCombat = true;
        }
        
        public void ResetCombat()
        {
            _isInCombat = false;
        }
        
        public void StartMode_NormalAttack()
        {
            _attackCount = 0;
            _tutorialMode = TutorialMode.NormalAttack;
        }

        public void StartMode_PowerAttack()
        {
            _attackCount = 0;
            _tutorialMode = TutorialMode.PowerAttack;
        }

        public void StartMode_Guard() 
        {
            _attackCount = 0;
            _tutorialMode = TutorialMode.Guard;
        }

        public void StartMode_Evade() 
        {
            _attackCount = 0;
            _tutorialMode = TutorialMode.Evade;
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