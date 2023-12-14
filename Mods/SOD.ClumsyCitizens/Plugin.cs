using BepInEx;
using SOD.Common.BepInEx;

namespace SOD.ClumsyCitizens
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(Common.Plugin.PLUGIN_GUID)]
    public class Plugin : PluginController<IPluginBindings>
    {
        public const string PLUGIN_GUID = "Venomaus.SOD.ClumsyCitizens";
        public const string PLUGIN_NAME = "ClumsyCitizens";
        public const string PLUGIN_VERSION = "1.0.0";
    }
}