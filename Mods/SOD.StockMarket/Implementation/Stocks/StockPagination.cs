using System;
using System.Linq;

namespace SOD.StockMarket.Implementation.Stocks
{
    /// <summary>
    /// Returns pages of stocks for cruncher app
    /// </summary>
    internal sealed class StockPagination
    {
        internal int CurrentPage { get; private set; } = 0;

        private readonly int _maxStocksPerPage;
        private readonly IStocksContainer _stockContainer;

        internal StockPagination(IStocksContainer stockContainer, int maxStocksPerPage)
        {
            _stockContainer = stockContainer;
            _maxStocksPerPage = maxStocksPerPage;
        }

        /// <summary>
        /// Returns the current selection
        /// </summary>
        internal Stock[] Current
        {
            get
            {
                var stocks = new Stock[_maxStocksPerPage];
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
            var stocks = new Stock[_maxStocksPerPage];
            var maxPages = (int)Math.Ceiling((double)_stockContainer.Stocks.Count / _maxStocksPerPage);
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
        /// Returns the previous stock slots.
        /// </summary>
        /// <returns></returns>
        internal Stock[] Previous()
        {
            var stocks = new Stock[_maxStocksPerPage];
            if (CurrentPage > 0)
            {
                CurrentPage--;
                SetStocks(stocks);
            }
            else if (CurrentPage == 0)
            {
                var maxPages = (int)Math.Ceiling((double)_stockContainer.Stocks.Count / _maxStocksPerPage);
                CurrentPage = maxPages - 1;
                SetStocks(stocks);
            }
            return stocks;
        }

        internal Stock[] Reset()
        {
            var stocks = new Stock[_maxStocksPerPage];
            CurrentPage = 0;
            SetStocks(stocks);
            return stocks;
        }

        private Func<Stock, object> _currentSortingProperty;
        private bool _sortAscending = true;
        internal void SortBy(Func<Stock, object> selector)
        {
            // If sorting by the same property, change order
            if (_currentSortingProperty == selector)
                _sortAscending = !_sortAscending;
            else
                _sortAscending = true;

            // Set new property
            _currentSortingProperty = selector;
        }

        private void SetStocks(Stock[] stocks)
        {
            if (_stockContainer.Stocks.Count == 0) return;

            // Do some sorting if applicable
            var sortedCollection = _stockContainer.Stocks;
            if (_currentSortingProperty != null)
            {
                sortedCollection = _sortAscending ?
                    sortedCollection.OrderBy(_currentSortingProperty).ToList() :
                    sortedCollection.OrderByDescending(_currentSortingProperty).ToList();
            }

            var startIndex = CurrentPage * _maxStocksPerPage;
            for (int i = 0; i < _maxStocksPerPage; i++)
            {
                var stock = sortedCollection.Count > (startIndex + i) ? sortedCollection[startIndex + i] : null;
                stocks[i] = stock;
            }
        }
    }
}