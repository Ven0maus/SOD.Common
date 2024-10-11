using BepInEx;
using SOD.Common;
using SOD.Common.BepInEx;
using SOD.Narcotics.AddictionCore;
using SOD.Narcotics.Patches;
using System.IO;
using System.Reflection;

namespace SOD.Narcotics
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(Common.Plugin.PLUGIN_GUID)]
    public class Plugin : PluginController<Plugin, IPluginBindings>
    {
        public const string PLUGIN_GUID = "Venomaus.SOD.Narcotics";
        public const string PLUGIN_NAME = "Narcotics";
        public const string PLUGIN_VERSION = "1.0.0";

        public override void Load()
        {
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.LogInfo("Plugin is patched.");

            Lib.Time.OnHourChanged += Time_OnHourChanged;
            Lib.SaveGame.OnBeforeNewGame += SaveGame_OnBeforeNewGame;
            Lib.SaveGame.OnBeforeSave += SaveGame_OnBeforeSave;
            Lib.SaveGame.OnAfterLoad += SaveGame_OnAfterLoad;
            Lib.SaveGame.OnAfterDelete += SaveGame_OnAfterDelete;
            TakeOnePatches.OnItemConsumed += OnItemConsumed;
        }

        private void SaveGame_OnAfterDelete(object sender, Common.Helpers.SaveGameArgs e)
        {
            var seed = Lib.SaveGame.GetUniqueString(e.FilePath);
            var path = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"addictions_{seed}.json");

            if (File.Exists(path))
                File.Delete(path);

            Plugin.Log.LogInfo("Addictions save data has been removed.");
        }

        private void SaveGame_OnBeforeNewGame(object sender, System.EventArgs e)
        {
            AddictionManager.ClearExistingData();
        }

        private void SaveGame_OnAfterLoad(object sender, Common.Helpers.SaveGameArgs e)
        {
            AddictionManager.Load(e.FilePath);
        }

        private void SaveGame_OnBeforeSave(object sender, Common.Helpers.SaveGameArgs e)
        {
            AddictionManager.Save(e.FilePath);
        }

        private void Time_OnHourChanged(object sender, Common.Helpers.TimeChangedArgs e)
        {
            AddictionManager.NaturalRecovery();
        }

        private void OnItemConsumed(object sender, Interactable e)
        {
            if (e.preset == null || e.preset.retailItem == null) return;

            var addictionInfo = AddictionManager.GetAddictionTypeAndPotency(e);
            if (addictionInfo != null)
                AddictionManager.OnItemConsumed(addictionInfo.Value.addictionType, 1f, addictionInfo.Value.potency);
        }

        public override void OnConfigureBindings()
        {
            base.OnConfigureBindings();
            AddictionManager.InitConfigValues();
        }
    }
}