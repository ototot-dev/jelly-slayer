using System.Collections.Generic;
using UnityEngine;

namespace Game
{   
    public class PawnEventManager : MonoSingleton<PawnEventManager>
    {
        HashSet<IPawnEventListener> __eventListeners = new();
        public void RegisterEventListener(IPawnEventListener listener) { __eventListeners.Add(listener); }
        public void UnregisterEventListener(IPawnEventListener listener) { __eventListeners.Remove(listener); }

        public void SendPawnActionEvent(PawnBrainController sender, string eventName)
        {
            foreach (var l in __eventListeners)
                l.OnReceivePawnActionStart(sender, eventName);
        }
        public void SendPawnStatusEvent(PawnBrainController sender, PawnStatus status, float strength, float duration)
        {
            foreach (var l in __eventListeners)
                l.OnReceivePawnStatusChanged(sender, status, strength, duration);
        }
        public void SendPawnDamageEvent(PawnBrainController sender, PawnHeartPointDispatcher.DamageContext damageContext)
        {
            //* DamageContext는 senderBrain이 보내는 것으로 강제함
            Debug.Assert(damageContext.senderBrain == sender);

            foreach (var l in __eventListeners)
                l.OnReceivePawnDamageContext(sender, damageContext);
        }

        public void SendPawnSpawningEvent(PawnBrainController sender, PawnSpawnStates state)
        {
            foreach (var l in __eventListeners)
                l.OnReceivePawnSpawningStateChanged(sender, state);
        }
    }
}