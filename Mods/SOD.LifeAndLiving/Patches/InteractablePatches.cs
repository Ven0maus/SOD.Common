using HarmonyLib;
using System.Collections.Generic;

namespace SOD.LifeAndLiving.Patches
{
    internal class InteractablePatches
    {
        [HarmonyPatch(typeof(Interactable), nameof(Interactable.SetValue))]
        internal class Interactable_SetValue
        {
            [HarmonyPrefix]
            internal static void Prefix(Interactable __instance, ref float newValue)
            {
                if (__instance.preset == null) return;

                var moneyPresets = new Dictionary<string, int> 
                {
                    { "M1", Plugin.Instance.Config.MaxM1Crows },
                    { "M2", Plugin.Instance.Config.MaxM2Crows },
                    { "M3", Plugin.Instance.Config.MaxM3Crows },
                    { "M4", Plugin.Instance.Config.MaxM4Crows }
                };

                // Reduce the value of loose change laying around
                if (__instance.preset.isMoney &&
                    moneyPresets.TryGetValue(__instance.preset.presetName, out var maxValue) &&
                    newValue > maxValue)
                {
                    newValue = maxValue;
                }
            }
        }
    }
}
