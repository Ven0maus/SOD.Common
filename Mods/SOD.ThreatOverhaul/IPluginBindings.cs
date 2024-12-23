using SOD.Common.BepInEx.Configuration;

namespace SOD.ThreatOverhaul
{
    public interface IPluginBindings : IThreatCalculationBindings
    {
        [Binding(false, "Should debug mode be enabled to show extra info logging?", "General.EnableDebugMode")]
        bool EnableDebugMode { get; set; }

        [Binding(true, "Should threat calculation overhaul be enabled?", "General.EnableThreatCalculationOverhaul")]
        bool EnableThreatCalculationOverhaul { get; set; }
    }

    public interface IThreatCalculationBindings
    {
        [Binding(true, "Should neighbors of the target be included in the threat calculation?", "ThreatCalculation.IncludeNeighbors")]
        bool IncludeNeighbors { get; set; }

        [Binding(true, "Should friends of the target be included in the threat calculation?", "ThreatCalculation.IncludeFriends")]
        bool IncludeFriends { get; set; }

        [Binding(true, "Should work colleagues of the target be included in the threat calculation?", "ThreatCalculation.IncludeColleagues")]
        bool IncludeColleagues { get; set; }

        [Binding(true, "Should police officers be included in the threat calculation?", "ThreatCalculation.IncludePoliceOfficers")]
        bool IncludePoliceOfficers { get; set; }

        [Binding(true, "Should citizens see the threat action happening before they are allowed to react?", "ThreatCalculation.IncludeSeeingThreat")]
        bool IncludeSeeingThreat { get; set; }
    }
}
