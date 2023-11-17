using BepInEx;
using SOD.Common.BepInEx.Common;

namespace SOD.Common
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : PluginExt
    {
        public const string PLUGIN_GUID = "Venomaus.Plugins.SOD.Common";
        public const string PLUGIN_NAME = "SOD.Common";
        public const string PLUGIN_VERSION = "1.0.0";

        public override void Load()
        {
            Log.LogInfo($"Loaded Core Plugin \"{PLUGIN_GUID}\"");
        }
    }
}