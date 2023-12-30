using SOD.Common.Custom;
using UnityEngine;

namespace SOD.Common.Extensions
{
    public static class InteractableExtensions
    {
        public static InteractableInstanceData GetValidatedData(this Interactable interactable)
        {
            return interactable; // utilize implicit operator logic
        }

        public static InteractableInstanceData GetValidatedData(
            this InteractableController controller
        )
        {
            return controller; // utilize implicit operator logic
        }

        public static InteractableInstanceData GetValidatedInteractableData(Transform transform)
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

        public static InteractableInstanceData GetValidatedInteractableData(GameObject gameObject)
        {
            return GetValidatedInteractableData(gameObject.transform);
        }
    }
}
