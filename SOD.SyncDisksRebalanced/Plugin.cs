using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SOD.Common.BepInEx.Common;
using System.Reflection;

namespace SyncDisksRebalanced
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(SOD.Common.Plugin.PLUGIN_GUID)]
    public class Plugin : PluginExt
    {
        public const string PLUGIN_GUID = "Venomaus.Plugins.SOD.SyncDisksRebalanced";
        public const string PLUGIN_NAME = "Sync Disks Rebalanced";
        public const string PLUGIN_VERSION = "1.0.0";

        public new static ManualLogSource Log { get; private set; }

        private Harmony _harmony;

        public Plugin()
        {
            // Init configuration
            Config.Add(Constants.Configuration.PluginEnabled, true);
        }

        public override void Load()
        {
            Log = base.Log;

            if (!Config.Get<bool>(Constants.Configuration.PluginEnabled))
            {
                Log.LogInfo($"Plugin \"{PLUGIN_GUID}\" is disabled.");
                return;
            }

            Log.LogInfo($"Plugin \"{PLUGIN_GUID}\" is loaded.");

            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            Log.LogInfo($"Plugin \"{PLUGIN_GUID}\" is patched.");
        }

        public override bool Unload()
        {
            _harmony?.UnpatchSelf();
            return true;
        }
    }
}