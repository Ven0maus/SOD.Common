using SOD.Common.BepInEx.Configuration;

namespace SOD.RelationsPlus
{
    public interface IPluginBindings : IRelationGateBindings, IDecayModifierBindings, ISeenModifierBindings, IKnowModifierBindings, ILikeModifierBindings
    {
        [Binding(false, "Debug mode shows extra logging to track relational changes between citizens and the player.")]
        bool DebugMode { get; set; }
    }

    public interface IRelationGateBindings
    {
        [Binding(0.2f, "The first gate, when reached the citizen becomes aware of the player.", "Relation.Gates.GateOne")]
        float GateOne { get; set; }

        [Binding(0.4f, "The second gate, when reached the citizen becomes familiar with the player.", "Relation.Gates.GateTwo")]
        float GateTwo { get; set; }

        [Binding(0.6f, "The third gate, when reached the citizen becomes an acquaintance of the player.", "Relation.Gates.GateThree")]
        float GateThree { get; set; }

        [Binding(0.8f, "The fourth gate, when reached the citizen becomes a friend of the player.", "Relation.Gates.GateFour")]
        float GateFour { get; set; }
    }

    public interface IDecayModifierBindings
    {
        [Binding(60, "After how many in-game minutes is the decay check executed each time?", "Modifiers.Decay.DecayTimeMinutesCheck")]
        int DecayTimeMinutesCheck { get; set; }

        [Binding(120, "After how many in-game minutes that the citizen hasn't seen the player should 'Know' start decaying?")]
        int DecayKnowAfterUnseenMinutes { get; set; }

        [Binding(-0.005f, "How much does 'Know' decay automatically? (cannot decay past certain stages of 'Know')", "Modifiers.Decay.DecayKnowAmount")]
        float DecayKnowAmount { get; set; }

        [Binding(false, "Can the automatic decay of 'Know' go past the relation stages once reached.", "Modifiers.Decay.AllowDecayPastRelationGates")]
        bool AllowDecayPastRelationGates { get; set; }

        [Binding(0.0035f, "How much does 'Like' improve automatically back to neutral (0.5)?", "Modifiers.Decay.ImproveLikeAmount")]
        float ImproveLikeAmount { get; set; }

        [Binding(true, "Can 'Like' automatically improve back to neutral (0.5) when it is under neutral?", "Modifiers.Decay.AutoImproveLike")]
        bool AutoImproveLike { get; set; }
    }

    public interface ISeenModifierBindings
    {
        [Binding(5, "How often a check is executed per citizen in in-game minutes if they see the player.", "Modifiers.Seen.SeenTimeMinutesCheck")]
        int SeenTimeMinutesCheck { get; set; }
    }

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
