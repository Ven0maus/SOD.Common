using BepInEx;
using SOD.Plugins.Common.BepInEx;

namespace SOD.Plugins.Common
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : PluginExt
    {
        public const string PLUGIN_GUID = "Venomaus.Plugins.SOD.Common";
        public const string PLUGIN_NAME = "SOD.Common";
        public const string PLUGIN_VERSION = "1.0.0";

        protected override string Plugin_GUID => PLUGIN_GUID;

        public override void Configuration()
        {
            // Init universe lib
            UniverseLib.Universe.Init(0, null, null, new UniverseLib.Config.UniverseLibConfig()
            {
                Unhollowed_Modules_Folder = System.IO.Path.Combine(Paths.BepInExRootPath, "interop")
            });
        }
    }
}