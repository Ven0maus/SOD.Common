using SOD.Common.BepInEx.Configuration;

namespace SOD.Common
{
    public interface IPluginBindings
    {
        [Binding(false, "Enables extra logging for development.")]
        bool DebugMode { get; set; }
    }
}
