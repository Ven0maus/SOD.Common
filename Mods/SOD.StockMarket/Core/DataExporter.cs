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
                foreach (var history in stock.HistoricalData.OrderBy(a => a.Date))
                {
                    var dto = new StockDataDTO
                    {
                        Name = stock.Name,
                        Symbol = stock.Symbol,
                        Date = history.Date,
                        Open = history.Open,
                        Close = history.Close,
                        High = history.High,
                        Low = history.Low
                    };
                    dataDump.Add(dto);
                }
            }

            // Write dataDump to CSV file
            using var writer = new StreamWriter(path);
            // Write the header
            writer.WriteLine("Name,Symbol,Date,Open,Close,High,Low,Average");

            // Write each record
            foreach (var record in dataDump)
            {
                writer.WriteLine(
                    $"{EscapeCsvField(record.Name)}," +
                    $"{EscapeCsvField(record.Symbol)}," +
                    $"{EscapeCsvField(record.Date.ToString())}," +
                    $"{record.Open.ToString(CultureInfo.InvariantCulture)}," +
                    $"{record.Close.ToString(CultureInfo.InvariantCulture)}," +
                    $"{record.High.ToString(CultureInfo.InvariantCulture)}," +
                    $"{record.Low.ToString(CultureInfo.InvariantCulture)}," +
                    $"{record.Average.ToString(CultureInfo.InvariantCulture)}");
            }

            Plugin.Log.LogInfo($"Exported {dataDump.Count} stock market data rows.");
        }

        class StockDataDTO
        {
            public string Name { get; set; }
            public string Symbol { get; set; }
            public Time.TimeData Date { get; set; }
            public decimal Open { get; set; }
            public decimal Close { get; set; }
            public decimal High { get; set; }
            public decimal Low { get; set; }
            public decimal Average { get { return (Open + Close + High + Low) / 4m; } }
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
