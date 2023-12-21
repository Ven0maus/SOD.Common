using System;

namespace SOD.Common.Shadows.Implementations
{
    // TODO:
    // Figure out how the game does shift key (sprint toggle) detection
    // Potentially add mouse axis detection, etc.
    // The game doesn't call Rewired.Player.GetButtonUp(...) for anything, so the OnButtonUp event is sort of pointless...
    // but it would be good to find a way to hook into OnButtonUp eventually.
    public sealed class InputDetection
    {
        /// <summary>
        /// Raised once when a bound action changes from being released/up to being pressed/down.
        /// </summary>
        public event EventHandler<InputDetectionEventArgs> OnButtonDown;

        internal void OnButtonStateChanged(string actionName, bool isDown)
        {
            // if (isDown)
            OnButtonDown?.Invoke(this, new InputDetectionEventArgs(actionName));
            // else
            //   OnButtonUp?.Invoke(this, new InputDetectionEventArgs(actionName));
        }

        /// <summary>
        /// Raised once when a bound action changes from being pressed/down to being released/up.
        /// </summary>
        // public event EventHandler<InputDetectionEventArgs> OnButtonUp;
    }

    public sealed class InputDetectionEventArgs : EventArgs
    {
        public string ActionName { get; }
        public InputDetectionEventArgs(string actionName)
        {
            ActionName = actionName;
        }
    }
}
