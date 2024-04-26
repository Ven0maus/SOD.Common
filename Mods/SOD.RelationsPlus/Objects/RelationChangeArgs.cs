using System;

namespace SOD.RelationsPlus.Objects
{
    public sealed class RelationChangeArgs : EventArgs
    {
        /// <summary>
        /// The value before the change.
        /// </summary>
        public float OldValue { get; }
        /// <summary>
        /// The value after the change.
        /// </summary>
        public float NewValue { get; }

        internal RelationChangeArgs(float oldValue, float newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
