using System;
using System.Collections.Generic;
using System.Linq;

namespace SOD.StockMarket.Implementation.Stocks
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
            string initials = GetSymbol(companyName);

            _companySymbolCount ??= new();

            // Check if the company already has a symbol
            if (_companySymbolCount.TryGetValue(initials, out int count))
            {
                count++;
                _companySymbolCount[initials] = count;
            }
            else
            {
                _companySymbolCount[initials] = 0;
            }

            // If the initials are 4 but the count is higher (so its a dupe)
            if (initials.Length == 4 && count > 0)
            {
                // Then take first 3 initials, and check again for the count
                initials = new string(initials.Take(3).ToArray());
                if (_companySymbolCount.TryGetValue(initials, out count))
                {
                    count++;
                    _companySymbolCount[initials] = count;
                }
                else
                {
                    _companySymbolCount[initials] = 0;
                }
            }

            // Make sure there is always atleast 3 letters in the symbol
            string stockSymbol = initials + count;
            if (stockSymbol.Length == 1)
                stockSymbol = initials + count.ToString("D3");
            else if (stockSymbol.Length == 2)
                stockSymbol = initials + count.ToString("D2");

            // Truncate to a maximum of 4 characters
            stockSymbol = stockSymbol[..Math.Min(stockSymbol.Length, 4)];

            return stockSymbol.ToUpper();
        }

        internal static void Clear()
        {
            _companySymbolCount = null;
        }

        private static string GetSymbol(string companyName)
        {
            // Extract the first letter of each word in the company name
            char[] blockedChars = new[] { '-', '_', '&' };
            string[] words = companyName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(a => a.Length > 1) // Skip one length words (usually they are blocked chars)
                .ToArray();
            if (words.Length == 0)
                throw new Exception("Missing company name.");
            else if (words.Length == 1) // Return first 4 letters if there is only one word
                return new string(words[0]
                    .Where(a => !blockedChars.Contains(a))
                    .Take(4)
                    .ToArray());

            // Return first 2 letters of first word, and 1st letter of last
            return new string(words[0]
                .Where(a => !blockedChars.Contains(a))
                .Take(2)
                .Concat(words[1]
                    .Where(a => !blockedChars.Contains(a))
                    .Take(2))
                    .ToArray());
        }
    }
}
