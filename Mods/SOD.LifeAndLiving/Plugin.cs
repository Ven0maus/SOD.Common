using BepInEx;
using SOD.Common;
using SOD.Common.BepInEx;
using SOD.LifeAndLiving.Patches.EconomyRebalancePatches;
using SOD.LifeAndLiving.Patches.SocialRelationPatches;
using SOD.LifeAndLiving.Relations;
using System.Reflection;

namespace SOD.LifeAndLiving
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(Common.Plugin.PLUGIN_GUID)]
    public class Plugin : PluginController<Plugin, IPluginBindings>
    {
        public const string PLUGIN_GUID = "Venomaus.SOD.LifeAndLiving";
        public const string PLUGIN_NAME = "LifeAndLiving";
        public const string PLUGIN_VERSION = "2.0.0";

        public override void Load()
        {
            Lib.SaveGame.OnBeforeLoad += SaveGame_OnBeforeLoad;
            Lib.SaveGame.OnBeforeSave += SaveGame_OnBeforeSave;
            Lib.SaveGame.OnBeforeDelete += SaveGame_OnBeforeDelete;

            // Add new dialog between player and civilians
            CivilianDialogAdditions.Initialize();

            Harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.LogInfo("Plugin is patched.");
        }

        public override void OnConfigureBindings()
        {
            base.OnConfigureBindings();
            UpdateConfigFileLayout();
        }

        private void SaveGame_OnBeforeDelete(object sender, Common.Helpers.SaveGameArgs e)
        {
            var hash = Lib.SaveGame.GetUniqueString(e.FilePath);

            // Apartment price cache
            NewGameLocationPatches.NewGameLocation_GetPrice.Delete(hash);

            // Relations
            RelationManager.Instance.Delete(hash);
        }

        private void SaveGame_OnBeforeSave(object sender, Common.Helpers.SaveGameArgs e)
        {
            var hash = Lib.SaveGame.GetUniqueString(e.FilePath);

            // Apartment price cache
            NewGameLocationPatches.NewGameLocation_GetPrice.Save(hash);

            // Relations
            RelationManager.Instance.Save(hash);
        }

        private void SaveGame_OnBeforeLoad(object sender, Common.Helpers.SaveGameArgs e)
        {
            var hash = Lib.SaveGame.GetUniqueString(e.FilePath);

            // Apartment price cache
            NewGameLocationPatches.NewGameLocation_GetPrice.Load(hash);

            // Relations
            RelationManager.Instance.Load(hash);
        }
    }
}