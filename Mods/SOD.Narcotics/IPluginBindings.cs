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
        [Binding(0.3f, "How quickly does alcohol lead to addiction (higher = faster)", "Addiction.Potential.AlcoholAddictionPotential")]
        float AlcoholAddictionPotential { get; set; }

        [Binding(0.2f, "How quickly does nicotine lead to addiction (higher = faster)", "Addiction.Potential.NicotineAddictionPotential")]
        float NicotineAddictionPotential { get; set; }

        [Binding(0.5f, "How quickly does opioid lead to addiction (higher = faster)", "Addiction.Potential.OpioidAddictionPotential")]
        float OpioidAddictionPotential { get; set; }

        [Binding(0.1f, "How quickly does sugar lead to addiction (higher = faster)", "Addiction.Potential.SugarAddictionPotential")]
        float SugarAddictionPotential { get; set; }

        [Binding(0.15f, "How quickly does caffeine lead to addiction (higher = faster)", "Addiction.Potential.CaffeineAddictionPotential")]
        float CaffeineAddictionPotential { get; set; }
    }

    public interface IAddictionCalculationBindings
    {
        [Binding(0.48f, "The minimum susceptible a person can be (random chosen between min/max).", "Addiction.Calculations.MinimumSusceptibility")]
        float MinimumSusceptibility { get; set; }

        [Binding(0.52f, "The maximum susceptible a person can be (random chosen between min/max).", "Addiction.Calculations.MaximumSusceptibility")]
        float MaximumSusceptibility { get; set; }
    }
}
