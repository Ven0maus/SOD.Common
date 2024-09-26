using BepInEx;
using SOD.Common;
using SOD.Common.BepInEx;
using SOD.LifeAndLiving.Content.SocialRelation;
using SOD.LifeAndLiving.Content.SyncDisks;
using SOD.LifeAndLiving.Patches.EconomyRebalancePatches;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SOD.LifeAndLiving
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(Common.Plugin.PLUGIN_GUID)]
    [BepInDependency(RelationsPlus.Plugin.PLUGIN_GUID)]
    public class Plugin : PluginController<Plugin, IPluginBindings>
    {
        public const string PLUGIN_GUID = "Venomaus.SOD.LifeAndLiving";
        public const string PLUGIN_NAME = "LifeAndLiving";
        public const string PLUGIN_VERSION = "3.0.0";

        public readonly Random Random = new();

        public override void Load()
        {
            Lib.SaveGame.OnBeforeLoad += SaveGame_OnBeforeLoad;
            Lib.SaveGame.OnAfterLoad += SaveGame_OnAfterLoad;
            Lib.SaveGame.OnBeforeSave += SaveGame_OnBeforeSave;
            Lib.SaveGame.OnBeforeDelete += SaveGame_OnBeforeDelete;

            // Initialize submodules
            CivilianDialogAdditions.Initialize();
            Echolocation.Initialize();

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

            // Player interests deletion
            PlayerInterests.Instance.Delete(hash);
        }

        private void SaveGame_OnBeforeSave(object sender, Common.Helpers.SaveGameArgs e)
        {
            var hash = Lib.SaveGame.GetUniqueString(e.FilePath);

            // Apartment price cache
            NewGameLocationPatches.NewGameLocation_GetPrice.Save(hash);

            // Player interests saving
            PlayerInterests.Instance.Save(hash);
        }

        private void SaveGame_OnBeforeLoad(object sender, Common.Helpers.SaveGameArgs e)
        {
            var hash = Lib.SaveGame.GetUniqueString(e.FilePath);

            // Apartment price cache
            NewGameLocationPatches.NewGameLocation_GetPrice.Load(hash);

            // Player interests loading
            PlayerInterests.Instance.Load(hash);
        }

        private void SaveGame_OnAfterLoad(object sender, Common.Helpers.SaveGameArgs e)
        {
            // Update company prices that haven't been calculated properly yet from pre-mod savegames
            Random random = null;
            var list = new List<KeyValuePair<InteractablePreset, int>>();
            foreach (var company in CityData.Instance.companyDirectory)
            {
                foreach (var kvp in company.prices)
                    list.Add(new KeyValuePair<InteractablePreset, int>(kvp.Key, kvp.Value));

                foreach (var preset in list)
                {
                    var realValue = preset.Key.value;
                    if (preset.Value < realValue.x || preset.Value > realValue.y)
                    {
                        random ??= new Random((int)Lib.SaveGame.GetUniqueNumber(CityData.Instance.seed));
                        company.prices[preset.Key] = random.Next((int)realValue.x, (int)realValue.y + 1);
                    }
                }

                list.Clear();
            }
        }
    }
}