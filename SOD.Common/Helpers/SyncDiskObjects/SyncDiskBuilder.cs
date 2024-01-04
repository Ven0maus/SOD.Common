namespace SOD.Common.Helpers.SyncDiskObjects
{
    public sealed class SyncDiskBuilder
    {
        internal string Name { get; private set; }

        // TODO: Provide methods and properties to apply to the Sync Disk wrapper preset.

        internal SyncDiskBuilder(string syncDiskName) 
        {
            Name = syncDiskName;
        }

        /// <summary>
        /// Creates a new Sync Disk based on the current builder configuration.
        /// </summary>
        /// <returns></returns>
        public SyncDisk Create()
        {
            return SyncDisk.ConvertFrom(this);
        }
    }
}
