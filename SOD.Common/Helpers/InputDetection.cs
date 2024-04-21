using System;

namespace SOD.Common.Helpers
{
    public sealed class InputDetection
    {
        internal InputDetection() { }

        /// <summary>
        /// Raised when a button's state changes from up/released to down/pressed and vice versa.
        /// </summary>
        public event EventHandler<InputDetectionEventArgs> OnButtonStateChanged;

        internal void ReportButtonStateChange(string actionName, InteractablePreset.InteractionKey key, bool isDown)
        {
            OnButtonStateChanged?.Invoke(this, new InputDetectionEventArgs(actionName, key, isDown));
        }
    }

    public sealed class InputDetectionEventArgs : EventArgs
    {
        public string ActionName { get; }
        public InteractablePreset.InteractionKey Key { get; }
        public bool IsDown { get; }

        internal InputDetectionEventArgs(string actionName, InteractablePreset.InteractionKey key, bool isDown)
        {
            ActionName = actionName;
            Key = key;
            IsDown = isDown;
        }
    }
}
