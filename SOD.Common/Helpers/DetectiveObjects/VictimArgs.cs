using SOD.Common.Extensions;
using System;
using System.Linq;

namespace SOD.Common.Helpers.DetectiveObjects
{
    public sealed class VictimArgs : EventArgs
    {
        public Human Victim { get; }
        public Human Reporter { get; }

        /// <summary>
        /// How the report happened, (smell, audio, visual, etc..)
        /// </summary>
        public Human.Death.ReportType ReportType { get; }

        /// <summary>
        /// Contains information about the murder.
        /// </summary>
        public MurderController.Murder Murder { get; }

        internal VictimArgs(Human victim, Human reporter, Human.Death.ReportType reportType)
        {
            Victim = victim;
            Reporter = reporter;
            ReportType = reportType;
            Murder = MurderController.Instance.activeMurders
                .AsEnumerable()
                .FirstOrDefault(a => a.victim != null && a.victim.humanID == victim.humanID);
        }
    }
}
