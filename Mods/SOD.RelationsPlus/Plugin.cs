using BepInEx;
using SOD.Common;
using SOD.Common.BepInEx;
using System.Reflection;

namespace SOD.RelationsPlus
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(Common.Plugin.PLUGIN_GUID)]
    public class Plugin : PluginController<Plugin, IPluginBindings>
    {
        public const string PLUGIN_GUID = "Venomaus.SOD.RelationsPlus";
        public const string PLUGIN_NAME = "SOD.RelationsPlus";
        public const string PLUGIN_VERSION = "1.0.3";

        public override void Load()
        {
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.LogInfo("Plugin is patched.");

            // SaveGame Events
            Lib.SaveGame.OnBeforeNewGame += (sender, e) => RelationManager.Instance.Clear();
            Lib.SaveGame.OnBeforeLoad += (sender, e) => RelationManager.Instance.Load(e);
            Lib.SaveGame.OnBeforeSave += (sender, e) => RelationManager.Instance.Save(e);
            Lib.SaveGame.OnBeforeDelete += (sender, e) => RelationManager.Delete(e);

            // Time Events
            Lib.Time.OnMinuteChanged += RelationManager.Instance.Timed_DecayLogic;
        }

        public override void OnConfigureBindings()
        {
            base.OnConfigureBindings();

            // Make sure to always sync the config file layout if new config is introduced.
            UpdateConfigFileLayout();

            // Modify them back to original value if they are changed
            Config.LikeGateFive = 1f;
            Config.KnowGateFive = 1f;
        }
    }
}
