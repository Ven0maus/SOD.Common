using HarmonyLib;

namespace SOD.StockMarket.Patches
{
    internal class InteriorCreatorPatches
    {
        [HarmonyPatch(typeof(InteriorCreator), nameof(InteriorCreator.GenChunk))]
        internal class InteriorCreator_GenChunk
        {
            internal static bool Init = false;

            [HarmonyPostfix]
            internal static void Postfix()
            {
                if (Init) return;
                Init = true;
                Plugin.Instance.Market.PostStocksInitialization(typeof(InteriorCreator));
            }
        }
    }
}
