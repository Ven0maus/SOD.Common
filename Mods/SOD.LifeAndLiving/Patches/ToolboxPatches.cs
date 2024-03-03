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
            [HarmonyPostfix]
            internal static void Postfix(Toolbox __instance)
            {
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
                var percentageValueIncrease = Plugin.Instance.Config.PercentageValueIncrease;
                var minItemValue = Plugin.Instance.Config.MinItemValue;
                var random = new Random();
                int count = 0;
                foreach (var item in __instance.objectPresetDictionary)
                {
                    var preset = item.value;
                    if (preset != null && preset.value.y > 0)
                    {
                        // Set min
                        int minValue = Math.Max(minItemValue, (int)preset.value.x);
                        var adjustment = minValue + (minValue / 100 * percentageValueIncrease);
                        minValue = random.Next(adjustment, (adjustment * 2) + 1);

                        // Set max
                        int maxValue = Math.Max(minValue, (int)preset.value.y);
                        adjustment = maxValue + (maxValue / 100 * percentageValueIncrease);
                        maxValue = random.Next(minValue + 1, adjustment + 1);

                        // Set value
                        preset.value = new UnityEngine.Vector2(minValue, maxValue);
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
                var minSideJobResolveQuestion = Plugin.Instance.Config.MinSideJobResolveQuestion;
                foreach (var sideJob in sideJobs)
                {
                    foreach (var resolveQuestion in sideJob.resolveQuestions)
                    {
                        var min = (int)Math.Max(minSideJobResolveQuestion, resolveQuestion.rewardRange.x - (resolveQuestion.rewardRange.x / 100 * reduction));
                        var max = (int)Math.Max(minSideJobResolveQuestion, resolveQuestion.rewardRange.y - (resolveQuestion.rewardRange.y / 100 * reduction));
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
