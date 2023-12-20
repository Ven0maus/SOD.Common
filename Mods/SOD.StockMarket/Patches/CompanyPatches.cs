using HarmonyLib;
using SOD.StockMarket.Core;

namespace SOD.StockMarket.Patches
{
    internal class CompanyPatches
    {
        [HarmonyPatch(typeof(Company), nameof(Company.Setup))]
        internal class Company_Setup
        {
            private static bool _shownInitializingMessage = false;

            [HarmonyPostfix]
            internal static void Postfix(Company __instance)
            {
                if (Plugin.Instance.Market.Initialized) return;

                // Don't create stocks for these types of companies
                if (__instance.preset != null && __instance.preset.isSelfEmployed || 
                    __instance.preset.isIllegal || !__instance.preset.publicFacing) 
                    return;

                if (!_shownInitializingMessage)
                {
                    Plugin.Log.LogInfo("Initializing stock market data.");
                    _shownInitializingMessage = true;
                }

                Plugin.Instance.Market.InitStock(new Stock(__instance));
            }
        }
    }
}
