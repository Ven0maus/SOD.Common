using SOD.StockMarket.Implementation.Stocks;
using System;

namespace SOD.StockMarket.Implementation
{
    /// <summary>
    /// Returns pages of stocks for cruncher app
    /// </summary>
    internal sealed class StockPagination
    {
        internal int CurrentPage { get; private set; } = 0;

        private const int MaxStocksPerPage = 7;
        private readonly Market _market;

        internal StockPagination(Market market) 
        {
            _market = market;
        }

        /// <summary>
        /// Returns the current selection
        /// </summary>
        internal Stock[] Current
        {
            get
            {
                var stocks = new Stock[MaxStocksPerPage];
                SetStocks(stocks);
                return stocks;
            }
        }

        /// <summary>
        /// Returns the next 7 stock slots.
        /// </summary>
        /// <returns></returns>
        internal Stock[] Next()
        {
            var stocks = new Stock[MaxStocksPerPage];
            var maxPages = (int)Math.Ceiling((double)_market.Stocks.Count / MaxStocksPerPage);
            if (CurrentPage < maxPages - 1)
            {
                CurrentPage++;
                SetStocks(stocks);
            }
            else if (CurrentPage == maxPages - 1)
            {
                CurrentPage = 0;
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
            else if (CurrentPage == 0)
            {
                var maxPages = (int)Math.Ceiling((double)_market.Stocks.Count / MaxStocksPerPage);
                CurrentPage = maxPages - 1;
                SetStocks(stocks);
            }
            return stocks;
        }

        internal Stock[] Reset()
        {
            var stocks = new Stock[MaxStocksPerPage];
            CurrentPage = 0;
            SetStocks(stocks);
            return stocks;
        }

        private void SetStocks(Stock[] stocks)
        {
            var startIndex = CurrentPage * MaxStocksPerPage;
            for (int i = 0; i < MaxStocksPerPage; i++)
            {
                var stock = _market.Stocks.Count > (startIndex + i) ? _market.Stocks[startIndex + i] : null;
                stocks[i] = stock;
            }
        }
    }
}