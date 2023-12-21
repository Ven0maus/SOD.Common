using SOD.Common.Extensions;
using SOD.Common.Shadows;
using SOD.Common.Shadows.Implementations;
using SOD.StockMarket.Implementation.Stocks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SOD.StockMarket.Implementation
{
    internal class Market
    {
        private readonly List<Stock> _stocks;
        /// <summary>
        /// Contains all the stocks available within the stock market.
        /// </summary>
        internal IReadOnlyList<Stock> Stocks => _stocks;

        internal bool Initialized { get; private set; } = false;

        private bool _interiorCreatorFinished = false;
        private bool _citizenCreatorFinished = false;
        private bool _cityConstructorFinalized = false;

        private static int OpeningHour => Plugin.Instance.Config.OpeningHour;
        private static int ClosingHour => Plugin.Instance.Config.ClosingHour;

        internal Market()
        {
            _stocks = new List<Stock>();

            // Setup events
            Lib.SaveGame.OnBeforeLoad += OnFileLoad;
            Lib.SaveGame.OnBeforeSave += OnFileSave;
            Lib.SaveGame.OnBeforeDelete += OnFileDelete;
            Lib.Time.OnMinuteChanged += OnMinuteChanged;
            Lib.Time.OnHourChanged += OnHourChanged;
        }

        /// <summary>
        /// Initializes the market, when all required components are finalized.
        /// </summary>
        /// <param name="type"></param>
        internal void PostStocksInitialization(Type type)
        {
            if (type == typeof(CitizenCreator))
                _citizenCreatorFinished = true;
            else if (type == typeof(InteriorCreator))
                _interiorCreatorFinished = true;
            else if (type == typeof(CityConstructor))
                _cityConstructorFinalized = true;
            else if (type == typeof(StockDataIO))
            {
                // If we come from an import
                _citizenCreatorFinished = true;
                _interiorCreatorFinished = true;
                _cityConstructorFinalized = true;
                Initialized = true;
                _isLoading = false;
                Plugin.Log.LogInfo("Stock market loaded.");
                return;
            }

            // We need to wait until all 3 processes are completely finished initializing
            if (_isLoading || Initialized || !_citizenCreatorFinished || !_interiorCreatorFinished || !_cityConstructorFinalized)
                return;

            // Init helper
            MathHelper.Init(CityData.Instance.seed.GetHashCode());

            // Also add some default game related stocks and update prices
            foreach (var (data, basePrice) in CustomStocks.Stocks)
                InitStock(new Stock(data, basePrice));

            // Init the stocks
            foreach (var stock in _stocks)
                stock.Initialize();

            // Clear out memory
            StockSymbolGenerator.Clear();

            // Hook initialize for historical data
            Lib.Time.OnTimeInitialized += InitializeMarket;

            // Market is finished initializing stocks
            if (Plugin.Instance.Config.IsDebugEnabled)
                Plugin.Log.LogInfo("Stocks created: " + _stocks.Count);

            Plugin.Log.LogInfo("Stock market initialized.");
            Initialized = true;
        }

        /// <summary>
        /// Add's a new stock for the given company into the market.
        /// </summary>
        /// <param name="company"></param>
        internal void InitStock(Stock stock)
        {
            if (Initialized) return;
            _stocks.Add(stock);
        }

        private static string GetInitials(string companyName)
        {
            // Extract the first letter of each word in the company name
            string[] words = companyName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return string.Join("", words.Select(word => word[0]));
        }

        private void InitializeMarket(object sender, TimeChangedArgs e)
        {
            // Unsubscribe after first call
            Lib.Time.OnTimeInitialized -= InitializeMarket;

            int totalEntries = 0;
            var currentDate = Lib.Time.CurrentDate;
            int totalDays = Plugin.Instance.Config.DaysToKeepStockHistoricalData;
            float historicalDataPercentage = (float)Plugin.Instance.Config.PastHistoricalDataVolatility;
            foreach (var stock in _stocks)
            {
                StockData previous = null;
                // Not >= we don't want to add one for the current date
                for (int i = totalDays + 1; i > 0; i--)
                {
                    var newDate = currentDate.AddDays(-i);
                    var newStockData = new StockData
                    {
                        Date = newDate,
                        Open = previous?.Close ?? stock.Price
                    };

                    var historicalOne = -historicalDataPercentage * (float)stock.Volatility;
                    var historicalTwo = historicalDataPercentage * (float)stock.Volatility;

                    var sizeRange = stock.Volatility;
                    newStockData.Close = Math.Round(newStockData.Open + newStockData.Open / 100m * (decimal)MathHelper.Random.NextDouble(historicalOne, historicalTwo), 2);
                    if (newStockData.Close <= 0m)
                        newStockData.Close = 0.01m;
                    newStockData.Low = Math.Round(newStockData.Close.Value + newStockData.Close.Value / 100m * (decimal)MathHelper.Random.NextDouble(historicalOne, 0d), 2);
                    if (newStockData.Low <= 0m)
                        newStockData.Low = 0.01m;
                    newStockData.High = Math.Round(newStockData.Close.Value + newStockData.Close.Value / 100m * (decimal)MathHelper.Random.NextDouble(0d, historicalTwo), 2);
                    if (newStockData.High <= 0m)
                        newStockData.High = 0.01m;

                    stock.CreateHistoricalData(newStockData);
                    previous = newStockData;
                    totalEntries++;
                }

                if (Plugin.Instance.Config.IsDebugEnabled)
                    Plugin.Log.LogInfo($"Stock({stock.Symbol}) {stock.Name} [HA] | " +
                        $"Volatility ({stock.Volatility}) | " +
                        $"Close ({Math.Round(stock.HistoricalData.Average(a => a.Close.Value), 2)}) | " +
                        $"Open ({Math.Round(stock.HistoricalData.Average(a => a.Open), 2)}) | " +
                        $"High ({Math.Round(stock.HistoricalData.Average(a => a.High), 2)}) | " +
                        $"Low ({Math.Round(stock.HistoricalData.Average(a => a.Low), 2)}).");
            }

            if (Plugin.Instance.Config.IsDebugEnabled)
                Plugin.Log.LogInfo($"Initialized {totalEntries} historical data entries.");

            // Also attempt to generate some trends
            GenerateTrends();
        }

        /// <summary>
        /// Updates the stock market based on the current trends.
        /// </summary>
        private void OnMinuteChanged(object sender, TimeChangedArgs args)
        {
            // Don't execute calculations when the stock market is closed
            if (!IsOpen()) return;

            // Trigger price update every in game minute
            // Hour change price updates are handled seperately
            if (!args.IsHourChanged)
                Calculate();
        }

        private void OnHourChanged(object sender, TimeChangedArgs args)
        {
            var currentTime = Lib.Time.CurrentDateTime;
            if (Plugin.Instance.Config.IsDebugEnabled)
                Plugin.Log.LogInfo("Current time: " + currentTime.ToString());

            // Set the opening / closing price for each stock
            if (currentTime.Hour == OpeningHour)
            {
                OnOpen();
            }
            else if (currentTime.Hour == ClosingHour)
            {
                // We don't need to calculate the last tick
                OnClose();
            }

            if (!IsOpen()) return;

            // Update prices
            Calculate();

            // Generate hourly trends
            GenerateTrends();

            if (Plugin.Instance.Config.IsDebugEnabled)
            {
                Plugin.Log.LogInfo($"- New stock updates -");
                foreach (var stock in _stocks)
                    Plugin.Log.LogInfo($"Stock: \"({stock.Symbol}) {stock.Name}\" | Price: {stock.Price}.");
                Plugin.Log.LogInfo($"- End of Stocks -");
            }
        }

        public static bool IsOpen()
        {
            if (!Lib.Time.IsInitialized) return false;

            // Check the time
            var currentTime = Lib.Time.CurrentDateTime;
            var currentHour = currentTime.Hour;
            if (currentHour < OpeningHour || currentHour > ClosingHour)
                return false;

            // Check the current day
            var closedDays = Plugin.Instance.Config.DaysClosed.Split(new[] { ',' },
                StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(a => Enum.Parse<SessionData.WeekDay>(a.ToLower()))
                .ToHashSet();
            var currentDay = Lib.Time.CurrentDayEnum;
            if (closedDays.Contains(currentDay))
                return false;

            return true;
        }

        private void OnClose()
        {
            int historicalDataDeleted = 0;
            _stocks.ForEach(a =>
            {
                a.ClosingPrice = a.Price;
                a.CreateHistoricalData();
                historicalDataDeleted += a.CleanUpHistoricalData();
            });
            if (historicalDataDeleted > 0 && Plugin.Instance.Config.IsDebugEnabled)
                Plugin.Log.LogInfo($"Deleted {historicalDataDeleted} old historical data.");
            if (Plugin.Instance.Config.IsDebugEnabled)
                Plugin.Log.LogInfo("Stock market is closing.");
        }

        private void OnOpen()
        {
            _stocks.ForEach(a =>
            {
                a.OpeningPrice = a.ClosingPrice.Value;
                a.ClosingPrice = null;
            });
            if (Plugin.Instance.Config.IsDebugEnabled)
                Plugin.Log.LogInfo("Stock market is opening.");
        }

        /// <summary>
        /// Calculates the stocks. (each in-game minute)
        /// </summary>
        private void Calculate()
        {
            foreach (var stock in _stocks)
                stock.DeterminePrice();
        }

        /// <summary>
        /// Calculates new positive and negative trends to impact the economy. (each in-game hour)
        /// </summary>
        private void GenerateTrends()
        {
            var trendsGenerated = 0;

            // Validations for configuration
            var maxTrendSteps = Plugin.Instance.Config.MaxHoursTrendsCanPersist;
            var minTrendSteps = Plugin.Instance.Config.MinHoursTrendsMustPersist;
            var trendChancePerStock = Plugin.Instance.Config.StockTrendChancePercentage;
            var maxTrends = Plugin.Instance.Config.MaxTrends;
            var debugModeEnabled = Plugin.Instance.Config.IsDebugEnabled;

            // Check if we're already over the maximum trends
            if (maxTrends > -1 && Stocks.Count(a => a.Trend != null) >= maxTrends)
                return;

            // Shuffle stocks, give each stock a chance
            List<double> historicalPercentageChanges = new();

            // Generate new trends
            foreach (var stock in Stocks.Where(a => a.Trend == null))
            {
                var chance = MathHelper.Random.NextDouble() * 100 < trendChancePerStock;
                if (chance)
                {
                    // Generate mean and standard deviation based on historical data
                    double stockMean;
                    double stockStdDev;
                    if (stock.HistoricalData.Count >= 2)
                    {
                        // Calculate historical percentage changes for the current stock
                        for (int i = 1; i < stock.HistoricalData.Count; i++)
                        {
                            decimal previousClose = stock.HistoricalData[i - 1].Close.Value;
                            decimal currentClose = stock.HistoricalData[i].Close.Value;

                            double percentageChange = (double)((currentClose - previousClose) / previousClose * 100);
                            historicalPercentageChanges.Add(percentageChange);
                        }

                        // Calculate mean and standard deviation for the current stock
                        stockMean = historicalPercentageChanges.Average();
                        stockStdDev = MathHelper.CalculateStandardDeviation(historicalPercentageChanges);
                        historicalPercentageChanges.Clear();
                    }
                    else
                    {
                        // Use a uniform mean and deviation if there is no historical data yet
                        stockMean = 0.0d;
                        stockStdDev = 0.3d;
                    }

                    // Generate a realistic percentage change using a normal distribution
                    double percentage = Math.Round(MathHelper.NextGaussian(stockMean, stockStdDev));

                    // Skip 0 percentage differences
                    if (((int)percentage) == 0) continue;

                    // Total steps to full-fill trend (1 step = 1 in game minute)
                    int steps = MathHelper.Random.Next(60 * minTrendSteps, 60 * maxTrendSteps, true);

                    // Generate the stock trend entry
                    var stockTrend = new StockTrend(percentage, stock.Price, steps);
                    stock.SetTrend(stockTrend);
                    trendsGenerated++;

                    if (debugModeEnabled)
                        Plugin.Log.LogInfo($"[NEW TREND]: \"({stock.Symbol}) {stock.Name}\" | Price: {stockTrend.StartPrice} | Target {stockTrend.EndPrice} | Percentage: {Math.Round(stockTrend.Percentage, 2)} | MinutesLeft: {stockTrend.Steps}");
                }
            }

            if (trendsGenerated > 0 && debugModeEnabled && Lib.Time.IsInitialized)
                Plugin.Log.LogInfo($"[GameTime({Lib.Time.CurrentDateTime})] Created {trendsGenerated} new trends.");
        }

        private void OnFileSave(object sender, SaveGameArgs e)
        {
            var path = GetSaveFilePath(e.FilePath);

            // Export data to save file
            new StockDataIO(this).Export(path);

            if (Plugin.Instance.Config.IsDebugEnabled)
                Plugin.Log.LogInfo($"Saved market data to savestore \"{Path.GetFileName(path)}\".");
        }

        private bool _isLoading = false;
        private void OnFileLoad(object sender, SaveGameArgs e)
        {
            var path = GetSaveFilePath(e.FilePath);
            if (!File.Exists(path))
            {
                if (Plugin.Instance.Config.IsDebugEnabled)
                    Plugin.Log.LogInfo($"No market savestore file found at path \"{path}\", skipping load existing attempt.");
                return;
            }

            if (_isLoading) return;
            _isLoading = true;

            // Clear current market
            _stocks.Clear();
            _interiorCreatorFinished = true;
            _cityConstructorFinalized = true;
            _citizenCreatorFinished = true;
            Initialized = false;

            // Import data from save file
            new StockDataIO(this).Import(path);
        }

        private void OnFileDelete(object sender, SaveGameArgs e)
        {
            var path = GetSaveFilePath(e.FilePath);
            if (File.Exists(path))
            {
                File.Delete(path);
                if (Plugin.Instance.Config.IsDebugEnabled)
                    Plugin.Log.LogInfo($"Deleted stock market save data at path \"{path}\".");
                return;
            }
            if (Plugin.Instance.Config.IsDebugEnabled)
                Plugin.Log.LogInfo($"No stock market save data exists at path \"{path}\".");
        }

        /// <summary>
        /// Builds a unique save filepath for the current savegame.
        /// </summary>
        /// <param name="stateSaveData"></param>
        /// <returns></returns>
        private static string GetSaveFilePath(string filePath)
        {
            // TODO: Add configuration option to save it as a custom binary format or .csv format
            // Get market savestore
            var savecode = Lib.SaveGame.GetUniqueString(filePath);
            var fileName = $"stocks_{savecode}.csv";
            var saveStore = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly());
            return Path.Combine(saveStore, fileName);
        }
    }
}
