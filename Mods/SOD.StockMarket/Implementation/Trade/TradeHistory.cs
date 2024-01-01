using SOD.Common.Helpers;
using System;
using System.Text.Json.Serialization;

namespace SOD.StockMarket.Implementation.Trade
{
    internal class TradeHistory
    {
        [JsonIgnore]
        private Time.TimeData? _dateTime;
        [JsonIgnore]
        public Time.TimeData DateTime
        {
            get { return _dateTime ??= new Time.TimeData(Year, Month, Day, Hour, Minute); }
            set { _dateTime = value; }
        }

        // Serialization for timedata
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }

        // History data
        public string StockSymbol { get; set; }
        public OrderType OrderType { get; set; }
        public decimal Amount { get; set; }
        public decimal Price { get; set; }

        [JsonIgnore]
        public decimal Total => Math.Round(Price * Amount, 2);

        public TradeHistory() { }

        internal TradeHistory(Time.TimeData dateTime, string symbol, OrderType orderType, decimal amount, decimal price)
        {
            DateTime = dateTime;

            // History data
            StockSymbol = symbol;
            OrderType = orderType;
            Amount = amount;
            Price = price;

            // Timedata serialization
            Year = dateTime.Year; 
            Month = dateTime.Month; 
            Day = dateTime.Day; 
            Hour = dateTime.Hour; 
            Minute = dateTime.Minute;
        }
    }
}
