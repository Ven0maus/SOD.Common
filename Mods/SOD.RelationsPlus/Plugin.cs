using BepInEx;
using SOD.Common;
using SOD.Common.BepInEx;
using SOD.RelationsPlus.Objects;
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

                // Decay each recorded citizen's know value automatically
                foreach (var citizen in RelationManager.Instance)
                {
                    // Take decay gates into account to not decay past these values once reached.
                    var currentDecayGate = GetCurrentDecayGate(citizen);

                    // Apply decay
                    citizen.Know = Mathf.Clamp(citizen.Know + Config.DecayKnowAmount, currentDecayGate, 1f);
                }
            }
            else
            {
                _lastDecayCheckMinute++;
            }
        }

        /// <summary>
        /// Returns the current decay gate of the given relation.
        /// </summary>
        /// <param name="citizenRelation"></param>
        /// <returns></returns>
        private float GetCurrentDecayGate(CitizenRelation citizenRelation)
        {
            // If we allow decay past gates, we are always at gate 0f
            if (Config.AllowDecayPastRelationGates)
                return 0f;

            var know = citizenRelation.Know;
            if (know >= Config.GateFour)
                return Config.GateFour;
            if (know >= Config.GateThree)
                return Config.GateThree;
            if (know >= Config.GateTwo)
                return Config.GateTwo;
            if (know >= Config.GateOne)
                return Config.GateOne;

            return 0f;
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
