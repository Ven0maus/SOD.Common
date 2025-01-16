﻿using HarmonyLib;
using System.Collections.Generic;

namespace SOD.LifeAndLiving.Patches.EconomyRebalancePatches
{
    internal class InteractablePatches
    {
        [HarmonyPatch(typeof(Interactable), nameof(Interactable.UpdateName))]
        internal class Interactable_SetValue
        {
            [HarmonyPrefix]
            internal static void Prefix(Interactable __instance)
            {
                if (__instance.preset == null) return;
                if (Plugin.Instance.Config.DisableEconomyRebalance) return;

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
                    __instance.val > maxValue)
                {
                    __instance.val = maxValue;
                }
            }
        }
    }
}
