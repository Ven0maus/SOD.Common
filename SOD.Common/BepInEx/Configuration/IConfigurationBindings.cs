namespace SOD.Common.BepInEx.Configuration
{
    public interface IConfigurationBindings
    {
        [Configuration("General.Enabled", true, "Should the plugin be enabled?")]
        bool Enabled { get; set; }
    }
}
