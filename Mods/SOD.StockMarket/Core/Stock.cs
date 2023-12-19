using SOD.Common.Shadows;
using System;
using System.Collections.Generic;

namespace SOD.StockMarket.Core
{
    internal class Stock
    {
        internal string Name => _companyData.Name;
        internal string Symbol => _companyData.Symbol;
        internal decimal Price { get; private set; }
        internal decimal OpeningPrice { get; set; }
        internal decimal ClosingPrice { get; set; }
        internal double Volatility => _companyData.Volatility;
        internal StockTrend? Trend { get; private set; }
        internal IReadOnlyList<StockData> HistoricalData => _historicalData;

        private readonly List<StockData> _historicalData;
        private readonly CompanyData _companyData;

        private decimal? _lowestPrice;
        private decimal? _highestPrice;
        private readonly decimal? _basePrice;

        internal Stock(Company company) : this()
        {
            _companyData = new CompanyData(company);
        }

        internal Stock(CompanyData companyData, decimal? basePrice = null) : this(basePrice)
        {
            _companyData = companyData;
        }

        private Stock(decimal? basePrice = null)
        {
            _historicalData = new List<StockData>();
            _basePrice = basePrice;
        }

        // Total steps is 60, until it reaches the next hour
        private int _currentStep = 0;
        internal bool DeterminePrice()
        {
            // Have a small chance not to change price at all (only if there is no trend going on)
            if ((Trend == null || _currentStep >= Trend.Value.Steps) && Helpers.Random.Next(0, 100) < 10)
            {
                if (Trend != null && _currentStep >= Trend.Value.Steps)
                    RemoveTrend();
                return false;
            }

            decimal oldPrice = Price;
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
                stockPrice = Price + (decimal)(Helpers.Random.NextDouble() * (double)range * 2) - range;
            }

            // Make sure stock price never falls under 0.1
            if (stockPrice == null || stockPrice <= 0m)
            {
                stockPrice = 0.01m;
                RemoveTrend();
            }

            // Set price rounded
            Price = Math.Round(stockPrice.Value, 2);

            // Keep highest / lowest prices
            UpdateHighestLowestPrices();

            return oldPrice != Price;
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

        internal void CreateHistoricalData(StockData stockData = null)
        {
            if (stockData != null)
            {
                _historicalData.Add(stockData);
                return;
            }

            // Add new historical data entry
            var currentDate = Lib.Time.CurrentDate;
            _historicalData.Add(new StockData
            {
                Date = currentDate,
                Close = ClosingPrice,
                Open = OpeningPrice,
                Low = _lowestPrice.Value,
                High = _highestPrice.Value,
            });

            // Update for next day
            _lowestPrice = ClosingPrice;
            _highestPrice = ClosingPrice;
        }

        internal int CleanUpHistoricalData()
        {
            var currentDate = Lib.Time.CurrentDate;
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
            _companyData.UpdateInfo();

            // Set initial price
            Price = Math.Round(_basePrice ?? _companyData.AverageSales / (_companyData.MinSalary + _companyData.TopSalary) * 1000m / 10, 2);
            OpeningPrice = Price;
            _lowestPrice = Price;
            _highestPrice = Price;
        }

        private void UpdateHighestLowestPrices()
        {
            if (_lowestPrice == null || _lowestPrice > Price)
                _lowestPrice = Price;
            else if (_highestPrice == null || _highestPrice < Price)
                _highestPrice = Price;
        }
    }
}
