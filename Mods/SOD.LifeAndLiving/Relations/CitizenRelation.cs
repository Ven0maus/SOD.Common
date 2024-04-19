using System;

namespace SOD.LifeAndLiving.Relations
{
    /// <summary>
    /// Contains information about the relation between the player and the citizen.
    /// </summary>
    public class CitizenRelation
    {
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

        private int _seenAtHomeBuilding;
        public int SeenAtHomeBuilding
        {
            get => _seenAtHomeBuilding;
            set
            {
                if (_seenAtHomeBuilding != value)
                {
                    _seenAtHomeBuilding = value;
                    Calculate();
                }
            }
        }

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

            // TODO
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
        /// Player has been around the citizen a few times
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
        Antagonistic
    }
}
