using System;
using System.Collections;
using System.Drawing;
using Microsoft.VisualBasic;
using Mono.Cecil;
using UniverseLib.Utility;

namespace SOD.Common.Helpers
{
    public sealed class Interaction
    {
        internal SimpleActionArgs currentPlayerInteraction;
        private float lastAmount = float.MaxValue;
        internal bool longActionInProgress = false;

        public event EventHandler<SimpleActionArgs> OnBeforeActionStarted;
        public event EventHandler<SimpleActionArgs> OnAfterActionStarted;
        public event EventHandler<SimpleActionArgs> OnAfterLongActionCancelled;
        public event EventHandler<SimpleActionArgs> OnAfterLongActionCompleted;
        public event EventHandler<ProgressChangedActionArgs> OnAfterLongActionProgressed;

        internal void OnLongActionCancelled()
        {
            longActionInProgress = false;
            OnAfterLongActionCancelled?.Invoke(this, currentPlayerInteraction);
        }

        internal void OnLongActionCompleted()
        {
            longActionInProgress = false;
            OnAfterLongActionCompleted?.Invoke(this, currentPlayerInteraction);
        }

        internal void OnLongActionProgressed(float amountThisFrame, float amountTotal)
        {
            if (amountTotal < lastAmount)
            {
                // Just started making progress
                longActionInProgress = true;
                lastAmount = amountTotal;
                OnAfterActionStarted?.Invoke(this, currentPlayerInteraction);
            }
            OnAfterLongActionProgressed?.Invoke(
                this,
                new ProgressChangedActionArgs(amountThisFrame, amountTotal)
            );
        }

        internal void OnActionStarted(SimpleActionArgs args, bool after)
        {
            if (after)
                OnAfterActionStarted?.Invoke(this, args);
            else
                OnBeforeActionStarted?.Invoke(this, args);
        }


        // public void OverwriteObjectNameText(System.Func<string, string> textModifyingFunc)
        // {
        //     var tmpComponent = InterfaceController
        //         .Instance.gameWorldCanvas.transform.FindChild(
        //             "GameWorldDisplay/InteractionCursor/ObjectName/ObjectNameText"
        //         )
        //         .GetComponent<TMPro.TextMeshProUGUI>();
        //     var originalText = tmpComponent.text;
        //     var result = textModifyingFunc(originalText);
        //     tmpComponent.SetText(result);
        // }

        public sealed class SimpleActionArgs : EventArgs
        {
            internal InteractablePreset.InteractionAction Action { get; set; }
            internal Interactable.InteractableCurrentAction CurrentAction { get; set; }
            internal InteractablePreset.InteractionKey Key { get; set; }
            public Interactable Interactable { get; internal set; } // TODO: make this a safe reference
            public bool IsFpsItemTarget { get; internal set; }

            public bool TryGetAction(out InteractablePreset.InteractionAction action)
            {
                action = null;
                if (this.Action == null)
                {
                    return false;
                }
                action = this.Action;
                return true;
            }

            public bool TryGetCurrentAction(
                out Interactable.InteractableCurrentAction currentAction
            )
            {
                currentAction = null;
                if (this.CurrentAction == null)
                {
                    return false;
                }
                currentAction = this.CurrentAction;
                return true;
            }

            public bool TryGetKey(out InteractablePreset.InteractionKey key)
            {
                key = default;
                if (this.Key == default)
                {
                    return false;
                }
                key = this.Key;
                return true;
            }
        }

        public sealed class ProgressChangedActionArgs : EventArgs
        {
            public ProgressChangedActionArgs(float amountThisFrame, float amountTotal)
            {
                AmountThisFrame = amountThisFrame;
                AmountTotal = amountTotal;
            }

            public SimpleActionArgs ActionArgs => Lib.Interaction.currentPlayerInteraction;
            public float AmountThisFrame { get; }
            public float AmountTotal { get; }
        }
    }
}
