using BepInEx;
using SOD.Common;
using SOD.Common.BepInEx;
using SOD.LifeAndLiving.Patches;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace SOD.LifeAndLiving
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(Common.Plugin.PLUGIN_GUID)]
    public class Plugin : PluginController<Plugin, IPluginBindings>
    {
        public const string PLUGIN_GUID = "Venomaus.SOD.LifeAndLiving";
        public const string PLUGIN_NAME = "LifeAndLiving";
        public const string PLUGIN_VERSION = "1.0.6";

        public override void Load()
        {
            Lib.SaveGame.OnBeforeLoad += SaveGame_OnBeforeLoad;
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

        private void SaveGame_OnBeforeDelete(object sender, Common.Helpers.SaveGameArgs e)
        {
            var hash = Lib.SaveGame.GetUniqueString(e.FilePath);
            var path = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"apartmentpricecache_{hash}.json");
            if (File.Exists(path))
            {
                File.Delete(path);
                Log.LogInfo("Deleted apartment price cache.");
            }
        }

        private void SaveGame_OnBeforeSave(object sender, Common.Helpers.SaveGameArgs e)
        {
            var cache = NewGameLocationPatches.NewGameLocation_GetPrice.ApartementPriceCache;
            if (cache.Count > 0)
            {
                var hash = Lib.SaveGame.GetUniqueString(e.FilePath);
                var path = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"apartmentpricecache_{hash}.json");
                var json = JsonSerializer.Serialize(cache, new JsonSerializerOptions { WriteIndented = false });
                File.WriteAllText(path, json);
                Log.LogInfo("Saved apartment price cache.");
            }
        }

        private void SaveGame_OnBeforeLoad(object sender, Common.Helpers.SaveGameArgs e)
        {
            var cache = NewGameLocationPatches.NewGameLocation_GetPrice.ApartementPriceCache;
            cache.Clear();

            var hash = Lib.SaveGame.GetUniqueString(e.FilePath);
            var path = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"apartmentpricecache_{hash}.json");
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var jsonContent = JsonSerializer.Deserialize<Dictionary<string, int>>(json);
                foreach (var value in jsonContent)
                    cache.Add(value.Key, value.Value);
                Log.LogInfo("Loaded apartment price cache.");
            }
        }
    }
}