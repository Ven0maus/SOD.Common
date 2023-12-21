using SOD.Common.Shadows;
using SOD.Common.Shadows.Implementations;
using SOD.StockMarket.Core.DataConversion;
using System.Collections.Generic;
using System.Linq;

namespace SOD.StockMarket.Core
{
    internal sealed class StockDataIO
    {
        private readonly Market _market;

        internal StockDataIO(Market market) 
        {
            _market = market;
        }

        /// <summary>
        /// Exports the stock data from the market.
        /// </summary>
        /// <param name="market"></param>
        /// <param name="path"></param>
        public void Export(string path)
        {
            if (!_market.Initialized)
            {
                Plugin.Log.LogWarning("Cannot export stock market data, market not yet initialized.");
                return;
            }

            var dataDump = new List<StockDataDTO>();
            foreach (var stock in _market.Stocks.OrderBy(a => a.Symbol))
            {
                // Dump first the current state of the stock
                var currentDate = stock.HistoricalData.LastOrDefault()?.Date;
                var mainDto = new StockDataDTO
                {
                    Id = stock.Id,
                    Name = stock.Name,
                    Symbol = stock.Symbol,
                    Date = currentDate ?? new Time.TimeData(0, 0, 0, 0, 0),
                    Price = stock.Price,
                    Open = stock.OpeningPrice,
                    Close = stock.ClosingPrice,
                    High = stock.HighPrice,
                    Low = stock.LowPrice,
                    Volatility = stock.Volatility,
                    TrendPercentage = stock.Trend?.Percentage,
                    TrendStartPrice = stock.Trend?.StartPrice,
                    TrendEndPrice = stock.Trend?.EndPrice,
                    TrendSteps = stock.Trend?.Steps,
                    Average = (stock.HighPrice + stock.LowPrice + stock.Price) / 3m,
                };
                dataDump.Add(mainDto);

                // Next dump all the historical data
                foreach (var history in stock.HistoricalData.OrderBy(a => a.Date))
                {
                    var historicalDto = new StockDataDTO
                    {
                        Id = stock.Id,
                        Name = stock.Name,
                        Symbol = stock.Symbol,
                        Date = history.Date,
                        Open = history.Open,
                        Close = history.Close,
                        High = history.High,
                        Low = history.Low,
                        Average = (history.Open + history.Close.Value + history.High + history.Low) / 4m,
                        Price = null,
                        Volatility = null,
                        TrendPercentage = history.Trend?.Percentage,
                        TrendStartPrice = history.Trend?.StartPrice,
                        TrendEndPrice = history.Trend?.EndPrice,
                        TrendSteps = history.Trend?.Steps,
                    };
                    dataDump.Add(historicalDto);
                }
            }

            // Convert and export data
            var converter = ConverterFactory.Get(path);
            converter.Save(dataDump, Helpers.Random, path);

            Plugin.Log.LogInfo($"Exported {dataDump.Count} stock market data rows.");
        }

        /// <summary>
        /// Imports the stock data into the market.
        /// </summary>
        /// <param name="market"></param>
        /// <param name="path"></param>
        public void Import(string path)
        {
            if (_market.Initialized)
            {
                Plugin.Log.LogWarning("Cannot import stock market data, market is already initialized.");
                return;
            }

            Plugin.Log.LogInfo("Loading stock data..");

            // Convert and import data
            var converter = ConverterFactory.Get(path);
            var stockDtos = converter.Load(path);

            // Each stockdto that doesn't have a price is an historical data entry, create a dictionary lookup on stock Id.
            var historicalDatas = stockDtos
                .Where(a => a.Price == null)
                .GroupBy(a => a.Id)
                .Select(a => 
                {
                    var stockDatas = a.Select(a => 
                    {
                        var data = new StockData
                        {
                            Close = a.Close,
                            High = a.High,
                            Low = a.Low,
                            Date = a.Date,
                            Open = a.Open,
                        };
                        if (a.TrendPercentage != null && a.TrendStartPrice != null &&
                            a.TrendEndPrice != null && a.TrendSteps != null)
                        {
                            var trend = new StockTrend
                            {
                                Percentage = a.TrendPercentage.Value,
                                StartPrice = a.TrendStartPrice.Value,
                                EndPrice = a.TrendEndPrice.Value,
                                Steps = a.TrendSteps.Value
                            };
                            data.Trend = trend;
                        }
                        return data;
                    });
                    return new { a.Key, Data = stockDatas.ToArray() };
                })
                .ToDictionary(a => a.Key, a => a.Data);

            // Import the actual stocks, each stockdto that has a price is the "most recent" version of the stock.
            foreach (var stockDto in stockDtos.Where(a => a.Price != null))
            {
                var stock = new Stock(stockDto, historicalDatas[stockDto.Id]);

                // Init the stock into the market
                _market.InitStock(stock);
            }

            if (Plugin.Instance.Config.IsDebugEnabled)
                Plugin.Log.LogInfo("Stocks data loaded: " + _market.Stocks.Count);

            _market.PostStocksInitialization(typeof(StockDataIO));
        }

        internal sealed class StockDataDTO
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Symbol { get; set; }
            public Time.TimeData Date { get; set; }
            public decimal Open { get; set; }
            public decimal? Close { get; set; }
            public decimal High { get; set; }
            public decimal Low { get; set; }
            public decimal Average { get; set; }
            public decimal? Price { get; set; }
            public double? Volatility { get; set; }
            public double? TrendPercentage { get; set; }
            public decimal? TrendStartPrice { get; set; }
            public decimal? TrendEndPrice { get; set; }
            public int? TrendSteps { get; set; }
        }
    }
}
