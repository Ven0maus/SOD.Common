using HarmonyLib;
using SOD.Common;
using SOD.Common.Extensions;
using System.Linq;

namespace SOD.LifeAndLiving.Patches.ImmersiveRequirementPatches
{
    internal class AirVentScrewdriverPatches
    {
        private static bool _attemptOpenOrCloseAirVent = false;

        [HarmonyPatch(typeof(Interactable), nameof(Interactable.OnInteraction), new[] 
        {
            typeof(InteractablePreset.InteractionAction),
            typeof(Actor),
            typeof(bool),
            typeof(float)
        })]
        internal class Interactable_OnInteraction
        {
            private static void Prefix(Interactable __instance, InteractablePreset.InteractionAction action, Actor who)
            {
                if (__instance.preset == null || string.IsNullOrWhiteSpace(__instance.preset.presetName) || !__instance.preset.presetName.Equals("AirVent")) return;
                if (action == null || who == null || !who.isPlayer || !Plugin.Instance.Config.RequireScrewdriverForVents) return;
                if (!action.interactionName.Equals("Open") && !action.interactionName.Equals("Close")) return;

                // We are opening or closing an airvent
                _attemptOpenOrCloseAirVent = true;
            }
        }

        [HarmonyPatch(typeof(Interactable), nameof(Interactable.SetSwitchState))]
        internal class Interactable_SetSwitchState
        {
            private static bool Prefix(Actor interactor)
            {
                if (interactor == null || !interactor.isPlayer || !_attemptOpenOrCloseAirVent) return true;
                _attemptOpenOrCloseAirVent = false;

                // Check if we have a screwdriver in our inventory
                var hasScrewdriver = FirstPersonItemController.Instance.slots.ToList()
                    .Select(a =>
                    {
                        if (a.interactableID == -1) return null;
                        var inter = a.GetInteractable();
                        if (inter == null || inter.preset == null || inter.preset.presetName == null) return null;
                        return inter.preset.presetName;
                    })
                    .Any(a => a != null && a.Equals("Screwdriver"));

                if (!hasScrewdriver)
                {
                    Lib.GameMessage.ShowPlayerSpeech("I could open this with a screwdriver, maybe I should look for one.", 5);
                    return false;
                }

                return true;
            }
        }
    }
}
