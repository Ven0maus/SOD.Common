using SOD.Common.BepInEx.Configuration;

namespace SOD.LifeAndLiving.Bindings.EconomicBindings
{
    public interface ISideJobBindings
    {
        [Binding(150, "The minimum amount a side job should reward.", "LifeAndLiving.SideJobs.MinSideJobReward")]
        public int MinSideJobReward { get; set; }

        [Binding(75, "The percentage reduction of job resolve payouts.", "LifeAndLiving.SideJobs.PayoutReductionJobs")]
        public int PayoutReductionJobs { get; set; }

        [Binding(-150, "The percentage reduction of lost items payouts.", "LifeAndLiving.SideJobs.PayoutReductionLostItems")]
        public int PayoutReductionLostItems { get; set; }
    }

    public interface IMurderCaseBindings
    {
        [Binding(80, "The percentage reduction of murder case resolve payouts.", "LifeAndLiving.MurderCases.PayoutReductionMurders")]
        public int PayoutReductionMurders { get; set; }

        [Binding(50, "The minimum a resolve question of a murder case should payout.", "LifeAndLiving.MurderCases.MinimumMurderResolveQuestionPayout")]
        public int MinimumMurderResolveQuestionPayout { get; set; }
    }

    public interface ISpawnRateBindings
    {
        [Binding(true, "This drastically reduces lockpicks per room", "LifeAndLiving.SpawnRate.ReduceLockPickSpawnPerRoom")]
        public bool ReduceLockPickSpawnPerRoom { get; set; }

        [Binding(true, "This drastically reduces lockpicks per address", "LifeAndLiving.SpawnRate.ReduceLockPickSpawnPerAddress")]
        public bool ReduceLockPickSpawnPerAddress { get; set; }

        [Binding(true, "This drastically reduces diamonds spawned per address", "LifeAndLiving.SpawnRate.ReduceDiamondSpawnPerAddress")]
        public bool ReduceDiamondSpawnPerAddress { get; set; }

        [Binding(true, "Should diamonds only spawn in apartements.", "LifeAndLiving.SpawnRate.SpawnDiamondsOnlyInApartements")]
        public bool SpawnDiamondsOnlyInApartements { get; set; }

        [Binding(true, "Should hair pins only spawn in apartment homes?", "LifeAndLiving.SpawnRate.LimitHairPinToHomeOnly")]
        public bool LimitHairPinToHomeOnly { get; set; }

        [Binding(true, "Should paper clips only spawn in office buildings?", "LifeAndLiving.SpawnRate.LimitPaperclipToOfficeOnly")]
        public bool LimitPaperclipToOfficeOnly { get; set; }

        [Binding(true, "Should loose change only spawn in apartements.", "LifeAndLiving.SpawnRate.LimitLooseMoneyToApartementsOnly")]
        public bool LimitLooseMoneyToApartementsOnly { get; set; }

        [Binding(true, "This drastically reduces sync disk upgrade modules spawns.", "LifeAndLiving.SpawnRate.LimitSpawnrateSyncDiskUpgradeModules")]
        public bool LimitSpawnrateSyncDiskUpgradeModules { get; set; }

        [Binding(true, "This drastically reduces sync disk spawns.", "LifeAndLiving.SpawnRate.LimitSpawnrateSyncDisks")]
        public bool LimitSpawnrateSyncDisks { get; set; }
    }

    public interface IHousingBindings
    {
        [Binding(50, "The percentage increase of apartement cost based on the existing price", "LifeAndLiving.Housing.ApartementCostPercentage")]
        public int ApartementCostPercentage { get; set; }

        [Binding(5000, "The minimum $ cost of an apartement, if the percentage increase does not cover this minimum it will be adjusted.", "LifeAndLiving.Housing.MinimumApartementCost")]
        public int MinimumApartementCost { get; set; }

        [Binding(40, "The percentage increase of furniture cost based on the existing price", "LifeAndLiving.Housing.FurniteCostPercentage")]
        public int FurniteCostPercentage { get; set; }

