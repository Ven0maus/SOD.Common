using BepInEx;
using SOD.Common;
using SOD.Common.BepInEx;
using System.Reflection;
using UnityEngine;

namespace SOD.RelationsPlus
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(Common.Plugin.PLUGIN_GUID)]
    public class Plugin : PluginController<Plugin, IPluginBindings>
    {
        public const string PLUGIN_GUID = "Venomaus.SOD.RelationsPlus";
        public const string PLUGIN_NAME = "RelationsPlus";
        public const string PLUGIN_VERSION = "1.0.0";

        private int _lastDecayCheckMinute;

        public override void Load()
        {
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.LogInfo("Plugin is patched.");

            // SaveGame Events
            Lib.SaveGame.OnBeforeNewGame += SaveGame_OnBeforeNewGame;
            Lib.SaveGame.OnBeforeLoad += SaveGame_OnBeforeLoad;
            Lib.SaveGame.OnBeforeSave += SaveGame_OnBeforeSave;
            Lib.SaveGame.OnBeforeDelete += SaveGame_OnBeforeDelete;

            // Time Events
            Lib.Time.OnMinuteChanged += Timed_DecayLogic;
        }

        /// <summary>
        /// Handles the automatic decay logic.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timed_DecayLogic(object sender, Common.Helpers.TimeChangedArgs e)
        {
            if (_lastDecayCheckMinute >= Config.DecayTimeMinutesCheck)
            {
                _lastDecayCheckMinute = 0;

                // Decay each known citizen's know
                foreach (var citizen in RelationManager.Instance)
                {
                    // TODO: Take decay gates into account to not decay past these values once reached.

                    // Add decay value
                    citizen.Know += Config.DecayKnowAmount;

                    // Make sure to clamp the values
                    citizen.Know = Mathf.Clamp01(citizen.Know);
                }
            }
            else
            {
                _lastDecayCheckMinute++;
            }
        }

        private void SaveGame_OnBeforeNewGame(object sender, System.EventArgs e)
        {
            RelationManager.Instance.Clear();
        }

        private void SaveGame_OnBeforeDelete(object sender, Common.Helpers.SaveGameArgs e)
        {
            var hash = Lib.SaveGame.GetUniqueString(e.FilePath);

            // Relations
            RelationManager.Delete(hash);
        }

        private void SaveGame_OnBeforeSave(object sender, Common.Helpers.SaveGameArgs e)
        {
            var hash = Lib.SaveGame.GetUniqueString(e.FilePath);

            // Relations
            RelationManager.Instance.Save(hash);
        }

        private void SaveGame_OnBeforeLoad(object sender, Common.Helpers.SaveGameArgs e)
        {
            var hash = Lib.SaveGame.GetUniqueString(e.FilePath);

            // Relations
            RelationManager.Instance.Load(hash);
        }
    }
}
