using BepInEx;
using SOD.Common;
using SOD.Common.BepInEx;
using SOD.QoL.Patches;
using System;
using System.Reflection;

namespace SOD.QoL
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(Common.Plugin.PLUGIN_GUID)]
    public class Plugin : PluginController<Plugin, IPluginBindings>
    {
        public const string PLUGIN_GUID = "Venomaus.SOD.QoL";
        public const string PLUGIN_NAME = "QoL";
        public const string PLUGIN_VERSION = "1.1.6";

        internal static Random Random { get; } = new Random(1337);

        public override void Load()
        {
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.LogInfo("Plugin is patched.");

            Lib.SaveGame.OnBeforeLoad += SaveGame_OnBeforeLoad;
            Lib.SaveGame.OnBeforeNewGame += SaveGame_OnBeforeNewGame;
            Lib.SaveGame.OnAfterNewGame += SaveGame_OnAfterNewGame;
            Lib.SaveGame.OnBeforeSave += SaveGame_OnBeforeSave;
            Lib.SaveGame.OnAfterDelete += SaveGame_OnAfterDelete;

            // Hook the event only in the case side jobs can expire
            if (Config.AutoExpireJobs)
                Lib.Time.OnMinuteChanged += Time_OnMinuteChanged;
        }

        private void SaveGame_OnAfterDelete(object sender, Common.Helpers.SaveGameArgs e)
        {
            SideJobPatches.DeleteSaveData(e.FilePath);
            LostAndFoundPatches.DeleteSaveData(e.FilePath);
        }

        private void SaveGame_OnBeforeSave(object sender, Common.Helpers.SaveGameArgs e)
        {
            SideJobPatches.SaveExpireTimes(e.FilePath);
            LostAndFoundPatches.SaveExpireTimes(e.FilePath);
        }

        private void SaveGame_OnBeforeNewGame(object sender, System.EventArgs e)
        {
            SideJobPatches.InitializeExpireTimes(null);
            LostAndFoundPatches.InitializeExpireTimes(null);
        }

        private void SaveGame_OnBeforeLoad(object sender, Common.Helpers.SaveGameArgs e)
        {
            SideJobPatches.InitializeExpireTimes(e.FilePath);
            LostAndFoundPatches.InitializeExpireTimes(e.FilePath);
        }

        private void Time_OnMinuteChanged(object sender, Common.Helpers.TimeChangedArgs e)
        {
            SideJobPatches.ExpireTimedOutJobs();
            LostAndFoundPatches.ExpireTimedOutLafs();
        }

        private void SaveGame_OnAfterNewGame(object sender, System.EventArgs e)
        {
            UnlockBusinessDoors();
        }

        private static void UnlockBusinessDoors()
        {
            int count = 0;
            foreach (Company company in CityData.Instance.companyDirectory)
            {
                if (company.placeOfBusiness.thisAsAddress != null && company.IsOpenAtThisTime(SessionData.Instance.gameTime))
                {
                    foreach (var nodeAccess in company.address.entrances)
                    {
                        if (nodeAccess.door != null)
                            nodeAccess.door.SetLocked(false, null, false);
                    }

                    foreach (NewRoom newRoom in company.address.rooms)
                        newRoom.SetMainLights(true, "SOD.QoL: Unlock business door on new game.", null, true, true);

                    company.SetOpen(true, true);
                    count++;
                }
            }
            Log.LogInfo($"Unlocked \"{count}\" business doors.");
        }
    }
}