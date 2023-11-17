using BepInEx;
using SOD.Common.BepInEx.Common;

namespace SyncDisksRebalanced
{
    [BepInPlugin(Identifier, Name, Version)]
    [BepInDependency(SOD.Common.Plugin.Identifier)]
    public class Plugin : PluginExt
    {
        private const string Identifier = "Venomaus.Plugins.SOD.SyncDisksRebalanced";
        private const string Name = "Sync Disks Rebalanced";
        private const string Version = "1.0.0";

        public Plugin()
        {
            // Init configuration
            Config.Add(Constants.Configuration.PluginEnabled, true);
        }

        public override void Load()
        {
            if (!Config.Get<bool>(Constants.Configuration.PluginEnabled))
            {
                Log.LogInfo($"Plugin is disabled in the configuration.");
                return;
            }

            Log.LogInfo($"Loaded Plugin {Identifier}");
        }
    }
}