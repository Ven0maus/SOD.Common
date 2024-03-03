using SOD.Common.BepInEx.Configuration;

namespace SOD.LifeAndLiving
{
    public interface IPluginBindings : IJobBindings, IMurderBindings, ILockPickBindings, IApartementBindings, IMoneyBindings, IShopBindings
    { }

    public interface IJobBindings
    {
        [Binding(80, "The percentage reduction of job resolve payouts.", "LifeAndLiving.Jobs.PayoutReductionJobs")]
        public int PayoutReductionJobs { get; set; }

        [Binding(10, "The minimum amount a side job resolve question should pay (these are all added up to calculate the full side job price).", "LifeAndLiving.Jobs.MinSideJob")]
        public int MinSideJobResolveQuestion { get; set; }
    }

    public interface IMurderBindings
    {
        [Binding(90, "The percentage reduction of murder case resolve payouts.", "LifeAndLiving.Murders.PayoutReductionMurders")]
        public int PayoutReductionMurders { get; set; }
    }

    public interface ILockPickBindings
    {
        [Binding(true, "This drastically reduces lockpicks per room", "LifeAndLiving.Lockpicks.ReduceLockPickSpawnPerRoom")]
        public bool ReduceLockPickSpawnPerRoom { get; set; }

        [Binding(true, "This drastically reduces lockpicks per address", "LifeAndLiving.Lockpicks.ReduceLockPickSpawnPerAddress")]
        public bool ReduceLockPickSpawnPerAddress { get; set; }

        [Binding(5, "The amount of lockpicks the buyable kit should give", "LifeAndLiving.Lockpicks.LockPickKitAmount")]
        public int LockPickKitAmount { get; set; }

        [Binding(true, "Should hair pins only spawn in apartment homes?", "LifeAndLiving.Lockpicks.LimitHairPinToHomeOnly")]
        public bool LimitHairPinToHomeOnly { get; set; }

        [Binding(true, "Should paper clips only spawn in office buildings?", "LifeAndLiving.Lockpicks.LimitPaperclipToOfficeOnly")]
        public bool LimitPaperclipToOfficeOnly { get; set; }
    }

    public interface IApartementBindings
    {
        [Binding(60, "The percentage increase of apartement cost based on the existing price", "LifeAndLiving.Apartements.ApartementCostPercentage")]
        public int ApartementCostPercentage { get; set; }

        [Binding(3000, "The minimum $ cost of an apartement, if the percentage increase does not cover this minimum it will be adjusted.", "LifeAndLiving.Apartements.MinimumApartementCost")]
        public int MinimumApartementCost { get; set; }

        [Binding(true, "Should diamonds only spawn in apartements.", "LifeAndLiving.Apartements.SpawnDiamondsOnlyInApartements")]
        public bool SpawnDiamondsOnlyInApartements { get; set; }
    }

    public interface IMoneyBindings
    {
        [Binding(7, "The maximum $ of loose change in a small stack.", "LifeAndLiving.Money.MaxM1Crows")]
        public int MaxM1Crows { get; set; }

        [Binding(14, "The maximum $ of loose change in a medium stack.", "LifeAndLiving.Money.MaxM2Crows")]
        public int MaxM2Crows { get; set; }

        [Binding(18, "The maximum $ of loose change in a big stack.", "LifeAndLiving.Money.MaxM3Crows")]
        public int MaxM3Crows { get; set; }

        [Binding(25, "The maximum $ of loose change in a huge stack.", "LifeAndLiving.Money.MaxM4Crows")]
        public int MaxM4Crows { get; set; }

        [Binding(true, "Should loose change only spawn in apartements.", "LifeAndLiving.Money.LimitLooseMoneyToApartementsOnly")]
        public bool LimitLooseMoneyToApartementsOnly { get; set; }
    }

    public interface IShopBindings
    {
        [Binding(40, "The percentage the value should increase of items.", "LifeAndLiving.Shop.PercentageValueIncrease")]
        public int PercentageValueIncrease { get; set; }

        [Binding(32, "The minimum value of an item.", "LifeAndLiving.Shop.MinItemValue")]
        public int MinItemValue { get; set; }

        [Binding(25, "The percentage of item value that is taken for selling items to general stores (default game is 50%)", "LifeAndLiving.Shop.PercentageSalePriceGeneral")]
        public int PercentageSalePriceGeneral { get; set; }

        [Binding(50, "The percentage of item value that is taken for selling items to blackmarket (default game is 80%)", "LifeAndLiving.Shop.PercentageSalePriceBlackMarket")]
        public int PercentageSalePriceBlackMarket { get; set; }

        [Binding(400, "The minimum value of a diamond.", "LifeAndLiving.Shop.MinDiamondValue")]
        public int MinDiamondValue { get; set; }

        [Binding(1000, "The maximum value of a diamond.", "LifeAndLiving.Shop.MaxDiamondValue")]
        public int MaxDiamondValue { get; set; }
    }
}
