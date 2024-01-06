﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
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
        /// The sync disk's name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The sync disk's price.
        /// </summary>
        public int Price => Preset.price;

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
        .Select(a => new SyncDisks.Effect(a.Id, a.Name["custom_".Length..]))
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
                    _sideEffect ??= new SyncDisks.Effect((int)Preset.sideEffect, Preset.sideEffectDescription["custom_".Length..]);
                return _sideEffect;
            }
        }

        internal HashSet<string> MenuPresetLocations { get; set; }

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
        /// Converts a sync disk builder instance into a sync disk object.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        internal static SyncDisk ConvertFrom(SyncDiskBuilder builder)
        {
            // Set basic properties
            var syncDisk = new SyncDisk();
            syncDisk.Preset.name = $"custom_{builder.Name}";
            syncDisk.Preset.presetName = syncDisk.Preset.name;
            syncDisk.Name = builder.Name;
            syncDisk.Preset.price = builder.Price;
            syncDisk.Preset.rarity = builder.Rarity;
            syncDisk.Preset.manufacturer = builder.Manufacturer;
            syncDisk.Preset.canBeSideJobReward = builder.CanBeSideJobReward;
            syncDisk.MenuPresetLocations = builder.MenuPresetLocations;

            for (int i=0; i < builder.Effects.Count; i++)
            {
                var effect = builder.Effects[i];
                if (i == 0)
                {
                    syncDisk.Preset.mainEffect1 = effect.EffectValue;
                    syncDisk.Preset.mainEffect1Name = $"custom_{effect.Name}";
                    syncDisk.Preset.mainEffect1Description = $"custom_{effect.Description}";
                }
                else if (i == 1)
                {
                    syncDisk.Preset.mainEffect2 = effect.EffectValue;
                    syncDisk.Preset.mainEffect2Name = $"custom_{effect.Name}";
                    syncDisk.Preset.mainEffect2Description = $"custom_{effect.Description}";
                }
                else
                {
                    syncDisk.Preset.mainEffect3 = effect.EffectValue;
                    syncDisk.Preset.mainEffect3Name = $"custom_{effect.Name}";
                    syncDisk.Preset.mainEffect3Description = $"custom_{effect.Description}";
                }
            }

            if (builder.SideEffect != null)
            {
                syncDisk.Preset.sideEffect = builder.SideEffect.EffectValue;
                syncDisk.Preset.sideEffectDescription = $"custom_{builder.SideEffect.Description}";
                syncDisk._sideEffect = new SyncDisks.Effect((int)builder.SideEffect.EffectValue, builder.SideEffect.Description);
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
            return new SyncDisk(false) { Name = preset.name, Preset = preset };
        }
    }
}
