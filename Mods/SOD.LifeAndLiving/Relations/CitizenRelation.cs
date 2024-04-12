using System;

namespace SOD.LifeAndLiving.Relations
{
    /// <summary>
    /// Contains information about the relation between the player and the citizen.
    /// </summary>
    public sealed class CitizenRelation
    {
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
        /// The relation between the player and the citizen.
        /// </summary>
        public Relation Relation { get; set; }
    }

    /// <summary>
    /// All possible relations between the player and the citizens
    /// </summary>
    public enum Relation
    {
        /// <summary>
        /// Player is unaware of the citizen, never interacted
        /// </summary>
        Stranger,
        /// <summary>
        /// Player is aware of the citizen, never interacted
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
