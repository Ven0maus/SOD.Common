using SOD.Common.Helpers.DetectiveObjects;
using System;

namespace SOD.Common.Helpers
{
    public sealed class Gameplay
    {
        internal Gameplay() { }

        /// <summary>
        /// Raises when a victim is reported by a civilian, and provides some arguments related to this.
        /// </summary>
        public event EventHandler<VictimArgs> OnVictimReported;

        internal void ReportVictim(Human victim, Human reporter, Human.Death.ReportType reportType)
        {
            OnVictimReported?.Invoke(this, new VictimArgs(victim, reporter, reportType));
        }
    }
}
