using UniRx;
using UnityEngine;

namespace Game
{
    public class InteractableHandler : MonoBehaviour, IBubbleDialogueAttachable, IInteractionKeyAttachable
    {
        public virtual Vector3 GetBubbleDialogueAttachPoint() => Vector3.zero;
        public virtual Vector3 GetInteractionKeyAttachPoint() => Vector3.zero;
        public virtual float GetVisibleRadius() => -1f;
        public virtual bool GetEnanbled() => enabled;
        public virtual string GetCommand() => string.Empty;
    }
}