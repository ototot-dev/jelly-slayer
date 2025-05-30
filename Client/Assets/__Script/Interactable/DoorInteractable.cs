using UnityEngine;

namespace Game
{
    public class DoorInteractable : InteractableHandler
    {
        public override Vector3 GetInteractionKeyAttachPoint() => interactableKeyAttachPoint.position;
        public override float GetVisibleRadius() => interactableRadius;
        public override string GetCommand() => command;
        public Transform interactableKeyAttachPoint;
        public float interactableRadius = -1f;
        public string command;
    }
}