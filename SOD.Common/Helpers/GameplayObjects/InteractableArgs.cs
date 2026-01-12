using System;

namespace SOD.Common.Helpers.GameplayObjects
{
    public sealed class InteractableArgs : EventArgs
    {
        public Interactable Interactable { get; }
        public bool WasThrown { get; }

        internal InteractableArgs(Interactable interactable, bool wasThrown = false)
        {
            Interactable = interactable;
            WasThrown = wasThrown;
        }
    }
}
