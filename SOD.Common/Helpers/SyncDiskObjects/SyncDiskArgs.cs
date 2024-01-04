using System;

namespace SOD.Common.Helpers.SyncDiskObjects
{
    public sealed class SyncDiskArgs : EventArgs
    {
        /// <summary>
        /// The sync disk related to this event.
        /// </summary>
        public SyncDisk SyncDisk { get; }

        internal SyncDiskArgs(SyncDiskPreset syncDiskPreset)
        {
            SyncDisk = SyncDisk.ConvertFrom(syncDiskPreset);
        }
    }
}