        [Binding(250, "The cost of a lower suite in a hotel per day.", "LifeAndLiving.Housing.CostLowerSuiteHotel")]
        public int CostLowerSuiteHotel { get; set; }

        [Binding(500, "The cost of a higher suite in a hotel per day.", "LifeAndLiving.Housing.CostHigherSuiteHotel")]
        public int CostHigherSuiteHotel { get; set; }
    }

    public interface IDialogBindings
    {
        [Binding(80, "The percentage increase of things such as guest pass cost, things within dialogs.", "LifeAndLiving.Dialog.DialogCostPricePercentage")]
        public int DialogCostPricePercentage { get; set; }
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
    }

    public interface IShopsBindings
    {
        [Binding(5, "The amount of lockpicks the buyable kit should give", "LifeAndLiving.Shops.LockPickKitAmount")]
        public int LockPickKitAmount { get; set; }

        [Binding(15, "The percentage of item value that is taken for selling items to general stores (default game is 50%)", "LifeAndLiving.Shops.PercentageSalePriceGeneral")]
        public int PercentageSalePriceGeneral { get; set; }

        [Binding(40, "The percentage of item value that is taken for selling items to blackmarket (default game is 80%)", "LifeAndLiving.Shops.PercentageSalePriceBlackMarket")]
        public int PercentageSalePriceBlackMarket { get; set; }

        [Binding(70, "The max price an item can be sold for in general stores. (diamond excluded)", "LifeAndLiving.Shops.MaxSellPriceAllItemsGeneral")]
        public int MaxSellPriceAllItemsGeneral { get; set; }

        [Binding(200, "The max price an item can be sold for in blackmarket. (diamond excluded)", "LifeAndLiving.Shops.MaxSellPriceAllItemsBlackMarket")]
        public int MaxSellPriceAllItemsBlackMarket { get; set; }

        [Binding(false, "Should the max selling price clamp be disabled? Max price defined by \"MaxSellPriceAllItemsGeneral\" and \"MaxSellPriceAllItemsBlackMarket\" (diamond excluded)", "LifeAndLiving.Shops.DisableSellPriceClamping")]
        public bool DisableSellPriceClamping { get; set; }
    }

    public interface IItemPriceBindings
    {
        [Binding(65, "The percentage the value should increase of general items that isn't food.", "LifeAndLiving.ItemPrice.PercentageValueIncreaseGeneral")]
        public int PercentageValueIncreaseGeneral { get; set; }

        [Binding(25, "The percentage the value should increase of edible food items.", "LifeAndLiving.ItemPrice.PercentageValueIncreaseFood")]
        public int PercentageValueIncreaseFood { get; set; }

        [Binding(25, "The minimum value of a general item that isn't food.", "LifeAndLiving.ItemPrice.MinGeneralItemValue")]
        public int MinGeneralItemValue { get; set; }

        [Binding(10, "The minimum value of an edible food item.", "LifeAndLiving.ItemPrice.MinFoodItemValue")]
        public int MinFoodItemValue { get; set; }

        [Binding(300, "The minimum value of a diamond.", "LifeAndLiving.ItemPrice.MinDiamondValue")]
        public int MinDiamondValue { get; set; }

        [Binding(1000, "The maximum value of a diamond.", "LifeAndLiving.ItemPrice.MaxDiamondValue")]
        public int MaxDiamondValue { get; set; }

        [Binding(750, "The minimum value of a sync disk upgrade module.", "LifeAndLiving.ItemPrice.MinSyncDiskUpgradeModuleValue")]
        public int MinSyncDiskUpgradeModuleValue { get; set; }

        [Binding(1000, "The maximum value of a sync disk upgrade module.", "LifeAndLiving.ItemPrice.MaxSyncDiskUpgradeModuleValue")]
        public int MaxSyncDiskUpgradeModuleValue { get; set; }
    }
}
