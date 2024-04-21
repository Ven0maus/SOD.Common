using SOD.Common.Custom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SOD.Common.Helpers
{
    public sealed class PlayerStatus
    {
        internal PlayerStatus() { }

        /// <summary>
        /// Raised right before illegal action status will be set (either enabled or disabled).
        /// </summary>
        public event EventHandler<IllegalActionStatusArgs> OnBeforeSetIllegalStatus;
        /// <summary>
        /// Raised right after illegal action status is successfully set (either enabled or disabled).
        /// </summary>
        public event EventHandler<IllegalActionStatusArgs> OnAfterSetIllegalStatus;

        /// <summary>
        /// Dictionary used to track illegal status for the player entity.
        /// </summary>
        internal Dictionary<string, IllegalStatusModifier> IllegalStatusModifierDictionary { get; private set; }

        /// <summary>
        /// Sets an illegal status modifier with an optional duration.
        /// <br>The modifier will cause the Player to have an Illegal Action status while it or any other modifier is present.</br>
        /// <br>At the end of the duration (if provided), the modifier will be automatically removed.</br>
        /// </summary>
        /// <param name="key">The key identifying the modifier. Used to overwrite, remove, or toggle the modifier later.</param>
        /// <param name="overwrite">Whether to overwrite any existing modifier with this key, applying the new duration.</param>
        /// <param name="duration">The duration that the modifier lasts. (does not progress while the game is paused).</param>
        public void SetIllegalStatusModifier(string key, bool overwrite = false, TimeSpan? duration = null)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));

            if (IllegalStatusModifierDictionary == null)
                IllegalStatusModifierDictionary = new();

            if (IllegalStatusModifierDictionary.TryGetValue(key, out var modifier))
            {
                if (overwrite)
                {
                    modifier.Stop();
                    IllegalStatusModifierDictionary.Remove(key);
                }
                else
                {
                    return;
                }
            }

            // Add modifier to dictionary to track
            modifier = new IllegalStatusModifier(key, duration);
            IllegalStatusModifierDictionary.Add(key, modifier);

            // Start timer
            if (duration != null)
                modifier.Start();

            // Update status
            Update();
        }

        /// <summary>
        /// Removes an illegal status modifier, if present.
        /// </summary>
        /// <param name="key">The key identifying the modifier.</param>
        public void RemoveIllegalStatusModifier(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));

            if (IllegalStatusModifierDictionary != null && IllegalStatusModifierDictionary.TryGetValue(key, out var modifier))
            {
                modifier.Stop();
                IllegalStatusModifierDictionary.Remove(key);

                // Update status
                Update();
            }
        }

        internal void Update()
        {
            if (IllegalStatusModifierDictionary.Any())
                AdjustIllegalStatus(Player.Instance.illegalStatus, true);
            else
                AdjustIllegalStatus(Player.Instance.illegalStatus, false);
        }

        private static void AdjustIllegalStatus(bool previousStatus, bool newStatus)
        {
            if (previousStatus != newStatus)
            {
                var args = new IllegalActionStatusArgs(previousStatus, newStatus);

                // Raise before event
                Lib.PlayerStatus.OnSetIllegalActionStatus(args, false);

                // Adjust illegal status
                Player.Instance.illegalStatus = newStatus;
                Player.Instance.illegalActionTimer = newStatus ? float.MaxValue : 0f;
                Player.Instance.UpdateIllegalStatus();

                // Raise after event
                Lib.PlayerStatus.OnSetIllegalActionStatus(args, newStatus);
            }
        }

        public sealed class IllegalActionStatusArgs : EventArgs
        {
            public bool PreviousIllegalStatus { get; }
            public bool NewIllegalStatus { get; }

            internal IllegalActionStatusArgs(bool previous, bool @new)
            {
                PreviousIllegalStatus = previous;
                NewIllegalStatus = @new;
            }
        }

        internal void OnSetIllegalActionStatus(IllegalActionStatusArgs eventArgs, bool after)
        {
            if (after)
                OnAfterSetIllegalStatus?.Invoke(this, eventArgs);
            else
                OnBeforeSetIllegalStatus?.Invoke(this, eventArgs);
        }
    }
}
