using System;

namespace SOD.Common.Helpers.GameplayObjects
{
    public class VictimKilledArgs : EventArgs
    {
        /// <summary>
        /// Contains information about the murder.
        /// </summary>
        public MurderController.Murder Murder { get; }

        /// <summary>
        /// The murder victim.
        /// </summary>
        public Human Victim { get; }
        
        internal VictimKilledArgs(MurderController.Murder murder, Human victim = null)
        {
            Murder = murder;
            Victim = victim ?? murder.victim;
        }
    }
}
