using SOD.Common.Shadows.Implementations;
using System.Collections.Generic;
using System.IO;

namespace SOD.StockMarket.Core.DataConversion.Converters
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
        public void Save(List<StockDataIO.StockDataDTO> data, MersenneTwisterRandom random, string path)
        {
            using var writer = new BinaryWriter(new FileStream(path, FileMode.Create, FileAccess.Write));

            // Save the header as metadata so we can see if the file we import is a valid binary file
            writer.Write("BinaryStockData");

            // Save random state
            var (index, mt) = random.SaveState();
            writer.Write(index);
            foreach (var value in mt)        
                writer.Write(value);

            // Save each record
            foreach (var record in data)
            {
                writer.Write(record.Id);
                WriteString(writer, record.Name);
                WriteString(writer, record.Symbol);
                WriteDateTime(writer, record.Date);
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
                var mt = new uint[624];
                for (int i = 0; i < mt.Length; i++)
                    mt[i] = reader.ReadUInt32();
                MathHelper.Init(new MersenneTwisterRandom((index, mt)));

                // Read each record
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    var stockData = new StockDataIO.StockDataDTO
                    {
                        Id = reader.ReadInt32(),
                        Name = ReadString(reader),
                        Symbol = ReadString(reader),
                        Date = ReadDateTime(reader),
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
                        Average = reader.ReadDecimal(),
                    };
                    stockDataList.Add(stockData);
                }
            }
            return stockDataList;
        }

        private static void WriteString(BinaryWriter writer, string value)
        {
            writer.Write(value ?? string.Empty);
        }

        private static string ReadString(BinaryReader reader)
        {
            return reader.ReadString();
        }

        private static void WriteDateTime(BinaryWriter writer, Time.TimeData value)
        {
            writer.Write(value.Serialize());
        }

        private static Time.TimeData ReadDateTime(BinaryReader reader)
        {
            return Time.TimeData.Deserialize(reader.ReadString());
        }

        private static void WriteNullableDecimal(BinaryWriter writer, decimal? value)
        {
            writer.Write(value ?? 0m);
        }

        private static decimal? ReadNullableDecimal(BinaryReader reader)
        {
            var value = reader.ReadDecimal();
            return value == 0m ? null : value;
        }

        private static void WriteNullableDouble(BinaryWriter writer, double? value)
        {
            writer.Write(value ?? 0.0);
        }

        private static double? ReadNullableDouble(BinaryReader reader)
        {
            var value = reader.ReadDouble();
            return value == 0.0 ? null : value;
        }

        private static void WriteNullableInt(BinaryWriter writer, int? value)
        {
            writer.Write(value ?? 0);
        }

        private static int? ReadNullableInt(BinaryReader reader)
        {
            var value = reader.ReadInt32();
            return value == 0 ? null : value;
        }
    }
}