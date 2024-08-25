using SOD.LifeAndLiving.Bindings.EconomicBindings;
using SOD.LifeAndLiving.Bindings.ImRequirementBindings;
using SOD.LifeAndLiving.Bindings.SyncDiskBindings;

namespace SOD.LifeAndLiving
{
    public interface IPluginBindings : ISideJobBindings, IMurderCaseBindings, ISpawnRateBindings, 
        IHousingBindings, IDialogBindings, IMoneyBindings, IShopsBindings, IItemPriceBindings,
        Bindings.RelationBindings.IDialogBindings, ImRequirementBindings, ISyncDiskBindings
    { }
}
