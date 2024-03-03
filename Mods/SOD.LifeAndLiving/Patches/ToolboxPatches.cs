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

                // Adjusts all the item purchase and selling prices
                AdjustItemPrices(__instance);
                AdjustCompanyItemSellMultipliers(__instance);
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
                foreach (var item in __instance.allItems)
                {
                    var preset = item.itemPreset;
                    if (preset != null)
                    {
                        int minValue = (int)preset.value.x;
                        if (minValue < minItemValue)
                        {
                            var newMin = minItemValue + (minValue / 100 * percentageValueIncrease);
                            minValue = random.Next(newMin, newMin + minValue + 1);
                        }

                        int maxValue = (int)preset.value.y;
                        if (maxValue < minValue)
                        {
                            var newMin = minValue + (maxValue / 100 * percentageValueIncrease);
                            maxValue = random.Next(minValue + 1, newMin + 1);
                        }

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
                        var min = Math.Max(minSideJobResolveQuestion, resolveQuestion.rewardRange.x - (resolveQuestion.rewardRange.x / 100 * reduction));
                        var max = Math.Max(minSideJobResolveQuestion, resolveQuestion.rewardRange.y - (resolveQuestion.rewardRange.y / 100 * reduction));
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
