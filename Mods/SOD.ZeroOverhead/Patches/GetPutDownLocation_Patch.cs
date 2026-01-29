using HarmonyLib;
using SOD.ZeroOverhead.Framework;

namespace SOD.ZeroOverhead.Patches
{
    internal class GetPutDownLocation_Patch
    {
        [HarmonyPatch(typeof(NewGameLocation), nameof(NewGameLocation.GetPutDownLocation))]
        internal static class NewGameLocation_GetPutDownLocation
        {
            [HarmonyPrefix]
            internal static void Prefix()
            {
                ProfilingHelper.Profile(typeof(NewGameLocation), nameof(NewGameLocation.GetPutDownLocation));
            }

            [HarmonyPostfix]
            internal static void Postfix()
            {
                ProfilingHelper.Conclude(typeof(NewGameLocation), nameof(NewGameLocation.GetPutDownLocation));
            }
        }
    }
}
