using SOD.Common.BepInEx.Configuration;

namespace SOD.ZeroOverhead
{
    public interface IPluginBindings
    {
        [Binding(false, "Enables debug mode, logging a lot more detailed information to the console.", "General.EnableDebugMode")]
        bool EnableDebugMode { get; set; }
    }
}
