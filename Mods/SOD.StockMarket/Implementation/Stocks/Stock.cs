using SOD.Common;
using SOD.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SOD.StockMarket.Implementation.Stocks
{
    internal class Stock
    {
        private static int _id;
        internal int Id { get; }
        internal string Name => _companyData.Name;
        internal string Symbol => _companyData.Symbol;
        internal double Volatility => _companyData.Volatility;
        internal decimal Price { get; private set; }
        internal decimal OpeningPrice { get; set; }
        internal decimal? ClosingPrice { get; set; }
        internal decimal HighPrice { get; private set; }
        internal decimal LowPrice { get; private set; }
        internal StockTrend? Trend { get; private set; }

        [JsonIgnore]
        internal decimal TodayDiff => Math.Round(Price - OpeningPrice, 2);

        [JsonIgnore]
        internal decimal DailyPercentage => GetPercentage(Price, OpeningPrice);

        [JsonIgnore]
        internal decimal? WeeklyPercentage
        {
            get
            {
                var currentDate = Lib.Time.CurrentDate;
                var weekHistorical = HistoricalData
                    .OrderByDescending(a => a.Date)
                    .FirstOrDefault(a => (currentDate - a.Date).TotalDays >= 7);
                if (weekHistorical == null) return null;
                return GetPercentage(Price, weekHistorical.Open);
            }
        }

        [JsonIgnore]
        internal decimal? MonthlyPercentage
        {
            get
            {
                var currentDate = Lib.Time.CurrentDate;
                var monthHistorical = HistoricalData
                    .OrderByDescending(a => a.Date)
                    .FirstOrDefault(a => (currentDate - a.Date).TotalDays >= 30);
                if (monthHistorical == null) return null;
                return GetPercentage(Price, monthHistorical.Open);
            }
        }

        internal IReadOnlyList<StockData> HistoricalData => _historicalData;

        private readonly List<StockData> _historicalData;
        private readonly CompanyStockData _companyData;
        private readonly decimal? _basePrice;
        private readonly bool _imported;

        internal Stock(Company company) : this()
        {
            _companyData = new CompanyStockData(company);
        }

        internal Stock(CompanyStockData companyData, decimal? basePrice = null) : this(basePrice: basePrice)
        {
            _companyData = companyData;
        }

        /// <summary>
        /// Called from a data import.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="historicalData"></param>
        internal Stock(StockDataIO.StockDataDTO dto, IEnumerable<StockData> historicalData) : this(dto.Id, dto.Price)
        {
            _companyData = new CompanyStockData(dto.Name, dto.Volatility.Value, dto.Symbol);

            // Set initial values
            Price = dto.Price.Value;
            OpeningPrice = dto.Open;
            ClosingPrice = dto.Close;
            LowPrice = dto.Low;
            HighPrice = dto.High;

            // Set trend if there is one
            if (dto.TrendPercentage != null && dto.TrendStartPrice != null &&
                dto.TrendEndPrice != null && dto.TrendSteps != null)
            {
                var trend = new StockTrend
                {
                    Percentage = dto.TrendPercentage.Value,
                    StartPrice = dto.TrendStartPrice.Value,
                    EndPrice = dto.TrendEndPrice.Value,
                    Steps = dto.TrendSteps.Value
                };
                Trend = trend;
            }

            // Add historical data to the stock
            foreach (var data in historicalData.OrderBy(a => a.Date))
                CreateHistoricalData(data);

            _imported = true;
        }

        internal Stock(Stock stock)
        {
            Id = stock.Id;
            _companyData = stock._companyData;
            Price = stock.Price;
            OpeningPrice = stock.OpeningPrice;
            ClosingPrice = stock.ClosingPrice;
            LowPrice = stock.LowPrice;
            HighPrice = stock.HighPrice;
            Trend = stock.Trend;
            _historicalData = new List<StockData>();
            foreach (var data in stock._historicalData)
                _historicalData.Add(new StockData(data));
            _basePrice = stock._basePrice;
            _imported = stock._imported;
        }

        private Stock(int? id = null, decimal? basePrice = null)
        {
            Id = id ?? _id++;
            _historicalData = new List<StockData>();
            _basePrice = basePrice;
            _imported = false;
        }

        private static decimal GetPercentage(decimal currentPrice, decimal openingPrice)
        {
            double percentageChange;
            if (openingPrice != 0)
            {
                percentageChange = (double)((currentPrice - openingPrice) / openingPrice * 100);
            }
            else
            {
                // Handle the case when openingPrice is zero
                if (currentPrice > 0)
                {
                    // If currentPrice is positive, consider percentage change as infinite
                    percentageChange = double.PositiveInfinity;
                }
                else if (currentPrice < 0)
                {
                    // If currentPrice is negative, consider percentage change as negative infinite
                    percentageChange = double.NegativeInfinity;
                }
                else
                {
                    // If currentPrice is also zero, consider percentage change as zero
                    percentageChange = 0;
                }
            }
            return Math.Round((decimal)percentageChange, 2);
        }

        // Total steps is 60, until it reaches the next hour
        private int _currentStep = 0;
        internal void DeterminePrice()
        {
            // Have a small chance not to change price at all (only if there is no trend going on)
            if ((Trend == null || _currentStep >= Trend.Value.Steps) && MathHelper.Random.Next(0, 99) < 10)
            {
                if (Trend != null && _currentStep >= Trend.Value.Steps)
                    RemoveTrend();
                return;
            }

            decimal? stockPrice = null;

            if (Trend != null)
            {
                var trend = Trend.Value;
                if (_currentStep >= trend.Steps)
                {
                    RemoveTrend();
                }
                else
                {
                    // Calculate the interpolation factor
                    decimal interpolationFactor = (decimal)_currentStep / trend.Steps;

                    // Linear interpolation between start and end prices
                    stockPrice = trend.StartPrice + (trend.EndPrice - trend.StartPrice) * interpolationFactor;

                    _currentStep++;
                    if (_currentStep >= trend.Steps)
                        RemoveTrend();
                }
            }

            // Fallback to default calculation, small fluctuations
            if (stockPrice == null)
            {
                // Add a small random flunctuation in the price based on a percentage
                decimal maxPercentageAdjustment = (decimal)Plugin.Instance.Config.PriceFluctuationPercentage * (decimal)Volatility;

                // Calculate the range based on the current price
                decimal range = Price * (maxPercentageAdjustment / 100m);

                // Generate a random adjustment within the calculated range
                stockPrice = Price + (decimal)(MathHelper.Random.NextDouble() * (double)range * 2) - range;
            }

            // Make sure stock price never falls under 0.1
            if (stockPrice == null || stockPrice <= 0m)
            {
                stockPrice = 0.01m;

                if (Trend != null && Trend.Value.EndPrice <= 0.01m)
                    RemoveTrend();
            }

            // Set price rounded
            Price = Math.Round(stockPrice.Value, 2);

            // Keep highest / lowest prices
            UpdateHighestLowestPrices();
        }

        internal void SetTrend(StockTrend stockTrend)
        {
            if (Trend != null) return;
            Trend = stockTrend;
        }

        internal void RemoveTrend()
        {
            // Update base price and current steps
            _currentStep = 0;
            Trend = null;
        }

        internal void CreateHistoricalData(StockData stockData = null, Time.TimeData? date = null)
        {
            if (stockData != null)
            {
                _historicalData.Add(stockData);
                return;
            }

            // Add new historical data entry
            var currentDate = date ?? Lib.Time.CurrentDate;
            _historicalData.Add(new StockData
            {
                Date = currentDate,
                Close = ClosingPrice,
                Open = OpeningPrice,
                Low = LowPrice,
                High = HighPrice,
                Trend = Trend
            });

            // Update for next day
            LowPrice = ClosingPrice.Value;
            HighPrice = ClosingPrice.Value;
        }

        internal int CleanUpHistoricalData(Time.TimeData? date)
        {
            var currentDate = date ?? Lib.Time.CurrentDate;
            var maxDays = Plugin.Instance.Config.DaysToKeepStockHistoricalData;

            // Remove all historical data that is atleast 30 days old
            return _historicalData.RemoveAll(stockData =>
            {
                var diff = currentDate - stockData.Date;
                if (diff.Days > maxDays)
                    return true;
                return false;
            });
        }

        internal void Initialize()
        {
            if (_imported)
                throw new Exception("An imported stock is already initialized, no need to call Initialize();");
            _companyData.Initialize();

            // Set initial prices
            Price = Math.Round(_basePrice ?? (_companyData.AverageSales / (_companyData.MinSalary + _companyData.TopSalary) * 1000m), 2);
            OpeningPrice = Price;
            LowPrice = Price;
            HighPrice = Price;
        }

        private void UpdateHighestLowestPrices()
        {
            if (LowPrice > Price)
                LowPrice = Price;
            else if (HighPrice < Price)
                HighPrice = Price;
        }
    }
}
