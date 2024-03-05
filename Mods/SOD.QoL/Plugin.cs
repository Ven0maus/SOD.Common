using BepInEx;
using SOD.Common;
using SOD.Common.BepInEx;
using System.Reflection;

namespace SOD.QoL
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(Common.Plugin.PLUGIN_GUID)]
    public class Plugin : PluginController<Plugin, IPluginBindings>
    {
        public const string PLUGIN_GUID = "Venomaus.SOD.QoL";
        public const string PLUGIN_NAME = "QoL";
        public const string PLUGIN_VERSION = "1.1.0";

        public override void Load()
        {
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.LogInfo("Plugin is patched.");

            Lib.SaveGame.OnAfterNewGame += SaveGame_OnAfterNewGame;
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
                    if (company.placeOfBusiness.streetAccess != null && company.placeOfBusiness.streetAccess.door != null)
                        company.placeOfBusiness.streetAccess.door.SetLocked(false, null, false);

                    foreach (NewRoom newRoom in company.address.rooms)
                        newRoom.SetMainLights(true, "SOD.QoL: Unlock business door on new game.", null, true, true);
                    count++;
                }
            }
            Log.LogInfo($"Unlocked \"{count}\" business doors.");
        }
    }
}