using HarmonyLib;

namespace SOD.StockMarket.Patches
{
    internal class CitizenCreatorPatches
    {
        [HarmonyPatch(typeof(CitizenCreator), nameof(CitizenCreator.Populate))]
        internal class CitizenCreator_Populate
        {
            private static bool _init = false;

            [HarmonyPostfix]
            internal static void Postfix()
            {
                if (_init) return;
                _init = true;
                Plugin.Instance.Market.PostStocksInitialization(typeof(CitizenCreator));
            }
        }
    }
}
