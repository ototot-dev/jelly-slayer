#if UNITY_INPUTSYSTEM
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UIElements;

namespace FinalFactory.UIElements.Input
{
    public abstract class OnScreenControlElement : AdvancedVisualElement
    {
        private VirtualDeviceManager.VirtualDeviceInfo _info;
        private bool _propagateToInputSystem = true;

        protected VirtualDeviceManager.VirtualDeviceInfo Info => _info;

        /// <summary>
        /// The control path (see <see cref="InputControlPath"/>) for the control that the on-screen
        /// control will feed input into.
        /// </summary>
        /// <remarks>
        /// A device will be created from the device layout referenced by the control path (see
        /// <see cref="InputControlPath.TryGetDeviceLayout"/>). The path is then used to look up
        /// <see cref="Control"/> on the device. The resulting control will be fed values from
        /// the on-screen control.
        ///
        /// Multiple on-screen controls sharing the same device layout will together create a single
        /// virtual device. If, for example, one component uses <c>"&lt;Gamepad&gt;/buttonSouth"</c>
        /// and another uses <c>"&lt;Gamepad&gt;/leftStick"</c> as the control path, a single
        /// <see cref="Gamepad"/> will be created and the first component will feed data to
        /// <see cref="Gamepad.buttonSouth"/> and the second component will feed data to
        /// <see cref="Gamepad.leftStick"/>.
        /// </remarks>
        /// <seealso cref="InputControlPath"/>
        public string ControlPath
        {
            get => ControlPathInternal;
            set
            {
                ControlPathInternal = value;
                if (PropagateToInputSystem && IsActiveAndEnabled)
                {
                    DestroyInputControl();
                    SetupInputControl();
                }
            }
        }


        /// <summary>
        /// If true, the on-screen control will feed input into the control referenced by <see cref="ControlPath"/>.
        /// </summary>
        public bool PropagateToInputSystem
        {
            get => _propagateToInputSystem;
            set
            {
                if (_propagateToInputSystem && IsActiveAndEnabled && !value)
                {
                    DestroyInputControl();
                }
                _propagateToInputSystem = value;
                if (value && IsActiveAndEnabled)
                {
                    SetupInputControl();
                }
            }
        }

        /// <summary>
        /// The actual control that is fed input from the on-screen control.
        /// </summary>
        /// <remarks>
        /// This is only valid while the on-screen control is enabled. Otherwise, it is <c>null</c>. Also,
        /// if no <see cref="ControlPath"/> has been set, this will remain <c>null</c> even if the component is enabled.
        /// </remarks>
        public InputControl Control { get; private set; }

        /// <summary>
        /// Accessor for the <see cref="ControlPath"/> of the component. Must be implemented by subclasses.
        /// </summary>
        /// <remarks>
        /// Moving the definition of how the control path is stored into subclasses allows them to
        /// apply their own <see cref="InputControlAttribute"/> attributes to them and thus set their
        /// own layout filters.
        /// </remarks>
        protected abstract string ControlPathInternal { get; set; }

        private void SetupInputControl()
        {
            if (!IsRuntime)
                return;
            
            Debug.Assert(Control == null, "InputControl already initialized");
            Debug.Assert(Info == null, "Previous InputControl has not been properly uninitialized (VirtualDeviceInfo still set)");

            // Nothing to do if we don't have a control path.
            var path = ControlPathInternal;
            if (string.IsNullOrEmpty(path))
                return;

            // Determine what type of device to work with.
            var layoutName = InputControlPath.TryGetDeviceLayout(path);
            if (layoutName == null)
            {
                Debug.LogError($"Cannot determine device layout to use based on control path '{path}' used in {GetType().Name} component");
                return;
            }

            // Try to find existing on-screen device that matches.
            if (!VirtualDeviceManager.TryGetDeviceByLayout(new InternedString(layoutName), default, out _info))
            {
                _info = VirtualDeviceManager.CreateVirtualDevice(layoutName, "OnScreen", this);
            }
            else
            {
                VirtualDeviceManager.RegisterControl(Info, this);
            }

            // Try to find control on device.
            Control = InputControlPath.TryFindControl(Info.Device, path);
            if (Control == null)
            {
                Debug.LogError($"Cannot find control with path '{path}' on device of type '{layoutName}' referenced by component '{GetType().Name}'");

                VirtualDeviceManager.UnregisterControl(Info, this);
                return;
            }

#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        }

        protected void SendValueToControl<TValue>(TValue value) where TValue : struct
        {
            if (Control == null)
                return;

            if (Control is not InputControl<TValue> control)
                throw new ArgumentException(
                    $"The control path {ControlPath} yields a control of type {Control.GetType().Name} which is not an InputControl with value type {typeof(TValue).Name}", nameof(value));

            Info.EventPtr.time = Time.realtimeSinceStartupAsDouble;
            control.WriteValueIntoEvent(value, Info.EventPtr);
            InputSystem.QueueEvent(Info.EventPtr);
        }

        protected void SentDefaultValueToControl()
        {
            if (Control == null)
                return;

            Info.EventPtr.time = Time.realtimeSinceStartupAsDouble;
            Control.ResetToDefaultStateInEvent(Info.EventPtr);
            InputSystem.QueueEvent(Info.EventPtr);
        }

        protected override void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            base.OnAttachedToPanel(evt);
            if (PropagateToInputSystem)
            {
                SetupInputControl();
            }
        }

        protected override void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            base.OnDetachedFromPanel(evt);
            DestroyInputControl();
        }

        private void DestroyInputControl()
        {
            if (Control == null)
                return;
#if UNITY_EDITOR
            // Optionally, unregister from the play mode state changes
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
            VirtualDeviceManager.UnregisterControl(Info, this);
            try
            {
                if (!Control.CheckStateIsAtDefault())
                    SentDefaultValueToControl();
            }
            catch (InvalidOperationException)
            {
                // Ignore.
            }
            Control = null;
            _info = null;
        }

#if UNITY_EDITOR
        private void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange stateChange)
        {
            if (stateChange == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                DestroyInputControl();
            }
        }
#endif
      
        /// <summary>
        ///   <para>Defines UxmlTraits for the Button.</para>
        /// </summary>
        public new class UxmlTraits : AdvancedVisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _controlPath = new() { name = "control-path", defaultValue = "" };
            private readonly UxmlBoolAttributeDescription _propagateToInputSystem = new() { name = "propagate-to-input-system", defaultValue = true };

            /// <summary>
            ///   <para>Initializer for the UxmlTraits for the TextElement.</para>
            /// </summary>
            /// <param name="ve">VisualElement to initialize.</param>
            /// <param name="bag">Bag of attributes where to get the value from.</param>
            /// <param name="cc">Creation Context, not used.</param>
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var joystickElement = (OnScreenControlElement)ve;

                joystickElement.PropagateToInputSystem = _propagateToInputSystem.GetValueFromBag(bag, cc);
                joystickElement.ControlPath = _controlPath.GetValueFromBag(bag, cc);
            }
        }
    }
}
#endif