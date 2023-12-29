using SOD.Common;
using SOD.Common.Helpers;
using SOD.StockMarket.Implementation.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SOD.StockMarket.Implementation.Trade
{
    /// <summary>
    /// Controller for all transactions of stocks
    /// </summary>
    internal class TradeController : IStocksContainer
    {
        internal readonly Market Market;

        // Player transactions
        private Dictionary<int, int> _playerStocks;
        private List<TradeOrder> _playerTradeOrders;
        private List<HistoricalPortfolio> _historicalPortfolio;

        internal decimal AvailableFunds { get; private set; }

        /// <summary>
        /// Money of the player
        /// </summary>
        internal static int Money
        {
            get => GameplayController.Instance.money;
            set
            {
                GameplayController.Instance.money = value;
                FirstPersonItemController.Instance.PlayerMoneyCheck();
                if (InterfaceControls.Instance.cashText != null)
                {
                    InterfaceControls.Instance.cashText.text = CityControls.Instance.cityCurrency + Money.ToString();
                    if (BioScreenController.Instance.cashText != null)
                    {
                        BioScreenController.Instance.cashText.text = InterfaceControls.Instance.cashText.text;
                    }
                }
            }
        }

        public IReadOnlyList<Stock> Stocks => _playerStocks.Join(Market.Stocks,
            playerStock => playerStock.Key,
            marketStock => marketStock.Id,
            (playerStock, marketStock) => marketStock)
            .ToList();

        public IReadOnlyList<TradeOrder> TradeOrders => _playerTradeOrders;
        public IReadOnlyList<HistoricalPortfolio> HistoricalPortfolioData => _historicalPortfolio;

        internal decimal TotalInvestedInStocks
        {
            get
            {
                decimal investment = 0;
                foreach (var item in Stocks)
                {
                    var amount = _playerStocks[item.Id];
                    investment += amount * item.Price;
                }

                // Add also the ongoing sell offers for their original market value
                investment += TradeOrders
                    .Where(a => a.OrderType == OrderType.Sell)
                    .Select(a => Market.Stocks[a.StockId].Price * a.Amount)
                    .Sum();
                return Math.Round(investment, 2);
            }
        }

        internal decimal PortfolioWorth
        {
            get
            {
                return Math.Round(TotalInvestedInStocks, 2);
            }
        }

        internal int GetInvestedVolume(Stock stock)
        {
            if (_playerStocks.TryGetValue(stock.Id, out var volume)) return volume;
            return 0;
        }

        /// <summary>
        /// Basic constructor, can be called at game start with no save data, or when loading a new game (from first init main menu).
        /// </summary>
        /// <param name="market"></param>
        /// <param name="saveData"></param>
        internal TradeController(Market market)
        {
            Market = market;
            Market.OnCalculate += Market_OnCalculate;

            // Initial data setup
            Import(null);
        }

        internal void CreatePortfolioHistoricalDataEntry()
        {
            var currentDate = Lib.Time.CurrentDate;
            if (_historicalPortfolio.Any(a => a.Date == currentDate)) return;
            _historicalPortfolio.Add(new HistoricalPortfolio(currentDate, PortfolioWorth));
        }

        internal decimal? GetPortfolioPercentageChange(int days)
        {
            var currentDate = Lib.Time.CurrentDate;
            var orderedEntries = HistoricalPortfolioData
                .OrderByDescending(a => a.Date)
                .ToArray();

            var previous = orderedEntries.FirstOrDefault(a => (currentDate - a.Date).TotalDays >= days)?.Worth ?? 0.01m;
            if (previous == 0)
                previous = 0.01m;

            var current = PortfolioWorth;

            if (current == 0 && previous == 0.01m)
                return 0;

            // Compare percentage to now
            return ((current - previous) / previous) * 100;
        }

        /// <summary>
        /// Resets all trade information, call this on a new game or loadgame.
        /// </summary>
        internal void Reset()
        {
            _playerStocks.Clear();
            _playerTradeOrders.Clear();
        }

        /// <summary>
        /// Adds money that can be invested into stocks into the market account
        /// </summary>
        /// <param name="money"></param>
        internal void DepositFunds(int money)
        {
            if (money <= 0) return;
            if (Money >= money)
            {
                AvailableFunds += money;
                Money -= money;
            }
        }

        /// <summary>
        /// Withdraws money from the market account into the games money balance
        /// </summary>
        /// <param name="money"></param>
        internal void WithdrawFunds(int money)
        {
            if (money <= 0) return;
            if (AvailableFunds >= money)
            {
                AvailableFunds -= money;
                Money += money;
            }
        }

        /// <summary>
        /// Initializes data from a save state.
        /// </summary>
        /// <param name="saveData"></param>
        internal void Import(TradeSaveData saveData)
        {
            _playerStocks = saveData?.PlayerStocks ?? new Dictionary<int, int>();
            _playerTradeOrders = saveData?.PlayerTradeOrders ?? new List<TradeOrder>();
            _historicalPortfolio = saveData?.HistoricalPortfolio ?? new List<HistoricalPortfolio>();
            AvailableFunds = saveData?.AvailableFunds ?? 0;
        }

        /// <summary>
        /// Returns the save state of this instance
        /// </summary>
        /// <returns></returns>
        internal TradeSaveData Export()
        {
            // Return a copy of the data
            return new TradeSaveData
            {
                PlayerStocks = _playerStocks.ToDictionary(a => a.Key, a => a.Value),
                PlayerTradeOrders = _playerTradeOrders.ToList(),
                HistoricalPortfolio = _historicalPortfolio.ToList(),
                AvailableFunds = AvailableFunds
            };
        }

        /// <summary>
        /// Buy's a certain amount of a certain stock immediately.
        /// <br>Validation is done on money of the player.</br>
        /// </summary>
        /// <param name="stock"></param>
        /// <param name="amount"></param>
        /// <returns>True/False depending if call succeeded or not.</returns>
        internal bool InstantBuy(Stock stock, int amount, bool deductMoney = true)
        {
            if (!IsValidOrder(OrderType.Buy, stock, amount)) return false;

            // Deduct money
            if (deductMoney)
            {
                // Calculate price for the current stock
                var totalPrice = Math.Round(stock.Price * amount, 2);
                AvailableFunds -= totalPrice;
            }

            // Set stocks
            if (_playerStocks.TryGetValue(stock.Id, out var total))
                _playerStocks[stock.Id] = total + amount;
            else
                _playerStocks[stock.Id] = amount;

            return true;
        }

        /// <summary>
        /// Sell's a certain amount of a certain stock immediately.
        /// <br>Validation is done if player owns enough of the stock.</br>
        /// </summary>
        /// <param name="stock"></param>
        /// <param name="amount"></param>
        /// <returns>True/False depending if call succeeded or not.</returns>
        internal bool InstantSell(Stock stock, int amount, bool removeStock = true)
        {
            // Check if player has this amount of stocks
            if (!IsValidOrder(OrderType.Sell, stock, amount)) return false;

            // Remove stocks
            if (removeStock)
            {
                _playerStocks[stock.Id] -= amount;
                if (_playerStocks[stock.Id] <= 0)
                    _playerStocks.Remove(stock.Id);
            }

            // Calculate price for the current stock
            var totalPrice = Math.Round(stock.Price * amount, 2);

            // Add money
            AvailableFunds += totalPrice;

            return true;
        }

        /// <summary>
        /// Add's an order for a specific price, when the price is reached the stock is bought for this price.
        /// </summary>
        /// <param name="stock"></param>
        /// <param name="price"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        internal bool BuyLimitOrder(Stock stock, decimal price, int amount)
        {
            if (!IsValidOrder(OrderType.Buy, stock, amount)) return false;

            // Since we are placing a buy limit order, we need to already deduct the money from the player
            // Calculate price for the current stock
            var totalPrice = Math.Round(stock.Price * amount, 2);
            AvailableFunds -= totalPrice;

            _playerTradeOrders.Add(new TradeOrder(OrderType.Buy, stock.Id, price, amount));
            return true;
        }

        /// <summary>
        /// Add's an order for a specific price, when the price is reached the stock is sold for this price.
        /// </summary>
        /// <param name="stock"></param>
        /// <param name="price"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        internal bool SellLimitOrder(Stock stock, decimal price, int amount)
        {
            if (!IsValidOrder(OrderType.Sell, stock, amount)) return false;
            _playerTradeOrders.Add(new TradeOrder(OrderType.Sell, stock.Id, price, amount));

            // Remove it from player stocks already
            _playerStocks[stock.Id] -= amount;
            if (_playerStocks[stock.Id] <= 0)
                _playerStocks.Remove(stock.Id);

            return true;
        }

        /// <summary>
        /// Cancel's a pending trade order.
        /// </summary>
        /// <param name="order"></param>
        internal void CancelOrder(TradeOrder order)
        {
            // If buy order, give back the money and cancel the order
            if (order.OrderType == OrderType.Buy)
                AvailableFunds += Math.Round(order.Price * order.Amount, 2);
            else if (order.OrderType == OrderType.Sell)
            {
                // Add stocks back into player stocks
                if (_playerStocks.TryGetValue(order.StockId, out var total))
                    _playerStocks[order.StockId] = total + order.Amount;
                else
                    _playerStocks[order.StockId] = order.Amount;
            }
            order.Complete();
            _playerTradeOrders.Remove(order);
        }

        private void Market_OnCalculate(object sender, EventArgs e)
        {
            foreach (var order in _playerTradeOrders)
            {
                if (order.Completed) continue;

                // Check if order can be full-filled.
                var stock = Market.Stocks[order.StockId];
                var currentPrice = stock.Price;
                if ((order.OrderType == OrderType.Buy && currentPrice <= order.Price) ||
                    (order.OrderType == OrderType.Sell && currentPrice >= order.Price))
                {
                    // Execute order
                    _ = order.OrderType switch
                    {
                        OrderType.Buy => InstantBuy(stock, order.Amount, false),
                        OrderType.Sell => InstantSell(stock, order.Amount, false),
                        _ => throw new NotImplementedException($"OrderType \"{order.OrderType}\" doesn't have an implementation."),
                    };

                    // Set completed
                    order.Complete();
                }
            }
            _playerTradeOrders.RemoveAll(a => a.Completed);
        }

        private bool IsValidOrder(OrderType orderType, Stock stock, int amount)
        {
            return orderType switch
            {
                OrderType.Buy => AvailableFunds >= Math.Round(stock.Price * amount, 2),
                OrderType.Sell => _playerStocks.TryGetValue(stock.Id, out var total) && total >= amount,
                _ => false,
            };
        }
    }
}
