using System;

namespace SOD.Common.Helpers.GameplayObjects
{
    public sealed class InteractableArgs : EventArgs
    {
        public Interactable Interactable { get; }

        internal InteractableArgs(Interactable interactable) 
        { 
            Interactable = interactable;
        }
    }
}
