using SOD.Common.Custom;
using System;

namespace SOD.Common.Helpers
{
    public sealed class Interaction
    {
        internal SimpleActionArgs CurrentPlayerInteraction;
        internal bool LongActionInProgress = false;

        internal Interaction() { }

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
    }
}
