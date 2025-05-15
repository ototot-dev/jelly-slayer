using FinalFactory.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace FinalFactory.Editor.UIElements
{
    public class EditorPanManipulator : PointerManipulator
    {
        private readonly IEditorZoomPanTarget _editor;
        private int _lastDownButton;
        private bool _isPanning;

        public EditorPanManipulator(IEditorZoomPanTarget editor, VisualElement target)
        {
            _editor = editor;
            target.AddManipulator(this);
            activators.Add(new ManipulatorActivationFilter
            {
                button = MouseButton.LeftMouse,
                modifiers = EventModifiers.Control | EventModifiers.Alt
            });
            activators.Add(new ManipulatorActivationFilter
            {
                button = MouseButton.MiddleMouse
            });
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (!CanStartManipulation(evt))
                return;
            _isPanning = true;
            _lastDownButton = evt.button;
            target.CaptureMouse();
            evt.StopImmediatePropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (_lastDownButton != evt.button || !CanStopManipulation(evt))
                return;
            _isPanning = false;
            target.ReleaseMouse();
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_isPanning)
                return;
            _editor.ContentOffset += new Vector2(evt.deltaPosition.x, evt.deltaPosition.y);
            evt.StopPropagation();
        }
    }
}