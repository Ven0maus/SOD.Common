using SOD.StockMarket.Implementation.DataConversion;

namespace SOD.StockMarket
{
    internal class Constants
    {
        internal const bool IsDebugEnabled = false;
        internal const DataSaveFormat StockDataSaveFormat = DataSaveFormat.Csv;
        internal const int OpeningHour = 9;
        internal const int ClosingHour = 16;
        internal const string DaysClosed = "Saturday,Sunday";
        internal const double StockTrendChancePercentage = 3.0;
        internal const int MaxTrends = 10;
        internal const int MaxHoursTrendsCanPersist = 8;
        internal const int MinHoursTrendsMustPersist = 1;
        internal const double PriceFluctuationPercentage = 0.35;
        internal const int DaysToKeepStockHistoricalData = 14;
        internal const double PastHistoricalDataVolatility = 15.0;
    }
}
