using HarmonyLib;
using System;
using System.Collections.Generic;

namespace SOD.LifeAndLiving.Patches
{
    internal class NewGameLocationPatches
    {
        [HarmonyPatch(typeof(NewGameLocation), nameof(NewGameLocation.GetPrice))]
        internal class NewGameLocation_GetPrice
        {
            private static readonly Random _valueRandom = new();
            private static readonly Dictionary<string, int> _apartementPriceCache = new();

            [HarmonyPostfix]
            internal static void Postfix(NewGameLocation __instance, ref int __result)
            {
                if (_apartementPriceCache.TryGetValue(__instance.seed, out int newValue))
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

                _apartementPriceCache.Add(__instance.seed, __result);
            }

            private static int RoundToNearestInterval(int number, int interval1, int interval2)
            {
                int roundedToInterval1 = (int)Math.Round(number / (double)interval1) * interval1;
                int roundedToInterval2 = (int)Math.Round(number / (double)interval2) * interval2;

                int difference1 = Math.Abs(number - roundedToInterval1);
                int difference2 = Math.Abs(number - roundedToInterval2);

                if (difference1 < difference2)
                {
                    return roundedToInterval1;
                }
                else
                {
                    return roundedToInterval2;
                }
            }
        }
    }
}
