using SOD.Common;
using SOD.Common.Helpers;
using System;

namespace SOD.RelationsPlus.Objects
{
    public sealed class SeenPlayerArgs : EventArgs
    {
        /// <summary>
        /// The id of the citizen.
        /// </summary>
        public int CitizenId { get; }
        /// <summary>
        /// The value that "Know" was increased by.
        /// </summary>
        public float KnowChange { get; }
        /// <summary>
        /// The value that "Like" was increased by.
        /// <br>It can be changed in the event of tresspassing.</br>
        /// </summary>
        public float LikeChange { get; }
        /// <summary>
        /// Where the player was seen by the citizen.
        /// </summary>
        public SeenLocation Location { get; }
        /// <summary>
        /// The time the player was seen by this citizen in real time.
        /// </summary>
        public DateTime SeenRealTime { get; }
        /// <summary>
        /// The time the player was seen by this citizen in game time.
        /// </summary>
        public Time.TimeData SeenGameTime { get; }

        internal SeenPlayerArgs(int citizenId, SeenLocation location, float knowChange, float likeChange)
        {
            CitizenId = citizenId;
            KnowChange = knowChange;
            LikeChange = likeChange;
            Location = location;
            SeenRealTime = DateTime.Now;
            SeenGameTime = Lib.Time.CurrentDateTime;
        }

        public enum SeenLocation
        {
            Street,
            Workplace,
            Home,
            HomeBuilding
        }
    }
}
