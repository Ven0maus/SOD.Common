using HarmonyLib;
using SOD.Common;
using SOD.Common.Extensions;
using SOD.Common.Helpers.ObjectiveObjects;
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

        private static Interactable ScrewDriverForVent = null;

        [HarmonyPatch(typeof(ChapterIntro), nameof(ChapterIntro.InvestigateWriterAddress))]
        internal class ChapterIntro_InvestigateWriterAddress
        {
            private static void Prefix(ChapterIntro __instance)
            {
                if (!Plugin.Instance.Config.RequireScrewdriverForVents) return;

                // Check if we already have a screwdriver in our inventory
                var hasScrewdriver = FirstPersonItemController.Instance.slots.ToList()
                    .Select(a =>
                    {
                        if (a.interactableID == -1) return null;
                        var inter = a.GetInteractable();
                        if (inter == null || inter.preset == null || inter.preset.presetName == null) return null;
                        return inter.preset.presetName;
                    })
                    .Any(a => a != null && a.Equals("Screwdriver"));
                if (hasScrewdriver)
                {
                    Plugin.Log.LogInfo("Already have a screwdriver in the inventory, no need to add objective or spawn a new one.");
                    return;
                }

                // Check if there is atleast a vent
                var airVent = __instance.kidnapper.home.rooms
                    .AsEnumerable()
                    .SelectMany(a => a.airVents.AsEnumerable())
                    .FirstOrDefault();
                if (airVent == null)
                {
                    Plugin.Log.LogInfo("No airvents found, no need to spawn a screwdriver.");
                    return;
                }

                if (!Toolbox.Instance.objectPresetDictionary.TryGetValue("Screwdriver", out var screwDriverPreset))
                {
                    Plugin.Log.LogInfo("Unable to find screwdriver preset, cannot spawn screwdriver.");
                    return;
                }

                ScrewDriverForVent = __instance.kidnapper.home.nodes
                    .AsEnumerable()
                    .SelectMany(a => a.interactables.Where(b => b.preset.name.Equals(screwDriverPreset.name)))
                    .FirstOrDefault();
                if (ScrewDriverForVent == null)
                {
                    Plugin.Log.LogInfo("No screwdrivers found, spawning atleast one.");

                    FurnitureLocation furnitureLocation;
                    ScrewDriverForVent = __instance.kidnapper.home.PlaceObject(screwDriverPreset, null, null, null, out furnitureLocation, false, Interactable.PassedVarType.jobID, -1, true, 0, InteractablePreset.OwnedPlacementRule.nonOwnedOnly, 0, null, false, null, null, null, "", true);

                    Plugin.Log.LogInfo("Screwdriver was spawned in: " + ScrewDriverForVent.node.room.name);
                }
                else
                {
                    Plugin.Log.LogInfo("A screwdriver is already present, no need to spawn an extra one.");
                }

                Lib.CaseObjectives.OnObjectiveCompleted += CaseObjectives_OnObjectiveCompleted;
            }

            private static void CaseObjectives_OnObjectiveCompleted(object sender, ObjectiveArgs e)
            {
                if (ScrewDriverForVent != null && e.EntryRef.StartsWith("Look around for vents for a potential quick exit"))
                {
                    // Create a new objective
                    Lib.CaseObjectives.Builder(e.Case)
                        .SetText("Look around for a screwdriver to open the air vent")
                        .SetIcon(InterfaceControls.Icon.lookingGlass)
                        .SetPointer(ScrewDriverForVent.GetWorldPosition())
                        .SetCompletionTrigger(PredefinedTrigger.GoToNode(ScrewDriverForVent.node))
                        .Register();

                    ScrewDriverForVent = null;
                    Lib.CaseObjectives.OnObjectiveCompleted -= CaseObjectives_OnObjectiveCompleted;
                }
            }
        }
    }
}
