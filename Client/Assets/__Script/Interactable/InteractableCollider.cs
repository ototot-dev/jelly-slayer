using UnityEngine;
using UGUI.Rx;
using UniRx;

namespace Game
{
    public class InteractableCollider : MonoBehaviour
    {
        [SerializeField] InteractableHandler _handler;

        [SerializeField] InteractionKeyController _keyController;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (GameContext.Instance.canvasManager == null)
                return;

            var controller = new InteractionKeyController("E", _handler.GetCommand(), -1f, _handler);
            _keyController = controller.Load().Show(GameContext.Instance.canvasManager.body.transform as RectTransform);
        }
    }
}
