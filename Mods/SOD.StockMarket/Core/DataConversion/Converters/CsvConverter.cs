using SOD.Common.Shadows.Implementations;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SOD.StockMarket.Core.DataConversion.Converters
{
    internal sealed class CsvConverter : IDataConverter
    {
        internal static CsvConverter Create()
        {
            return new CsvConverter();
        }

        /// <inheritdoc/>
        public void Save(List<StockDataIO.StockDataDTO> data, MersenneTwisterRandom random, string path)
        {
            using var writer = new StreamWriter(path);
            // Write the header
            writer.WriteLine("Id,Name,Symbol,Date,Price,Open,Close,High,Low,Volatility,TrendPercentage,TrendStartPrice,TrendEndPrice,TrendSteps,Average");

            // Write random state
            var (index, mt) = random.SaveState();
            writer.WriteLine($"{index}|{Convert.ToBase64String(ConvertUIntArrayToBytes(mt))}");

            // Write each record
            foreach (var record in data)
            {
                writer.WriteLine(
                    $"{record.Id}," +
                    $"{EscapeCsvField(record.Name)}," +
                    $"{EscapeCsvField(record.Symbol)}," +
                    $"{EscapeCsvField(record.Date.Serialize())}," +
                    $"{record.Price?.ToString(CultureInfo.InvariantCulture) ?? string.Empty}," +
                    $"{record.Open.ToString(CultureInfo.InvariantCulture)}," +
                    $"{(record.Close != null ? record.Close.Value.ToString(CultureInfo.InvariantCulture) : string.Empty)}," +
                    $"{record.High.ToString(CultureInfo.InvariantCulture)}," +
                    $"{record.Low.ToString(CultureInfo.InvariantCulture)}," +
                    $"{(record.Volatility != null ? record.Volatility.Value.ToString(CultureInfo.InvariantCulture) : string.Empty)}," +
                    $"{(record.TrendPercentage != null ? record.TrendPercentage.Value.ToString(CultureInfo.InvariantCulture) : string.Empty)}," +
                    $"{(record.TrendStartPrice != null ? record.TrendStartPrice.Value.ToString(CultureInfo.InvariantCulture) : string.Empty)}," +
                    $"{(record.TrendEndPrice != null ? record.TrendEndPrice.Value.ToString(CultureInfo.InvariantCulture) : string.Empty)}," +
                    $"{(record.TrendSteps != null ? record.TrendSteps.Value.ToString(CultureInfo.InvariantCulture) : string.Empty)}," +
                    $"{record.Average.ToString(CultureInfo.InvariantCulture)}");
            }
        }

        /// <inheritdoc/>
        public List<StockDataIO.StockDataDTO> Load(string path)
        {
            var stockDataList = new List<StockDataIO.StockDataDTO>();
            using (var reader = new StreamReader(path))
            {
                // Skip the header line
                reader.ReadLine();

                // Second line is the random state
                if (!reader.EndOfStream)
                {
                    var randomState = reader.ReadLine();
                    var data = randomState.Split('|');
                    var index = int.Parse(data[0]);
                    var mt = ConvertByteArrayToUIntArray(Convert.FromBase64String(data[1]));
                    Helpers.Init(new MersenneTwisterRandom((index, mt)));
                }

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    var stockData = new StockDataIO.StockDataDTO
                    {
                        Id = int.Parse(values[0]),
                        Name = values[1],
                        Symbol = values[2],
                        Date = Time.TimeData.Deserialize(values[3]),
                        Price = ParseDecimal(values[4]),
                        Open = ParseDecimal(values[5]).Value,
                        Close = ParseDecimal(values[6]),
                        High = ParseDecimal(values[7]).Value,
                        Low = ParseDecimal(values[8]).Value,
                        Volatility = ParseDouble(values[9]),
                        TrendPercentage = ParseDouble(values[10]),
                        TrendStartPrice = ParseDecimal(values[11]),
                        TrendEndPrice = ParseDecimal(values[12]),
                        TrendSteps = ParseInt(values[13]),
                        Average = ParseDecimal(values[14]).Value,
                    };
                    stockDataList.Add(stockData);
                }
            }
            return stockDataList;
        }

        static byte[] ConvertUIntArrayToBytes(uint[] uintArray)
        {
            List<byte> byteList = new();
            foreach (uint value in uintArray)
                byteList.AddRange(BitConverter.GetBytes(value));
            return byteList.ToArray();
        }

        static uint[] ConvertByteArrayToUIntArray(byte[] byteArray)
        {
            if (byteArray.Length % 4 != 0)
                throw new ArgumentException("Byte array length is not a multiple of 4.", nameof(byteArray));
            int uintCount = byteArray.Length / 4;
            uint[] uintArray = new uint[uintCount];
            for (int i = 0; i < uintCount; i++)
                uintArray[i] = BitConverter.ToUInt32(byteArray, i * 4);
            return uintArray;
        }

        static decimal? ParseDecimal(string decimalValue)
        {
            if (string.IsNullOrWhiteSpace(decimalValue)) return null;
            if (!decimal.TryParse(decimalValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)) return null;
            return result;
        }

        static double? ParseDouble(string doubleValue)
        {
            if (string.IsNullOrWhiteSpace(doubleValue)) return null;
            if (!double.TryParse(doubleValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)) return null;
            return result;
        }

        static int? ParseInt(string intValue)
        {
            if (string.IsNullOrWhiteSpace(intValue)) return null;
            if (!int.TryParse(intValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)) return null;
            return result;
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
