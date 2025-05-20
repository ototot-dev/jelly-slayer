using UnityEngine;

namespace Game
{
    public class PawnInteractionController : MonoBehaviour
    {
        public enum InteractionModes
        {
            None = 0,
            PhoneBooth,
            OpenDoor,
            Max,
        }

        protected InteractionModes __currInteractionMode;
        public InteractionModes CurrInteractionMode => __currInteractionMode;
        public virtual bool IsInteractionRunning() => __currInteractionMode != InteractionModes.None;

        public virtual void StartInteraction(InteractionModes interactionMode)
        {
            __currInteractionMode = interactionMode;
        }

        public virtual void FinishInteraction()
        {
            __currInteractionMode = InteractionModes.None;
        }
    }
}