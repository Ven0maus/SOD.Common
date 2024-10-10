using BepInEx;
using SOD.Common;
using SOD.Common.BepInEx;
using SOD.Narcotics.AddictionCore;
using SOD.Narcotics.Patches;
using System.Reflection;

namespace SOD.Narcotics
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(Common.Plugin.PLUGIN_GUID)]
    public class Plugin : PluginController<Plugin, IPluginBindings>
    {
        public const string PLUGIN_GUID = "Venomaus.SOD.Narcotics";
        public const string PLUGIN_NAME = "Narcotics";
        public const string PLUGIN_VERSION = "1.0.0";

        public override void Load()
        {
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.LogInfo("Plugin is patched.");

            Lib.Time.OnHourChanged += Time_OnHourChanged;
            TakeOnePatches.FirstPersonItemController_TakeOne.OnItemConsumed += OnItemConsumed;
        }

        private void Time_OnHourChanged(object sender, Common.Helpers.TimeChangedArgs e)
        {
            AddictionManager.NaturalRecovery();
        }

        private void OnItemConsumed(object sender, Interactable e)
        {
            if (e.preset == null || e.preset.retailItem == null) return;
            var ri = e.preset.retailItem;

            if (ri.drunk > 0)
            {
                AddictionManager.OnItemConsumed(Player.Instance.humanID, AddictionType.Alcohol, ri.drunk);
            }
            else if (ri.numb > 0 || ri.desireCategory == CompanyPreset.CompanyCategory.medical)
            {
                if (e.preset.name != "Bandage" && e.preset.name != "Splint" && e.preset.name != "HeatPack")
                    AddictionManager.OnItemConsumed(Player.Instance.humanID, AddictionType.Opioid);
            }
            else if (ri.nourishment > 0)
            {
                if (e.preset.name == "ChocolateBar" || e.preset.name == "Donut" || e.preset.name == "Eclair")
                    AddictionManager.OnItemConsumed(Player.Instance.humanID, AddictionType.Sugar);
            }
            else if (ri.desireCategory == CompanyPreset.CompanyCategory.caffeine)
            {
                if (e.preset.name == "MugCoffee" || e.preset.name == "TakeawayCoffee")
                    AddictionManager.OnItemConsumed(Player.Instance.humanID, AddictionType.Caffeine);
            }
            
            // TODO: Add nicotine
        }

        public override void OnConfigureBindings()
        {
            base.OnConfigureBindings();
            AddictionManager.InitConfigValues();
        }
    }
}