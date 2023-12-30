using System;
using HarmonyLib;
using SOD.Common.Helpers;
using UniverseLib;

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
                InteractionController __instance,
                InteractablePreset.InteractionKey key,
                Interactable newInteractable,
                Interactable.InteractableCurrentAction newCurrentAction,
                bool fpsItem = false,
                int forcePriority = -1
            )
            {
                Lib.Interaction.currentPlayerInteraction = new Interaction.SimpleActionArgs
                {
                    CurrentAction = newCurrentAction ?? null,
                    Action = newCurrentAction?.currentAction ?? null,
                    Key = key,
                    Interactable = newInteractable,
                    IsFpsItemTarget = fpsItem
                };
            }
        }

        [HarmonyPatch(
            typeof(Interactable),
            nameof(Interactable.OnInteraction),

            [
                typeof(InteractablePreset.InteractionAction),
                typeof(Actor),
                typeof(bool),
                typeof(float)
            ]
        )]
        internal class Interactable_OnInteraction
        {
            public static Interaction.SimpleActionArgs TempArgs { get; private set; }

            [HarmonyPrefix]
            internal static void Prefix(
                Interactable __instance,
                InteractablePreset.InteractionAction action,
                Actor who,
                bool allowDelays,
                float additionalDelay
            )
            {
                if (!who.isPlayer)
                {
                    return;
                }

                // Check if the last player interaction is the same
                var last = Lib.Interaction.currentPlayerInteraction;
                if (
                    last != null
                    && last.CurrentAction?.currentAction == action
                    && last.Interactable == __instance
                )
                {
                    Lib.Interaction.OnActionStarted(last, false);
                    return;
                }

                TempArgs = new Interaction.SimpleActionArgs
                {
                    Action = action,
                    Interactable = __instance,
                    IsFpsItemTarget = false,
                };
                Lib.Interaction.OnActionStarted(TempArgs, false);
            }

            [HarmonyPostfix]
            internal static void Postfix(
                Interactable __instance,
                InteractablePreset.InteractionAction action,
                Actor who,
                bool allowDelays,
                float additionalDelay
            )
            {
                if (!who.isPlayer)
                {
                    return;
                }

                // Check if the last player interaction is the same
                var last = Lib.Interaction.currentPlayerInteraction;
                if (
                    last != null
                    && last.CurrentAction?.currentAction == action
                    && last.Interactable == __instance
                )
                {
                    Lib.Interaction.OnActionStarted(last, true);
                    return;
                }

                Lib.Interaction.OnActionStarted(TempArgs, true);
            }
        }

        [HarmonyPatch(
            typeof(FirstPersonItemController),
            nameof(FirstPersonItemController.OnInteraction)
        )]
        internal class FirstPersonItemController_OnInteraction
        {
            public static Interaction.SimpleActionArgs TempArgs { get; private set; }

            [HarmonyPrefix]
            internal static void Prefix(
                FirstPersonItemController __instance,
                InteractablePreset.InteractionKey input
            )
            {
                TempArgs = new Interaction.SimpleActionArgs
                {
                    Action = __instance.currentActions[input].currentAction,
                    Interactable = Player.Instance.interactingWith,
                    IsFpsItemTarget = true,
                };
                Lib.Interaction.OnActionStarted(TempArgs, false);
            }

            [HarmonyPostfix]
            internal static void Postfix(
                FirstPersonItemController __instance,
                InteractablePreset.InteractionKey input
            )
            {
                Lib.Interaction.OnActionStarted(TempArgs, true);
            }
        }
    }
}
