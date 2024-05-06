using SOD.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SOD.Common.Helpers.SyncDiskObjects
{
    /// <summary>
    /// Provides a useful wrapper for sync disks in game.
    /// </summary>
    public sealed class SyncDisk
    {
        /// <summary>
        /// Lazy loaded interactable preset for the sync disk
        /// </summary>
        internal static Lazy<InteractablePreset> SyncDiskInteractablePreset = new(() =>
        {
            var syncDisk = Resources.FindObjectsOfTypeAll<InteractablePreset>()
               .Where(preset => preset.presetName == "SyncDisk")
               .LastOrDefault();
#pragma warning disable IDE0029 // Use coalesce expression
            return syncDisk == null
                ? throw new Exception("Could not find sync disk interactable, did something change in this game version?")
                : syncDisk;
#pragma warning restore IDE0029 // Use coalesce expression
        });

        private SyncDisk(bool createPresetInstance = true)
        {
            // Assign a new sync disk preset
            if (createPresetInstance)
                Preset = ScriptableObject.CreateInstance<SyncDiskPreset>();
        }

        /// <summary>
        /// True if the sync disk is available in game.
        /// </summary>
        public bool RegisteredInGame
        {
            get
            {
                if (Lib.SyncDisks.RegisteredSyncDisks.Contains(this)) return true;
                if (Toolbox.Instance == null) return false;
                if (Toolbox.Instance.allSyncDisks == null) return false;
                if (Preset == null) return false;
                return Toolbox.Instance.allSyncDisks.Contains(Preset);
            }
        }

        /// <summary>
        /// The preset tied to this sync disk.
        /// </summary>
        public SyncDiskPreset Preset { get; private set; }

        /// <summary>
        /// A unique hash based on the plugin guid and disk name
        /// </summary>
        internal string Hash { get; private set; }

        /// <summary>
        /// Used to re raise events on save load
        /// </summary>
        internal bool ReRaiseEventsOnSaveLoad { get; private set; }

        /// <summary>
        /// The sync disk's name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The sync disk's number.
        /// </summary>
        public int Number => Preset.syncDiskNumber;

        /// <summary>
        /// The sync disk's price.
        /// </summary>
        public int Price => Preset.price;

        /// <summary>
        /// Determines if the sync disk can spawn in the world generation.
        /// </summary>
        public bool EnableWorldSpawn { get; private set; } = true;

        /// <summary>
        /// The sync disk's rarity.
        /// </summary>
        public SyncDiskPreset.Rarity Rarity => Preset.rarity;

        /// <summary>
        /// The sync disk's manufacturer.
        /// </summary>
        public SyncDiskPreset.Manufacturer Manufacturer => Preset.manufacturer;

        /// <summary>
        /// The effects in the correct order (effect 1, 2, 3)
        /// <br>Note if this sync disk has only one effect, the array will be also of length 1</br>
        /// </summary>
        public SyncDisks.Effect[] Effects => new[]
        {
            new SyncDisks.Effect((int)Preset.mainEffect1, Preset.mainEffect1Name),
            new SyncDisks.Effect((int)Preset.mainEffect2, Preset.mainEffect2Name),
            new SyncDisks.Effect((int)Preset.mainEffect3, Preset.mainEffect3Name)
        }
        .Where(a => a.Id != 0)
        .ToArray();

        private SyncDisks.Effect? _sideEffect;
        /// <summary>
        /// The unique side effect of the sync disk if there is one.
        /// </summary>
        public SyncDisks.Effect? SideEffect
        {
            get
            {
                if (_sideEffect == null && Preset.sideEffect != 0)
                    _sideEffect ??= new SyncDisks.Effect((int)Preset.sideEffect, Preset.sideEffectDescription);
                return _sideEffect;
            }
        }

        /// <summary>
        /// The upgrade options in the correct order (effect 1, 2, 3)
        /// <br>Note if this sync disk has only one effect, the array will be also of length 1</br>
        /// </summary>
        public SyncDisks.UpgradeOption[] UpgradeOptions => new[]
        {
            new SyncDisks.UpgradeOption(new SyncDiskBuilder.Options(Preset, 1)),
            new SyncDisks.UpgradeOption(new SyncDiskBuilder.Options(Preset, 2)),
            new SyncDisks.UpgradeOption(new SyncDiskBuilder.Options(Preset, 3))
        }
        .Where(a => a.HasOptions)
        .ToArray();

        internal HashSet<string> MenuPresetLocations { get; set; }
        internal string[] Icons;

        /// <summary>
        /// Registers the sync disk so that it can be used in-game.
        /// <br>Has optional registration options that allow you to define exactly how it can be used in game.</br>
        /// </summary>
        /// <param name="registrationOptions"></param>
        public void Register()
        {
            if (RegisteredInGame) return;

            // Add to game so it can be used as sync disk, set also sync disk number to the latest
            Lib.SyncDisks.RegisteredSyncDisks.Add(this);
        }

        /// <summary>
        /// A unique code that identifies custom sync disks.
        /// </summary>
        internal static int UniqueDiskIdentifier = Plugin.PLUGIN_GUID.GetFnvHashCode();

        /// <summary>
        /// Converts a dds record to its actual name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static string GetName(string name)
        {
            if (name == null) return null;
            if (name.StartsWith($"{UniqueDiskIdentifier}_"))
            {
                var data = name.Split('_');
                var identifier = $"{data[0]}_{data[1]}_";
                return name[identifier.Length..];
            }
            return name;
        }

        /// <summary>
        /// Converts a sync disk builder instance into a sync disk object.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        internal static SyncDisk ConvertFrom(SyncDiskBuilder builder)
        {
            // Set basic properties
            var syncDisk = new SyncDisk
            {
                Name = builder.Name,
                Hash = Lib.SaveGame.GetUniqueString($"{builder.PluginGuid}_{builder.Name}"),
                ReRaiseEventsOnSaveLoad = builder.ReRaiseEventsOnSaveLoad,
                EnableWorldSpawn = builder.EnableWorldSpawn
            };

            syncDisk.Preset.name = $"{UniqueDiskIdentifier}_{syncDisk.Hash}_{syncDisk.ReRaiseEventsOnSaveLoad}_{builder.Name}";
            syncDisk.Preset.presetName = syncDisk.Preset.name;
            syncDisk.Preset.price = builder.Price;
            syncDisk.Preset.rarity = builder.Rarity;
            syncDisk.Preset.manufacturer = builder.Manufacturer;
            syncDisk.Preset.canBeSideJobReward = builder.CanBeSideJobReward;
            if (builder.OccupationPresets != null && builder.OccupationPresets.Length > 0)
            {
                syncDisk.Preset.occupation = new Il2CppSystem.Collections.Generic.List<OccupationPreset>();
                builder.OccupationPresets.ForEach(syncDisk.Preset.occupation.Add);
                syncDisk.Preset.occupationWeight = builder.OccupationWeight;
            }
            if (builder.Traits != null && builder.Traits.Length > 0)
            {
                syncDisk.Preset.traits = new Il2CppSystem.Collections.Generic.List<SyncDiskPreset.TraitPick>();
                builder.Traits.ForEach(syncDisk.Preset.traits.Add);
                syncDisk.Preset.traitWeight = builder.TraitWeight;
            }
            syncDisk.MenuPresetLocations = builder.MenuPresetLocations;

            // Add effects
            syncDisk.Icons = new string[builder.Effects.Count];
            for (int i = 0; i < builder.Effects.Count; i++)
            {
                var effect = builder.Effects[i];
                syncDisk.Icons[i] = effect.Icon;

                if (i == 0)
                {
                    syncDisk.Preset.mainEffect1 = effect.EffectValue;
                    syncDisk.Preset.mainEffect1Name = $"{UniqueDiskIdentifier}_{syncDisk.Hash}_{effect.Name}";
                    syncDisk.Preset.mainEffect1Description = $"{UniqueDiskIdentifier}_{syncDisk.Hash}_{effect.Description}";
                }
                else if (i == 1)
                {
                    syncDisk.Preset.mainEffect2 = effect.EffectValue;
                    syncDisk.Preset.mainEffect2Name = $"{UniqueDiskIdentifier}_{syncDisk.Hash}_{effect.Name}";
                    syncDisk.Preset.mainEffect2Description = $"{UniqueDiskIdentifier}_{syncDisk.Hash}_{effect.Description}";
                }
                else
                {
                    syncDisk.Preset.mainEffect3 = effect.EffectValue;
                    syncDisk.Preset.mainEffect3Name = $"{UniqueDiskIdentifier}_{syncDisk.Hash}_{effect.Name}";
                    syncDisk.Preset.mainEffect3Description = $"{UniqueDiskIdentifier}_{syncDisk.Hash}_{effect.Description}";
                }
            }

            // Add side effect
            if (builder.SideEffect != null)
            {
                syncDisk.Preset.sideEffect = builder.SideEffect.EffectValue;
                syncDisk.Preset.sideEffectDescription = $"{UniqueDiskIdentifier}_{syncDisk.Hash}_{builder.SideEffect.Description}";
                syncDisk._sideEffect = new SyncDisks.Effect((int)builder.SideEffect.EffectValue, syncDisk.Preset.sideEffectDescription);
            }

            // Add upgrade options
            for (int i = 0; i < builder.UpgradeOptions.Count; i++)
            {
                var options = builder.UpgradeOptions[i];
                var nameReferences = i == 0 ? syncDisk.Preset.option1UpgradeNameReferences : i == 1 ? syncDisk.Preset.option2UpgradeNameReferences : syncDisk.Preset.option3UpgradeNameReferences;
                var upgradeEffects = i == 0 ? syncDisk.Preset.option1UpgradeEffects : i == 1 ? syncDisk.Preset.option2UpgradeEffects : syncDisk.Preset.option3UpgradeEffects;
                var upgradeValues = i == 0 ? syncDisk.Preset.option1UpgradeValues : i == 1 ? syncDisk.Preset.option2UpgradeValues : syncDisk.Preset.option3UpgradeValues;

                if (!string.IsNullOrWhiteSpace(options.Option1))
                {
                    nameReferences.Add($"{UniqueDiskIdentifier}_{syncDisk.Hash}_{options.Option1}");
                    upgradeEffects.Add(options.Option1Effect);
                    upgradeValues.Add(0f);
                }
                if (!string.IsNullOrWhiteSpace(options.Option2))
                {
                    nameReferences.Add($"{UniqueDiskIdentifier}_{syncDisk.Hash}_{options.Option2}");
                    upgradeEffects.Add(options.Option2Effect);
                    upgradeValues.Add(0f);
                }
                if (!string.IsNullOrWhiteSpace(options.Option3))
                {
                    nameReferences.Add($"{UniqueDiskIdentifier}_{syncDisk.Hash}_{options.Option3}");
                    upgradeEffects.Add(options.Option3Effect);
                    upgradeValues.Add(0f);
                }
            }

            return syncDisk;
        }

        /// <summary>
        /// Converts an existing preset into a sync disk object.
        /// </summary>
        /// <param name="preset"></param>
        /// <returns></returns>
        internal static SyncDisk ConvertFrom(SyncDiskPreset preset)
        {
            if (!preset.name.StartsWith($"{UniqueDiskIdentifier}_"))
                return new SyncDisk(false) { Name = preset.name, Preset = preset };

            var split = preset.name.Split('_');
            var hash = split[1];
            var reRaiseEvents = bool.Parse(split[2]);
            var identifier = $"{split[0]}_{hash}_{reRaiseEvents}_";
            var name = preset.name[identifier.Length..];

            return new SyncDisk(false) { Name = name, Preset = preset, Hash = hash, ReRaiseEventsOnSaveLoad = reRaiseEvents };
        }

        /// <summary>
        /// Retrieves the name from the preset name of a sync disk.
        /// </summary>
        /// <param name="presetName"></param>
        /// <returns></returns>
        internal static string GetNameFromPreset(string presetName)
        {
            if (presetName == null || !presetName.StartsWith($"{UniqueDiskIdentifier}_"))
                return presetName;

            var split = presetName.Split('_');
            var identifier = $"{split[0]}_{split[1]}_{split[2]}_";
            var name = presetName[identifier.Length..];

            return name;
        }
    }
}
