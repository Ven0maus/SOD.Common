using System;

namespace SOD.Common.Helpers
{
    public sealed class InputDetection
    {
        /// <summary>
        /// Raised when a button's state changes from up/released to down/pressed and vice versa.
        /// </summary>
        public event EventHandler<InputDetectionEventArgs> OnButtonStateChanged;

        internal void ReportButtonStateChange(string actionName, bool isDown)
        {
            OnButtonStateChanged?.Invoke(this, new InputDetectionEventArgs(actionName, isDown));
        }
    }

    public sealed class InputDetectionEventArgs : EventArgs
    {
        public string ActionName { get; }
        public bool IsDown { get; }

        public InputDetectionEventArgs(string actionName, bool isDown)
        {
            ActionName = actionName;
            IsDown = isDown;
        }
    }
}
