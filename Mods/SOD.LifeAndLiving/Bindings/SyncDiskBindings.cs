using SOD.Common.BepInEx.Configuration;

namespace SOD.LifeAndLiving.Bindings.SyncDiskBindings
{
    public interface ISyncDiskBindings
    {
        [Binding(true, "Add's the Echolocation sync disk to the game.", "LifeAndLiving.SyncDisks.IncludeEcholocationSyncDisk")]
        public bool IncludeEcholocationSyncDisk { get; set; }

        [Binding("#00fbff", "The color for the Echolocation in hex.", "LifeAndLiving.SyncDisks.EcholocationTextColor")]
        public string EcholocationTextColor { get; set; }

        [Binding(1250, "The price of the Echolocation sync disk.", "LifeAndLiving.SyncDisks.EcholocationDiskPrice")]
        public int EcholocationDiskPrice { get; set; }
    }
}
