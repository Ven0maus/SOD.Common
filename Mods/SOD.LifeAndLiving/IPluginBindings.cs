using SOD.Common.BepInEx.Configuration;
using SOD.LifeAndLiving.Bindings.EconomicBindings;
using SOD.LifeAndLiving.Bindings.ImRequirementBindings;
using SOD.LifeAndLiving.Bindings.SyncDiskBindings;

namespace SOD.LifeAndLiving
{
    public interface IPluginBindings : ISideJobBindings, IMurderCaseBindings, ISpawnRateBindings, 
        IHousingBindings, IDialogBindings, IMoneyBindings, IShopsBindings, IItemPriceBindings,
        Bindings.RelationBindings.IDialogBindings, ImRequirementBindings, ISyncDiskBindings
    {
        [Binding(false, "Should all features of Economy Rebalance be disabled?", "General.DisableEconomyRebalance")]
        public bool DisableEconomyRebalance { get; set; }

        [Binding(false, "Should all features of Social Relations be disabled?", "General.DisableSocialRelations")]
        public bool DisableSocialRelations { get; set; }

        [Binding(false, "Should all features of Immersive Requirements be disabled?", "General.DisableImmersiveRequirements")]
        public bool DisableImmersiveRequirements { get; set; }

        [Binding(false, "Should all features of Extra Sync Disks be disabled?", "General.DisableExtraSyncDisks")]
        public bool DisableExtraSyncDisks { get; set; }
    }
}
