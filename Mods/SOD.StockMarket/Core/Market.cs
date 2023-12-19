using SOD.Common.Extensions;
using SOD.Common.Shadows;
using SOD.Common.Shadows.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SOD.StockMarket.Core
{
    internal class Market
    {
        private readonly List<Stock> _stocks;
        /// <summary>
        /// Contains all the stocks available within the stock market.
        /// </summary>
        internal IReadOnlyList<Stock> Stocks => _stocks;

        private readonly Dictionary<int, Dictionary<Stock, int>> _ownedStocks;
        /// <summary>
        /// Contains all the stocks and their amount owned by citizens id.
        /// </summary>
        internal IReadOnlyDictionary<int, IReadOnlyDictionary<Stock, int>> OwnedStocks => _ownedStocks
            .ToDictionary(a => a.Key, a => (IReadOnlyDictionary<Stock, int>)a.Value);

        private readonly Dictionary<int, decimal> _citizens;
        /// <summary>
        /// The money each citizen has including the player.
        /// </summary>
        internal IReadOnlyDictionary<int, decimal> Citizens => _citizens;

        internal bool Initialized { get; private set; } = false;

        private bool _interiorCreatorFinished = false;
        private bool _citizenCreatorFinished = false;
        private bool _cityConstructorFinalized = false;

        private static int OpeningHour => Plugin.Instance.Config.OpeningHour;
        private static int ClosingHour => Plugin.Instance.Config.ClosingHour;

        internal Market()
        {
            Plugin.Log.LogInfo("Initializing stock market.");
            // TODO: Init from savegame if it exists
            _stocks = new List<Stock>();
            _ownedStocks = new Dictionary<int, Dictionary<Stock, int>>();
            _citizens = new Dictionary<int, decimal>();
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

            // We need to wait until all 3 processes are completely finished initializing
            if (Initialized || !_citizenCreatorFinished || !_interiorCreatorFinished || !_cityConstructorFinalized)
                return;

            // Init helper
            Helpers.Init(UnityEngine.Random.seed);

            // Also add some default game related stocks and update prices
            AddIconicStocks();

            // Init the stocks
            foreach (var stock in _stocks)
                stock.Initialize();

            // Add player to the citizens
            _citizens.Add(Player.Instance.humanID, GameplayController.Instance.money);

            // TODO: Save current stocks

            // Market is finished initializing stocks
            if (Plugin.Instance.Config.IsDebugEnabled)
                Plugin.Log.LogInfo("Stocks created: " + _stocks.Count);

            // Subscribe to events
            Lib.Time.OnMinuteChanged += InitializeHistoricalData;
            Lib.Time.OnMinuteChanged += (sender, args) => OnMinuteChanged(args);
            Lib.Time.OnHourChanged += (sender, args) => OnHourChanged();

            Plugin.Log.LogInfo("Stock market initialized.");
            Initialized = true;
        }

        /// <summary>
        /// Add's a new stock for the given company into the market.
        /// </summary>
        /// <param name="company"></param>
        internal void AddStock(Stock stock)
        {
            if (Initialized) return;
            _stocks.Add(stock);
        }

        internal void BuyStock(Human human, Stock stock, int amount)
        {
            // A dead citizen shouldn't be able to buy new stocks
            if (human.isDead || human.currentHealth <= 0f) return;

            // Check if human can afford stock
            var money = GetCitizenMoney(human);
            if (money < stock.Price * amount)
            {
                // Calculate the maximum amount of stock the human can purchase
                var amountToBuy = (int)Math.Floor(money / stock.Price);
                if (amountToBuy == 0) return;
                amount = amountToBuy;
            }

            // Add stock dictionary if it doesn't yet exist
            if (!_ownedStocks.TryGetValue(human.humanID, out var stocks))
                _ownedStocks[human.humanID] = stocks = new Dictionary<Stock, int>();

            // Add stock and amount
            if (!stocks.ContainsKey(stock))
                stocks.Add(stock, amount);
            else
                stocks[stock] += amount;

            // Deduct the money from the human
            AddCitizenMoney(human, -stock.Price * amount);
        }

        internal void SellStock(Human human, Stock stock, int amount)
        {
            if (!_ownedStocks.TryGetValue(human.humanID, out var stocks))
                return;
            if (!stocks.TryGetValue(stock, out var stockAmount))
                return;

            int totalStocksToCashOut = amount;
            if (stockAmount <= amount)
            {
                // Set stock to cashout to the correct amount and remove the stock
                totalStocksToCashOut = stockAmount;
                stocks.Remove(stock);

                // Remove left over dictionary instance if no stocks are left
                if (stocks.Count == 0)
                    _ownedStocks.Remove(human.humanID);
            }
            else if (stockAmount > amount)
            {
                // Deduct sold stocks
                stocks[stock] -= amount;
            }

            // Cash out the human
            AddCitizenMoney(human, stock.Price * totalStocksToCashOut);
        }

        private decimal GetCitizenMoney(Human human)
        {
            if (_citizens.TryGetValue(human.humanID, out decimal money))
                return money;
            
            // Add citizen and his money.
            _citizens.Add(human.humanID, money);
            return money;
        }

        private void AddCitizenMoney(Human human, decimal amount)
        {
            if (!_citizens.ContainsKey(human.humanID))
                _citizens.Add(human.humanID, amount);
            else
                _citizens[human.humanID] += amount;
        }

        /// <summary>
        /// Adds some hardcoded game related stocks.
        /// </summary>
        private void AddIconicStocks()
        {
            var customCompanies = new (CompanyData data, decimal? basePrice)[] 
            {
                (new CompanyData("Starch Kola", "STK", 0.4d), (decimal)Toolbox.Instance.Rand(5000f, 10000f, true)),
                (new CompanyData("Kaizen", "KAI", 0.3d), (decimal)Toolbox.Instance.Rand(2000f, 5000f, true)),
                (new CompanyData("Crow", "CRO", 0.05d), (decimal)Toolbox.Instance.Rand(0.85f, 1.25f, true))
            };
            foreach (var (data, basePrice) in customCompanies)
                AddStock(new Stock(data, basePrice));
        }

        private void InitializeHistoricalData(object sender, TimeChangedArgs e)
        {
            // Unsubscribe after first call
            Lib.Time.OnMinuteChanged -= InitializeHistoricalData;

            int totalEntries = 0;
            var currentDate = Lib.Time.CurrentDate;
            int totalDays = Plugin.Instance.Config.DaysToKeepStockHistoricalData;
            foreach (var stock in _stocks)
            {
                StockData previous = null;
                for (int i = totalDays; i >= 0; i--)
                {
                    var newDate = currentDate.AddDays(-i);
                    var newStockData = new StockData
                    {
                        Date = newDate,
                        Open = previous?.Close ?? stock.Price
                    };

                    var sizeRange = stock.Volatility;
                    newStockData.Close = Math.Round(newStockData.Open + newStockData.Open / 100m * (decimal)Toolbox.Instance.Rand(-20f * (float)stock.Volatility, 21f * (float)stock.Volatility, true), 2);
                    if (newStockData.Close <= 0m)
                        newStockData.Close = 0.01m;
                    newStockData.Low = Math.Round(newStockData.Close + newStockData.Close / 100m * (decimal)Toolbox.Instance.Rand(-20f * (float)stock.Volatility, 1, true), 2);
                    if (newStockData.Low <= 0m)
                        newStockData.Low = 0.01m;
                    newStockData.High = Math.Round(newStockData.Close + newStockData.Close / 100m * (decimal)Toolbox.Instance.Rand(0f, 21f * (float)stock.Volatility, true), 2);
                    if (newStockData.High <= 0m)
                        newStockData.High = 0.01m;

                    stock.CreateHistoricalData(newStockData);
                    previous = newStockData;
                    totalEntries++;
                }

                if (Plugin.Instance.Config.IsDebugEnabled)
                    Plugin.Log.LogInfo($"Stock({stock.Symbol}) {stock.Name} [HA] | " +
                        $"Volatility ({stock.Volatility}) | " +
                        $"Close ({Math.Round(stock.HistoricalData.Average(a => a.Close), 2)}) | " +
                        $"Open ({Math.Round(stock.HistoricalData.Average(a => a.Open), 2)}) | " +
                        $"High ({Math.Round(stock.HistoricalData.Average(a => a.High), 2)}) | " +
                        $"Low ({Math.Round(stock.HistoricalData.Average(a => a.Low), 2)}).");
            }

            if (Plugin.Instance.Config.IsDebugEnabled)
                Plugin.Log.LogInfo($"Initialized {totalEntries} historical data entries.");

            GenerateTrends();
        }

        private int _minutesPassed;
        /// <summary>
        /// Updates the stock market based on the current trends.
        /// </summary>
        private void OnMinuteChanged(TimeChangedArgs args)
        {
            // Don't execute calculations when the stock market is closed
            if (!IsOpen()) return;

            // If there is no hour change, we do a new update of the prices
            // When the hour changes, we need to update prices after setting opening price
            // If this is indeed applicable it will be handled within OnHourChanged
            if (!args.IsHourChanged)
            {
                // Execute everything to simulate the economy each in game minute
                Calculate();
            }

            // Logging every 1 hour
            _minutesPassed++;
            if (_minutesPassed % 60 == 0)
            {
                _minutesPassed = 0;
                if (Plugin.Instance.Config.IsDebugEnabled)
                {
                    Plugin.Log.LogInfo($"- New stock updates -");
                    foreach (var stock in _stocks)
                        Plugin.Log.LogInfo($"Stock: \"({stock.Symbol}) {stock.Name}\" | Price: {stock.Price}.");
                    Plugin.Log.LogInfo($"- End of Stocks -");
                }
            }
        }

        private void OnHourChanged()
        {
            var currentTime = Lib.Time.CurrentDateTime;
            if (Plugin.Instance.Config.IsDebugEnabled)
                Plugin.Log.LogInfo("Current time: " + currentTime.ToString());

            // Set the opening / closing price for each stock
            if (currentTime.Hour == OpeningHour)
            {
                OnOpen();

                // Update prices after the opening price was set
                Calculate();
            }
            else if (currentTime.Hour == ClosingHour)
            {
                // We don't need to calculate the last tick
                OnClose();
            }

            if (!IsOpen()) return;

            // Generate hourly trends
            GenerateTrends();
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
            var currentDay = Lib.Time.CurrentWeekDay;
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
            _stocks.ForEach(a => a.OpeningPrice = a.ClosingPrice);
            if (Plugin.Instance.Config.IsDebugEnabled)
                Plugin.Log.LogInfo("Stock market is opening.");
        }

        /// <summary>
        /// Calculates the stocks. (each in-game minute)
        /// </summary>
        private void Calculate()
        {
            foreach (var stock in _stocks)
            {
                _ = stock.DeterminePrice();
            }
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
                var chance = Helpers.Random.NextDouble() * 100 < trendChancePerStock;
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
                            decimal previousClose = stock.HistoricalData[i - 1].Close;
                            decimal currentClose = stock.HistoricalData[i].Close;

                            double percentageChange = (double)((currentClose - previousClose) / previousClose * 100);
                            historicalPercentageChanges.Add(percentageChange);
                        }

                        // Calculate mean and standard deviation for the current stock
                        stockMean = historicalPercentageChanges.Average();
                        stockStdDev = Helpers.CalculateStandardDeviation(historicalPercentageChanges);
                        historicalPercentageChanges.Clear();
                    }
                    else
                    {
                        // Use a uniform mean and deviation if there is no historical data yet
                        stockMean = 0.0d;
                        stockStdDev = 0.3d;
                    }

                    // Generate a realistic percentage change using a normal distribution
                    double percentage = Math.Round(Helpers.NextGaussian(stockMean, stockStdDev));

                    // Ensure the generated percentage is within a reasonable range
                    //percentage = Math.Clamp(percentage, -25, 25);

                    // Skip 0 percentage differences
                    if (((int)percentage) == 0) continue; 

                    // Total steps to full-fill trend (1 step = 1 in game minute)
                    int steps = Helpers.Random.Next(60 * minTrendSteps, 60 * maxTrendSteps);

                    // Generate the stock trend entry
                    var stockTrend = new StockTrend(percentage, stock.Price, steps);
                    stock.SetTrend(stockTrend);
                    trendsGenerated++;

                    if (debugModeEnabled)
                        Plugin.Log.LogInfo($"[NEW TREND]: \"({stock.Symbol}) {stock.Name}\" | Price: {stockTrend.StartPrice} | Target {stockTrend.EndPrice} | Percentage: {Math.Round(stockTrend.Percentage, 2)} | MinutesLeft: {stockTrend.Steps}");
                }
            }

            if (trendsGenerated > 0 && Plugin.Instance.Config.IsDebugEnabled && Lib.Time.IsInitialized)
                Plugin.Log.LogInfo($"[GameTime({Lib.Time.CurrentDateTime})] Created {trendsGenerated} new trends.");
        }
    }
}
