using SOD.Common.Helpers.SyncDiskObjects;
using System;

namespace SOD.Common.Helpers
{
    public sealed class SyncDisks
    {
        internal SyncDisks() { }

        /// <summary>
        /// Raised before a new sync disk is installed on the player.
        /// </summary>
        public event EventHandler<SyncDiskInstallUpgradeArgs> OnBeforeSyncDiskInstalled;
        /// <summary>
        /// Raised after a new sync disk is installed on the player.
        /// </summary>
        public event EventHandler<SyncDiskInstallUpgradeArgs> OnAfterSyncDiskInstalled;
        /// <summary>
        /// Raised before an existing sync disk is upgraded on the player.
        /// </summary>
        public event EventHandler<SyncDiskInstallUpgradeArgs> OnBeforeSyncDiskUpgraded;
        /// <summary>
        /// Raised after an existing sync disk is upgraded on the player.
        /// </summary>
        public event EventHandler<SyncDiskInstallUpgradeArgs> OnAfterSyncDiskUpgraded;
        /// <summary>
        /// Raised before an existing sync disk is uninstalled on the player.
        /// </summary>
        public event EventHandler<SyncDiskArgs> OnBeforeSyncDiskUninstalled;
        /// <summary>
        /// Raised after an existing sync disk is uninstalled on the player.
        /// </summary>
        public event EventHandler<SyncDiskArgs> OnAfterSyncDiskUninstalled;

        /// <summary>
        /// Creates a builder object that can help you build a custom sync disk.
        /// <br>When finished setting up your properties on the builder, call builder.Create();</br>
        /// <br>This will return a SyncDisk object which you can call .Register() on the register it into the game.</br>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public SyncDiskBuilder Builder(string syncDiskName)
        {
            return new SyncDiskBuilder(syncDiskName);
        }

        internal void RaiseSyncDiskEvent(SyncDiskEvent syncDiskEvent, bool after, UpgradesController.Upgrades upgrade, int? option = null)
        {
            switch (syncDiskEvent)
            {
                case SyncDiskEvent.OnInstall:
                    if (after)
                        OnAfterSyncDiskInstalled?.Invoke(this, new SyncDiskInstallUpgradeArgs(upgrade, option));
                    else
                        OnBeforeSyncDiskInstalled?.Invoke(this, new SyncDiskInstallUpgradeArgs(upgrade, option));
                    break;
                case SyncDiskEvent.OnUninstall:
                    if (after)
                        OnAfterSyncDiskUninstalled?.Invoke(this, new SyncDiskInstallUpgradeArgs(upgrade));
                    else
                        OnBeforeSyncDiskUninstalled?.Invoke(this, new SyncDiskInstallUpgradeArgs(upgrade));
                    break;
                case SyncDiskEvent.OnUpgrade:
                    if (after)
                        OnAfterSyncDiskUpgraded?.Invoke(this, new SyncDiskInstallUpgradeArgs(upgrade));
                    else
                        OnBeforeSyncDiskUpgraded?.Invoke(this, new SyncDiskInstallUpgradeArgs(upgrade));
                    break;
                default:
                    throw new NotSupportedException($"Invalid event: {syncDiskEvent}");
            }
        }

        internal enum SyncDiskEvent
        {
            OnInstall,
            OnUninstall,
            OnUpgrade
        }
    }
}
