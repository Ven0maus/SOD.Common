using HarmonyLib;

namespace SOD.StockMarket.Patches
{
    internal class CityConstructorPatches
    {
        [HarmonyPatch(typeof(CityConstructor), nameof(CityConstructor.Finalized))]
        internal class CityConstructor_Finalized
        {
            internal static bool Init = false;

            [HarmonyPostfix]
            internal static void Postfix()
            {
                if (Init) return;
                Init = true;
                Plugin.Instance.Market.PostStocksInitialization(typeof(CityConstructor));
            }
        }
    }
}
