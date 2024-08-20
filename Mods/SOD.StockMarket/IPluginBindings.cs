using SOD.Common.BepInEx.Configuration;

namespace SOD.StockMarket
{
    public interface IPluginBindings : IDebugBindings, IMarketBindings, IEconomyBindings, IntegrationBindings
    { }

    public interface IDebugBindings
    {
        [Binding(Constants.IsDebugEnabled, "Enables debug mode which prints several useful information into the console.", "Debugging.EnableDebugMode")]
        bool IsDebugEnabled { get; set; }

        [Binding(Constants.StockDataSaveFormat, "The save format for the stock data. Options: \"csv\", \"bin\")", "Debugging.StockDataSaveFormat")]
        string StockDataSaveFormat { get; set; }

        [Binding(Constants.RunSimulation, "If this option is enabled, it will run a simulation after initial economy load and export it as a csv.", "Debugging.RunSimulation")]
        bool RunSimulation { get; set; }

        [Binding(Constants.SimulationDays, "The total amount of days to simulate for (only if RunSimulation is enabled).", "Debugging.SimulationDays")]
        int SimulationDays { get; set; }
    }

    public interface IntegrationBindings
    {
        [Binding(true, "Should murder's impact the stock market?", "Integrations.EnableMurderIntegration")]
        bool EnableMurderIntegration { get; set; }
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

        [Binding(Constants.PastHistoricalDataVolatility, "The base percentage of volatility the market has been for the past [DaysToKeepStockHistoricalData] on market initialization. (MIN 1.0)", "StockMarket.Economy.PastHistoricalDataVolatility")]
        double PastHistoricalDataVolatility { get; set; }

        [Binding(Constants.MinimumStocksInMarket, "The minimum amount of stocks that should be in the market on generation.", "StockMarket.Economy.MinimumStocksInMarket")]
        int MinimumStocksInMarket { get; set; }

        [Binding(Constants.MinimumMurderTrendPercentage, "The minimum percentage effect on the stock on a company employee murder.", "StockMarket.Economy.MinimumMurderTrendPercentage")]
        int MinimumMurderTrendPercentage { get; set; }

        [Binding(Constants.MaximumMurderTrendPercentage, "The maximum percentage effect on the stock on a company employee murder.", "StockMarket.Economy.MaximumMurderTrendPercentage")]
        int MaximumMurderTrendPercentage { get; set; }
    }
}
