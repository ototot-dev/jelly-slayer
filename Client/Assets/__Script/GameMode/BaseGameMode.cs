using System;
using UGUI.Rx;
using UniRx;
using UnityEngine;

namespace Game
{
    public abstract class BaseGameMode : MonoBehaviour
    {
        public virtual GameModeTypes GetGameModeType() => GameModeTypes.None;
        public virtual DialogueRunnerDispatcher GetDialogueRunnerDispatcher() => null;
        public virtual bool IsInCombat() => false;
        public virtual void Enter() { EnterAsObservable()?.Subscribe(); }
        public virtual void Exit() { ExitAsObservable()?.Subscribe(); }
        public virtual IObservable<Unit> EnterAsObservable() { return null; }
        public virtual IObservable<Unit> ExitAsObservable() { return null; }
    }
}
