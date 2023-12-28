using SOD.Common.Helpers;
using SOD.StockMarket.Implementation.DataConversion;
using SOD.StockMarket.Implementation.Trade;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SOD.StockMarket.Implementation.Stocks
{
    /// <summary>
    /// Handles export and import to/from files of stock data into the market.
    /// </summary>
    internal static class StockDataIO
    {
        /// <summary>
        /// Exports the stock data from the market.
        /// </summary>
        /// <param name="market"></param>
        /// <param name="path"></param>
        internal static void Export(Market market, TradeController tradeController, string path, Market simulation = null)
        {
            if (!market.Initialized)
            {
                Plugin.Log.LogWarning("Cannot export stock market data, market not yet initialized.");
                return;
            }

            // Data dump list
            var dataDump = new List<StockDataDTO> 
            {
                new StockDataDTO { TradeSaveData = tradeController.Export() }
            };

            foreach (var stock in market.Stocks.OrderBy(a => a.Id))
            {
                // Dump first the current state of the stock
                var mainDto = new StockDataDTO
                {
                    Id = stock.Id,
                    Name = stock.Name,
                    Symbol = stock.Symbol,
                    Date = null,
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
                    Average = Math.Round((stock.HighPrice + stock.LowPrice + stock.Price) / 3m, 2),
                };

                // Add also the original price if its a simulation
                if (simulation != null)
                {
                    var matchingStock = simulation.Stocks.First(a => a.Id == stock.Id);
                    mainDto.OriginalPrice = matchingStock.Price;
                }

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
                        Average = Math.Round((history.Open + history.Close.Value + history.High + history.Low) / 4m),
                        Price = null,
                        Volatility = null,
                        TrendPercentage = history.Trend?.Percentage,
                        TrendStartPrice = history.Trend?.StartPrice,
                        TrendEndPrice = history.Trend?.EndPrice,
                        TrendSteps = history.Trend?.Steps,
                        OriginalPrice = null,
                    };
                    dataDump.Add(historicalDto);
                }
            }

            // Convert and export data
            var converter = ConverterFactory.Get(path);
            converter.Save(dataDump, MathHelper.Random, path, simulation != null);

            Plugin.Log.LogInfo($"Exported {dataDump.Count} stock market data rows.");
        }

        /// <summary>
        /// Imports the stock data into the market.
        /// </summary>
        /// <param name="market"></param>
        /// <param name="path"></param>
        internal static void Import(Market market, TradeController tradeController, string path)
        {
            if (market.Initialized)
            {
                Plugin.Log.LogWarning("Cannot import stock market data, market is already initialized.");
                return;
            }

            Plugin.Log.LogInfo("Loading stock data..");

            // Convert and import data
            var converter = ConverterFactory.Get(path);
            var stockDtos = converter.Load(path);

            // Each stock dto that doesn't have a price is a historical data entry, create a dictionary lookup on stock Id.
            var historicalDatas = stockDtos
                .Where(a => a.TradeSaveData == null && a.Price == null)
                .GroupBy(a => a.Id)
                .Select(a =>
                {
                    // Order from oldest to newest (last entry is the newest)
                    var stockDatas = a
                        .OrderBy(a => a.Date)
                        .Select(a =>
                    {
                        var data = new StockData
                        {
                            Close = a.Close,
                            High = a.High,
                            Low = a.Low,
                            Date = a.Date.Value,
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

            // Import the actual stocks, each stock dto that has a price is the "most recent" version of the stock.
            // Ordering by id is important to keep the random state working correctly.
            foreach (var stockDto in stockDtos.Where(a => a.TradeSaveData == null && a.Price != null).OrderBy(a => a.Id))
            {
                // Create a stock an init it
                var stock = new Stock(stockDto, historicalDatas[stockDto.Id]);
                market.InitStock(stock);
            }

            // Import data into trade controller
            var tradeSaveData = stockDtos[0].TradeSaveData;
            if (tradeSaveData != null)
            {
                Plugin.Log.LogInfo("Loading trade data..");
                tradeController.Import(tradeSaveData);
            }

            if (Plugin.Instance.Config.IsDebugEnabled)
            {
                Plugin.Log.LogInfo("Stocks data loaded: " + market.Stocks.Count);
                Plugin.Log.LogInfo($"- Loaded stocks -");
                foreach (var stock in market.Stocks.OrderBy(a => a.Id))
                    Plugin.Log.LogInfo($"Stock: \"({stock.Symbol}) {stock.Name}\" | Price: {stock.Price}.");
                Plugin.Log.LogInfo($"- End of Stocks -");
            }

            market.PostStocksInitialization(typeof(StockDataIO));
        }

        internal sealed class StockDataDTO
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Symbol { get; set; }
            public Time.TimeData? Date { get; set; }
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
            public TradeSaveData TradeSaveData { get; set; }
            /// <summary>
            /// Only used for simulations
            /// </summary>
            public decimal? OriginalPrice { get; set; }
            /// <summary>
            /// Only used for simulations
            /// </summary>
            public decimal SimulationChange { get { return Math.Round(Price.Value - OriginalPrice.Value, 2); } }
        }
    }
}
