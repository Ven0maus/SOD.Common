using SOD.Common.BepInEx.Configuration;

namespace SOD.StockMarket
{
    public interface IPluginBindings : IDebugBindings, IMarketBindings, IEconomyBindings
    { }

    public interface IDebugBindings
    {
        [Binding(false, "Enables debug mode which prints several useful information into the console.", "Debugging.EnableDebugMode")]
        bool IsDebugEnabled { get; set; }
    }

    public interface IMarketBindings
    {
        [Binding(9, "The hour the stock market opens everyday. (24h clock)", "StockMarket.OpeningHour")]
        int OpeningHour { get; set; }

        [Binding(16, "The hour the stock market closes everyday. (24h clock)", "StockMarket.ClosingHour")]
        int ClosingHour { get; set; }

        [Binding("Saturday,Sunday", "The days the stock market is closed.", "StockMarket.DaysClosed")]
        string DaysClosed { get; set; }
    }

    public interface IEconomyBindings
    {
        [Binding(3.0, "The percentage change a stock has to start a trend. (0-100)", "StockMarket.Economy.StockTrendChancePercentage")]
        double StockTrendChancePercentage { get; set; }

        [Binding(10, "The maximum amount of trends that can be ongoing at once. (-1 for unlimited)", "StockMarket.Economy.MaxTrends")]
        int MaxTrends { get; set; }

        [Binding(8, "The maximum amount of hours a trend can persist until its completed. (MIN 1)", "StockMarket.Economy.MaxHoursTrendsCanPersist")]
        int MaxHoursTrendsCanPersist { get; set; }

        [Binding(1, "The minimum amount of hours a trend must persist until its completed. (MIN 1)", "StockMarket.Economy.MinHoursTrendsMustPersist")]
        int MinHoursTrendsMustPersist { get; set; }

        [Binding(0.35, "The base price fluctuation percentage of stocks. (based on stock volatility)", "StockMarket.Economy.PriceFluctuationPercentage")]
        double PriceFluctuationPercentage { get; set; }

        [Binding(14, "The amount of days the historical data should be kept per stock.", "StockMarket.Economy.DaysToKeepStockHistoricalData")]
        int DaysToKeepStockHistoricalData { get; set; }
    }
}
