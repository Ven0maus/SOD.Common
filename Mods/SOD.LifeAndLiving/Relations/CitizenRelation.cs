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

        private int _interactedAtWork;
        /// <summary>
        /// How many times the player has interacted with this citizen at work.
        /// </summary>
        public int InteractedAtWork
        { 
            get => _interactedAtWork;
            set
            {
                if (_interactedAtWork != value)
                {
                    _interactedAtWork = value;
                    Calculate();
                }
            }
        }

        private int _interactedOutsideOfWork;
        /// <summary>
        /// How many times the player has interacted with this citizen outside of work.
        /// </summary>
        public int InteractedOutsideOfWork
        {
            get => _interactedOutsideOfWork;
            set
            {
                if (_interactedOutsideOfWork != value)
                {
                    _interactedOutsideOfWork = value;
                    Calculate();
                }
            }
        }

        private int _interactedAtHome;
        /// <summary>
        /// How many times the player has interacted with this citizen at home.
        /// </summary>
        public int InteractedAtHome
        {
            get => _interactedAtHome;
            set
            {
                if (_interactedAtHome != value)
                {
                    _interactedAtHome = value;
                    Calculate();
                }
            }
        }

        private int _seenAtWork;
        /// <summary>
        /// How many times the player has seen this citizen while at work.
        /// </summary>
        public int SeenAtWork
        {
            get => _seenAtWork;
            set
            {
                if (_seenAtWork != value)
                {
                    _seenAtWork = value;
                    Calculate();
                }
            }
        }

        private int _seenOutsideOfWork;
        /// <summary>
        /// How many times the player has seen this citizen outside of work.
        /// </summary>
        public int SeenOutsideOfWork
        {
            get => _seenOutsideOfWork;
            set
            {
                if (_seenOutsideOfWork != value)
                {
                    _seenOutsideOfWork = value;
                    Calculate();
                }
            }
        }

        private int _seenAtHome;
        /// <summary>
        /// How many times the player has seen this citizen at home.
        /// </summary>
        public int SeenAtHome
        {
            get => _seenAtHome;
            set
            {
                if (_seenAtHome != value)
                {
                    _seenAtHome = value;
                    Calculate();
                }
            }
        }

        /// <summary>
        /// The last time the player interacted with this citizen.
        /// </summary>
        public DateTime? LastInteraction { get; set; }
        /// <summary>
        /// The last time the player has seen this citizen.
        /// </summary>
        public DateTime? LastSeen { get; set; }

        private Relation _relation;
        /// <summary>
        /// The relation between the player and the citizen.
        /// </summary>
        public Relation Relation
        {
            get { return _relation; }
            set
            {
                if (_relation != value) 
                {
                    Plugin.Log.LogInfo($"Adjusted relation from \"{_relation}\" to \"{value}\".");
                    _relation = value;
                }
            }
        }

        private void Calculate()
        {
            if (RelationManager.Instance.IsLoading)
                return;


        }
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
        /// Player is aware of the citizen
        /// </summary>
        Neutral,
        /// <summary>
        /// Player has been around the citizen often
        /// </summary>
        Familiar,
        /// <summary>
        /// Player has met them and interacted often
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
