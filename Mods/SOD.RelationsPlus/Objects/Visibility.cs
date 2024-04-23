using System;

namespace SOD.RelationsPlus.Objects
{
    public class Visibility
    {
        /// <summary>
        /// How many times the citizen has seen the player while at work.
        /// </summary>
        public int SeenAtWork { get; internal set; }

        /// <summary>
        /// How many times the citzen has seen the player outside of work.
        /// </summary>
        public int SeenOutsideOfWork { get; internal set; }

        /// <summary>
        /// How many times the citizen has seen the player at the citizen's home.
        /// </summary>
        public int SeenAtHome { get; internal set; }

        /// <summary>
        /// How many times the citizen has seen the player within the citizen's home building.
        /// </summary>
        public int SeenAtHomeBuilding { get; internal set; }

        /// <summary>
        /// The last time the citizen has seen the player.
        /// </summary>
        public DateTime? LastSeen { get; internal set; }
    }
}
