using HarmonyLib;

namespace SOD.LifeAndLiving.Patches
{
    internal class CityControlsPatches
    {
        [HarmonyPatch(typeof(CityControls), nameof(CityControls.Awake))]
        internal class CityControls_Awake
        {
            [HarmonyPrefix]
            internal static void Prefix(CityControls __instance)
            {
                __instance.hotelCostLower = Plugin.Instance.Config.CostLowerSuiteHotel;
                __instance.hotelCostUpper = Plugin.Instance.Config.CostHigherSuiteHotel;
                Plugin.Log.LogInfo("Updated hotel suite prices.");
            }
        }
    }
}
