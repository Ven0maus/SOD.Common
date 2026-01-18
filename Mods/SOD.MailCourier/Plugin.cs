using BepInEx;
using SOD.Common;
using SOD.Common.BepInEx;
using SOD.MailCourier.Core;
using System.Reflection;

namespace SOD.CourierJobs
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(Common.Plugin.PLUGIN_GUID)]
    public class Plugin : PluginController<Plugin, IPluginBindings>
    {
        public const string PLUGIN_GUID = "Venomaus.SOD.MailCourier";
        public const string PLUGIN_NAME = "MailCourier";
        public const string PLUGIN_VERSION = "1.0.0";

        public override void Load()
        {
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.LogInfo("Plugin is patched.");

            Lib.SaveGame.OnBeforeLoad += SaveGame_OnBeforeLoad;
            Lib.SaveGame.OnBeforeSave += SaveGame_OnBeforeSave;
        }

        private void SaveGame_OnBeforeLoad(object sender, Common.Helpers.SaveGameArgs e)
        {
            CourierJobGenerator.LoadFromFile(e);
        }

        private void SaveGame_OnBeforeSave(object sender, Common.Helpers.SaveGameArgs e)
        {
            CourierJobGenerator.SaveToFile(e);
        }
    }
}
