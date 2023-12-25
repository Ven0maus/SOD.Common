using SOD.Common.Custom;
using SOD.Common.Helpers;
using SOD.StockMarket.Implementation.Stocks;
using SOD.StockMarket.Implementation.Trade;
using System.Collections.Generic;
using System.IO;

namespace SOD.StockMarket.Implementation.DataConversion.Converters
{
    /// <summary>
    /// Converter to convert stock data into/from binary files.
    /// </summary>
    internal sealed class BinaryConverter : IDataConverter
    {
        private BinaryConverter() { }

        /// <summary>
        /// Create's a new instance.
        /// </summary>
        /// <returns></returns>
        internal static BinaryConverter Create()
        {
            return new BinaryConverter();
        }

        /// <inheritdoc/>
        public void Save(List<StockDataIO.StockDataDTO> data, MersenneTwister random, string path)
        {
            using var writer = new BinaryWriter(new FileStream(path, FileMode.Create, FileAccess.Write));

            // Save the header as metadata so we can see if the file we import is a valid binary file
            writer.Write("BinaryStockData");

            // Save random state
            var (index, mt) = random.SaveState();
            writer.Write(index);
            foreach (var value in mt)
                writer.Write(value);

            // Save trade save data
            var tradeSaveData = data[0].TradeSaveData.ToJson();
            writer.Write(tradeSaveData);

            // Save each record, start at one because 0 is trade save data
            for (int i = 1; i < data.Count; i++)
            {
                var record = data[i];
                writer.Write(record.Id);
                writer.Write(record.Name);
                writer.Write(record.Symbol);
                WriteNullableTimeData(writer, record.Date);
                WriteNullableDecimal(writer, record.Price);
                writer.Write(record.Open);
                WriteNullableDecimal(writer, record.Close);
                writer.Write(record.High);
                writer.Write(record.Low);
                WriteNullableDouble(writer, record.Volatility);
                WriteNullableDouble(writer, record.TrendPercentage);
                WriteNullableDecimal(writer, record.TrendStartPrice);
                WriteNullableDecimal(writer, record.TrendEndPrice);
                WriteNullableInt(writer, record.TrendSteps);
                writer.Write(record.Average);
            }
        }

        /// <inheritdoc/>
        public List<StockDataIO.StockDataDTO> Load(string path)
        {
            var stockDataList = new List<StockDataIO.StockDataDTO>();
            using (var reader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read)))
            {
                // Read and verify the header
                var header = reader.ReadString();
                if (header != "BinaryStockData")
                    throw new InvalidDataException("Invalid binary file format.");

                // Read random state
                var index = reader.ReadInt32();
                var mt = new uint[MersenneTwister.ArraySize];
                for (int i = 0; i < mt.Length; i++)
                    mt[i] = reader.ReadUInt32();
                MathHelper.Init(new MersenneTwister((index, mt)));

                // Read trade save data
                var tradeSaveData = TradeSaveData.FromJson(reader.ReadString());
                stockDataList.Add(new StockDataIO.StockDataDTO { TradeSaveData = tradeSaveData });

                // Read each record
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    var stockData = new StockDataIO.StockDataDTO
                    {
                        Id = reader.ReadInt32(),
                        Name = reader.ReadString(),
                        Symbol = reader.ReadString(),
                        Date = ReadNullableTimeData(reader),
                        Price = ReadNullableDecimal(reader),
                        Open = reader.ReadDecimal(),
                        Close = ReadNullableDecimal(reader),
                        High = reader.ReadDecimal(),
                        Low = reader.ReadDecimal(),
                        Volatility = ReadNullableDouble(reader),
                        TrendPercentage = ReadNullableDouble(reader),
                        TrendStartPrice = ReadNullableDecimal(reader),
                        TrendEndPrice = ReadNullableDecimal(reader),
                        TrendSteps = ReadNullableInt(reader),
                        Average = reader.ReadDecimal()
                    };
                    stockDataList.Add(stockData);
                }
            }
            return stockDataList;
        }

        private static void WriteNullableTimeData(BinaryWriter writer, Time.TimeData? value)
        {
            writer.Write(value.HasValue);
            if (value.HasValue)
                writer.Write(value.Value.Serialize());
        }

        private static void WriteNullableDecimal(BinaryWriter writer, decimal? value)
        {
            writer.Write(value.HasValue);
            if (value.HasValue)
                writer.Write(value.Value);
        }

        private static void WriteNullableDouble(BinaryWriter writer, double? value)
        {
            writer.Write(value.HasValue);
            if (value.HasValue)
                writer.Write(value.Value);
        }

        private static void WriteNullableInt(BinaryWriter writer, int? value)
        {
            writer.Write(value.HasValue);
            if (value.HasValue)
                writer.Write(value.Value);
        }

        private static Time.TimeData? ReadNullableTimeData(BinaryReader reader)
        {
            if (reader.ReadBoolean())
                return Time.TimeData.Deserialize(reader.ReadString());
            return null;
        }

        private static decimal? ReadNullableDecimal(BinaryReader reader)
        {
            if (reader.ReadBoolean())
                return reader.ReadDecimal();
            return null;
        }

        private static double? ReadNullableDouble(BinaryReader reader)
        {
            if (reader.ReadBoolean())
                return reader.ReadDouble();
            return null;
        }

        private static int? ReadNullableInt(BinaryReader reader)
        {
            if (reader.ReadBoolean())
                return reader.ReadInt32();
            return null;
        }
    }
}