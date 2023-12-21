using SOD.Common.Shadows.Implementations;
using SOD.StockMarket.Implementation.Stocks;
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
                // TODO: Write record value
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
                    // TODO: Create stock data
                }
            }
            return stockDataList;
        }
    }
}