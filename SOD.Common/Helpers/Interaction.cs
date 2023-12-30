using SOD.Common.Custom;
using System;

namespace SOD.Common.Helpers
{
    public sealed class Interaction
    {
        private float _lastAmount = float.MaxValue;
        internal SimpleActionArgs CurrentPlayerInteraction;
        internal bool LongActionInProgress = false;

        /// <summary>
        /// Raised just prior to when the player starts an action, whether the action is long or immediate.
        /// </summary>
        public event EventHandler<SimpleActionArgs> OnBeforeActionStarted;

        /// <summary>
        /// Raised just after when the player starts an action, whether the action is long or immediate.
        /// </summary>
        public event EventHandler<SimpleActionArgs> OnAfterActionStarted;

        /// <summary>
        /// Raised just after the player cancels a long action like lockpicking or searching.
        /// </summary>
        public event EventHandler<SimpleActionArgs> OnAfterLongActionCancelled;

        /// <summary>
        /// Raised just after the player completes a long action like lockpicking or searching.
        /// </summary>
        public event EventHandler<SimpleActionArgs> OnAfterLongActionCompleted;

        /// <summary>
        /// Raised each frame when the player has made progress on a long action (while the player is looking at a lock they are picking, for example).
        /// </summary>
        public event EventHandler<ProgressChangedActionArgs> OnAfterLongActionProgressed;

        internal void OnLongActionCancelled()
        {
            LongActionInProgress = false;
            OnAfterLongActionCancelled?.Invoke(this, CurrentPlayerInteraction);
        }

        internal void OnLongActionCompleted()
        {
            LongActionInProgress = false;
            OnAfterLongActionCompleted?.Invoke(this, CurrentPlayerInteraction);
        }

        internal void OnLongActionProgressed(float amountThisFrame, float amountTotal)
        {
            if (amountTotal < _lastAmount)
            {
                // Just started making progress
                LongActionInProgress = true;
                _lastAmount = amountTotal;
                OnAfterActionStarted?.Invoke(this, CurrentPlayerInteraction);
            }
            OnAfterLongActionProgressed?.Invoke(
                this,
                new ProgressChangedActionArgs(CurrentPlayerInteraction, amountThisFrame, amountTotal)
            );
        }

        internal void OnActionStarted(SimpleActionArgs args, bool after)
        {
            if (after)
                OnAfterActionStarted?.Invoke(this, args);
            else
                OnBeforeActionStarted?.Invoke(this, args);
        }

        public class SimpleActionArgs : EventArgs
        {
            internal InteractablePreset.InteractionAction Action { get; set; }
            internal Interactable.InteractableCurrentAction CurrentAction { get; set; }

            public InteractablePreset.InteractionKey Key { get; set; }
            public InteractableInstanceData InteractableInstanceData { get; internal set; }
            public bool IsFpsItemTarget { get; internal set; }

            internal SimpleActionArgs() { }

            public bool TryGetAction(out InteractablePreset.InteractionAction action)
            {
                action = Action;
                return action != null;
            }

            public bool TryGetCurrentAction(
                out Interactable.InteractableCurrentAction currentAction
            )
            {
                currentAction = CurrentAction;
                return currentAction != null;
            }
        }

        public sealed class ProgressChangedActionArgs : SimpleActionArgs
        {
            internal ProgressChangedActionArgs(SimpleActionArgs args, float amountThisFrame, float amountTotal)
            {
                // Simple action args
                Action = args.Action;
                CurrentAction = args.CurrentAction;
                Key = args.Key;
                InteractableInstanceData = args.InteractableInstanceData;
                IsFpsItemTarget = args.IsFpsItemTarget;

                // Progress changed action args
                AmountThisFrame = amountThisFrame;
                AmountTotal = amountTotal;
            }

            public float AmountThisFrame { get; }
            public float AmountTotal { get; }
        }
    }
}
