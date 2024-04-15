using BepInEx;
using SOD.Common;
using SOD.Common.BepInEx;
using SOD.LifeAndLiving.Relations;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using SOD.LifeAndLiving.Patches.EconomyRebalancePatches;

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
            Lib.SaveGame.OnAfterLoad += SaveGame_OnAfterLoad;
            Lib.SaveGame.OnBeforeSave += SaveGame_OnBeforeSave;
            Lib.SaveGame.OnBeforeDelete += SaveGame_OnBeforeDelete;
            
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.LogInfo("Plugin is patched.");
        }

        public override void OnConfigureBindings()
        {
            base.OnConfigureBindings();
            UpdateConfigFileLayout();
        }

        private void SaveGame_OnAfterLoad(object sender, Common.Helpers.SaveGameArgs e)
        {
            var hash = Lib.SaveGame.GetUniqueString(e.FilePath);
            var relationFilePath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"citizenrelations_{hash}.json");
            if (File.Exists(relationFilePath))
            {
                RelationManager.Instance.Load(relationFilePath);
                Log.LogInfo("Loaded citizen relations information.");
            }
        }

        private void SaveGame_OnBeforeDelete(object sender, Common.Helpers.SaveGameArgs e)
        {
            var hash = Lib.SaveGame.GetUniqueString(e.FilePath);
            var apartmentFilePath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"apartmentpricecache_{hash}.json");
            if (File.Exists(apartmentFilePath))
            {
                File.Delete(apartmentFilePath);
                Log.LogInfo("Deleted apartment price cache.");
            }

            var relationFilePath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"citizenrelations_{hash}.json");
            if (File.Exists(relationFilePath))
            {
                File.Delete(relationFilePath);
                Log.LogInfo("Deleted citizen relations information.");
            }
        }

        private void SaveGame_OnBeforeSave(object sender, Common.Helpers.SaveGameArgs e)
        {
            var hash = Lib.SaveGame.GetUniqueString(e.FilePath);

            var cache = NewGameLocationPatches.NewGameLocation_GetPrice.ApartementPriceCache;
            if (cache.Count > 0)
            {
                var apartmentFilePath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"apartmentpricecache_{hash}.json");
                var json = JsonSerializer.Serialize(cache, new JsonSerializerOptions { WriteIndented = false });
                File.WriteAllText(apartmentFilePath, json);
                Log.LogInfo("Saved apartment price cache.");
            }

            var relationFilePath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"citizenrelations_{hash}.json");
            RelationManager.Instance.Save(relationFilePath);
            Log.LogInfo("Saved citizen relations information.");
        }

        private void SaveGame_OnBeforeLoad(object sender, Common.Helpers.SaveGameArgs e)
        {
            var cache = NewGameLocationPatches.NewGameLocation_GetPrice.ApartementPriceCache;
            cache.Clear();

            var hash = Lib.SaveGame.GetUniqueString(e.FilePath);
            var apartmentPath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"apartmentpricecache_{hash}.json");
            if (File.Exists(apartmentPath))
            {
                var json = File.ReadAllText(apartmentPath);
                var jsonContent = JsonSerializer.Deserialize<Dictionary<string, int>>(json);
                foreach (var value in jsonContent)
                    cache.Add(value.Key, value.Value);
                Log.LogInfo("Loaded apartment price cache.");
            }
        }
    }
}