using SOD.Common.BepInEx.Configuration;

namespace SOD.Narcotics
{
    public interface IPluginBindings : IAddictionBindings, IAddictionCalculationBindings, IAddictionPotentialBindings
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

    public interface IAddictionPotentialBindings
    {
        [Binding(0.25f, "How quickly does alcohol lead to addiction (higher = faster)", "Addiction.Potential.AlcoholAddictionPotential")]
        float AlcoholAddictionPotential { get; set; }

        [Binding(0.25f, "How quickly does nicotine lead to addiction (higher = faster)", "Addiction.Potential.NicotineAddictionPotential")]
        float NicotineAddictionPotential { get; set; }

        [Binding(0.4f, "How quickly does opioid lead to addiction (higher = faster)", "Addiction.Potential.OpioidAddictionPotential")]
        float OpioidAddictionPotential { get; set; }

        [Binding(0.15f, "How quickly does sugar lead to addiction (higher = faster)", "Addiction.Potential.SugarAddictionPotential")]
        float SugarAddictionPotential { get; set; }

        [Binding(0.10f, "How quickly does caffeine lead to addiction (higher = faster)", "Addiction.Potential.CaffeineAddictionPotential")]
        float CaffeineAddictionPotential { get; set; }
    }

    public interface IAddictionCalculationBindings
    {
        [Binding(0.48f, "The minimum susceptible the player can be (random chosen between min/max for each addiction type).", "Addiction.Calculations.MinimumSusceptibility")]
        float MinimumSusceptibility { get; set; }

        [Binding(0.52f, "The maximum susceptible the player can be (random chosen between min/max for each addiction type).", "Addiction.Calculations.MaximumSusceptibility")]
        float MaximumSusceptibility { get; set; }

        [Binding(0.1f, "The hourly exponential rate to recover from addictions. (Formula used(e: Euler’s constant): currentAddictionValue * e^-hourlyRecoveryRate).", "Addiction.Calculations.AddictionHourlyRecoveryRate")]
        float AddictionHourlyRecoveryRate { get; set; }
    }
}
