using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UniRx;
using UGUI.Rx;
using Yarn.Unity;
using ZLinq;
using Game.UI;
using System.Collections.Generic;
using MissionTable;
using System.Linq;

namespace Game
{
    public class MissionGameMode : BaseGameMode, IPawnEventListener
    {
        void Awake()
        {
            __dialogueDispatcher = gameObject.AddComponent<TutorialDialogueDispatcher>();
        }
        private void Start()
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

        public void OnReceivePawnActionStart(PawnBrainController sender, string actionName)
        {
        }

        public void OnReceivePawnDamageContext(PawnBrainController sender, PawnHeartPointDispatcher.DamageContext damageContext)
        {
        }

        public void OnReceivePawnSpawningStateChanged(PawnBrainController sender, PawnSpawnStates state)
        {
        }

        public void OnReceivePawnStatusChanged(PawnBrainController sender, PawnStatus status, float strength, float duration)
        {
        }
    }
}