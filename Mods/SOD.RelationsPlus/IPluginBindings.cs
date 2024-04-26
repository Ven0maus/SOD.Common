using SOD.Common.BepInEx.Configuration;

namespace SOD.RelationsPlus
{
    public interface IPluginBindings : IKnowModifierBindings, ILikeModifierBindings
    { }

    public interface IKnowModifierBindings
    {
        [Binding(0.015f, "How much the \"Know\" property changes for the citizen and player when seen passing by on the street.", "Modifiers.Know.SeenOnStreetModifier")]
        float SeenOnStreetModifier { get; set; }

        [Binding(0.025f, "How much the \"Know\" property changes for the citizen and player when seen at their workplace.", "Modifiers.Know.SeenAtWorkplaceModifier")]
        float SeenAtWorkplaceModifier { get; set; }

        [Binding(0.045f, "How much the \"Know\" property changes for the citizen and player when seen inside their home.", "Modifiers.Know.SeenInHome")]
        float SeenInHomeModifier { get; set; }

        [Binding(0.035f, "How much the \"Know\" property changes for the citizen and player when seen inside their home's building/apartement.", "Modifiers.Know.SeenInHomeBuilding")]
        float SeenInHomeBuildingModifier { get; set; }
    }

    public interface ILikeModifierBindings
    {
        [Binding(-0.05f, "How much the \"Like\" property changes for the citizen and player when seen trespassing.", "Modifiers.Like.SeenTrespassingModifier")]
        float SeenTrespassingModifier { get; set; }
    }
}
