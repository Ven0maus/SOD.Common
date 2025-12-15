using SOD.Common.BepInEx.Configuration;

namespace SOD.CourierJobs
{
    public interface IPluginBindings
    {
        [Binding(false, "Should debug mode be enabled to show extra info logging?", "General.EnableDebugMode")]
        bool EnableDebugMode { get; set; }
    }
}
