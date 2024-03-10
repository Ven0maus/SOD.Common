using HarmonyLib;
using SOD.Common.Extensions;
using System;
using System.Linq;

namespace SOD.LifeAndLiving.Patches
{
    internal class ToolboxPatches
    {
        [HarmonyPatch(typeof(Toolbox), nameof(Toolbox.LoadAll))]
        internal class Toolbox_LoadAll
        {
            private static bool _appliedOnce = false;

            [HarmonyPostfix]
            internal static void Postfix(Toolbox __instance)
            {
                if (_appliedOnce) return;
                _appliedOnce = true;

                // Reduces job payouts from rewards by a percentage
                ReduceJobPayouts(__instance);

                // Reduce spawn rates of certain interactables
                AdjustInteractableSpawns(__instance);

                // Adjust cost prices of furniture
                AdjustFurnitureCostPrices(__instance);

                // Adjust cost prices of dialog such as guest pass
                AdjustDialogCostPrices(__instance);

                // Adjusts all the item purchase and selling prices
                AdjustItemPrices(__instance);
                AdjustCompanyItemSellMultipliers(__instance);
            }

            private static void AdjustDialogCostPrices(Toolbox __instance)
            {
                var percentage = Plugin.Instance.Config.DialogCostPricePercentage;
                int count = 0;
                foreach (var dialogPreset in __instance.allDialog)
                {
                    if (dialogPreset.cost > 0)
                    {
                        dialogPreset.cost += dialogPreset.cost / 100 * percentage;
                        count++;
                    }
                }
                Plugin.Log.LogInfo($"Adjusted \"{count}\" dialog cost prices.");
            }

            private static void AdjustFurnitureCostPrices(Toolbox __instance)
            {
                var percentage = Plugin.Instance.Config.FurniteCostPercentage;
                int count = 0;
                foreach (var furniturePreset in __instance.allFurniture)
                {
                    if (furniturePreset.cost > 0)
                    {
                        furniturePreset.cost += furniturePreset.cost / 100 * percentage;
                        count++;
                    }
                }
                Plugin.Log.LogInfo($"Adjusted \"{count}\" furniture cost prices.");
            }

            private static void AdjustCompanyItemSellMultipliers(Toolbox __instance)
            {
                var generalPercentage = Plugin.Instance.Config.PercentageSalePriceGeneral;
                var blackMarketPercentage = Plugin.Instance.Config.PercentageSalePriceBlackMarket;
                int count = 0;
                foreach (var companyPreset in __instance.allCompanyPresets)
                {
                    if (companyPreset.enableSellingOfIllegalItems)
                    {
                        companyPreset.sellValueMultiplier = blackMarketPercentage / 100f;
                    }
                    else
                    {
                        companyPreset.sellValueMultiplier = generalPercentage / 100f;
                    }
                    count++;
                }
                Plugin.Log.LogInfo($"Adjusted \"{count}\" store prices.");
            }

            private static void AdjustItemPrices(Toolbox __instance)
            {
                // Static seed because the LoadAll is ran at startup of the game exe
                // The city gen will randomize it properly
                var random = new Random(1000);
                int count = 0;
                foreach (var item in __instance.objectPresetDictionary)
                {
                    var preset = item.value;
                    if (preset != null && preset.value.y > 0)
                    {
                        // Both these cases are hardcoded to certain values
                        if (preset.presetName.Equals("Diamond") || preset.presetName.Equals("SyncDiskUpgrade"))
                            continue;

                        var percentageValueIncrease = Plugin.Instance.Config.PercentageValueIncreaseGeneral;
                        var minItemValue = Plugin.Instance.Config.MinGeneralItemValue;

                        // If its food, look at different config values
                        if (preset.retailItem != null)
                        {
                            if (preset.retailItem.isConsumable &&
                                (preset.retailItem.menuCategory == RetailItemPreset.MenuCategory.drinks ||
                                preset.retailItem.menuCategory == RetailItemPreset.MenuCategory.food ||
                                preset.retailItem.menuCategory == RetailItemPreset.MenuCategory.snacks) &&
                                (preset.retailItem.nourishment > 0 || preset.retailItem.hydration > 0))
                            {
                                percentageValueIncrease = Plugin.Instance.Config.PercentageValueIncreaseFood;
                                minItemValue = Plugin.Instance.Config.MinFoodItemValue;
                            }
                        }

                        var percentage = (percentageValueIncrease / 100f) + 1;

                        // Calculate initial min and max values
                        var initialMinValue = Math.Max(minItemValue, (int)preset.value.x);
                        var initialMaxValue = Math.Max(initialMinValue + ((int)preset.value.y - (int)preset.value.x), (int)preset.value.y);

                        // Apply percentages
                        initialMinValue = (int)(initialMinValue * percentage);
                        initialMaxValue = (int)(initialMaxValue * percentage);

                        // Add some random off-set to min value
                        // First calculate how much we can max off-set
                        var diff = initialMinValue - minItemValue;
                        if (diff > 0)
                            initialMinValue -= random.Next(0, Math.Min(diff, 15) + 1);

                        // Add some random off-set to max value
                        initialMaxValue += random.Next(0, 16);

                        // Set value
                        preset.value = new UnityEngine.Vector2(initialMinValue, initialMaxValue);
                        count++;
                    }
                }
                Plugin.Log.LogInfo($"Adjusted \"{count}\" item purchase prices.");
            }

