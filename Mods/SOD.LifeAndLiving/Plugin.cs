using BepInEx;
using SOD.Common;
using SOD.Common.BepInEx;
using SOD.LifeAndLiving.Patches.EconomyRebalancePatches;
using SOD.LifeAndLiving.Relations;
using System;
using System.Collections.Generic;
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

        public readonly Random Random = new();

        public override void Load()
        {
            Lib.SaveGame.OnBeforeLoad += SaveGame_OnBeforeLoad;
            Lib.SaveGame.OnAfterLoad += SaveGame_OnAfterLoad;
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
            var random = new Random(CityData.Instance.seed.GetHashCode());
            foreach (var company in CityData.Instance.companyDirectory)
            {
                var arr = new List<KeyValuePair<InteractablePreset, int>>(company.prices.Keys.Count);
                foreach (var kvp in company.prices)
                    arr.Add(new KeyValuePair<InteractablePreset, int>(kvp.Key, kvp.Value));

                foreach (var preset in arr)
                {
                    var realValue = preset.Key.value;
                    if (preset.Value < realValue.x || preset.Value > realValue.y)
                        company.prices[preset.Key] = random.Next((int)realValue.x, (int)realValue.y + 1);
                }
            }
        }
    }
}