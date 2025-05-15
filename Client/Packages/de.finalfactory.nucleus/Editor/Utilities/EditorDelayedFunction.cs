using System;
using UnityEditor;

namespace FinalFactory.Editor.Utilities
{
    public class EditorDelayedFunction : IDisposable
    {
        private readonly Action _callback;
        private double _executeTime = -1;
        private bool _isSubscribed;
        public EditorDelayedFunction(Action callback, float delay)
        {
            _callback = callback;
            Delay = delay;
        }
        
        public float Delay { get; set; }
        
        public bool IsRunning => _isSubscribed;
        
        public float RemainingTime => (float) (_executeTime - EditorApplication.timeSinceStartup);
        
        public void Trigger()
        {
            _executeTime = EditorApplication.timeSinceStartup + Delay;
            if (!_isSubscribed)
            {
                EditorApplication.update += Update;
                _isSubscribed = true;
            }
        }

        public void Cancel()
        {
            EditorApplication.update -= Update;
            _isSubscribed = false;
        }

        private void Update()
        {
            if (EditorApplication.timeSinceStartup >= _executeTime)
            {
                _callback();
                Cancel();
            }
        }

        public void Dispose() => Cancel();
    }
}