using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UniRx;
using UGUI.Rx;
using Yarn.Unity;
using ZLinq;


namespace Game
{
    public enum TutorialMode 
    {
        None,
        NormalAttack,
    }
    public class TutorialGameMode : BaseGameMode
    {
        public override GameModeTypes GetGameModeType() => GameModeTypes.Tutorial;
        public override DialogueDispatcher GetDialogueDispatcher() => __dialogueDispatcher;
        TutorialDialogueDispatcher __dialogueDispatcher;
        LoadingPageController __loadingPageCtrler;
        string __currSceneName;

        private int _attackCount = 0;

        public TutorialMode _mode = TutorialMode.None;

        public override bool CanPlayerConsumeInput()
        {
            if (__loadingPageCtrler != null) return false;
            return !__dialogueDispatcher.IsDialogueRunning() || 
                GameContext.Instance.playerCtrler.interactionKeyCtrlers.AsValueEnumerable().Any(i => i.IsInteractableEnabled) ||
                _mode != TutorialMode.None;
        }

        void Awake()
        {
            __dialogueDispatcher = gameObject.AddComponent<TutorialDialogueDispatcher>();
        }

        public override IObservable<Unit> EnterAsObservable()
        {
            GameContext.Instance.canvasManager.FadeInImmediately(Color.black);

            __loadingPageCtrler = new LoadingPageController(new string[] { "Pawn/Player/Slayer-K", "Pawn/Player/DrontBot" }, new string[] { "Tutorial-0" });
            __currSceneName = "Tutorial-0";

            //* 로딩 시작
            __loadingPageCtrler.Load().Show(GameContext.Instance.canvasManager.dimmed.transform as RectTransform);
            __loadingPageCtrler.onLoadingCompleted += () =>
            {
                var spawnPoint = TaggerSystem.FindGameObjectWithTag("PlayerSpawnPoint").transform;

                //* 슬레이어 스폰
                GameContext.Instance.playerCtrler.SpawnSlayerPawn(true);
                GameContext.Instance.playerCtrler.possessedBrain.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
                GameContext.Instance.playerCtrler.possessedBrain.TryGetComponent<SlayerAnimController>(out var animCtrler);

                //* 카메라 타겟 셋팅
                GameContext.Instance.cameraCtrler.virtualCamera.LookAt = GameContext.Instance.playerCtrler.possessedBrain.coreColliderHelper.transform;
                GameContext.Instance.cameraCtrler.virtualCamera.Follow = GameContext.Instance.playerCtrler.possessedBrain.coreColliderHelper.transform;

                //* 카메라 이동 영역 셋팅
                var confinerBoundingBox = TaggerSystem.FindGameObjectWithTag("ConfinerBoundingBox").GetComponent<BoxCollider>();
                if (confinerBoundingBox != null)
                    GameContext.Instance.cameraCtrler.RefreshConfinerVolume(confinerBoundingBox, Quaternion.Euler(45f, 45f, 0f), 10f);

                //* 로딩 화면 종료
                __loadingPageCtrler.HideAsObservable().Subscribe(__ =>
                {
                    __loadingPageCtrler.Unload();
                    __loadingPageCtrler = null;

                    new BubbleDialoqueController().Load(GameObject.Find("3d-bubble-dialogue").GetComponent<Template>()).Show(GameContext.Instance.canvasManager.body.transform as RectTransform);
                    __dialogueDispatcher.StartDialogue("Tutorial-0");
                });
            };

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

        public IEnumerator ChangeRoom_Coroutine()
        {
            __dialogueDispatcher.StopDialogue();

            //* 현재 맵이 Unload되면 에러가 발생해서 강제 비활성화
            GameContext.Instance.playerCtrler.possessedBrain.AnimCtrler.legAnimator.enabled = false;
            GameContext.Instance.canvasManager.FadeIn(Color.black, 1f);
            yield return new WaitForSeconds(1f);

            //* 현재 Scene 제거
            yield return SceneManager.UnloadSceneAsync(__currSceneName).AsObservable().ToYieldInstruction();

            //* 로딩 시작
            __loadingPageCtrler = new LoadingPageController(new string[] { }, new string[] { "Tutorial-1" });
            __loadingPageCtrler.Load().Show(GameContext.Instance.canvasManager.dimmed.transform as RectTransform);

            __loadingPageCtrler.onLoadingCompleted += () =>
            {
                //* 슬레이어 초기 위치 
                var spawnPoint = TaggerSystem.FindGameObjectWithTag("PlayerSpawnPoint").transform;
                (GameContext.Instance.playerCtrler.possessedBrain as IPawnMovable).Teleport(spawnPoint.position);

                //* LegAnimator 다시 활성화
                Observable.Timer(TimeSpan.FromSeconds(0.1f)).Subscribe(_ => GameContext.Instance.playerCtrler.possessedBrain.AnimCtrler.legAnimator.enabled = true);

                //* 카메라 이동 영역 셋팅
                var confinerBoundingBox = TaggerSystem.FindGameObjectWithTag("ConfinerBoundingBox").GetComponent<BoxCollider>();
                if (confinerBoundingBox != null)
                    GameContext.Instance.cameraCtrler.RefreshConfinerVolume(confinerBoundingBox, Quaternion.Euler(45f, 45f, 0f), 10f);

                //* 로딩 화면 종료
                __loadingPageCtrler.HideAsObservable().Subscribe(__ =>
                {
                    __loadingPageCtrler.Unload();
                    __loadingPageCtrler = null;

                    __dialogueDispatcher.onRunLine = null;
                    __dialogueDispatcher.onDialoqueComplete = null;

                    new BubbleDialoqueController().Load().Show(GameContext.Instance.canvasManager.body.transform as RectTransform);
                    __dialogueDispatcher.StartDialogue("Tutorial-1");
                });
            };

            yield return new WaitUntil(() => __loadingPageCtrler == null);

            GameContext.Instance.canvasManager.FadeOut(1f);
            yield return new WaitForSeconds(1f);
        }

        public void StartTutorialAttack() 
        {
            _mode = TutorialMode.NormalAttack;
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
    }
}