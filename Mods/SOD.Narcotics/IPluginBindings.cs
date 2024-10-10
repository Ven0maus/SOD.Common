using SOD.Common.BepInEx.Configuration;

namespace SOD.Narcotics
{
    public interface IPluginBindings
    {
        [Binding(false, "Enables debug mode, which adds extra logging and other utilities for the mod.")]
        bool DebugMode { get; set; }
    }
}
