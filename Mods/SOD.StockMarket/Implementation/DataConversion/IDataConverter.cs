using SOD.Common.Custom;
using SOD.StockMarket.Implementation.Stocks;
using System.Collections.Generic;

namespace SOD.StockMarket.Implementation.DataConversion
{
    internal interface IDataConverter
    {
        /// <summary>
        /// Save data to file path
        /// </summary>
        /// <param name="data"></param>
        /// <param name="path"></param>
        void Save(List<StockDataIO.StockDataDTO> data, MersenneTwister random, string path);
        /// <summary>
        /// Load data from file path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        List<StockDataIO.StockDataDTO> Load(string path);
    }
}
