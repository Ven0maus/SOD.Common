﻿using SOD.Common.BepInEx.Configuration;

namespace SOD.RelationsPlus
{
    public interface IPluginBindings : IRelationGateBindings, IDecayModifierBindings, ISeenModifierBindings, IKnowModifierBindings, ILikeModifierBindings
    {
        [Binding(false, "Debug mode shows extra logging to track relational changes between citizens and the player.")]
        bool DebugMode { get; set; }
    }

    public interface IRelationGateBindings
    {
        // Know gates
        [Binding(0.2f, "The first gate, (0 -> GateOneValue): The player is a stranger to the citizen.", "Relation.Gates.KnowGateOne")]
        float KnowGateOne { get; set; }

        [Binding(0.4f, "The second gate, (GateOneValue -> GateTwoValue): The citizen becomes aware of the player.", "Relation.Gates.KnowGateTwo")]
        float KnowGateTwo { get; set; }

        [Binding(0.6f, "The third gate, (GateTwoValue -> GateThreeValue): The citizen becomes familiar with the player.", "Relation.Gates.KnowGateThree")]
        float KnowGateThree { get; set; }

        [Binding(0.8f, "The fourth gate, (GateThreeValue -> GateFourValue): The citizen knows the player.", "Relation.Gates.KnowGateFour")]
        float KnowGateFour { get; set; }
        
        // Gate five can not be modified, it is the last stage (this is just shown for info)
        [Binding(1f, "The fifth gate, (GateFourValue -> GateFiveValue): The citizen knows the player very well. (CANNOT BE MODIFIED)", "Relation.Gates.KnowGateFive")]
        float KnowGateFive { get; set; }

        // Like gates
        [Binding(0.2f, "The first gate, (0 -> GateOneValue): The citizen hates the player.", "Relation.Gates.LikeGateOne")]
        float LikeGateOne { get; set; }

        [Binding(0.4f, "The second gate, (GateOneValue -> GateTwoValue): The citizen dislikes the player", "Relation.Gates.LikeGateTwo")]
        float LikeGateTwo { get; set; }

        [Binding(0.6f, "The third gate, (GateTwoValue -> GateThreeValue): The citizen is neutral towards the player.", "Relation.Gates.LikeGateThree")]
        float LikeGateThree { get; set; }

        [Binding(0.8f, "The fourth gate, (GateThreeValue -> GateFourValue): The citizen likes the player.", "Relation.Gates.LikeGateFour")]
        float LikeGateFour { get; set; }

        // Gate five can not be modified, it is the last stage (this is just shown for info)
        [Binding(1f, "The fifth gate, (GateFourValue -> GateFiveValue): The citizen loves the player. (CANNOT BE MODIFIED)", "Relation.Gates.LikeGateFive")]
        float LikeGateFive { get; set; }
    }

    public interface IDecayModifierBindings
    {
        [Binding(60, "After how many in-game minutes is the decay check executed each time?", "Modifiers.Decay.DecayTimeMinutesCheck")]
        int DecayTimeMinutesCheck { get; set; }

        [Binding(120, "After how many in-game minutes that the citizen hasn't seen the player should decay start? (both 'Know' and 'Like' if enabled)", "Modifiers.Decay.DecayKnowAfterUnseenMinutes")]
        int DecayKnowAfterUnseenMinutes { get; set; }

        [Binding(-0.005f, "How much does 'Know' decay automatically? (cannot decay past certain stages of 'Know')", "Modifiers.Decay.DecayKnowAmount")]
        float DecayKnowAmount { get; set; }

        [Binding(false, "Can the automatic decay of 'Know' go past the relation stages once reached.", "Modifiers.Decay.AllowDecayPastRelationGates")]
        bool AllowDecayPastKnowGates { get; set; }

        [Binding(false, "Can 'Like' automatically decay back to neutral?", "Modifiers.Decay.AllowDecayLikeToNeutral")]
        bool AllowDecayLikeToNeutral { get; set; }

        [Binding(-0.0025f, "How much does 'Like' decay automatically? (cannot decay past neutral)", "Modifiers.Decay.DecayLikeAmount")]
        float DecayLikeAmount { get; set; }

        [Binding(0.0035f, "How much does 'Like' improve automatically back to neutral (0.5)?", "Modifiers.Decay.ImproveLikeAmount")]
        float ImproveLikeAmount { get; set; }

        [Binding(true, "Can 'Like' automatically improve back to neutral (0.5) when it is under neutral?", "Modifiers.Decay.AutoImproveLike")]
        bool AllowAutoImproveLikeToNeutral { get; set; }
    }

    public interface ISeenModifierBindings
    {
        [Binding(10, "How often a check is executed per citizen in in-game minutes if they see the player.", "Modifiers.Seen.SeenTimeMinutesCheck")]
        int SeenTimeMinutesCheck { get; set; }
    }

    public interface IKnowModifierBindings
    {
        [Binding(0.01f, "How much the \"Know\" property changes for the citizen and player when seen passing by on the street.", "Modifiers.Know.SeenOnStreetModifier")]
        float SeenOnStreetModifier { get; set; }

        [Binding(0.02f, "How much the \"Know\" property changes for the citizen and player when seen at their workplace.", "Modifiers.Know.SeenAtWorkplaceModifier")]
        float SeenAtWorkplaceModifier { get; set; }

        [Binding(0.035f, "How much the \"Know\" property changes for the citizen and player when seen inside their home.", "Modifiers.Know.SeenInHome")]
        float SeenInHomeModifier { get; set; }

        [Binding(0.025f, "How much the \"Know\" property changes for the citizen and player when seen inside their home's building/apartement.", "Modifiers.Know.SeenInHomeBuilding")]
        float SeenInHomeBuildingModifier { get; set; }

        [Binding(0.035f, "How much the \"Know\" property changes for the citizen when the player accepts a job for them.", "Modifiers.Know.AcceptedJobKnowModifier")]
        float AcceptedJobKnowModifier { get; set; }

        [Binding(0.015f, "How much the \"Know\" property changes for the citizen and player when they talk to eachother.", "Modifiers.Know.SpeakingToCitizenModifier")]
        float SpeakingToCitizenModifier { get; set; }
    }

    public interface ILikeModifierBindings
    {
        [Binding(-0.05f, "How much the \"Like\" property changes for the citizen and player when seen trespassing.", "Modifiers.Like.SeenTrespassingModifier")]
        float SeenTrespassingModifier { get; set; }

        [Binding(-0.05f, "How much the \"Like\" property changes for the citizen when they see the player doing something illegal.", "Modifiers.Like.SeenDoingIllegalModifier")]
        float SeenDoingIllegalModifier { get; set; }

        [Binding(0.125f, "How much the \"Like\" property changes for the citizen when the player solves a job from them.", "Modifiers.Like.SolvedJobModifier")]
        float SolvedJobModifier { get; set; }

        [Binding(-0.065f, "How much the \"Like\" property changes for the citizen when the player fails a job from them.", "Modifiers.Like.FailedJobModifier")]
        float FailedJobModifier { get; set; }

        [Binding(-0.1f, "How much the \"Like\" property changes for the citizen when the player attacks them.", "Modifiers.Like.OnAttackCitizenModifier")]
        float OnAttackCitizenModifier { get; set; }

        [Binding(0.025f, "How much the \"Know\" property changes for the citizen when the player accepts a job for them.", "Modifiers.Know.AcceptedJobLikeModifier")]
        float AcceptedJobLikeModifier { get; set; }
    }
}
