using SOD.LifeAndLiving.Bindings.EconomicBindings;

namespace SOD.LifeAndLiving
{
    public interface IPluginBindings : ISideJobBindings, IMurderCaseBindings, ISpawnRateBindings, 
        IHousingBindings, IDialogBindings, IMoneyBindings, IShopsBindings, IItemPriceBindings,
        Bindings.RelationBindings.IDialogBindings
    { }
}
