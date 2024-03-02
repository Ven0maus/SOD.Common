using SOD.Common.BepInEx.Configuration;

namespace SOD.LifeAndLiving
{
    public interface IPluginBindings : IJobBindings, IMurderBindings, ILockPickBindings, IApartementBindings, IMoneyBindings
    { }

    public interface IJobBindings
    {
        [Binding(90, "The percentage reduction of job resolve payouts.", "LifeAndLiving.Jobs.PayoutReductionJobs")]
        public int PayoutReductionJobs { get; set; }
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
        [Binding(60, "The percentage increase of apartement cost", "LifeAndLiving.Apartements.ApartementCostPercentage")]
        public int ApartementCostPercentage { get; set; }

        [Binding(true, "Should diamonds only spawn in apartements.", "LifeAndLiving.Apartements.SpawnDiamondsOnlyInApartements")]
        public bool SpawnDiamondsOnlyInApartements { get; set; }
    }

    public interface IMoneyBindings
    {
        [Binding(10, "The maximum $ of loose change in a small stack.", "LifeAndLiving.Money.MaxM1Crows")]
        public int MaxM1Crows { get; set; }

        [Binding(20, "The maximum $ of loose change in a medium stack.", "LifeAndLiving.Money.MaxM2Crows")]
        public int MaxM2Crows { get; set; }

        [Binding(30, "The maximum $ of loose change in a big stack.", "LifeAndLiving.Money.MaxM3Crows")]
        public int MaxM3Crows { get; set; }

        [Binding(50, "The maximum $ of loose change in a huge stack.", "LifeAndLiving.Money.MaxM4Crows")]
        public int MaxM4Crows { get; set; }

        [Binding(true, "Should loose change only spawn in apartements.", "LifeAndLiving.Money.LimitLooseMoneyToApartementsOnly")]
        public bool LimitLooseMoneyToApartementsOnly { get; set; }
    }
}
