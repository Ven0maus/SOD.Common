using HarmonyLib;

namespace SOD.StockMarket.Patches
{
    internal class CitizenCreatorPatches
    {
        [HarmonyPatch(typeof(CitizenCreator), nameof(CitizenCreator.Populate))]
        internal class CitizenCreator_Populate
        {
            internal static bool Init = false;

            [HarmonyPostfix]
            internal static void Postfix()
            {
                if (Init) return;
                Init = true;
                Plugin.Instance.Market.PostStocksInitialization(typeof(CitizenCreator));
            }
        }
    }
}
