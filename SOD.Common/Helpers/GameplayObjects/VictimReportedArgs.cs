using SOD.Common.Extensions;
using System.Linq;

namespace SOD.Common.Helpers.GameplayObjects
{
    public sealed class VictimReportedArgs : VictimKilledArgs
    {
        public Human Reporter { get; }

        /// <summary>
        /// How the report happened, (smell, audio, visual, etc..)
        /// </summary>
        public Human.Death.ReportType ReportType { get; }

        internal VictimReportedArgs(Human victim, Human reporter, Human.Death.ReportType reportType) 
            : base(MurderController.Instance.activeMurders
                .AsEnumerable()
                .FirstOrDefault(a => a.victim != null && a.victim.humanID == victim.humanID), victim)
        {
            Reporter = reporter;
            ReportType = reportType;
        }
    }
}
