using BepInEx;
using SOD.Common.BepInEx;
using System.Reflection;

namespace SOD.CourierJobs
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(Common.Plugin.PLUGIN_GUID)]
    public class Plugin : PluginController<Plugin, IPluginBindings>
    {
        public const string PLUGIN_GUID = "Venomaus.SOD.CourierJobs";
        public const string PLUGIN_NAME = "CourierJobs";
        public const string PLUGIN_VERSION = "1.0.0";

        public override void Load()
        {
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.LogInfo("Plugin is patched.");
        }
    }
}
