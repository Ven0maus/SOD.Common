using BepInEx;
using SOD.Common;
using SOD.Common.BepInEx;
using SOD.Common.Extensions;
using SOD.LethalActionReborn.Patches;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SOD.LethalActionReborn
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(Common.Plugin.PLUGIN_GUID)]
    public class Plugin : PluginController<Plugin, IPluginBindings>
    {
        public const string PLUGIN_GUID = "Venomaus.SOD.LethalActionReborn";
        public const string PLUGIN_NAME = "LethalActionReborn";
        public const string PLUGIN_VERSION = "1.0.4";

        public override void Load()
        {
            Lib.SaveGame.OnAfterLoad += SaveGame_OnAfterLoad;
            Lib.SaveGame.OnAfterSave += SaveGame_OnAfterSave;
            Lib.SaveGame.OnAfterDelete += SaveGame_OnAfterDelete;

            Harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.LogInfo("Plugin is patched.");
        }

        public override void OnConfigureBindings()
        {
            base.OnConfigureBindings();

            // Define the excluded weapon types
            CitizenPatches.Citizen_RecieveDamage.ExcludedWeaponTypes = Config.WeaponTypesExcludedFromKilling
                .Split(',')
                .Select(a =>
                {
                    if (Enum.TryParse(typeof(MurderWeaponPreset.WeaponType), a, true, out var result))
                        return (MurderWeaponPreset.WeaponType?)result;
                    return null;
                })
                .Where(a => a != null)
                .Select(a => a.Value)
                .ToHashSet();
        }

        private void SaveGame_OnAfterDelete(object sender, Common.Helpers.SaveGameArgs e)
        {
            var hash = Lib.SaveGame.GetUniqueString(e.FilePath);
            var path = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"lethalActioned_{hash}.txt");
            if (File.Exists(path))
                File.Delete(path);
        }

        private void SaveGame_OnAfterSave(object sender, Common.Helpers.SaveGameArgs e)
        {
            var hash = Lib.SaveGame.GetUniqueString(e.FilePath);
            var path = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"lethalActioned_{hash}.txt");
            File.WriteAllText(path, string.Join(",", CitizenPatches.Citizen_RecieveDamage.KilledCitizens));
        }

        private void SaveGame_OnAfterLoad(object sender, Common.Helpers.SaveGameArgs e)
        {
            CitizenPatches.Citizen_RecieveDamage.CitizenHitsTakenOnKo.Clear();

            var hash = Lib.SaveGame.GetUniqueString(e.FilePath);
            var path = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"lethalActioned_{hash}.txt");
            if (!File.Exists(path)) return;

            var ids = File.ReadAllText(path)
                .Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries)
                .Select(a => int.Parse(a))
                .ToHashSet();

            var deadCitizens = CityData.Instance.deadCitizensDirectory
                .ToList() //il2cpp -> enumerable
                .Select(a => a.humanID)
                .ToHashSet();

            foreach (var citizenId in ids)
            {
                CitizenPatches.Citizen_RecieveDamage.KilledCitizens.Add(citizenId);

                if (CityData.Instance.citizenDictionary.TryGetValue(citizenId, out var citizen))
                {
                    // Kill them again, because savegame doesn't do it properly??
                    citizen.SetHealth(0f);
                    citizen.animationController.cit.Murder(Player.Instance, false, null, null);

                    // Add to dead citizens
                    if (!deadCitizens.Contains(citizenId))
                    {
                        CityData.Instance.deadCitizensDirectory.Add(citizen);
                        deadCitizens.Add(citizenId);
                    }

                    int check = 0;
                    while (MurderController.Instance.currentMurderer != null && MurderController.Instance.currentMurderer.humanID == citizenId)
                    {
                        if (check >= 500) break;
                        check++;
                        MurderController.Instance.currentMurderer = null;
                        MurderController.Instance.PickNewMurderer();
                    }

                    check = 0;
                    while (MurderController.Instance.currentVictim != null && MurderController.Instance.currentVictim.humanID == citizenId)
                    {
                        if (check >= 500) break;
                        check++;
                        MurderController.Instance.currentVictim = null;
                        MurderController.Instance.PickNewVictim();
                    }
                }
            }
        }
    }
}