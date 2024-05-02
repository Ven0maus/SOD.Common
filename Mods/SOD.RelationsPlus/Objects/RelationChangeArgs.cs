using System;

namespace SOD.RelationsPlus.Objects
{
    public sealed class RelationChangeArgs : EventArgs
    {
        /// <summary>
        /// The id of the citizen.
        /// </summary>
        public int CitizenId { get; }
        /// <summary>
        /// The value before the change.
        /// </summary>
        public float OldValue { get; }
        /// <summary>
        /// The value after the change.
        /// </summary>
        public float NewValue { get; }

        internal RelationChangeArgs(int citizenId, float oldValue, float newValue)
        {
            CitizenId = citizenId;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
