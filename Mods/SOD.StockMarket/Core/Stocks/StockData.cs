using System;
using TimeData = SOD.Common.Shadows.Implementations.Time.TimeData;

namespace SOD.StockMarket.Core.Stocks
{
    internal class StockData : IEquatable<StockData>
    {
        public TimeData Date { get; set; }
        public decimal Open { get; set; }
        public decimal? Close { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public StockTrend? Trend { get; set; }

        public bool Equals(StockData other)
        {
            return other != null && other.Date.Equals(Date);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as StockData);
        }

        public override int GetHashCode()
        {
            return Date.GetHashCode();
        }
    }
}
