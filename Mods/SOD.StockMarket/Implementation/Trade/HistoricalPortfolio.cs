using SOD.Common.Helpers;
using System.Text.Json.Serialization;

namespace SOD.StockMarket.Implementation.Trade
{
    internal class HistoricalPortfolio
    {
        [JsonIgnore]
        private Time.TimeData? _timeData;
        [JsonIgnore]
        internal Time.TimeData Date
        {
            get
            {
                return _timeData ??= new Time.TimeData(Year, Month, Day, 0, 0);
            }
            set { _timeData = value; }
        }

        internal decimal Worth { get; private set; }
        internal int Year { get; private set; }
        internal int Month { get; private set; }
        internal int Day { get; private set; }

        internal HistoricalPortfolio() { }

        internal HistoricalPortfolio(Time.TimeData date, decimal worth)
        {
            Date = date;
            Worth = worth;

            // For save/loading
            Year = date.Year;
            Month = date.Month;
            Day = date.Day;
        }
    }
}
