using HarmonyLib;
using System.Linq;

namespace SOD.LifeAndLiving.Patches
{
    internal class ToolboxPatches
    {
        [HarmonyPatch(typeof(Toolbox), nameof(Toolbox.LoadAll))]
        internal class Toolbox_LoadAll
        {
            [HarmonyPostfix]
            internal static void Postfix(Toolbox __instance)
            {
                // Reduces job payouts from rewards by a percentage
                ReduceJobPayouts(__instance);

                // Reduce spawn rates of certain interactables
                AdjustInteractableSpawns(__instance);
            }

            private static void ReduceJobPayouts(Toolbox __instance)
            {
                var sideJobs = __instance.allSideJobs;
                var reduction = Plugin.Instance.Config.PayoutReductionJobs;
                int jobCount = 0;
                foreach (var sideJob in sideJobs)
                {
                    foreach (var resolveQuestion in sideJob.resolveQuestions)
                    {
                        var min = resolveQuestion.rewardRange.x - (resolveQuestion.rewardRange.x / 100 * reduction);
                        var max = resolveQuestion.rewardRange.y - (resolveQuestion.rewardRange.y / 100 * reduction);
                        resolveQuestion.rewardRange = new UnityEngine.Vector2(min, max);
                    }
                    jobCount++;
                }
                Plugin.Log.LogInfo($"Reduced \"{jobCount}\" job payouts.");
            }

            private static void AdjustInteractableSpawns(Toolbox __instance)
            {
                var diamondPreset = __instance.GetInteractablePreset("Diamond");
                var hairPinPreset = __instance.GetInteractablePreset("Hairpin");
                var paperClipPreset = __instance.GetInteractablePreset("Paperclip");
                var looseChange = new[] { "M1", "M2", "M3", "M4" }
                    .Select(a => __instance.GetInteractablePreset(a))
                    .ToArray();

                // Diamonds, only one per address instead of 2
                diamondPreset.perAddressLimit = 1;
                diamondPreset.limitPerRoom = true;
                diamondPreset.limitPerAddress = true;
                if (Plugin.Instance.Config.SpawnDiamondsOnlyInApartements)
                    diamondPreset.autoPlacement = InteractablePreset.AutoPlacement.onlyInHomes;

                // Lockpicks
                AdjustHairPins(hairPinPreset);
                AdjustPaperClips(paperClipPreset);

                // Loose change laying around
                AdjustLooseChange(looseChange);
            }

            private static void AdjustLooseChange(InteractablePreset[] looseChange)
            {
                foreach (var change in looseChange)
                {
                    change.perRoomLimit = 1;
                    change.perAddressLimit = 1;
                    change.frequencyPerOwnerMax = 1;
                    change.limitPerAddress = true;
                    change.limitPerRoom = true;
                    change.limitPerObject = true;
                    if (Plugin.Instance.Config.LimitLooseMoneyToApartementsOnly)
                        change.autoPlacement = InteractablePreset.AutoPlacement.onlyInHomes;
                }
            }

            private static void AdjustPaperClips(InteractablePreset paperClipPreset)
            {
                paperClipPreset.frequencyPerOwnerMin = 0;
                paperClipPreset.frequencyPerOwnerMax = 1;
                if (Plugin.Instance.Config.ReduceLockPickSpawnPerRoom)
                {
                    paperClipPreset.perRoomLimit = 1;
                    paperClipPreset.limitPerRoom = true;
                }
                if (Plugin.Instance.Config.ReduceLockPickSpawnPerAddress)
                {
                    paperClipPreset.perAddressLimit = 2;
                    paperClipPreset.limitPerAddress = true;
                }
                if (Plugin.Instance.Config.LimitPaperclipToOfficeOnly)
                    paperClipPreset.autoPlacement = InteractablePreset.AutoPlacement.onlyInCompany;
            }

            private static void AdjustHairPins(InteractablePreset hairPinPreset)
            {
                hairPinPreset.frequencyPerOwnerMin = 0;
                hairPinPreset.frequencyPerOwnerMax = 1;
                if (Plugin.Instance.Config.ReduceLockPickSpawnPerRoom)
                {
                    hairPinPreset.perRoomLimit = 1;
                    hairPinPreset.limitPerRoom = true;
                }
                if (Plugin.Instance.Config.ReduceLockPickSpawnPerAddress)
                {
                    hairPinPreset.perAddressLimit = 2;
                    hairPinPreset.limitPerAddress = true;
                }
                if (Plugin.Instance.Config.LimitHairPinToHomeOnly)
                    hairPinPreset.autoPlacement = InteractablePreset.AutoPlacement.onlyInHomes;
            }
        }
    }
}
