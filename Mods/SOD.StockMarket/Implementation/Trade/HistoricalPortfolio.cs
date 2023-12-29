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

        public decimal Worth { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }

        public HistoricalPortfolio() { }

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
