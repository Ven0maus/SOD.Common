using System;
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
            return Resources.FindObjectsOfTypeAll<InteractablePreset>()
               .Where(preset => preset.presetName == "SyncDisk")
               .LastOrDefault();
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
        public bool RegisteredInGame { get { return Toolbox.Instance.allSyncDisks.Contains(Preset); } }

        /// <summary>
        /// The preset tied to this sync disk.
        /// </summary>
        public SyncDiskPreset Preset { get; private set; }

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

        internal RegistrationOptions RegistrationOptions { get; private set; }

        /// <summary>
        /// Registers the sync disk so that it can be used in-game.
        /// <br>Has optional registration options that allow you to define exactly how it can be used in game.</br>
        /// </summary>
        /// <param name="registrationOptions"></param>
        public void Register(RegistrationOptions registrationOptions = null)
        {
            if (RegisteredInGame) return;

            // Set and store registration options
            RegistrationOptions = registrationOptions ?? new RegistrationOptions();

            // Configure options
            Preset.canBeSideJobReward = RegistrationOptions.CanBeSideJobReward;

            // Add to game so it can be used as sync disk, set also sync disk number to the latest
            SyncDisks.RegisteredSyncDisks.Add(this);
            Preset.syncDiskNumber = Toolbox.Instance.allSyncDisks.Count + 1;
            Toolbox.Instance.allSyncDisks.Add(Preset);
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
            syncDisk.Preset.name = $"syncdiskpreset_{builder.Name}";
            syncDisk.Preset.presetName = builder.Name;
            syncDisk.Preset.price = builder.Price;
            syncDisk.Preset.rarity = builder.Rarity;
            syncDisk.Preset.manufacturer = builder.Manufacturer;
            syncDisk.Preset.interactable = SyncDiskInteractablePreset.Value;

            for (int i=0; i < builder.Effects.Count; i++)
            {
                var effect = builder.Effects[i];
                if (i == 0)
                {
                    syncDisk.Preset.mainEffect1 = effect.EffectValue;
                    syncDisk.Preset.mainEffect1Name = effect.Name;
                    syncDisk.Preset.mainEffect1Description = effect.Description;
                }
                else if (i == 1)
                {
                    syncDisk.Preset.mainEffect2 = effect.EffectValue;
                    syncDisk.Preset.mainEffect2Name = effect.Name;
                    syncDisk.Preset.mainEffect2Description = effect.Description;
                }
                else
                {
                    syncDisk.Preset.mainEffect3 = effect.EffectValue;
                    syncDisk.Preset.mainEffect3Name = effect.Name;
                    syncDisk.Preset.mainEffect3Description = effect.Description;
                }
            }

            if (builder.SideEffect != null)
            {
                syncDisk.Preset.sideEffect = builder.SideEffect.EffectValue;
                syncDisk.Preset.sideEffectDescription = builder.SideEffect.Description;
                syncDisk._sideEffect = new SyncDisks.Effect((int)builder.SideEffect.EffectValue, builder.SideEffect.Name);
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
            return new SyncDisk(false) { Preset = preset };
        }
    }
}
