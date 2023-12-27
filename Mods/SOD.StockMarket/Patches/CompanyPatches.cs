using HarmonyLib;
using SOD.StockMarket.Implementation.Stocks;

namespace SOD.StockMarket.Patches
{
    internal class CompanyPatches
    {
        [HarmonyPatch(typeof(Company), nameof(Company.Setup))]
        internal class Company_Setup
        {
            internal static bool ShownInitializingMessage = false;

            [HarmonyPostfix]
            internal static void Postfix(Company __instance)
            {
                if (Plugin.Instance.Market.Initialized) return;

                // Don't create stocks for these types of companies
                if (__instance.preset != null && __instance.preset.isSelfEmployed ||
                    __instance.preset.isIllegal || !__instance.preset.publicFacing)
                    return;

                if (!ShownInitializingMessage)
                {
                    Plugin.Log.LogInfo("Initializing stock market data.");
                    ShownInitializingMessage = true;
                }

                Plugin.Instance.Market.InitStock(new Stock(__instance));
            }
        }
    }
}
