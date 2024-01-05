using System;

namespace SOD.Common.Helpers.SyncDiskObjects
{
    public abstract class SyncDiskArgs : EventArgs
    {
        /// <summary>
        /// This is considered the change applied to the sync disk object that was installed/upgraded/uninstalled
        /// </summary>
        public UpgradesController.Upgrades SyncDiskChange { get; }

        /// <summary>
        /// The sync disk wrapper related to this event.
        /// </summary>
        public SyncDisk SyncDisk { get; }

        /// <summary>
        /// The effect of the sync disk related to this event.
        /// <br>This will be one of the 3 main effects.</br>
        /// </summary>
        public SyncDisks.Effect? Effect { get; }

        internal SyncDiskArgs(UpgradesController.Upgrades upgrades)
        {
            SyncDiskChange = upgrades;
            SyncDisk = SyncDisk.ConvertFrom(upgrades.preset);

            // Set the effect properly
            var option = (int)upgrades.state;
            if (option == 0)
            {
                Effect = null;
            }
            else
            {
                var realOptionArrayId = option - 1;
                if (SyncDisk.Effects.Length > realOptionArrayId)
                    Effect = SyncDisk.Effects[realOptionArrayId];
            }
        }
    }

    public sealed class SyncDiskInstallUpgradeArgs : SyncDiskArgs
    {
        /// <summary>
        /// The selected option that was installed / upgraded.
        /// </summary>
        public UpgradesController.SyncDiskState Option { get; }

        internal SyncDiskInstallUpgradeArgs(UpgradesController.Upgrades upgrades, int? option = null) 
            : base(upgrades)
        {
            Option = (UpgradesController.SyncDiskState)(option ?? (int)upgrades.state);
        }
    }
}
