using HarmonyLib;
using System;
using System.Collections.Generic;

namespace SOD.LifeAndLiving.Patches.EconomyRebalancePatches
{
    internal class NewGameLocationPatches
    {
        [HarmonyPatch(typeof(NewGameLocation), nameof(NewGameLocation.GetPrice))]
        internal class NewGameLocation_GetPrice
        {
            private static readonly Random _valueRandom = new();
            internal static readonly Dictionary<string, int> ApartementPriceCache = new();

            [HarmonyPostfix]
            internal static void Postfix(NewGameLocation __instance, ref int __result)
            {
                var key = __instance.building.buildingID + "_" + __instance.residenceNumber;
                if (ApartementPriceCache.TryGetValue(key, out int newValue))
                {
                    __result = newValue;
                    return;
                }

                // Initial calculation, some values may pass (high values that are over the minimum)
                __result += __result / 100 * Plugin.Instance.Config.ApartementCostPercentage;

                // Minimum value should be respected
                if (__result < Plugin.Instance.Config.MinimumApartementCost)
                {
                    var half = Plugin.Instance.Config.MinimumApartementCost / 2;
                    var smallPercentage = half / 100 * 25;
                    __result = RoundToNearestInterval(_valueRandom.Next(half, half + smallPercentage) * 2, 50, 100);
                }

                ApartementPriceCache.Add(key, __result);
            }

            private static int RoundToNearestInterval(int number, int lowerInterval, int higherInterval)
            {
                int roundedToLowerInterval = (int)Math.Round(number / (double)lowerInterval) * lowerInterval;
                int roundedToHigherInterval = (int)Math.Round(number / (double)higherInterval) * higherInterval;

                int difference1 = Math.Abs(number - roundedToLowerInterval);
                int difference2 = Math.Abs(number - roundedToHigherInterval);

                if (difference1 < difference2)
                {
                    return roundedToLowerInterval;
                }
                else
                {
                    return roundedToHigherInterval;
                }
            }
        }
    }
}
