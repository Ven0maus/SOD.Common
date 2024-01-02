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
            internal static void Prefix(Interactable __instance, out Interaction.SimpleActionArgs __state,
                InteractablePreset.InteractionAction action, Actor who)
            {
                __state = null;
                if (who == null || !who.isPlayer)
                    return;

                __state = new Interaction.SimpleActionArgs
                {
                    Action = action,
                    InteractableInstanceData = __instance,
                    IsFpsItemTarget = false,
                };
                Lib.Interaction.OnActionStarted(__state, false);
            }

            [HarmonyPostfix]
            internal static void Postfix(Interaction.SimpleActionArgs __state, Actor who)
            {
                if (__state == null || who == null || !who.isPlayer)
                    return;

                Lib.Interaction.OnActionStarted(__state, true);
            }
        }
    }
}
