using HarmonyLib;

namespace SOD.StockMarket.Patches
{
    internal class CityConstructorPatches
    {
        [HarmonyPatch(typeof(CityConstructor), nameof(CityConstructor.Finalized))]
        internal class CityConstructor_Finalized
        {
            private static bool _init = false;

            [HarmonyPostfix]
            internal static void Postfix()
            {
                if (_init) return;
                _init = true;
                Plugin.Instance.Market.PostStocksInitialization(typeof(CityConstructor));
            }
        }
    }
}
