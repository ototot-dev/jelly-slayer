using System;
using System.Collections.Generic;
using System.Linq;
using FinalFactory.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace FinalFactory.Editor.UIElements
{
    public class EditorZoomManipulator : PointerManipulator
    {
        private readonly IEditorZoomPanTarget _editor;
        private Vector2 _lastPressPosition;
        private Vector2 _lastDownPosition;
        private bool _isDown;

        public EditorZoomManipulator(IEditorZoomPanTarget editor, VisualElement target, IEnumerable<float> zoomSteps)
        {
            ZoomSteps = zoomSteps.ToList();
            _editor = editor;
            target.AddManipulator(this);
            activators.Add(new ManipulatorActivationFilter()
            {
                button = MouseButton.RightMouse,
                modifiers = EventModifiers.Alt
            });
        }
        
        public EditorZoomManipulator(IEditorZoomPanTarget editor, VisualElement target) : this(editor , target, new List<float>())
        {
            for (var index = 25; index < 150; index += 5)
                ZoomSteps.Add((float)Math.Round(index / 100.0, 2));
            for (var index = 150; index <= 500; index += 25)
                ZoomSteps.Add((float)Math.Round(index / 100.0, 2));
        }

        public List<float> ZoomSteps { get; }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
            target.RegisterCallback<WheelEvent>(OnWheel);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            target.UnregisterCallback<WheelEvent>(OnWheel);
        }

        private void OnWheel(WheelEvent evt)
        {
            if (MouseCaptureController.IsMouseCaptured())
                return;
            Zoom(-evt.delta.y);
            evt.StopPropagation();
        }
        
        private void OnPointerDown(PointerDownEvent evt)
        {
            if (!CanStartManipulation(evt))
                    return;
            _isDown = true;
            _lastPressPosition = evt.localPosition;
            _lastDownPosition = _lastPressPosition;
            target.CaptureMouse();
            evt.StopImmediatePropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_isDown || (evt.localPosition.y - _lastDownPosition.y).Abs() < 10.0)
                return;
            Zoom(evt.deltaPosition.y);
            _lastDownPosition = evt.localPosition;
            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!_isDown || !CanStopManipulation(evt))
                return;
            _isDown = false;
            target.ReleaseMouse();
            evt.StopPropagation();
        }

        private void Zoom(float delta)
        {
            var currentZoom = _editor.ZoomScale;
            var newtZoom = currentZoom.Clamp(ZoomSteps[0], ZoomSteps[^1]);
            if (delta.NearEqual(0f))
                _editor.ZoomScale = newtZoom;
            else
            {
                var currentIndex = ZoomSteps.IndexOf(newtZoom);
                if (currentIndex == -1)
                    _editor.ZoomScale = 1f;
                else
                {
                    var index = (currentIndex + (delta > 0.0 ? 1 : -1)).Clamp(0, ZoomSteps.Count - 1);
                    _editor.ZoomScale = ZoomSteps[index];
                }
            }
        }
    }
}
