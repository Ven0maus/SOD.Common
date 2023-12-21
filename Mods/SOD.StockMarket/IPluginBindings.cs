using SOD.Common.BepInEx.Configuration;
using SOD.StockMarket.Implementation.DataConversion;

namespace SOD.StockMarket
{
    public interface IPluginBindings : IDebugBindings, IMarketBindings, IEconomyBindings
    { }

    public interface IDebugBindings
    {
        [Binding(Constants.IsDebugEnabled, "Enables debug mode which prints several useful information into the console.", "Debugging.EnableDebugMode")]
        bool IsDebugEnabled { get; set; }

        [Binding(Constants.StockDataSaveFormat, "The default save format for the stock data. (binary = smaller file size)", "Debugging.StockDataSaveFormat")]
        DataSaveFormat StockDataSaveFormat { get; set; }
    }

    public interface IMarketBindings
    {
        [Binding(Constants.OpeningHour, "The hour the stock market opens everyday. (24h clock)", "StockMarket.OpeningHour")]
        int OpeningHour { get; set; }

        [Binding(Constants.ClosingHour, "The hour the stock market closes everyday. (24h clock)", "StockMarket.ClosingHour")]
        int ClosingHour { get; set; }

        [Binding(Constants.DaysClosed, "The days the stock market is closed.", "StockMarket.DaysClosed")]
        string DaysClosed { get; set; }
    }

    public interface IEconomyBindings
    {
        [Binding(Constants.StockTrendChancePercentage, "The percentage change a stock has to start a trend. (0-100)", "StockMarket.Economy.StockTrendChancePercentage")]
        double StockTrendChancePercentage { get; set; }

        [Binding(Constants.MaxTrends, "The maximum amount of trends that can be ongoing at once. (-1 for unlimited)", "StockMarket.Economy.MaxTrends")]
        int MaxTrends { get; set; }

        [Binding(Constants.MaxHoursTrendsCanPersist, "The maximum amount of hours a trend can persist until its completed. (MIN 1)", "StockMarket.Economy.MaxHoursTrendsCanPersist")]
        int MaxHoursTrendsCanPersist { get; set; }

        [Binding(Constants.MinHoursTrendsMustPersist, "The minimum amount of hours a trend must persist until its completed. (MIN 1)", "StockMarket.Economy.MinHoursTrendsMustPersist")]
        int MinHoursTrendsMustPersist { get; set; }

        [Binding(Constants.PriceFluctuationPercentage, "The base price fluctuation percentage of stocks. (based on stock volatility)", "StockMarket.Economy.PriceFluctuationPercentage")]
        double PriceFluctuationPercentage { get; set; }

        [Binding(Constants.DaysToKeepStockHistoricalData, "The amount of days the historical data should be kept per stock.", "StockMarket.Economy.DaysToKeepStockHistoricalData")]
        int DaysToKeepStockHistoricalData { get; set; }

        [Binding(Constants.PastHistoricalDataVolatility, "The base percentage of volatility the market has been for the past [DaysToKeepStockHistoricalData] on market initialization. (MIN 1.0)", "Stockmarket.Economy.PastHistoricalDataVolatility")]
        double PastHistoricalDataVolatility { get; set; }
    }
}
