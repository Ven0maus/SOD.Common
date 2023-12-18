using System;

namespace SOD.StockMarket.Core
{
    internal readonly struct StockTrend
    {
        /// <summary>
        /// Percentage difference
        /// </summary>
        internal double Percentage { get; }
        /// <summary>
        /// The price the trend started at
        /// </summary>
        internal decimal StartPrice { get; }
        /// <summary>
        /// The goal price for the trend
        /// </summary>
        internal decimal EndPrice { get; }
        /// <summary>
        /// Total steps to take to reach the end price.
        /// </summary>
        internal int Steps { get; }

        internal StockTrend(double percentage, decimal currentPrice, int steps) 
        {
            Percentage = percentage;
            StartPrice = currentPrice;
            Steps = steps;
            EndPrice = Math.Round(StartPrice + (StartPrice / 100m * (decimal)Percentage), 2);
        }
    }
}
