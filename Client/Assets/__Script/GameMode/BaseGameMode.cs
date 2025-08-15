using System;
using System.Collections;
using UGUI.Rx;
using UniRx;
using UnityEngine;
using MissionTable;
using System.Linq;
using UnityEngine.SceneManagement;

namespace Game
{
    public abstract class BaseGameMode : MonoBehaviour
    {
        public int _curMissionID = 0;
        protected MissionData _curMissiondata;

        protected string __currSceneName;

        protected LoadingPageController __loadingPageCtrler;

        protected DialogueDispatcher __dialogueDispatcher;

        public virtual GameModeTypes GetGameModeType() => GameModeTypes.None;
        public virtual DialogueDispatcher GetDialogueDispatcher() => __dialogueDispatcher;
        public virtual string GetCurrentSceneName() => string.Empty;
        public virtual bool CanPlayerConsumeInput() => true;
        public virtual bool IsInCombat() => false;
        public virtual void Enter() { EnterAsObservable()?.Subscribe(); }
        public virtual void Exit() { ExitAsObservable()?.Subscribe(); }
        public virtual void ChangeScene(string sceneName) { ChangeSceneAsObservable(sceneName)?.Subscribe(); }
        public virtual IObservable<Unit> EnterAsObservable() { return null; }
        public virtual IObservable<Unit> ExitAsObservable() { return null; }
        public virtual IObservable<Unit> ChangeSceneAsObservable(string sceneName) { return null; }

        protected virtual void InitPlayerCharacter(Transform spawnPoint)
        {
            if (GameContext.Instance.playerCtrler.possessedBrain == null)
            {
                //* 슬레이어 스폰
                GameContext.Instance.playerCtrler.SpawnSlayerPawn(true);
                GameContext.Instance.playerCtrler.possessedBrain.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
                GameContext.Instance.playerCtrler.possessedBrain.TryGetComponent<SlayerAnimController>(out var animCtrler);

                //InitSlayerBrainHandler();
                //new PlayerStatusBarController().Load(GameObject.FindFirstObjectByType<PlayerStatusBarTemplate>()).Show(GameContext.Instance.canvasManager.body.transform as RectTransform);
            }
        }

        protected virtual bool InitRoom(int mission_id)
        {
            if (mission_id <= 0)
            {
                Debug.Log("Invalid Mission, " + mission_id);
                return false;
            }
            _curMissionID = mission_id;

            GameContext.Instance.canvasManager.FadeInImmediately(Color.black);

            MissionData data = MissionTable.MissionData.MissionDataList.First(d => d.id == mission_id);
            if (data == null)
            {
                Debug.Log("Mission 로드 Fail, " + mission_id);
                return false;
            }
            _curMissiondata = data;
            __currSceneName = data.sceneName;

            //* 로딩 시작
            __loadingPageCtrler = new LoadingPageController(new string[] { data.resPath1, data.resPath2 },
                new string[] { __currSceneName });
            __loadingPageCtrler.Load().Show(GameContext.Instance.canvasManager.dimmed.transform as RectTransform);
            __loadingPageCtrler.onLoadingCompleted += () =>
            {
                //* 슬레이어 초기 위치
                var spawnPoint = TaggerSystem.FindGameObjectWithTag("PlayerSpawnPoint");
                if (spawnPoint != null)
                {
                    RefreshPlayerCharacter(spawnPoint.transform);
                }
                InitCamera();

                // Yarn Script
                InitLoadingPageCtrler(data.startNode, () => { });
            };
            return true;
        }

        void RefreshPlayerCharacter(Transform spawnPoint)
        {
            (GameContext.Instance.playerCtrler.possessedBrain as IPawnMovable).Teleport(spawnPoint.position, spawnPoint.rotation);

            //* LegAnimator 다시 활성화
            Observable.Timer(TimeSpan.FromSeconds(0.1f)).Subscribe(_ => GameContext.Instance.playerCtrler.possessedBrain.AnimCtrler.legAnimator.enabled = true);
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

        protected void InitBubbleDialogue(string nodeName)
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
        public virtual IEnumerator ChangeRoom_Coroutine()
        {
            __dialogueDispatcher.StopDialogue();

            //* 현재 맵이 Unload되면 에러가 발생해서 강제 비활성화
            GameContext.Instance.playerCtrler.possessedBrain.AnimCtrler.legAnimator.enabled = false;
            GameContext.Instance.canvasManager.FadeIn(Color.black, 1f);

            yield return new WaitForSeconds(1f);

            //* 현재 Scene 제거
            yield return SceneManager.UnloadSceneAsync(__currSceneName).AsObservable().ToYieldInstruction();

            /*
            foreach (var pawn in _pawnList)
            {
                if (pawn != null && pawn.gameObject != null)
                    Destroy(pawn.gameObject);
            }
            _pawnList.Clear();
            */

            // Next Room
            if (_curMissiondata != null)
            {
                InitRoom(_curMissiondata.door1);
            }
            yield return new WaitUntil(() => __loadingPageCtrler == null);

            GameContext.Instance.canvasManager.FadeOut(1f);
            yield return new WaitForSeconds(1f);
        }
    }
}
