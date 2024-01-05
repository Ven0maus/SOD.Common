using BepInEx.Logging;
using HarmonyLib;
using SOD.Common.Helpers;
using SOD.Common.Helpers.SyncDiskObjects;
using System;
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
            private static bool _loaded = false;

            [HarmonyPostfix]
            internal static void Postfix(List<ScriptableObject> __result)
            {
                if (_loaded) return;
                _loaded = true;

                // Insert all the registered sync disk presets
                __result.AddRange(SyncDisks.RegisteredSyncDisks.Select(a => a.Preset));
            }
        }

        [HarmonyPatch(typeof(Toolbox), nameof(Toolbox.LoadAll))]
        internal static class Toolbox_LoadAll
        {
            private static bool _loaded = false;

            [HarmonyPostfix]
            internal static void Postfix()
            {
                if (_loaded) return;
                _loaded = true;

                // Load all the data into the game properly



                // Sync disks
                AddToMenuPresets(SyncDisks.RegisteredSyncDisks.Where(a => a.RegistrationOptions.SaleLocations.Count > 0));
            }

            /// <summary>
            /// Potentially adds sync disk presets to menu presets if they need to be there based on the registration options
            /// </summary>
            /// <param name="syncDisk"></param>
            private static void AddToMenuPresets(IEnumerable<SyncDisk> syncDisk)
            {
                var groupedPresets = syncDisk
                    .SelectMany(a => a.RegistrationOptions.SaleLocations.Select(saleLocation => new { a.Preset, SaleLocation = saleLocation }))
                    .GroupBy(x => x.SaleLocation)
                    .ToDictionary(group => group.Key, group => group.Select(item => item.Preset).ToArray());
                if (groupedPresets.Count == 0) return;

                var menus = Resources.FindObjectsOfTypeAll<MenuPreset>();
                var enumType = typeof(RegistrationOptions.SyncDiskSaleLocation);
                foreach (var menu in menus)
                {
                    var menuPresetName = menu.GetPresetName();
                    if (Enum.TryParse(menuPresetName, true, out RegistrationOptions.SyncDiskSaleLocation location) &&
                        groupedPresets.TryGetValue(location, out var syncDiskPresets))
                    {
                        // Add sync disk presets to this menu preset
                        foreach (var syncDiskPreset in syncDiskPresets)
                            menu.syncDisks.Add(syncDiskPreset);
                    }
                }
            }
        }
    }
}
