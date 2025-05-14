using System;

namespace SOD.Common.Helpers.GameplayObjects
{
    public sealed class HumanPickedArgs : EventArgs
    {
        /// <summary>
        /// The previous picked human
        /// </summary>
        public Human Previous { get; }

        /// <summary>
        /// The newly picked human
        /// </summary>
        public Human New { get; }

        internal HumanPickedArgs(Human previous, Human @new)
        {
            Previous = previous;
            New = @new;
        }
    }
}
