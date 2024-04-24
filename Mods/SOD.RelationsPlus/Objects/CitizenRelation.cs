using UnityEngine;

namespace SOD.RelationsPlus.Objects
{
    /// <summary>
    /// Contains information about the relation between the player and the citizen.
    /// </summary>
    public class CitizenRelation
    {
        internal CitizenRelation(int citizenId)
        {
            CitizenId = citizenId;
            Visibility = new Visibility();
        }

        /// <summary>
        /// The citizen's ID
        /// </summary>
        public int CitizenId { get; }

        /// <summary>
        /// Contains information about how often the citizen has seen the player.
        /// </summary>
        public Visibility Visibility { get; }

        private float _know = 0f;
        /// <summary>
        /// How much the citizen knows the player. (how often seen, interacted)
        /// <br>Range between 0-1 (0: unknown, 1: well known)</br>
        /// <br>Default value: 0</br>
        /// </summary>
        public float Know
        {
            get => _know;
            set => _know = Mathf.Clamp01(value);
        }

        private float _like = 0.5f;
        /// <summary>
        /// How much the citizen actually likes the player. (From actively doing something for the citizen)
        /// <br>Range between 0-1 (0: dislike, 0.5 neutral, 1: like)</br>
        /// <br>Default value: 0.5</br>
        /// </summary>
        public float Like
        {
            get => _like;
            set => _like = Mathf.Clamp01(value);
        }
    }
}