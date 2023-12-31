using UnityEngine;
using UniverseLib.Utility;

namespace SOD.Common.Custom
{
    /// <summary>
    /// Provides safe access to Interactable data, including TryGet methods for InteractableControllers and Unity objects. Use this to avoid issues when Interactables get pooled.
    /// </summary>
    public sealed class InteractableInstanceData
    {
        internal InteractableInstanceData(Interactable interactable, InteractablePreset preset)
        {
            Interactable = interactable;
            Preset = preset;
        }

        /// <summary>
        /// Whether the interactable is loaded. The interactable will be unloaded while it is pooled.
        /// </summary>
        public bool IsLoaded => Interactable.loadedGeometry;

        /// <summary>
        /// The interactable.
        /// </summary>
        public Interactable Interactable { get; }

        /// <summary>
        /// The interactable's preset.
        /// </summary>
        public InteractablePreset Preset { get; }

        /// <summary>
        /// Get the interactable controller, if the interactable is loaded.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns>False if the InteractableController component is null or destroyed, true otherwise.</returns>
        public bool TryGetInteractableController(out InteractableController controller)
        {
            controller = null;
            if (!Interactable.controller.IsNullOrDestroyed(true))
            {
                controller = Interactable.controller;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get the interactable's transform, if the interactable is loaded.
        /// </summary>
        /// <param name="transform"></param>
        /// <returns>False if the GameObject is null or destroyed, true otherwise.</returns>
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

        /// <summary>
        /// Get the interactable's GameObject, if the interactable is loaded.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns>False if the GameObject is null or destroyed, true otherwise.</returns>
        public bool TryGetTransform(out GameObject gameObject)
        {
            gameObject = null;
            if (!TryGetInteractableController(out var controller))
            {
                return false;
            }
            gameObject = controller.gameObject;
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
