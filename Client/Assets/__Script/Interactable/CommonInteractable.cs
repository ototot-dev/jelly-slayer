using UnityEngine;

namespace Game
{
    public class CommonInteractable : InteractableHandler
    {
        public override Vector3 GetInteractionKeyAttachPoint() => interactableKeyAttachPoint.position;
        public override Vector3 GetBubbleDialogueAttachPoint() => bubbleDialogueAttachPoint.position;
        public override float GetVisibleRadius() => interactableRadius;
        public override string GetCommand() => command;
        public Transform interactableKeyAttachPoint;
        public Transform bubbleDialogueAttachPoint;
        public float interactableRadius = -1f;
        public string command;
    }
}