using SOD.Common.Game.LibraryComponents;

namespace SOD.Common.Game
{
    /// <summary>
    /// Helper library which contains a collection of properties and methods to retrieve or manipulate game data
    /// </summary>
    public sealed class SodHelpersLibrary
    {
        internal SodHelpersLibrary() { }

        /// <summary>
        /// Helper properties and methods related to murders.
        /// </summary>
        public MurderComponent Murders => new();
        /// <summary>
        /// Helper properties and methods related to sync disks.
        /// </summary>
        public SyncDiskComponent SyncDisks = new();
    }
}
