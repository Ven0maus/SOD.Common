using UnityEngine;

namespace SOD.Common.Helpers.SyncDiskObjects
{
    /// <summary>
    /// Provides a useful wrapper for sync disks in game.
    /// </summary>
    public sealed class SyncDisk
    {
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
        /// Registers the sync disk so that it can be used in-game.
        /// <br>Has optional registration options that allow you to define exactly how it can be used in game.</br>
        /// </summary>
        /// <param name="registrationOptions"></param>
        public void Register(RegistrationOptions registrationOptions = null)
        {
            if (RegisteredInGame) return;

            // No options: default options
            registrationOptions ??= new RegistrationOptions();

            // TODO: Configure options

            // Add to game so it can be used as sync disk
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

            // TODO: Set custom properties based on the builder properties
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
