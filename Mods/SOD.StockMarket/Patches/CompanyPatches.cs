using HarmonyLib;
using SOD.StockMarket.Core;

namespace SOD.StockMarket.Patches
{
    internal class CompanyPatches
    {
        [HarmonyPatch(typeof(Company), nameof(Company.Setup))]
        internal class Company_Setup
        {
            [HarmonyPostfix]
            internal static void Postfix(Company __instance)
            {
                if (Plugin.Instance.Market.Initialized) return;
                Plugin.Instance.Market.AddStock(new Stock(__instance));
            }
        }
    }
}
