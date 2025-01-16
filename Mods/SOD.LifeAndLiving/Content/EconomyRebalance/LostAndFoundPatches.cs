using HarmonyLib;
using SOD.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SOD.LifeAndLiving.Content.EconomyRebalance
{
    internal class LostAndFoundPatches
    {
        [HarmonyPatch(typeof(NewBuilding), nameof(NewBuilding.TriggerNewLostAndFound))]
        internal class NewBuilding_TriggerNewLostAndFound
        {
            private static HashSet<int> _lostAndFound;

            [HarmonyPrefix]
            internal static void Prefix(NewBuilding __instance)
            {
                if (Plugin.Instance.Config.DisableEconomyRebalance) return;
                // Store the original (before modification)
                _lostAndFound = __instance.lostAndFound.AsEnumerable()
                    .Select(a => HashCode.Combine(a.ownerID, a.buildingID, a.spawnedItem, a.rewardMoney))
                    .ToHashSet();
            }

            [HarmonyPostfix]
            internal static void Postfix(NewBuilding __instance)
            {
                if (Plugin.Instance.Config.DisableEconomyRebalance) return;
                // This reduces the amount that is eventually paid out
                var reductionPercentage = Plugin.Instance.Config.PayoutReductionLostItems;
                int count = 0;
                foreach (var lfObjective in __instance.lostAndFound)
                {
                    var hashCode = HashCode.Combine(lfObjective.ownerID, lfObjective.buildingID, lfObjective.spawnedItem, lfObjective.rewardMoney);
                    if (!_lostAndFound.Contains(hashCode))
                    {
                        // A new one that didn't exist before, modify the reward money.
                        lfObjective.rewardMoney -= (int)(lfObjective.rewardMoney / 100 * (float)reductionPercentage);
                        _lostAndFound.Add(hashCode);
                        count++;
                    }
                }

                // Reset memory
                _lostAndFound = null;
            }
        }

        [HarmonyPatch(typeof(InteractableCreator), nameof(InteractableCreator.CreateFurnitureSpawnedInteractableThreadSafe))]
        internal class InteractableCreator_CreateFurnitureSpawnedInteractableThreadSafe
        {
            [HarmonyPrefix]
            internal static void Prefix(InteractablePreset preset, Il2CppSystem.Collections.Generic.List<Interactable.Passed> passedVars)
            {
                // This reduces the amount shown on the note
                if (preset != null && preset.Equals(CityControls.Instance.lostAndFoundNote))
                {
                    var lostItemReward = passedVars
                        .AsEnumerable()
                        .FirstOrDefault(a => a.varType == Interactable.PassedVarType.lostItemReward);
                    if (lostItemReward != null)
                    {
                        var reductionPercentage = Plugin.Instance.Config.PayoutReductionLostItems;
                        lostItemReward.value -= (int)(lostItemReward.value / 100 * reductionPercentage);
                    }
                }
            }
        }
    }
}
