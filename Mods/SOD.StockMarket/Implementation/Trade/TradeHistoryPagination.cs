using SOD.StockMarket.Implementation.Stocks;
using System;
using System.Linq;

namespace SOD.StockMarket.Implementation.Trade
{
    internal sealed class TradeHistoryPagination
    {
        internal int CurrentPage { get; private set; } = 0;

        private readonly int _maxTradeHistoryPerPage;
        private readonly TradeController _tradeController;

        internal TradeHistoryPagination(TradeController tradeController, int maxTradeHistoryPerPage)
        {
            _tradeController = tradeController;
            _maxTradeHistoryPerPage = maxTradeHistoryPerPage;
        }

        /// <summary>
        /// Returns the current selection
        /// </summary>
        internal TradeHistory[] Current
        {
            get
            {
                var stocks = new TradeHistory[_maxTradeHistoryPerPage];
                SetStocks(stocks);
                return stocks;
            }
        }

        /// <summary>
        /// Returns the next 7 stock slots.
        /// </summary>
        /// <returns></returns>
        internal TradeHistory[] Next()
        {
            var stocks = new TradeHistory[_maxTradeHistoryPerPage];
            var maxPages = (int)Math.Ceiling((double)_tradeController.TradeOrders.Count / _maxTradeHistoryPerPage);
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
        internal TradeHistory[] Previous()
        {
            var stocks = new TradeHistory[_maxTradeHistoryPerPage];
            if (CurrentPage > 0)
            {
                CurrentPage--;
                SetStocks(stocks);
            }
            else if (CurrentPage == 0)
            {
                var maxPages = (int)Math.Ceiling((double)_tradeController.TradeOrders.Count / _maxTradeHistoryPerPage);
                CurrentPage = maxPages - 1;
                SetStocks(stocks);
            }
            return stocks;
        }

        internal TradeHistory[] Reset()
        {
            var stocks = new TradeHistory[_maxTradeHistoryPerPage];
            CurrentPage = 0;
            SetStocks(stocks);
            return stocks;
        }

        private Func<TradeHistory, object> _currentSortingProperty;
        private bool _sortAscending = true;
        internal void SortBy(Func<TradeHistory, object> selector, bool? ascending = null)
        {
            // If sorting by the same property, change order
            if (_currentSortingProperty == selector)
                _sortAscending = !_sortAscending;
            else
                _sortAscending = true;

            // Set new property
            _currentSortingProperty = selector;

            // Override if applicable
            if (ascending != null)
                _sortAscending = ascending.Value;
        }

        private void SetStocks(TradeHistory[] slots)
        {
            if (_tradeController.TradeHistory.Count == 0) return;

            // Do some sorting if applicable
            var sortedCollection = _tradeController.TradeHistory;
            if (_currentSortingProperty != null)
            {
                sortedCollection = _sortAscending ?
                    sortedCollection.OrderBy(_currentSortingProperty).ToList() :
                    sortedCollection.OrderByDescending(_currentSortingProperty).ToList();
            }

            var startIndex = CurrentPage * _maxTradeHistoryPerPage;
            for (int i = 0; i < _maxTradeHistoryPerPage; i++)
            {
                var history = sortedCollection.Count > (startIndex + i) ? sortedCollection[startIndex + i] : null;
                if (history == null)
                {
                    slots[i] = null;
                    continue;
                }
                slots[i] = history;
            }
        }
    }
}
