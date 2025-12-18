using SOD.Common;
using SOD.Common.Extensions;
using SOD.Common.Helpers;
using SOD.StockMarket.Implementation.Cruncher.News;
using SOD.StockMarket.Implementation.DataConversion;
using SOD.StockMarket.Implementation.Stocks;
using SOD.StockMarket.Implementation.Trade;
using SOD.StockMarket.Patches;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SOD.StockMarket.Implementation
{
    internal class Market : IStocksContainer
    {
        private readonly List<Stock> _stocks;
        /// <summary>
        /// Contains all the stocks available within the stock market.
        /// </summary>
        public IReadOnlyList<Stock> Stocks => _stocks;

        internal event EventHandler<EventArgs> OnCalculate, OnInitialized;

        internal bool Initialized { get; private set; } = false;

        private bool _interiorCreatorFinished = false;
        private bool _citizenCreatorFinished = false;
        private bool _cityConstructorFinalized = false;

        private static int OpeningHour => Plugin.Instance.Config.OpeningHour;
        private static int ClosingHour => Plugin.Instance.Config.ClosingHour;

        internal TradeController TradeController { get; private set; }
        internal readonly bool Simulation = false;
        internal Time.TimeData? SimulationTime;

        internal Market()
        {
            _stocks = new List<Stock>();
            TradeController = new TradeController(this);

            // Setup events
            Lib.SaveGame.OnBeforeNewGame += OnBeforeNewGame;
            Lib.SaveGame.OnBeforeLoad += OnLoadSave;
            Lib.SaveGame.OnBeforeSave += OnSaveGame;
            Lib.SaveGame.OnBeforeDelete += OnDeleteSave;
            Lib.Time.OnTimeInitialized += InitDdsRecords;
            Lib.Time.OnMinuteChanged += OnMinuteChanged;
            Lib.Time.OnHourChanged += OnHourChanged;

            // Init the murder integration
            MurderIntegration.Initialize();

            // Trigger simulation if enabled
            if (Plugin.Instance.Config.RunSimulation)
            {
                OnInitialized += (sender, args) =>
                {
                    // Simulate on time init, for loading savegame
                    if (!Lib.Time.IsInitialized)
                        Lib.Time.OnTimeInitialized += OnTimeInit;
                    else
                        Simulate(Plugin.Instance.Config.SimulationDays);
                };
            }
        }

        private void InitDdsRecords(object sender, TimeChangedArgs e)
        {
            // Set the dds entry for the app's name
            Lib.DdsStrings["computer", "stockmarketpreset"] = "Stock Market";
            Lib.Time.OnTimeInitialized -= InitDdsRecords;
        }

        private void OnTimeInit(object sender, TimeChangedArgs args)
        {
            // Simulate economy
            if (Plugin.Instance.Config.RunSimulation)
                Simulate(Plugin.Instance.Config.SimulationDays);
            Lib.Time.OnTimeInitialized -= OnTimeInit;
        }

        /// <summary>
        /// Constructor for a simulation market
        /// </summary>
        /// <param name="market"></param>
        private Market(Market market)
        {
            _stocks = new List<Stock>();
            foreach (var stock in market.Stocks)
                _stocks.Add(new Stock(stock));
            TradeController = new TradeController(this);
            Simulation = true;
            SimulationTime = new Time.TimeData(1979, 1, 1, Plugin.Instance.Config.OpeningHour, 0);
            Initialized = true;
        }

        /// <summary>
        /// Create a market simulation export for x amount of days
        /// </summary>
        /// <param name="days"></param>
        internal void Simulate(int days)
        {
            var simulation = new Market(this);
            var tradeController = simulation.TradeController;

            var marketOpenHours = Plugin.Instance.Config.ClosingHour - Plugin.Instance.Config.OpeningHour;
            var openingHour = Plugin.Instance.Config.OpeningHour;

            for (int day = 0; day < days; day++)
            {
                for (int hour = 0; hour < marketOpenHours; hour++)
                {
                    // The last minute we let OnHourChange do the calculation
                    for (int minute = 0; minute < 59; minute++)
                    {
                        simulation.SimulationTime = new Time.TimeData(simulation.SimulationTime.Value.Year, simulation.SimulationTime.Value.Month,
                            simulation.SimulationTime.Value.Day, simulation.SimulationTime.Value.Hour, minute);
                        simulation.OnMinuteChanged(this, null);
                    }
                    simulation.SimulationTime = new Time.TimeData(simulation.SimulationTime.Value.Year, simulation.SimulationTime.Value.Month,
                        simulation.SimulationTime.Value.Day, simulation.SimulationTime.Value.Hour + 1, 0);
                    simulation.OnHourChanged(this, null);
                }

                simulation.SimulationTime = new Time.TimeData(simulation.SimulationTime.Value.Year, simulation.SimulationTime.Value.Month,
                    simulation.SimulationTime.Value.Day, openingHour, 0);
                simulation.SimulationTime = simulation.SimulationTime.Value.AddDays(1);
            }

            StockDataIO.Export(simulation, tradeController, Lib.SaveGame.GetPluginDataPath(Assembly.GetExecutingAssembly(), "Simulation.csv"), this);
        }

        private void OnBeforeNewGame(object sender, EventArgs e)
        {
            // Do a full market reset
            _stocks.Clear();
            TradeController.Reset();
            NewsGenerator.Clear();
            CitizenCreatorPatches.CitizenCreator_Populate.Init = false;
            CityConstructorPatches.CityConstructor_Finalized.Init = false;
            CompanyPatches.Company_Setup.ShownInitializingMessage = false;
            InteriorCreatorPatches.InteriorCreator_GenChunk.Init = false;
            _interiorCreatorFinished = false;
            _cityConstructorFinalized = false;
            _citizenCreatorFinished = false;
            Initialized = false;

            // Make sure we unhook any previous method
            Lib.Time.OnTimeInitialized -= InitializeMarket;

            // Hook the new method for the current run
            Lib.Time.OnTimeInitialized += InitializeMarket;
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
                OnInitialized?.Invoke(this, EventArgs.Empty);
                return;
            }

            // We need to wait until all 3 processes are completely finished initializing
            if (_isLoading || Initialized || !_citizenCreatorFinished || !_interiorCreatorFinished || !_cityConstructorFinalized)
                return;

            // Init helper
            MathHelper.Init(CityData.Instance.seed.GetFnvHashCode());

            // Also add some default game related stocks and update prices
            foreach (var (data, basePrice) in CustomStocks.Stocks)
                InitStock(new Stock(data, basePrice));

            // Make sure we reach the minimum stocks threshold
            if (_stocks.Count < Plugin.Instance.Config.MinimumStocksInMarket)
            {
                // Generate the remaining stocks
                var remainingCount = Plugin.Instance.Config.MinimumStocksInMarket - _stocks.Count;
                for (int i = 0; i < remainingCount; i++)
                {
                    var stock = new Stock(new CompanyStockData(StockNameGenerator.GenerateStockName(),
                        Math.Round(MathHelper.Random.NextDouble(0.15d, 0.85d), 2)));
                    InitStock(stock);
                }
            }

            // Init the stocks
            foreach (var stock in _stocks)
                stock.Initialize();

            // Clear out memory
            StockSymbolGenerator.Clear();

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

        private void InitializeMarket(object sender, TimeChangedArgs e)
        {
            // Unsubscribe after first call
            Lib.Time.OnTimeInitialized -= InitializeMarket;

            // Create first portfolio historical data entry
            if (!Simulation)
                TradeController.CreatePortfolioHistoricalDataEntry();

            // Create the initial historical data
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
                    Plugin.Log.LogInfo($"Stock({stock.Symbol}) {stock.Name} | " +
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

            // Invoke initialized
            OnInitialized?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Updates the stock market based on the current trends.
        /// </summary>
        private void OnMinuteChanged(object sender, TimeChangedArgs args)
        {
            // Make sure unreleased articles can be released at random times
            NewsGenerator.TickToBeReleased();

            // Don't execute calculations when the stock market is closed
            if (!IsOpen()) return;

            // Trigger price update every in game minute
            // Hour change price updates are handled seperately
            if (args == null || !args.IsHourChanged)
                Calculate();
        }

        private void OnHourChanged(object sender, TimeChangedArgs args)
        {
            var currentTime = SimulationTime ?? Lib.Time.CurrentDateTime;

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

            // Do a check to remove any outdated articles
            NewsGenerator.RemoveOutdatedArticles();

            if (Plugin.Instance.Config.IsDebugEnabled && !Simulation)
            {
                Plugin.Log.LogInfo($"- New stock updates -");
                foreach (var stock in _stocks.OrderBy(a => a.Id))
                    Plugin.Log.LogInfo($"Stock: \"({stock.Symbol}) {stock.Name}\" | Price: {stock.Price}.");
                Plugin.Log.LogInfo($"- End of Stocks -");
            }
        }

        internal bool IsOpen()
        {
            if (!Simulation && !Lib.Time.IsInitialized) return false;

            // Check the time
            var currentTime = SimulationTime ?? Lib.Time.CurrentDateTime;
            var currentHour = currentTime.Hour;
            if (currentHour < OpeningHour || currentHour >= ClosingHour)
                return false;

            // Check the current day
            var closedDays = Plugin.Instance.Config.DaysClosed.Split(new[] { ',' },
                StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(a => Enum.Parse<SessionData.WeekDay>(a.ToLower()))
                .ToHashSet();
            var currentDay = SimulationTime?.DayEnum ?? Lib.Time.CurrentDayEnum;
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
                a.CreateHistoricalData(date: SimulationTime);
                historicalDataDeleted += a.CleanUpHistoricalData(SimulationTime);
            });
            if (historicalDataDeleted > 0 && Plugin.Instance.Config.IsDebugEnabled && !Simulation)
                Plugin.Log.LogInfo($"Deleted {historicalDataDeleted} old historical data.");
            if (Plugin.Instance.Config.IsDebugEnabled && !Simulation)
                Plugin.Log.LogInfo("Stock market is closing.");
        }

        private void OnOpen()
        {
            _stocks.ForEach(a =>
            {
                a.OpeningPrice = a.ClosingPrice ?? a.Price;
                a.ClosingPrice = null;
            });
            if (!Simulation)
                TradeController.CreatePortfolioHistoricalDataEntry();
            if (Plugin.Instance.Config.IsDebugEnabled && !Simulation)
                Plugin.Log.LogInfo("Stock market is opening.");
        }

        /// <summary>
        /// Calculates the stocks. (each in-game minute)
        /// </summary>
        private void Calculate()
        {
            foreach (var stock in _stocks)
                stock.DeterminePrice();
            OnCalculate?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Calculates new positive and negative trends to impact the economy. (each in-game hour)
        /// </summary>
        private void GenerateTrends()
        {
            var trendsGenerated = 0;

            // Validations for configuration
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
                var chance = (MathHelper.Random.NextDouble() * 100) < trendChancePerStock;
                if (chance)
                {
                    if (!CalculateStockTrend(historicalPercentageChanges, stock, out StockTrend? strend)) 
                        continue;

                    // Set the trend that was calculated
                    var stockTrend = strend.Value;
                    stock.SetTrend(stockTrend);

                    // Generate news entry
                    if (!Simulation)
                        NewsGenerator.GenerateArticle(stock, stockTrend);

                    if (debugModeEnabled && !Simulation)
                        Plugin.Log.LogInfo($"[NEW TREND]: \"({stock.Symbol}) {stock.Name}\" | Price: {stockTrend.StartPrice} | Target {stockTrend.EndPrice} | Percentage: {Math.Round(stockTrend.Percentage, 2)} | MinutesLeft: {stockTrend.Steps}");
                    
                    trendsGenerated++;
                }
            }

            if (trendsGenerated > 0 && debugModeEnabled && !Simulation && Lib.Time.IsInitialized)
                Plugin.Log.LogInfo($"[GameTime({Lib.Time.CurrentDateTime})] Created {trendsGenerated} new trends.");
        }

        private static bool CalculateStockTrend(List<double> historicalPercentageChanges, Stock stock, out StockTrend? stockTrend)
        {
            stockTrend = null;

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
            var intPercent = (int)percentage;
            if (intPercent == 0) return false;

            // Not too small percentages
            if (Math.Abs(intPercent) < 2)
                percentage = percentage < 0 ? percentage - 3 : percentage + 3;

            // Small 7% chance to have a really high percentage spike
            if ((MathHelper.Random.NextDouble() * 100) < 7)
                percentage *= MathHelper.Random.Next(2, 5);

            // Generate the stock trend entry
            stockTrend = new StockTrend(percentage, stock.Price, CalculateRandomSteps());
            return true;
        }

        /// <summary>
        /// Generates a random amount of total steps to full-fill a trend (1 step = 1 in game minute)
        /// </summary>
        /// <returns></returns>
        internal static int CalculateRandomSteps()
        {
            var maxTrendSteps = Plugin.Instance.Config.MaxHoursTrendsCanPersist;
            var minTrendSteps = Plugin.Instance.Config.MinHoursTrendsMustPersist;
            return MathHelper.Random.Next(60 * minTrendSteps, 60 * maxTrendSteps);
        }

        private void OnSaveGame(object sender, SaveGameArgs e)
        {
            var path = GetSaveFilePath(e);

            // Export data to save file
            StockDataIO.Export(this, TradeController, path);
        }

        private bool _isLoading = false;
        private void OnLoadSave(object sender, SaveGameArgs e)
        {
            var path = GetSaveFilePath(e);
            if (!File.Exists(path))
            {
                // Savegame with no market data,
                // Generate a custom new market economy.
                Plugin.Log.LogInfo("Attempting to load a premod install savegame, a new market economy will be generated for this savegame.");
                InitPreModInstallEconomyExistingSavegame(path, true);
                return;
            }

            if (_isLoading) return;
            _isLoading = true;

            // Clear current market
            _stocks.Clear();
            TradeController.Reset();
            NewsGenerator.Clear();
            _interiorCreatorFinished = true;
            _cityConstructorFinalized = true;
            _citizenCreatorFinished = true;
            Initialized = false;

            // Import data from save file
            if (!StockDataIO.Import(this, TradeController, path))
            {
                _isLoading = false;
                InitPreModInstallEconomyExistingSavegame(path, false);
            }
        }

        private void OnDeleteSave(object sender, SaveGameArgs e)
        {
            // Support migration, but handle deletion in sod.common
            _ = GetSaveFilePath(e);
        }

        private void InitPreModInstallEconomyExistingSavegame(string filePath, bool saveImmediately)
        {
            // Do a full-reset
            _stocks.Clear();
            TradeController.Reset();
            NewsGenerator.Clear();
            CitizenCreatorPatches.CitizenCreator_Populate.Init = false;
            CityConstructorPatches.CityConstructor_Finalized.Init = false;
            CompanyPatches.Company_Setup.ShownInitializingMessage = false;
            InteriorCreatorPatches.InteriorCreator_GenChunk.Init = false;
            _interiorCreatorFinished = false;
            _cityConstructorFinalized = false;
            _citizenCreatorFinished = false;
            Initialized = false;

            // Start init process
            PostStocksInitialization(typeof(CitizenCreator));
            PostStocksInitialization(typeof(InteriorCreator));
            PostStocksInitialization(typeof(CityConstructor));

            // Set premod path
            _afterPreModPath = filePath;

            // Hook method if its not initialized yet, otherwise just call it
            if (!Lib.Time.IsInitialized)
            {
                Lib.Time.OnTimeInitialized += InitializeMarket;
                if (saveImmediately)
                    Lib.Time.OnTimeInitialized += AfterPreModInit;
                else
                    _afterPreModPath = null;
            }
            else
            {
                InitializeMarket(this, null);
                if (saveImmediately)
                    AfterPreModInit(this, null);
                else
                    _afterPreModPath = null;
            }
        }

        private string _afterPreModPath;
        private void AfterPreModInit(object sender, TimeChangedArgs e)
        {
            Lib.Time.OnTimeInitialized -= AfterPreModInit;

            // Export the data to this savegame
            StockDataIO.Export(this, TradeController, _afterPreModPath);
            _afterPreModPath = null;
        }

        /// <summary>
        /// Builds a unique save filepath for the current savegame.
        /// </summary>
        /// <param name="stateSaveData"></param>
        /// <returns></returns>
        private static string GetSaveFilePath(SaveGameArgs saveGameArgs)
        {
            // Get market savestore
            var savecode = Lib.SaveGame.GetUniqueString(saveGameArgs.FilePath);

            if (!Enum.TryParse<DataSaveFormat>(Plugin.Instance.Config.StockDataSaveFormat.Trim(), true, out var extType))
                throw new Exception($"Invalid save format \"{Plugin.Instance.Config.StockDataSaveFormat}\".");

            string extension = $".{extType.ToString().ToLower()}";
            var fileName = $"stocks_{savecode}{extension}";
#pragma warning disable CS0618 // Type or member is obsolete
            var oldPath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), fileName);
#pragma warning restore CS0618 // Type or member is obsolete
            return Lib.SaveGame.MigrateOldSaveStructure(oldPath, saveGameArgs, $"sod_stockmarket_stocks.{extension}");
        }
    }

    internal interface IStocksContainer
    {
        IReadOnlyList<Stock> Stocks { get; }
    }
}
