using BepInEx;
using SOD.Common.BepInEx;

namespace SOD.Common
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : PluginController
    {
        public const string PLUGIN_GUID = "Venomaus.Plugins.SOD.Common";
        public const string PLUGIN_NAME = "SOD.Common";
        public const string PLUGIN_VERSION = "1.0.0";

        public override void OnConfigureBindings(out bool savebindings)
        {
            // Init universe lib
            UniverseLib.Universe.Init(0, null, null, new UniverseLib.Config.UniverseLibConfig()
            {
                Unhollowed_Modules_Folder = System.IO.Path.Combine(Paths.BepInExRootPath, "interop")
            });
            base.OnConfigureBindings(out savebindings);
        }
    }
}