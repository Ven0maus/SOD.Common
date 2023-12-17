using BepInEx;
using SOD.Common.BepInEx;
using System.Reflection;

namespace SOD.QoL
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(Common.Plugin.PLUGIN_GUID)]
    public class Plugin : PluginController<IPluginBindings>
    {
        public const string PLUGIN_GUID = "Venomaus.SOD.QoL";
        public const string PLUGIN_NAME = "QoL";
        public const string PLUGIN_VERSION = "1.0.3";

        public override void Load()
        {
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.LogInfo("Plugin is patched.");
        }
    }
}