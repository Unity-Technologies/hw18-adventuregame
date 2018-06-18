using System;
using Unity.Collections;
using UnityEngine.Experimental.Input.LowLevel;
using UnityEngine.Experimental.Input.Utilities;

namespace UnityEngine.Experimental.Input.Plugins.OnScreen
{
    public class OnScreenDeviceManager : IDisposable
    {
        private struct OnScreenDeviceEventData
        {
            public InputEventPtr eventPtr;
            public NativeArray<byte> buffer;
        }

        static OnScreenDeviceManager s_Instance;
        // static InlinedArray<>

        public static OnScreenDeviceManager GetOnScreenDeviceManager()
        {
            if (s_Instance == null)
                s_Instance = new OnScreenDeviceManager();

            return s_Instance;
        }

        private InputEventPtr GetInputEventPtrForDevice(InputDevice device)
        {
            InputEventPtr eventPtr;
            var buffer = StateEvent.From(device, out eventPtr, Allocator.Persistent);
            return eventPtr;
        }

        public InputControl SetupInputControl(string controlPath)
        {
            var layout = InputControlPath.TryGetDeviceLayout(controlPath);
            var device = InputSystem.TryGetDevice(layout);
            if (device == null)
                device = InputSystem.AddDevice(layout);

            return InputControlPath.TryFindControl(device, controlPath);
        }

        public void ProcessDeviceStateEventForValue<TValue>(InputDevice device, InputControl<TValue> control,
            TValue value)
        {
            var eventPtr = GetInputEventPtrForDevice(device);
            eventPtr.time = InputRuntime.s_Instance.currentTime;
            control.WriteValueInto(eventPtr, value);
            InputSystem.QueueEvent(eventPtr);
            InputSystem.Update();
        }

        public void Dispose()
        {
        }
    }
}
