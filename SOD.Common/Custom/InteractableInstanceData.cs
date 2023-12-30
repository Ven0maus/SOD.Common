using System;
using UnityEngine;

namespace SOD.Common.Custom
{
    public sealed class InteractableInstanceData
    {
        public InteractableInstanceData(Interactable interactable, InteractablePreset preset)
        {
            Interactable = interactable;
            Preset = preset;
        }

        public bool IsLoaded
        {
            get { return Interactable.loadedGeometry; }
        }
        public Interactable Interactable { get; }
        public InteractablePreset Preset { get; }

        public bool TryGetInteractableController(out InteractableController controller)
        {
            controller = null;
            if (Interactable.controller != null)
            {
                controller = Interactable.controller;
                return true;
            }
            return false;
        }

        public bool TryGetTransform(out Transform transform)
        {
            transform = null;
            if (!TryGetInteractableController(out var controller))
            {
                return false;
            }
            transform = controller.transform;
            return false;
        }

        public static implicit operator InteractableInstanceData(Interactable interactable)
        {
            return new InteractableInstanceData(interactable, interactable.preset);
        }

        public static implicit operator InteractableInstanceData(InteractableController controller)
        {
            return new InteractableInstanceData(
                controller.interactable,
                controller.interactable.preset
            );
        }
    }
}
