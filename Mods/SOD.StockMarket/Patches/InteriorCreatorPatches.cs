using HarmonyLib;

namespace SOD.StockMarket.Patches
{
    internal class InteriorCreatorPatches
    {
        [HarmonyPatch(typeof(InteriorCreator), nameof(InteriorCreator.GenChunk))]
        internal class InteriorCreator_GenChunk
        {
            private static bool _init = false;

            [HarmonyPostfix]
            internal static void Postfix()
            {
                if (_init) return;
                _init = true;
                Plugin.Instance.Market.PostStocksInitialization(typeof(InteriorCreator));
            }
        }
    }
}
