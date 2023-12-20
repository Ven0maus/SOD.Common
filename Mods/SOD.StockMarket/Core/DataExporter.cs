using SOD.Common.Shadows.Implementations;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SOD.StockMarket.Core
{
    internal sealed class DataExporter
    {
        DataExporter() { }

        /// <summary>
        /// Does a full data dump of the market.
        /// </summary>
        /// <param name="market"></param>
        /// <param name="path"></param>
        public static void Export(Market market, string path)
        {
            if (!market.Initialized)
            {
                Plugin.Log.LogInfo("Cannot export stock market data, market not yet initialized.");
                return;
            }

            var dataDump = new List<StockDataDTO>();
            foreach (var stock in market.Stocks.OrderBy(a => a.Symbol))
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
                    Trend = stock.Trend != null ? stock.Trend.Value.Percentage : 0,
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
                        Price = null,
                        Open = history.Open,
                        Close = history.Close,
                        High = history.High,
                        Low = history.Low,
                        Trend = history.Trend != null ? history.Trend.Value.Percentage : 0,
                        Average = (history.Open + history.Close + history.High + history.Low) / 4m,
                    };
                    dataDump.Add(historicalDto);
                }
            }

            // Write dataDump to CSV file
            using var writer = new StreamWriter(path);
            // Write the header
            writer.WriteLine("Id,Name,Symbol,Date,Price,Open,Close,High,Low,Trend,Average");

            // Write each record
            foreach (var record in dataDump)
            {
                writer.WriteLine(
                    $"{record.Id}," +
                    $"{EscapeCsvField(record.Name)}," +
                    $"{EscapeCsvField(record.Symbol)}," +
                    $"{EscapeCsvField(record.Date.ToString())}," +
                    $"{record.Price?.ToString(CultureInfo.InvariantCulture) ?? string.Empty}," +
                    $"{record.Open.ToString(CultureInfo.InvariantCulture)}," +
                    $"{record.Close.ToString(CultureInfo.InvariantCulture)}," +
                    $"{record.High.ToString(CultureInfo.InvariantCulture)}," +
                    $"{record.Low.ToString(CultureInfo.InvariantCulture)}," +
                    $"{record.Trend.ToString(CultureInfo.InvariantCulture)}," +
                    $"{record.Average.ToString(CultureInfo.InvariantCulture)}");
            }

            Plugin.Log.LogInfo($"Exported {dataDump.Count} stock market data rows.");
        }

        class StockDataDTO
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Symbol { get; set; }
            public Time.TimeData Date { get; set; }
            public decimal Open { get; set; }
            public decimal Close { get; set; }
            public decimal High { get; set; }
            public decimal Low { get; set; }
            public decimal? Price { get; set; }
            public double Trend { get; set; }
            public decimal Average { get; set; }
        }

        static string EscapeCsvField(string field)
        {
            if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
            {
                // Enclose the field in double quotes and escape any existing double quotes
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            return field;
        }
    }
}
