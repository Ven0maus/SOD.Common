using HarmonyLib;
using SOD.Common.Helpers;

namespace SOD.Common.Patches
{
    internal class InteractionControllerPatches
    {
        [HarmonyPatch(
            typeof(InteractionController),
            nameof(InteractionController.SetCurrentPlayerInteraction)
        )]
        internal class InteractionController_SetCurrentPlayerInteraction
        {
            [HarmonyPrefix]
            internal static void Prefix(
                InteractablePreset.InteractionKey key,
                Interactable newInteractable,
                Interactable.InteractableCurrentAction newCurrentAction,
                bool fpsItem = false)
            {
                Interaction.SimpleActionArgs currentPlayerInteraction = Lib.Interaction.CurrentPlayerInteraction;
                if (Lib.Interaction.LongActionInProgress && currentPlayerInteraction != null)
                    return;

                if (currentPlayerInteraction != null)
                {
                    // Reuse the object
                    currentPlayerInteraction.CurrentAction = newCurrentAction ?? null;
                    currentPlayerInteraction.Action = newCurrentAction?.currentAction ?? null;
                    currentPlayerInteraction.Key = key;
                    currentPlayerInteraction.InteractableInstanceData = newInteractable;
                    currentPlayerInteraction.IsFpsItemTarget = fpsItem;
                    return;
                }

                Lib.Interaction.CurrentPlayerInteraction = new Interaction.SimpleActionArgs
                {
                    CurrentAction = newCurrentAction ?? null,
                    Action = newCurrentAction?.currentAction ?? null,
                    Key = key,
                    InteractableInstanceData = newInteractable,
                    IsFpsItemTarget = fpsItem
                };
            }
        }

        [HarmonyPatch(
            typeof(Interactable),
            nameof(Interactable.OnInteraction),
            new[] 
            {
                typeof(InteractablePreset.InteractionAction),
                typeof(Actor),
                typeof(bool),
                typeof(float)
            }
        )]
        internal class Interactable_OnInteraction
        {
            [HarmonyPrefix]
            internal static void Prefix(
                Interactable __instance,
                out Interaction.SimpleActionArgs __state,
                InteractablePreset.InteractionAction action,
                Actor who)
            {
                __state = null;
                if (who == null || !who.isPlayer)
                    return;

                // Check if the last player interaction is the same
                if (IsSameAsLastPlayerInteraction(__instance, action, out var last))
                {
                    Plugin.Log.LogInfo("Before Same as last");
                    Lib.Interaction.OnActionStarted(last, false);
                    return;
                }

                __state = new Interaction.SimpleActionArgs
                {
                    Action = action,
                    InteractableInstanceData = __instance,
                    IsFpsItemTarget = false,
                };
                Lib.Interaction.OnActionStarted(__state, false);
            }

            [HarmonyPostfix]
            internal static void Postfix(
                Interactable __instance,
                Interaction.SimpleActionArgs __state,
                InteractablePreset.InteractionAction action,
                Actor who)
            {
                if (__state == null || who == null || !who.isPlayer)
                    return;

                // Check if the last player interaction is the same
                if (IsSameAsLastPlayerInteraction(__instance, action, out var last))
                {
                    Plugin.Log.LogInfo("After Same as last");
                    Lib.Interaction.OnActionStarted(last, true);
                    return;
                }

                Lib.Interaction.OnActionStarted(__state, true);
            }

            private static bool IsSameAsLastPlayerInteraction(Interactable interactable, InteractablePreset.InteractionAction action, out Interaction.SimpleActionArgs last)
            {
                last = Lib.Interaction.CurrentPlayerInteraction;
                Plugin.Log.LogInfo($"Current: {interactable.id} | Action: {action != null} | {action?.interactionName}");
                Plugin.Log.LogInfo($"Last: {last != null} | currentAction: {last.CurrentAction?.currentAction?.interactionName} | Interactable: {last.InteractableInstanceData?.Interactable?.id}");
                return last != null && last.CurrentAction?.currentAction == action && 
                       last.InteractableInstanceData?.Interactable == interactable;
            }
        }
    }
}
