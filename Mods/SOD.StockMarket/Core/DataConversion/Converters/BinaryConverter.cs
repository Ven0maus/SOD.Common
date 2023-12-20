using System;
using System.Collections.Generic;

namespace SOD.StockMarket.Core.DataConversion.Converters
{
    internal class BinaryConverter : IDataConverter
    {
        internal static BinaryConverter Create()
        {
            return new BinaryConverter();
        }

        /// <inheritdoc/>
        public List<StockDataIO.StockDataDTO> Load(string path)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Save(List<StockDataIO.StockDataDTO> data, string path)
        {
            throw new NotImplementedException();
        }
    }
}
