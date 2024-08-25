using SOD.Common.BepInEx.Configuration;

namespace SOD.LifeAndLiving.Bindings.SyncDiskBindings
{
    public interface ISyncDiskBindings
    {
        [Binding(true, "Add's the npc chatter sync disk to the game.", "LifeAndLiving.SyncDisks.NpcChatter")]
        public bool IncludeNpcChatterSyncDisk { get; set; }

        [Binding("#00fbff", "The color for the npc chatter in hex.", "LifeAndLiving.SyncDisks.NpcChatterTextColor")]
        public string NpcChatterTextColor { get; set; }

        [Binding(1250, "The price of the NpcChatter sync disk.", "LifeAndLiving.SyncDisks.NpcChatterDiskPrice")]
        public int NpcChatterDiskPrice { get; set; }
    }
}
