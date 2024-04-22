using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using SOD.Common.Helpers.SyncDiskObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SOD.Common.Patches
{
    internal class AssetLoaderPatches
    {
        internal static List<SyncDiskPreset> _createdSyncDiskPresets = new();
        [HarmonyPatch(typeof(AssetLoader), nameof(AssetLoader.GetAllPresets))]
        internal static class AssetLoader_GetAllPresets
        {
            private static bool _loaded = false;

            [HarmonyPostfix]
            internal static void Postfix(Il2CppSystem.Collections.Generic.List<ScriptableObject> __result)
            {
                if (_loaded) return;
                _loaded = true;

                var sprites = Lib.SyncDisks.RegisteredSyncDisks
                    .SelectMany(a => a.Icons)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
                var gameSprites = Resources.FindObjectsOfTypeAll<Sprite>()
                    .Where(a => sprites.Contains(a.name))
                    .GroupBy(a => a.name)
                    .ToDictionary(a => a.Key, a => a.First());

                // Insert all the registered sync disk presets
                foreach (var syncDisk in Lib.SyncDisks.RegisteredSyncDisks)
                {
                    var preset = syncDisk.Preset;
                    _createdSyncDiskPresets.Add(preset);

                    // Set the interactable and add it to the game
                    preset.interactable = SyncDisk.SyncDiskInteractablePreset.Value;

                    // Set icons
                    if (preset.mainEffect1 != SyncDiskPreset.Effect.none)
                        preset.mainEffect1Icon = GetSprite(gameSprites, syncDisk.Icons[0]);
                    if (preset.mainEffect2 != SyncDiskPreset.Effect.none)
                        preset.mainEffect2Icon = GetSprite(gameSprites, syncDisk.Icons[1]);
                    if (preset.mainEffect3 != SyncDiskPreset.Effect.none)
                        preset.mainEffect3Icon = GetSprite(gameSprites, syncDisk.Icons[2]);

                    // Also include it in the asset loader
                    __result.Add(preset);
                }

                if (Lib.SyncDisks.RegisteredSyncDisks.Count > 0)
                    Plugin.Log.LogInfo($"Loaded {Lib.SyncDisks.RegisteredSyncDisks.Count} custom sync disks.");

                // Clear out memory usage for icons as its no longer used
                Lib.SyncDisks.RegisteredSyncDisks.ForEach(a => a.Icons = null);
            }

            private static Sprite GetSprite(Dictionary<string, Sprite> gameSprites, string spriteName)
            {
                if (!gameSprites.TryGetValue(spriteName, out var sprite))
                    Plugin.Log.LogError($"Could not find sprite with name \"{spriteName}\", falling back to default dna sprite.");
                return sprite ?? gameSprites[SyncDiskBuilder.Effect.DefaultSprite];
            }
        }

        [HarmonyPatch(typeof(Toolbox), nameof(Toolbox.LoadAll))]
        internal static class Toolbox_LoadAll
        {
            private static bool _loaded = false;

            [HarmonyPostfix]
            internal static void Postfix()
            {
                // This should happen each time the toolbox is loaded.
                LoadDDSEntries();

                if (_loaded) return;
                _loaded = true;

                // Set sync disk numbers
                var total = Toolbox.Instance.allSyncDisks.Count;
                foreach (var loadedPreset in _createdSyncDiskPresets)
                {
                    loadedPreset.syncDiskNumber = total++;
                }
                _createdSyncDiskPresets = null;

                var menus = new Lazy<Il2CppArrayBase<MenuPreset>>(Resources.FindObjectsOfTypeAll<MenuPreset>);

                if (Plugin.Instance.Config.DebugMode)
                {
                    // Log any presets not available in the enum, so we can add them in new updates of SOD.Common incase new game updates introduce new ones.
                    var missingLocationsInEnum = menus.Value
                        .Select(a => a.GetPresetName())
                        .Except(Enum.GetValues<SyncDiskBuilder.SyncDiskSaleLocation>()
                            .Select(a => a.ToString()), StringComparer.OrdinalIgnoreCase);
                    foreach (var location in missingLocationsInEnum)
                        Plugin.Log.LogInfo("[UpdateRequired] Missing following new sale locations in enum: " + location);
                }

                // Load Sync disks into menu presets if applicable
                AddToMenuPresets(Lib.SyncDisks.RegisteredSyncDisks.Where(a => a.MenuPresetLocations.Count > 0), menus);
            }

            /// <summary>
            /// Potentially adds sync disk presets to menu presets if they need to be there based on the registration options
            /// </summary>
            /// <param name="syncDisk"></param>
            private static void AddToMenuPresets(IEnumerable<SyncDisk> syncDisk, Lazy<Il2CppArrayBase<MenuPreset>> menus)
            {
                var groupedPresets = syncDisk
                    .SelectMany(a => a.MenuPresetLocations.Select(saleLocation => new { a.Preset, SaleLocation = saleLocation }))
                    .GroupBy(x => x.SaleLocation)
                    .ToDictionary(group => group.Key, group => group.Select(item => item.Preset).ToArray(), StringComparer.OrdinalIgnoreCase);
                if (groupedPresets.Count == 0) return;

                foreach (var menu in menus.Value)
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

            /// <summary>
            /// Add's any custom dds entries into the game
            /// </summary>
            private static void LoadDDSEntries()
            {
                // Add's every dds entry for the created sync disks into the game, so we don't have to create them custom in files with a ddsloader.
                const string syncDiskDds = "evidence.syncdisks";
                foreach (var syncDisk in Lib.SyncDisks.RegisteredSyncDisks)
                {
                    // Add dds entry for sync disk name
                    Lib.DdsStrings[syncDiskDds, syncDisk.Preset.name] = syncDisk.Name;

                    // Add dds entries for effects
                    for (int i = 0; i < syncDisk.Effects.Length; i++)
                    {
                        var name = i == 0 ? syncDisk.Preset.mainEffect1Name : i == 1 ? syncDisk.Preset.mainEffect2Name : syncDisk.Preset.mainEffect3Name;
                        var description = i == 0 ? syncDisk.Preset.mainEffect1Description : i == 1 ? syncDisk.Preset.mainEffect2Description : syncDisk.Preset.mainEffect3Description;

                        // Add the dds entries
                        Lib.DdsStrings.AddOrUpdateEntries(syncDiskDds,
                            (name, SyncDisk.GetName(name)),
                            (description, SyncDisk.GetName(description)));
                    }

                    // Add dds entries for upgrades
                    for (int i = 0; i < syncDisk.UpgradeOptions.Length; i++)
                    {
                        var nameReferences = i == 0 ? syncDisk.Preset.option1UpgradeNameReferences : i == 1 ? syncDisk.Preset.option2UpgradeNameReferences : syncDisk.Preset.option3UpgradeNameReferences;

                        // Add the dds entries
                        for (int y = 0; y < nameReferences.Count; y++)
                            Lib.DdsStrings[syncDiskDds, nameReferences[y]] = SyncDisk.GetName(nameReferences[y]);
                    }

                    // Add dds entry for side effect
                    if (syncDisk.SideEffect != null)
                        Lib.DdsStrings[syncDiskDds, syncDisk.SideEffect.Value.DdsIdentifier] = syncDisk.SideEffect.Value.Name;
                }

                if (Lib.SyncDisks.RegisteredSyncDisks.Any())
                    Plugin.Log.LogInfo("Loaded custom sync disk dds entries.");

                // Add the initial dds records
                const string dialogDds = "dds.blocks";
                foreach (var customDialog in Lib.Dialogs.RegisteredDialogs)
                {
                    // TextGetter are initialized when the conversation is first started dynamically
                    // Makes no sense to already initialize them now, they could reference things that don't exist yet.
                    if (customDialog.Text != null)
                        Lib.DdsStrings[dialogDds, customDialog.BlockId] = customDialog.Text;
                    foreach (var response in customDialog.Responses)
                        if (response.Text != null)
                            Lib.DdsStrings[dialogDds, response.BlockId] = response.Text;

                    foreach (var block in customDialog.Blocks)
                        Toolbox.Instance.allDDSBlocks.Add(block.id, block);

                    foreach (var message in customDialog.Messages)
                        Toolbox.Instance.allDDSMessages.Add(message.id, message);
                }

                if (Lib.Dialogs.RegisteredDialogs.Any())
                    Plugin.Log.LogInfo("Loaded custom dialog dds entries.");
            }
        }
    }
}
