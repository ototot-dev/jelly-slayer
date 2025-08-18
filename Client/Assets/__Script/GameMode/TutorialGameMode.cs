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
using MissionTable;
using MainTable;
using System.Linq;

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

    public class TutorialGameMode : BaseGameMode, IPawnEventListener
    {
        public override GameModeTypes GetGameModeType() => GameModeTypes.Tutorial;
        public override DialogueDispatcher GetDialogueDispatcher() => __dialogueDispatcher;

        [Header("Tutorial")]
        public TutorialMode _tutorialMode = TutorialMode.None;
        [SerializeField] private bool _isInCombat = false;
        [SerializeField] private int _attackCount = 0;
        [SerializeField] private int _killCount = 0;
        public int KillCount { 
            get { return _killCount; }
            set {
                _killCount = value;

                if (_curMissiondata != null && _curMissiondata.clearCondition == MissionClearCondition.KILLALL)
                {
                    if (_killCount >= _pawnList.Count)
                    {
                        EndMode();
                    }
                }
            } 
        }

        private TutorialRoboSoldierBrain _roboBrain;

        private List<PawnBrainController> _pawnList = new ();

        public override bool IsInCombat() => _isInCombat;

        public override bool CanPlayerConsumeInput()
        {
            if (__loadingPageCtrler != null) return false;
            
            return !__dialogueDispatcher.IsDialogueRunning() ||
                GameContext.Instance.interactionKeyCtrlers.AsValueEnumerable().Any(i => i.PreprocessKeyDown()) ||
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
            InitRoom(GameContext.Instance.launcher._tutorialStartMission);

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

        protected override void InitPlayerCharacter(Transform spawnPoint)
        {
            base.InitPlayerCharacter(spawnPoint);

            GameContext.Instance.playerCtrler.possessedBrain.PawnHP.onDamaged += ((damageContex) => {
                int kk = 0;
            });
            GameContext.Instance.playerCtrler.possessedBrain.StatusCtrler.onStatusActive += ((pawnStatus) => {

                if (pawnStatus == PawnStatus.RegenHeartPoint && _tutorialMode == TutorialMode.Room3_Step2) {

                    EndMode();
                    InitBubbleDialogue("Tutorial3_step3");
                }
            });
            InitSlayerBrainHandler();

            new PlayerStatusBarController().Load(GameObject.FindFirstObjectByType<PlayerStatusBarTemplate>()).Show(GameContext.Instance.canvasManager.body.transform as RectTransform);
        }

        protected override bool InitRoom(int mission_id)
        {
            if (base.InitRoom(mission_id) == false)
                return false;

            _killCount = 0;

            return true;
        }

        public void DoEventCollider(int mode) 
        {
            //switch (mode) { case 0: InitBubbleDialogue("Tutorial2_step2_2"); break; }
        }

        public override IEnumerator ChangeRoom_Coroutine()
        {
            yield return base.ChangeRoom_Coroutine();

            foreach (var pawn in _pawnList)
            {
                if (pawn != null && pawn.gameObject != null)
                    Destroy(pawn.gameObject);
            }
            _pawnList.Clear();
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
                if (status == PawnStatus.RegenHeartPoint && _tutorialMode == TutorialMode.Heal)
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

            pawn.PawnHP.onDead += ((damageContext) =>
            {
                KillCount++;
            });
            switch (pawn.PawnBB.common.pawnId) 
            {
                case PawnId.RoboSoldier:
                    {
                        _roboBrain = (TutorialRoboSoldierBrain)pawn;
                        pawn.PawnHP.onDamaged += ((damageContext) =>
                        {
                            CheckRoboSoldierDamage(damageContext);
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
                                    if (_killCount >= 2)
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
                                    if (_killCount >= 3)
                                    {
                                        EndMode();
                                        InitBubbleDialogue("Tutorial3_step2");
                                    }
                                }
                                break;
                        }
                    });
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
            /*
            if (Input.GetKeyDown(KeyCode.F11) == true) 
            {
                if (_roboBrain != null) {
                    var heart = _roboBrain.GetComponent<PawnHeartPointDispatcher>();
                    if (heart != null)
                        heart.Die("cheat");
                }
            }
            //*/
        }

        #region TUTORIAL

        void EndMode() 
        {
            _attackCount = 0;
            __dialogueDispatcher._isWaitCheck = true;
            _tutorialMode = TutorialMode.None;

            if (_curMissiondata != null && _curMissiondata.endNode.Length > 0) 
            {
                InitBubbleDialogue(_curMissiondata.endNode);
            }
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