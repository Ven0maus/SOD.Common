using HarmonyLib;
using SOD.ZeroOverhead.Framework.Profiling;

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
                Profiler.BeginFrame();
                Profiler.StartProfileMethod(typeof(NewGameLocation), nameof(NewGameLocation.GetPutDownLocation));
            }

            [HarmonyPostfix]
            internal static void Postfix()
            {
                Profiler.ConcludeMethodProfile(typeof(NewGameLocation), nameof(NewGameLocation.GetPutDownLocation));
                Profiler.EndFrame();
            }
        }
    }
}
