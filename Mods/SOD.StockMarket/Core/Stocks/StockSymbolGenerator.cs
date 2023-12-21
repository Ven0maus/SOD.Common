using System;
using System.Collections.Generic;
using System.Linq;

namespace SOD.StockMarket.Core.Stocks
{
    /// <summary>
    /// Generator for stock symbols
    /// </summary>
    internal static class StockSymbolGenerator
    {
        private static Dictionary<string, int> _companySymbolCount;

        /// <summary>
        /// Generates a new stock symbol of (max 4 length)
        /// </summary>
        /// <param name="companyName"></param>
        /// <returns></returns>
        internal static string Generate(string companyName)
        {
            // Get company initials
            string initials = GetInitials(companyName);

            if (_companySymbolCount == null)
                _companySymbolCount = new();

            // Check if the company already has a symbol
            if (_companySymbolCount.TryGetValue(initials, out int count))
            {
                count++;
                _companySymbolCount[initials] = count;
            }
            else
            {
                _companySymbolCount[initials] = 1;
            }

            // Generate the stock symbol
            string stockSymbol = initials + count.ToString("D2");

            // Truncate to a maximum of 4 characters
            stockSymbol = stockSymbol.Substring(0, Math.Min(stockSymbol.Length, 4));

            return stockSymbol.ToUpper();
        }

        internal static void Clear()
        {
            _companySymbolCount = null;
        }

        private static string GetInitials(string companyName)
        {
            // Extract the first letter of each word in the company name
            string[] words = companyName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return string.Join("", words.Select(word => word[0]));
        }
    }
}
