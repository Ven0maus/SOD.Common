using SOD.Common.BepInEx.Configuration;

namespace SOD.Narcotics
{
    public interface IPluginBindings : IAddictionBindings, IAddictionCalculationBindings, IAddictionSeverityBindings
    {
        [Binding(false, "Enables debug mode, which adds extra logging and other utilities for the mod.")]
        bool DebugMode { get; set; }
    }

    public interface IAddictionBindings
    {
        [Binding(true, "Are alcohol addictions enabled?", "Addictions.EnableAlcoholAddiction")]
        bool EnableAlcoholAddiction { get; set; }

        [Binding(true, "Are nicotine addictions enabled?", "Addictions.EnableNicotineAddiction")]
        bool EnableNicotineAddiction { get; set; }

        [Binding(true, "Are opioid addictions enabled?", "Addictions.EnableOpioidAddiction")]
        bool EnableOpioidAddiction { get; set; }

        [Binding(true, "Are sugar addictions enabled?", "Addictions.EnableSugarAddiction")]
        bool EnableSugarAddiction { get; set; }

        [Binding(true, "Are caffeine addictions enabled?", "Addictions.EnableCaffeineAddiction")]
        bool EnableCaffeineAddiction { get; set; }
    }

    public interface IAddictionSeverityBindings
    {
        [Binding(1.15f, "How quickly does alcohol lead to addiction (higher = faster)", "Addiction.Potential.AlcoholAddictionPotential")]
        float AlcoholAddictionPotential { get; set; }

        [Binding(1.0f, "How quickly does nicotine lead to addiction (higher = faster)", "Addiction.Potential.NicotineAddictionPotential")]
        float NicotineAddictionPotential { get; set; }

        [Binding(1.35f, "How quickly does opioid lead to addiction (higher = faster)", "Addiction.Potential.OpioidAddictionPotential")]
        float OpioidAddictionPotential { get; set; }

        [Binding(0.75f, "How quickly does sugar lead to addiction (higher = faster)", "Addiction.Potential.SugarAddictionPotential")]
        float SugarAddictionPotential { get; set; }

        [Binding(0.85f, "How quickly does caffeine lead to addiction (higher = faster)", "Addiction.Potential.CaffeineAddictionPotential")]
        float CaffeineAddictionPotential { get; set; }
    }

    public interface IAddictionCalculationBindings
    {
        [Binding(10, "The base threshold used in calculating when a person becomes addicted.", "Addiction.Calculations.BaseAddictionThreshold")]
        int BaseAddictionThreshold { get; set; }

        [Binding(0.75f, "The minimum susceptible a person can be (random chosen between min/max).", "Addiction.Calculations.MinimumSusceptibility")]
        float MinimumSusceptibility { get; set; }

        [Binding(1.25f, "The maximum susceptible a person can be (random chosen between min/max).", "Addiction.Calculations.MaximumSusceptibility")]
        float MaximumSusceptibility { get; set; }

        [Binding(4, "The amount of hours the person must have not taken anymore narcotics to start recovering from their addiction.", "Addiction.Calculations.RecoveryStartTime")]
        int RecoveryStartTimeAfterHours { get; set; }

        [Binding(12, "The amount of hours needed for the count of the narcotic consumption to reset (12-24 is a good balance), example: if you put 12 then you can drink max X amount of vodkas every 12 hours and not get addicted.", "Addiction.Calculations.ResetConsumptionCounterAfterHours")]
        int ResetConsumptionCounterAfterHours { get; set; }
    }
}
