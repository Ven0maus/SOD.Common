using UnityEngine;

namespace SOD.Common.Helpers.ObjectiveObjects
{
    /// <summary>
    /// Contains static methods to create some predefined triggers.
    /// </summary>
    public sealed class PredefinedTrigger
    {
        internal readonly Objective.ObjectiveTrigger ObjectiveTrigger;
        internal readonly Vector3? PointerPosition;

        internal PredefinedTrigger(Objective.ObjectiveTrigger objectiveTrigger, Vector3? pointerPosition)
        {
            ObjectiveTrigger = objectiveTrigger;
            PointerPosition = pointerPosition;
        }

        public static PredefinedTrigger GoToNode(NewNode node)
        {
            return Create(Objective.ObjectiveTriggerType.goToNode, node: node);
        }

        public static PredefinedTrigger GoToRoom(NewRoom room)
        {
            return Create(Objective.ObjectiveTriggerType.goToNode, room: room);
        }

        public static PredefinedTrigger PickupInteractable(Interactable interactable, bool showPointer = true)
        {
            return Create(Objective.ObjectiveTriggerType.interactableRemoved, interactable: interactable, showPointer: showPointer);
        }

        public static PredefinedTrigger LookAtInteractable(Interactable interactable, bool showPointer = true)
        {
            return Create(Objective.ObjectiveTriggerType.viewInteractable, interactable: interactable, showPointer: showPointer);
        }

        public static PredefinedTrigger InspectInteractable(Interactable interactable, bool showPointer = true)
        {
            return Create(Objective.ObjectiveTriggerType.inspectInteractable, interactable: interactable, showPointer: showPointer);
        }

        private static PredefinedTrigger Create(Objective.ObjectiveTriggerType type, Interactable interactable = null, NewRoom room = null, NewNode node = null, bool showPointer = false)
        {
            return new PredefinedTrigger(new Objective.ObjectiveTrigger(type, "", false, 0f, room, interactable, null, node, null, null, null, "", false, default), showPointer ? interactable.GetWorldPosition() : null);
        }
    }
}
