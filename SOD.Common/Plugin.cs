using BepInEx;
using SOD.Common.BepInEx;
using SOD.Common.ConfigBindings;
using System.Reflection;

namespace SOD.Common
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : PluginController<Plugin, IPluginBindings>
    {
        public const string PLUGIN_GUID = "Venomaus.SOD.Common";
        public const string PLUGIN_NAME = "SOD.Common";
        public const string PLUGIN_VERSION = "2.0.9";

        public override void Load()
        {
            // Init universe lib
            UniverseLib.Universe.Init(0, null, null, new UniverseLib.Config.UniverseLibConfig()
            {
                Unhollowed_Modules_Folder = System.IO.Path.Combine(Paths.BepInExRootPath, "interop")
            });

            // Apply patches
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.LogInfo("Plugin is patched.");
        }
    }
}