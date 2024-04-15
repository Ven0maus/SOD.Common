using System;
using System.Collections.Generic;

namespace SOD.LifeAndLiving.Relations
{
    /// <summary>
    /// Contains information about the relation between the player and the citizen.
    /// </summary>
    public class CitizenRelation
    {
        /// <summary>
        /// An overview how the points are distributed for the relations.
        /// </summary>
        public static readonly Dictionary<int, Relation> RelationPoints = new()
        {
            { -30, Relation.Hostile },
            { -10, Relation.Antagonistic },
            { 0, Relation.Neutral },
            { 10, Relation.Acquaintance },
            { 30, Relation.Friend },
        };

        /// <summary>
        /// How many times the player has interacted with this citizen.
        /// </summary>
        public int Interacted { get; set; }
        /// <summary>
        /// The last time the player interacted with this citizen.
        /// </summary>
        public DateTime? LastInteraction { get; set; }
        /// <summary>
        /// How many times the player has seen this citizen.
        /// </summary>
        public int Seen { get; set; }
        /// <summary>
        /// The last time the player has seen this citizen.
        /// </summary>
        public DateTime? LastSeen { get; set; }
        /// <summary>
        /// Defines the relation level, increases / decreases based on interactions
        /// </summary>
        public int Points { get; set; }
        /// <summary>
        /// The relation between the player and the citizen.
        /// </summary>
        public Relation Relation { get; set; }
    }

    /// <summary>
    /// An intermediary object used to create setter instructions for citizen relations.
    /// </summary>
    internal sealed class CrInstruction : CitizenRelation
    {
        public Action<CitizenRelation> Instruction { get; private set; }

        private CrInstruction() { }

        public static CrInstruction Create(Action<CitizenRelation> Instruction)
        {
            return new CrInstruction { Instruction = Instruction };
        }
    }

    /// <summary>
    /// All possible relations between the player and the citizens
    /// </summary>
    public enum Relation
    {
        /// <summary>
        /// Player is unaware of the citizen
        /// </summary>
        Stranger,
        /// <summary>
        /// Player is aware of the citizen
        /// </summary>
        Neutral,
        /// <summary>
        /// Player has only briefly met them and interacted
        /// </summary>
        Acquaintance,
        /// <summary>
        /// Player and the citizen are friends
        /// </summary>
        Friend,
        /// <summary>
        /// Player and the citizen are in dislike of each other
        /// </summary>
        Antagonistic,
        /// <summary>
        /// Player and the citizen are hostile towards each other
        /// </summary>
        Hostile
    }
}
