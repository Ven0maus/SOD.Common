using SOD.Common.Custom;
using UnityEngine;

namespace SOD.Common.Extensions
{
    public static class InteractableExtensions
    {
        /// <summary>
        /// Get the safe reference to the interactable and related data.
        /// </summary>
        /// <param name="interactable"></param>
        /// <returns>An object that allows safe access to the interactable, its preset, and its controller (if it exists).</returns>
        public static InteractableInstanceData GetInteractableInstanceData(this Interactable interactable)
        {
            return interactable; // utilize implicit operator logic
        }

        /// <summary>
        /// Get the safe reference to the interactable and related data.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns>An object that allows safe access to the interactable, its preset, and its controller.</returns>
        public static InteractableInstanceData GetInteractableInstanceData(
            this InteractableController controller
        )
        {
            return controller; // utilize implicit operator logic
        }

        /// <summary>
        /// Get the safe reference to the interactable if an InteractableController component exists on this transform or its children, and related data.
        /// </summary>
        /// <param name="transform"></param>
        /// <returns>An object that allows safe access to the interactable, its preset, and its controller.</returns>
        public static InteractableInstanceData GetInteractableInstanceData(Transform transform)
        {
            // TODO: not certain if we should include inactive or not, decided not for now
            var controller = transform.GetComponentInChildren<InteractableController>(
                includeInactive: false
            );
            if (!controller)
            {
                throw new System.NullReferenceException(
                    $"Could not find InteractableController on Transform {transform.name} or any of its children."
                );
            }
            return controller; // utilize implicit operator logic
        }

        /// <summary>
        /// Get the safe reference to the interactable if an InteractableController component exists on this GameObject's transform or its children, and related data.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns>An object that allows safe access to the interactable, its preset, and its controller.</returns>
        public static InteractableInstanceData GetInteractableInstanceData(GameObject gameObject)
        {
            return GetInteractableInstanceData(gameObject.transform);
        }
    }
}
