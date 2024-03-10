using BepInEx;
using SOD.Common.BepInEx;
using System.Reflection;

namespace SOD.LethalActionReborn
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(Common.Plugin.PLUGIN_GUID)]
    public class Plugin : PluginController<Plugin>
    {
        public const string PLUGIN_GUID = "Venomaus.SOD.LethalActionReborn";
        public const string PLUGIN_NAME = "LethalActionReborn";
        public const string PLUGIN_VERSION = "1.0.2";

        public override void Load()
        {
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.LogInfo("Plugin is patched.");
        }
    }
}