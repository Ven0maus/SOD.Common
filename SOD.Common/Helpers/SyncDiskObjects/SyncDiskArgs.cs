using System;

namespace SOD.Common.Helpers.SyncDiskObjects
{
    public abstract class SyncDiskArgs : EventArgs
    {
        /// <summary>
        /// This is considered the game's sync disk object that was installed/upgraded/uninstalled,
        /// <br>This contains all the properties, it is prefered to use <see cref="SyncDisk"/> property instead as it is more streamlined.</br>
        /// </summary>
        public UpgradesController.Upgrades GameSyncDisk { get; }

        /// <summary>
        /// The sync disk wrapper related to this event.
        /// </summary>
        public SyncDisk SyncDisk { get; }

        internal SyncDiskArgs(UpgradesController.Upgrades upgrades)
        {
            GameSyncDisk = upgrades;
            SyncDisk = SyncDisk.ConvertFrom(upgrades.preset);
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
