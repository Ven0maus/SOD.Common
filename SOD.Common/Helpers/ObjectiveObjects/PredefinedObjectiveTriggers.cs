namespace SOD.Common.Helpers.ObjectiveObjects
{
    /// <summary>
    /// Contains static methods to create some predefined triggers.
    /// </summary>
    public sealed class PredefinedTrigger
    {
        internal readonly Objective.ObjectiveTrigger ObjectiveTrigger;

        internal PredefinedTrigger(Objective.ObjectiveTrigger objectiveTrigger)
        {
            ObjectiveTrigger = objectiveTrigger;
        }

        public static PredefinedTrigger GoToNode(NewNode node)
        {
            return Create(Objective.ObjectiveTriggerType.goToNode, node: node);
        }

        public static PredefinedTrigger GoToRoom(NewRoom room)
        {
            return Create(Objective.ObjectiveTriggerType.goToRoom, room: room);
        }

        public static PredefinedTrigger GoToAddress(NewGameLocation address)
        {
            return Create(Objective.ObjectiveTriggerType.goToAddress, address: address);
        }

        public static PredefinedTrigger PickupInteractable(Interactable interactable)
        {
            return Create(Objective.ObjectiveTriggerType.interactableRemoved, interactable: interactable);
        }

        public static PredefinedTrigger LookAtInteractable(Interactable interactable)
        {
            return Create(Objective.ObjectiveTriggerType.viewInteractable, interactable: interactable);
        }

        public static PredefinedTrigger InspectInteractable(Interactable interactable)
        {
            return Create(Objective.ObjectiveTriggerType.inspectInteractable, interactable: interactable);
        }

        private static PredefinedTrigger Create(Objective.ObjectiveTriggerType type, Interactable interactable = null, NewRoom room = null, NewNode node = null, NewGameLocation address = null)
        {
            return new PredefinedTrigger(new Objective.ObjectiveTrigger(type, "", false, 0f, room, interactable, null, node, null, address, null, "", false, default));
        }
    }
}
