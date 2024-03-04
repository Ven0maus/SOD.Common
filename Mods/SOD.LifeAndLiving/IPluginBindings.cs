using SOD.Common.BepInEx.Configuration;

namespace SOD.LifeAndLiving
{
    public interface IPluginBindings : IEconomyBindings
    { }

    public interface IEconomyBindings
    {
        [Binding(40, "The percentage increase of things such as guest pass cost, things within dialogs.", "LifeAndLiving.Economy.DialogCostPricePercentage")]
        public int DialogCostPricePercentage { get; set; }

        [Binding(87, "The percentage reduction of job resolve payouts.", "LifeAndLiving.Economy.PayoutReductionJobs")]
        public int PayoutReductionJobs { get; set; }

        [Binding(15, "The minimum amount a side job resolve question should pay (these are all added up to calculate the full side job price).", "LifeAndLiving.Economy.MinSideJob")]
        public int MinSideJobResolveQuestion { get; set; }

        [Binding(90, "The percentage reduction of murder case resolve payouts.", "LifeAndLiving.Economy.PayoutReductionMurders")]
        public int PayoutReductionMurders { get; set; }

        [Binding(true, "This drastically reduces lockpicks per room", "LifeAndLiving.Economy.ReduceLockPickSpawnPerRoom")]
        public bool ReduceLockPickSpawnPerRoom { get; set; }

        [Binding(true, "This drastically reduces lockpicks per address", "LifeAndLiving.Economy.ReduceLockPickSpawnPerAddress")]
        public bool ReduceLockPickSpawnPerAddress { get; set; }

        [Binding(true, "This drastically reduces diamonds spawned per address", "LifeAndLiving.Economy.ReduceDiamondSpawnPerAddress")]
        public bool ReduceDiamondSpawnPerAddress { get; set; }

        [Binding(5, "The amount of lockpicks the buyable kit should give", "LifeAndLiving.Economy.LockPickKitAmount")]
        public int LockPickKitAmount { get; set; }

        [Binding(true, "Should hair pins only spawn in apartment homes?", "LifeAndLiving.Economy.LimitHairPinToHomeOnly")]
        public bool LimitHairPinToHomeOnly { get; set; }

        [Binding(true, "Should paper clips only spawn in office buildings?", "LifeAndLiving.Economy.LimitPaperclipToOfficeOnly")]
        public bool LimitPaperclipToOfficeOnly { get; set; }

        [Binding(60, "The percentage increase of apartement cost based on the existing price", "LifeAndLiving.Economy.ApartementCostPercentage")]
        public int ApartementCostPercentage { get; set; }

        [Binding(3000, "The minimum $ cost of an apartement, if the percentage increase does not cover this minimum it will be adjusted.", "LifeAndLiving.Economy.MinimumApartementCost")]
        public int MinimumApartementCost { get; set; }

        [Binding(true, "Should diamonds only spawn in apartements.", "LifeAndLiving.Economy.SpawnDiamondsOnlyInApartements")]
        public bool SpawnDiamondsOnlyInApartements { get; set; }

        [Binding(40, "The percentage increase of apartement cost based on the existing price", "LifeAndLiving.Economy.FurniteCostPercentage")]
        public int FurniteCostPercentage { get; set; }

        [Binding(250, "The cost of a lower suite in a hotel per day.", "LifeAndLiving.Economy.CostLowerSuiteHotel")]
        public int CostLowerSuiteHotel { get; set; }

        [Binding(500, "The cost of a higher suite in a hotel per day.", "LifeAndLiving.Economy.CostHigherSuiteHotel")]
        public int CostHigherSuiteHotel { get; set; }

        [Binding(7, "The maximum $ of loose change in a small stack.", "LifeAndLiving.Economy.MaxM1Crows")]
        public int MaxM1Crows { get; set; }

        [Binding(14, "The maximum $ of loose change in a medium stack.", "LifeAndLiving.Economy.MaxM2Crows")]
        public int MaxM2Crows { get; set; }

        [Binding(18, "The maximum $ of loose change in a big stack.", "LifeAndLiving.Economy.MaxM3Crows")]
        public int MaxM3Crows { get; set; }

        [Binding(25, "The maximum $ of loose change in a huge stack.", "LifeAndLiving.Economy.MaxM4Crows")]
        public int MaxM4Crows { get; set; }

        [Binding(true, "Should loose change only spawn in apartements.", "LifeAndLiving.Economy.LimitLooseMoneyToApartementsOnly")]
        public bool LimitLooseMoneyToApartementsOnly { get; set; }

        [Binding(60, "The percentage the value should increase of items.", "LifeAndLiving.Economy.PercentageValueIncrease")]
        public int PercentageValueIncrease { get; set; }

        [Binding(32, "The minimum value of an item.", "LifeAndLiving.Economy.MinItemValue")]
        public int MinItemValue { get; set; }

        [Binding(25, "The percentage of item value that is taken for selling items to general stores (default game is 50%)", "LifeAndLiving.Economy.PercentageSalePriceGeneral")]
        public int PercentageSalePriceGeneral { get; set; }

        [Binding(50, "The percentage of item value that is taken for selling items to blackmarket (default game is 80%)", "LifeAndLiving.Economy.PercentageSalePriceBlackMarket")]
        public int PercentageSalePriceBlackMarket { get; set; }

        [Binding(85, "The max price an item can be sold for in general stores. (diamond excluded)", "LifeAndLiving.Economy.MaxSellPriceAllItemsGeneral")]
        public int MaxSellPriceAllItemsGeneral { get; set; }

        [Binding(225, "The max price an item can be sold for in blackmarket. (diamond excluded)", "LifeAndLiving.Economy.MaxSellPriceAllItemsBlackMarket")]
        public int MaxSellPriceAllItemsBlackMarket { get; set; }

        [Binding(400, "The minimum value of a diamond.", "LifeAndLiving.Economy.MinDiamondValue")]
        public int MinDiamondValue { get; set; }

        [Binding(1000, "The maximum value of a diamond.", "LifeAndLiving.Economy.MaxDiamondValue")]
        public int MaxDiamondValue { get; set; }

        [Binding(750, "The minimum value of a sync disk upgrade module.", "LifeAndLiving.Economy.MinSyncDiskUpgradeModuleValue")]
        public int MinSyncDiskUpgradeModuleValue { get; set; }

        [Binding(1000, "The maximum value of a sync disk upgrade module.", "LifeAndLiving.Economy.MaxSyncDiskUpgradeModuleValue")]
        public int MaxSyncDiskUpgradeModuleValue { get; set; }

        [Binding(true, "This severly reduces spawn rate of sync disk upgrade modules.", "LifeAndLiving.Economy.LimitSpawnrateSyncDiskUpgradeModules")]
        public bool LimitSpawnrateSyncDiskUpgradeModules { get; set; }

        [Binding(true, "This severly reduces spawn rate of sync disks.", "LifeAndLiving.Economy.LimitSpawnrateSyncDisks")]
        public bool LimitSpawnrateSyncDisks { get; set; }
    }
}
