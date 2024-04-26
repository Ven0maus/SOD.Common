using SOD.Common.BepInEx.Configuration;

namespace SOD.RelationsPlus
{
    public interface IPluginBindings : IModifierBindings
    { }

    public interface IModifierBindings
    {
        [Binding(0.015f, "How much the \"Know\" property changes for the citizen and player when seen passing by on the street.", "Modifiers.SeenOnStreetModifier")]
        float SeenOnStreetModifier { get; set; }

        [Binding(0.025f, "How much the \"Know\" property changes for the citizen and player when seen at their workplace.", "Modifiers.SeenAtWorkplaceModifier")]
        float SeenAtWorkplaceModifier { get; set; }

        [Binding(0.045f, "How much the \"Know\" property changes for the citizen and player when seen inside their home.", "Modifiers.SeenInHome")]
        float SeenInHomeModifier { get; set; }

        [Binding(0.035f, "How much the \"Know\" property changes for the citizen and player when seen inside their home's building/apartement.", "Modifiers.SeenInHomeBuilding")]
        float SeenInHomeBuildingModifier { get; set; }

        [Binding(-0.05f, "How much the \"Like\" property changes for the citizen and player when seen trespassing.", "Modifiers.SeenTrespassingModifier")]
        float SeenTrespassingModifier { get; set; }
    }
}
