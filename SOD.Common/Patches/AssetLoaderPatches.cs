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
            internal static void Postfix(Il2CppSystem.Collections.Generic.List<ScriptableObject> __result)
            {
                if (_loaded) return;
                _loaded = true;

                LoadDDSEntries();

                // Insert all the registered sync disk presets
                foreach (var preset in Lib.SyncDisks.RegisteredSyncDisks.Select(a => a.Preset))
                {
                    // Set the interactable and add it to the game
                    preset.interactable = SyncDisk.SyncDiskInteractablePreset.Value;

                    Plugin.Log.LogInfo("Added sync disk: " + preset.name);

                    // Also include it in the asset loader
                    __result.Add(preset);
                }
            }

            /// <summary>
            /// Add's any custom dds entries into the game
            /// </summary>
            private static void LoadDDSEntries()
            {
                // Add's every dds entry for the created sync disks into the game, so we don't have to create them custom in files with a ddsloader.
                const string syncDiskDds = "evidence.syncdisks";
                foreach (var syncDisk in Lib.SyncDisks.RegisteredSyncDisks)
                {
                    Lib.DdsStrings[syncDiskDds, syncDisk.Preset.name] = syncDisk.Name;
                    for (int i = 0; i < syncDisk.Effects.Length; i++)
                    {
                        var name = i == 0 ? syncDisk.Preset.mainEffect1Name : i == 1 ? syncDisk.Preset.mainEffect2Name : syncDisk.Preset.mainEffect3Name;
                        var description = i == 0 ? syncDisk.Preset.mainEffect1Description : i == 1 ? syncDisk.Preset.mainEffect2Description : syncDisk.Preset.mainEffect3Description;

                        // Add the dds entries
                        Lib.DdsStrings.AddOrUpdateEntries(syncDiskDds,
                            (name, name["custom_".Length..]),
                            (description, description["custom_".Length..]));
                    }

                    if (syncDisk.SideEffect != null)
                        Lib.DdsStrings[syncDiskDds, $"custom_{syncDisk.SideEffect.Value.Name}"] = syncDisk.SideEffect.Value.Name;
                }
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

                // Load Sync disks into menu presets if applicable
                AddToMenuPresets(Lib.SyncDisks.RegisteredSyncDisks.Where(a => a.MenuPresetLocations.Count > 0));
            }

            /// <summary>
            /// Potentially adds sync disk presets to menu presets if they need to be there based on the registration options
            /// </summary>
            /// <param name="syncDisk"></param>
            private static void AddToMenuPresets(IEnumerable<SyncDisk> syncDisk)
            {
                var groupedPresets = syncDisk
                    .SelectMany(a => a.MenuPresetLocations.Select(saleLocation => new { a.Preset, SaleLocation = saleLocation }))
                    .GroupBy(x => x.SaleLocation)
                    .ToDictionary(group => group.Key, group => group.Select(item => item.Preset).ToArray());
                if (groupedPresets.Count == 0) return;

                var menus = Resources.FindObjectsOfTypeAll<MenuPreset>();
                foreach (var menu in menus)
                {
                    var menuPresetName = menu.GetPresetName();
                    if (groupedPresets.TryGetValue(menuPresetName, out var syncDiskPresets))
                    {
                        // Add sync disk presets to this menu preset
                        foreach (var syncDiskPreset in syncDiskPresets)
                            menu.syncDisks.Add(syncDiskPreset);
                    }
                }

                // Remove the memory usage of this, no longer needed
                Lib.SyncDisks.RegisteredSyncDisks.ForEach(a => a.MenuPresetLocations = null);
            }
        }
    }
}
