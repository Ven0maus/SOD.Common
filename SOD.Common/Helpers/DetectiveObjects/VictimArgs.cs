using System;

namespace SOD.Common.Helpers.DetectiveObjects
{
    public sealed class VictimArgs : EventArgs
    {
        public Human Victim { get; }
        public Human Reporter { get; }
        public Human.Death.ReportType ReportType { get; }

        internal VictimArgs(Human victim, Human reporter, Human.Death.ReportType reportType)
        {
            Victim = victim;
            Reporter = reporter;
            ReportType = reportType;
        }
    }
}
