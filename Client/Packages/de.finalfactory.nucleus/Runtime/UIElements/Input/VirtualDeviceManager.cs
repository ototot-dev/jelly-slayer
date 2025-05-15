#if UNITY_INPUTSYSTEM

using System;
using System.Collections.Generic;
using FinalFactory.Logging;
using Unity.Collections;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace FinalFactory.UIElements.Input
{
    public static class VirtualDeviceManager
    {
        private static readonly Log Log = LogManager.GetLogger(typeof(VirtualDeviceManager));
        private static readonly List<VirtualDeviceInfo> Infos;
        
        static VirtualDeviceManager()
        {
            Infos = new List<VirtualDeviceInfo>();
        }

        public static bool TryGetDeviceByLayout(InternedString layout, InternedString usage, out VirtualDeviceInfo device)
        {
            for (var i = 0; i < Infos.Count; ++i)
            {
                var virtualDeviceInfo = Infos[i];
                var inputDevice = virtualDeviceInfo.Device;
                if (inputDevice.layout == layout && (usage.IsEmpty() || inputDevice.usages.Contains(usage)))
                {
                    device =  virtualDeviceInfo;
                    return true;
                }
            }
            device = null;
            return false;
        }
        
        public static VirtualDeviceInfo CreateVirtualDevice(string layoutName, string usage, object control)
        {
            InputDevice device;
            // Try to create device.
            try
            {
                device = InputSystem.AddDevice(layoutName);
            }
            catch (Exception exception)
            {
                Log.Error(exception, $"Could not create device with layout '{layoutName}' used in '{control.GetType().Name}' component");
                return null;
            }
            InputSystem.AddDeviceUsage(device, usage);

            // Create event buffer.
            var buffer = StateEvent.From(device, out var eventPtr, Allocator.Persistent);
                
            // Add to list.
            var virtualDeviceInfo = new VirtualDeviceInfo
            {
                EventPtr = eventPtr,
                Buffer = buffer,
                Device = device,
            };
            Infos.Add(virtualDeviceInfo);
            RegisterControl(virtualDeviceInfo, control);
            return virtualDeviceInfo;
        }
        
        public static void RegisterControl(VirtualDeviceInfo deviceInfo, object control)
        {
            deviceInfo.AddControl(control);
        }
        
        public static void UnregisterControl(VirtualDeviceInfo deviceInfo, object control)
        {
            deviceInfo.RemoveControl(control);
            if (deviceInfo.ControllerCount == 0)    
            {
                Infos.Remove(deviceInfo);
                deviceInfo.Destroy();
            }
        }
        
        public class VirtualDeviceInfo
        {
            public InputEventPtr EventPtr;
            public NativeArray<byte> Buffer;
            public InputDevice Device;

            private readonly HashSet<object> _controllers = new();

            internal int ControllerCount => _controllers.Count;
            
            internal void AddControl(object control)
            {
                _controllers.Add(control);
            }

            internal void RemoveControl(object control)
            {
                _controllers.Remove(control);
            }

            internal void Destroy()
            {
                if (Buffer.IsCreated)
                    Buffer.Dispose();
                if (Device != null)
                    InputSystem.RemoveDevice(Device);
                Device = null;
                Buffer = new NativeArray<byte>();
            }
        }
    }
}

#endif