using SOD.Common.BepInEx.Configuration;

namespace SOD.CourierJobs
{
    public interface IPluginBindings
    {
        [Binding(50, "Initial mail delivery cost. (refunded on completion)", "General.InitialCost")]
        int InitialCost { get; set; }

        [Binding(75, "The minimum reward for a mail delivery.", "General.BaseReward")]
        int BaseReward { get; set; }

        [Binding(0.75f, "The percentage of the distance to travel as a bonus on top of the base reward.", "General.DistanceBonusPercentage")]
        float DistanceBonusPercentage { get; set; }

        [Binding(false, "Should the target mailbox be highlighted?", "Optional.HighlightMailbox")]
        bool HighlightMailbox { get; set; }

        [Binding(false, "If you already have a waypoint, should it be overwritten when accepting a new courier job?", "Optional.OverwriteExistingWaypoint")]
        bool OverwriteExistingWaypoint { get; set; }
    }
}
