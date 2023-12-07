using BepInEx;
using SOD.Common.BepInEx;

namespace SOD.LightsOutTruce
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(Common.Plugin.PLUGIN_GUID)]
    public class Plugin : PluginController
    {
        public const string PLUGIN_GUID = "Venomaus.SOD.LightsOutTruce";
        public const string PLUGIN_NAME = "LightsOutTruce";
        public const string PLUGIN_VERSION = "1.0.0";
    }
}