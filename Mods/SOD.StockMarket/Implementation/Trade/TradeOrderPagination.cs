using SOD.StockMarket.Implementation.Stocks;
using System;
using System.Linq;

namespace SOD.StockMarket.Implementation.Trade
{
    internal sealed class TradeOrderPagination
    {
        internal int CurrentPage { get; private set; } = 0;

        private readonly int _maxTradeOrdersPerPage;
        private readonly TradeController _tradeController;
        private Stock _forStockOnly;

        internal TradeOrderPagination(TradeController tradeController, int maxTradeOrdersPerPage)
        {
            _tradeController = tradeController;
            _maxTradeOrdersPerPage = maxTradeOrdersPerPage;
        }

        internal void SetForStockOnly(Stock stock)
        {
            _forStockOnly = stock;
        }

        /// <summary>
        /// Returns the current selection
        /// </summary>
        internal StockOrder[] Current
        {
            get
            {
                var stocks = new StockOrder[_maxTradeOrdersPerPage];
                SetStocks(stocks);
                return stocks;
            }
        }

        /// <summary>
        /// Returns the next 7 stock slots.
        /// </summary>
        /// <returns></returns>
        internal StockOrder[] Next()
        {
            var stocks = new StockOrder[_maxTradeOrdersPerPage];
            var maxPages = (int)Math.Ceiling((double)_tradeController.TradeOrders.Count / _maxTradeOrdersPerPage);
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
        internal StockOrder[] Previous()
        {
            var stocks = new StockOrder[_maxTradeOrdersPerPage];
            if (CurrentPage > 0)
            {
                CurrentPage--;
                SetStocks(stocks);
            }
            else if (CurrentPage == 0)
            {
                var maxPages = (int)Math.Ceiling((double)_tradeController.TradeOrders.Count / _maxTradeOrdersPerPage);
                CurrentPage = maxPages - 1;
                SetStocks(stocks);
            }
            return stocks;
        }

        internal StockOrder[] Reset()
        {
            var stocks = new StockOrder[_maxTradeOrdersPerPage];
            CurrentPage = 0;
            SetStocks(stocks);
            return stocks;
        }

        private void SetStocks(StockOrder[] stocks)
        {
            var orders = _tradeController.TradeOrders;
            if (_forStockOnly != null)
                orders = orders.Where(a => a.StockId == _forStockOnly.Id).ToList();

            if (orders.Count == 0) return;

            var startIndex = CurrentPage * _maxTradeOrdersPerPage;
            for (int i = 0; i < _maxTradeOrdersPerPage; i++)
            {
                var order = orders.Count > (startIndex + i) ? orders[startIndex + i] : null;
                if (order == null)
                {
                    stocks[i] = null;
                    continue;
                }

                var stock = _tradeController.Market.Stocks.FirstOrDefault(a => a.Id == order.StockId);
                if (stock == null)
                {
                    stocks[i] = null;
                    continue;
                }

                stocks[i] = new StockOrder(stock, order);
            }
        }
    }

    internal sealed class StockOrder
    {
        internal Stock Stock { get; }
        internal TradeOrder TradeOrder { get; }

        internal StockOrder(Stock stock, TradeOrder tradeOrder)
        {
            Stock = stock;
            TradeOrder = tradeOrder;
        }
    }
}
