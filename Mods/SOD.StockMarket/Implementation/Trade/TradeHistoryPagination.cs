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
                var histories = new TradeHistory[_maxTradeHistoryPerPage];
                SetHistories(histories);
                return histories;
            }
        }

        /// <summary>
        /// Returns the next 7 stock slots.
        /// </summary>
        /// <returns></returns>
        internal TradeHistory[] Next()
        {
            var histories = new TradeHistory[_maxTradeHistoryPerPage];
            var maxPages = (int)Math.Ceiling((double)_tradeController.TradeHistory.Count / _maxTradeHistoryPerPage);
            if (CurrentPage < maxPages - 1)
            {
                CurrentPage++;
                SetHistories(histories);
            }
            else if (CurrentPage == maxPages - 1)
            {
                CurrentPage = 0;
                SetHistories(histories);
            }
            return histories;
        }

        /// <summary>
        /// Returns the previous stock slots.
        /// </summary>
        /// <returns></returns>
        internal TradeHistory[] Previous()
        {
            var histories = new TradeHistory[_maxTradeHistoryPerPage];
            if (CurrentPage > 0)
            {
                CurrentPage--;
                SetHistories(histories);
            }
            else if (CurrentPage == 0)
            {
                var maxPages = (int)Math.Ceiling((double)_tradeController.TradeHistory.Count / _maxTradeHistoryPerPage);
                CurrentPage = Math.Max(0, maxPages - 1);
                SetHistories(histories);
            }
            return histories;
        }

        internal TradeHistory[] Reset()
        {
            var histories = new TradeHistory[_maxTradeHistoryPerPage];
            CurrentPage = 0;
            SetHistories(histories);
            return histories;
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

        private void SetHistories(TradeHistory[] slots)
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
                slots[i] = sortedCollection.Count > (startIndex + i) ? sortedCollection[startIndex + i] : null;
        }
    }
}