            private static void ReduceJobPayouts(Toolbox __instance)
            {
                var sideJobs = __instance.allSideJobs;
                var reduction = Plugin.Instance.Config.PayoutReductionJobs;
                int jobCount = 0;
                var minSideJobReward = Plugin.Instance.Config.MinSideJobReward;
                foreach (var sideJob in sideJobs)
                {
                    int totalMin = 0, totalMax = 0;
                    foreach (var resolveQuestion in sideJob.resolveQuestions)
                    {
                        var min = Math.Max(1, (int)resolveQuestion.rewardRange.x - (int)(resolveQuestion.rewardRange.x / 100 * reduction));
                        var max = Math.Max(1, (int)resolveQuestion.rewardRange.y - (int)(resolveQuestion.rewardRange.y / 100 * reduction));
                        resolveQuestion.rewardRange = new UnityEngine.Vector2(min, max);
                        totalMin += min;
                        totalMax += max;
                    }

                    // Fall-back to make side job atleast min set
                    if (totalMin < minSideJobReward || totalMax < minSideJobReward)
                    {
                        var totalPerQuestion = (int)Math.Ceiling((float)minSideJobReward / sideJob.resolveQuestions.Count);
                        foreach (var resolveQuestion in sideJob.resolveQuestions)
                            resolveQuestion.rewardRange = new UnityEngine.Vector2(totalPerQuestion, totalPerQuestion);
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
                var syncDiskModulePreset = __instance.GetInteractablePreset("SyncDiskUpgrade");
                var syncDiskPreset = __instance.GetInteractablePreset("SyncDisk");
                var looseChange = new[] { "M1", "M2", "M3", "M4" }
                    .Select(a => __instance.GetInteractablePreset(a))
                    .ToArray();

                // Diamonds, only one per address instead of 2
                if (Plugin.Instance.Config.ReduceDiamondSpawnPerAddress)
                {
                    diamondPreset.perRoomLimit = 1;
                    diamondPreset.perAddressLimit = 1;
                    diamondPreset.limitPerRoom = true;
                    diamondPreset.limitPerAddress = true;
                }

                // Adjust value of diamond
                diamondPreset.value = new UnityEngine.Vector2(Plugin.Instance.Config.MinDiamondValue, Plugin.Instance.Config.MaxDiamondValue);
                if (Plugin.Instance.Config.SpawnDiamondsOnlyInApartements)
                    diamondPreset.autoPlacement = InteractablePreset.AutoPlacement.onlyInHomes;

                if (Plugin.Instance.Config.LimitSpawnrateSyncDiskUpgradeModules)
                {
                    syncDiskModulePreset.perRoomLimit = 1;
                    syncDiskModulePreset.perAddressLimit = 2;
                    syncDiskModulePreset.perObjectLimit = 1;
                    syncDiskModulePreset.perCommercialLimit = 2;
                    syncDiskModulePreset.limitInCommercial = true;
                    syncDiskModulePreset.limitPerRoom = true;
                    syncDiskModulePreset.limitPerAddress = true;
                    syncDiskModulePreset.limitPerObject = true;
                }

                // Set value
                syncDiskModulePreset.value = new UnityEngine.Vector2(Plugin.Instance.Config.MinSyncDiskUpgradeModuleValue, 
                    Plugin.Instance.Config.MaxSyncDiskUpgradeModuleValue);

                if (Plugin.Instance.Config.LimitSpawnrateSyncDisks)
                {
                    syncDiskPreset.perRoomLimit = 1;
                    syncDiskPreset.perAddressLimit = 2;
                    syncDiskPreset.perObjectLimit = 1;
                    syncDiskPreset.perCommercialLimit = 2;
                    syncDiskPreset.limitInCommercial = true;
                    syncDiskPreset.limitPerRoom = true;
                    syncDiskPreset.limitPerAddress = true;
                    syncDiskPreset.limitPerObject = true;
                }

                // Lockpicks
                AdjustHairPins(hairPinPreset);
                AdjustPaperClips(paperClipPreset);

                // Loose change laying around
                AdjustLooseChange(looseChange);

                Plugin.Log.LogInfo("Adjusted spawn rate of interactables.");
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
