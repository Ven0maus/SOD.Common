using SOD.LifeAndLiving.Bindings;
using SOD.LifeAndLiving.Bindings.EconomicBindings;
using SOD.LifeAndLiving.Bindings.ImRequirementBindings;

namespace SOD.LifeAndLiving
{
    public interface IPluginBindings : ISideJobBindings, IMurderCaseBindings, ISpawnRateBindings, 
        IHousingBindings, IDialogBindings, IMoneyBindings, IShopsBindings, IItemPriceBindings,
        Bindings.RelationBindings.IDialogBindings, ImRequirementBindings
    { }
}
