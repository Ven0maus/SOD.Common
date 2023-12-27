using SOD.StockMarket.Implementation.Stocks;
using System;
using System.Collections.Generic;

namespace SOD.StockMarket.Implementation
{
    /// <summary>
    /// Returns pages of stocks for cruncher app
    /// </summary>
    internal sealed class StockPagination
    {
        internal int CurrentPage { get; private set; } = 0;

        private const int MaxStocksPerPage = 7;
        private readonly IReadOnlyList<Stock> _stocks;

        internal StockPagination(IReadOnlyList<Stock> stocks) 
        {
            _stocks = stocks;
        }

        /// <summary>
        /// Returns the next 7 stock slots.
        /// </summary>
        /// <returns></returns>
        internal Stock[] Next()
        {
            var stocks = new Stock[MaxStocksPerPage];
            var maxPages = (int)Math.Ceiling((double)_stocks.Count / MaxStocksPerPage);
            if (CurrentPage < maxPages)
            {
                CurrentPage++;
                SetStocks(stocks);
            }
            return stocks;
        }

        /// <summary>
        /// Returns the previous 7 stock slots.
        /// </summary>
        /// <returns></returns>
        internal Stock[] Previous()
        {
            var stocks = new Stock[MaxStocksPerPage];
            if (CurrentPage > 0)
            {
                CurrentPage--;
                SetStocks(stocks);
            }
            return stocks;
        }

        /// <summary>
        /// Resets the paging back to 0
        /// </summary>
        internal void Reset()
        {
            CurrentPage = 0;
        }

        private void SetStocks(Stock[] stocks)
        {
            var startIndex = CurrentPage * MaxStocksPerPage;
            for (int i = startIndex; i < MaxStocksPerPage; i++)
            {
                var stock = _stocks.Count > i ? _stocks[i] : null;
                stocks[i] = stock;
            }
        }
    }
}