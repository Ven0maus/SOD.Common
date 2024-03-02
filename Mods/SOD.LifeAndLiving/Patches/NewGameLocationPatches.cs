using HarmonyLib;
using System;

namespace SOD.LifeAndLiving.Patches
{
    internal class NewGameLocationPatches
    {
        [HarmonyPatch(typeof(NewGameLocation), nameof(NewGameLocation.GetPrice))]
        internal class NewGameLocation_GetPrice
        {
            private static readonly Random _valueRandom = new();

            [HarmonyPostfix]
            internal static void Postfix(ref int __result)
            {
                __result += __result / 100 * Plugin.Instance.Config.ApartementCostPercentage;

                // Minimum should be around 3000-4000
                if (__result < 1500)
                {
                    __result = _valueRandom.Next(1500, 2001) * 2;
                }
            }
        }
    }
}
