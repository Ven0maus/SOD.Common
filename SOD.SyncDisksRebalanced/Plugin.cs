using BepInEx;
using SOD.Common.BepInEx.Common;

namespace SyncDisksRebalanced
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(SOD.Common.Plugin.PLUGIN_GUID)]
    public class Plugin : PluginExt
    {
        public const string PLUGIN_GUID = "Venomaus.Plugins.SOD.SyncDisksRebalanced";
        public const string PLUGIN_NAME = "Sync Disks Rebalanced";
        public const string PLUGIN_VERSION = "1.0.0";

        public Plugin()
        {
            // Init configuration
            Config.Add(Constants.Configuration.PluginEnabled, true);
        }

        public override void Load()
        {
            if (!Config.Get<bool>(Constants.Configuration.PluginEnabled))
            {
                Log.LogInfo($"Plugin \"{PLUGIN_GUID}\" is disabled in the configuration.");
                return;
            }

            Log.LogInfo($"Loaded Plugin \"{PLUGIN_GUID}\"");
        }
    }
}