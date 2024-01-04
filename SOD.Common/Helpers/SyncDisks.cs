using SOD.Common.Helpers.SyncDiskObjects;
using System;

namespace SOD.Common.Helpers
{
    public sealed class SyncDisks
    {
        internal SyncDisks() { }

        /// <summary>
        /// Raised when a new sync disk is installed on the player.
        /// </summary>
        public event EventHandler<SyncDiskArgs> OnSyncDiskInstalled;
        /// <summary>
        /// Raised when an existing sync disk is upgraded on the player.
        /// </summary>
        public event EventHandler<SyncDiskArgs> OnSyncDiskUpgraded;
        /// <summary>
        /// Raised when an existing sync disk is removed on the player.
        /// </summary>
        public event EventHandler<SyncDiskArgs> OnSyncDiskRemoved;

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
    }
}
