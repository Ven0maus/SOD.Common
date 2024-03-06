using HarmonyLib;
using SOD.Common.Extensions;

namespace SOD.QoL.Patches
{
    internal class ToolboxPatches
    {
        [HarmonyPatch(typeof(Toolbox), nameof(Toolbox.LoadAll))]
        internal static class Toolbox_LoadAll
        {
            [HarmonyPostfix]
            internal static void Postfix(Toolbox __instance)
            {
                foreach (var item in __instance.allItems
                    .Where(a => a.desireCategory == CompanyPreset.CompanyCategory.caffeine))
                {
                    // If the energy is 0 or smaller, we take 12% of the alertness value
                    if (item.energy <= 0f)
                        item.energy = item.alertness / 100 * 12;
                }
            }
        }
    }
}
