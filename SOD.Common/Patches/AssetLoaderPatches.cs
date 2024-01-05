using HarmonyLib;
using SOD.Common.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SOD.Common.Patches
{
    internal class AssetLoaderPatches
    {
        [HarmonyPatch(typeof(AssetLoader), nameof(AssetLoader.GetAllPresets))]
        internal static class AssetLoader_GetAllPresets
        {
            [HarmonyPostfix]
            internal static void Postfix(List<ScriptableObject> __result)
            {
                // Insert all the registered sync disk presets
                __result.AddRange(SyncDisks.RegisteredSyncDisks.Select(a => a.Preset));
            }
        }
    }
}
