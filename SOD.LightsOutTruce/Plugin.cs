using BepInEx;
using SOD.Plugins.Common.BepInEx;

namespace SOD.Plugins.LightsOutTruce
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(Common.Plugin.PLUGIN_GUID)]
    public class Plugin : PluginExt
    {
        public const string PLUGIN_GUID = "Venomaus.Plugins.SOD.LightsOutTruce";
        public const string PLUGIN_NAME = "LightsOutTruce";
        public const string PLUGIN_VERSION = "1.0.0";

        protected override string Plugin_GUID => PLUGIN_GUID;
    }
}