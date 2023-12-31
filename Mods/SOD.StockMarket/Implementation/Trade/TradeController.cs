﻿using SOD.Common;
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
        private Dictionary<int, decimal> _playerStocks;
        private List<TradeOrder> _playerTradeOrders;
        private List<HistoricalPortfolio> _historicalPortfolio;
        private List<TradeHistory> _tradeHistory;

        /// <summary>
        /// If true, will send an in-game notifications if a limit order has been bought/sold
        /// </summary>
        internal bool NotificationsEnabled { get; set; } = true;

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
        public IReadOnlyList<TradeHistory> TradeHistory => _tradeHistory;

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
                var stocks = Market.Stocks.ToDictionary(a => a.Id, a => a);
                investment += TradeOrders
                    .Where(a => a.OrderType == OrderType.Sell)
                    .Select(a => stocks[a.StockId].Price * a.Amount)
                    .Sum();
                return Math.Round(investment, 2);
            }
        }

        internal decimal PortfolioWorth
        {
            get
            {
                // All invested stocks + available funds + ongoing buy type trade orders
                var buyOrders = TradeOrders
                    .Where(a => a.OrderType == OrderType.Buy)
                    .Select(a => a.Price * a.Amount)
                    .Sum();
                return Math.Round(TotalInvestedInStocks + AvailableFunds + buyOrders, 2);
            }
        }

        internal decimal GetInvestedVolume(Stock stock)
        {
            return _playerStocks.TryGetValue(stock.Id, out var volume) ? volume : 0m;
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

            var previous = orderedEntries.FirstOrDefault(a => (currentDate - a.Date).TotalDays >= days)?.Worth;
            if (previous == null || previous == 0) return null;

            var current = PortfolioWorth;

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
            _historicalPortfolio.Clear();
            _tradeHistory.Clear();
            AvailableFunds = 0;
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
            _playerStocks = saveData?.PlayerStocks ?? new Dictionary<int, decimal>();
            _playerTradeOrders = saveData?.PlayerTradeOrders ?? new List<TradeOrder>();
            _historicalPortfolio = saveData?.HistoricalPortfolio ?? new List<HistoricalPortfolio>();
            _tradeHistory = saveData?.TradeHistory ?? new List<TradeHistory>();
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
                TradeHistory = _tradeHistory.ToList(),
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
        internal bool InstantBuy(Stock stock, decimal amount, bool deductMoney = true)
        {
            if (deductMoney && !IsValidOrder(OrderType.Buy, stock, amount)) return false;

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

            // Add new history line
            _tradeHistory.Add(new TradeHistory(Lib.Time.CurrentDateTime, stock.Symbol, OrderType.Buy, amount, stock.Price));

            return true;
        }

        /// <summary>
        /// Sell's a certain amount of a certain stock immediately.
        /// <br>Validation is done if player owns enough of the stock.</br>
        /// </summary>
        /// <param name="stock"></param>
        /// <param name="amount"></param>
        /// <returns>True/False depending if call succeeded or not.</returns>
        internal bool InstantSell(Stock stock, decimal amount, bool removeStock = true)
        {
            // Check if player has this amount of stocks
            if (removeStock && !IsValidOrder(OrderType.Sell, stock, amount)) return false;

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

            // Add new history line
            _tradeHistory.Add(new TradeHistory(Lib.Time.CurrentDateTime, stock.Symbol, OrderType.Sell, amount, stock.Price));

            return true;
        }

        /// <summary>
        /// Add's an order for a specific price, when the price is reached the stock is bought for this price.
        /// </summary>
        /// <param name="stock"></param>
        /// <param name="price"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        internal bool BuyLimitOrder(Stock stock, decimal price, decimal amount)
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
        internal bool SellLimitOrder(Stock stock, decimal price, decimal amount)
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
            if (order.Completed) return;
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
            order.Completed = true;
            _playerTradeOrders.Remove(order);
        }

        private void Market_OnCalculate(object sender, EventArgs e)
        {
            foreach (var order in _playerTradeOrders)
            {
                if (order.Completed) continue;

                // Check if order can be full-filled.
                var stock = Market.Stocks.FirstOrDefault(a => a.Id == order.StockId);
                if (stock == null)
                {
                    order.Completed = true;
                    continue;
                }

                var currentPrice = stock.Price;
                if ((order.OrderType == OrderType.Buy && currentPrice <= order.Price) ||
                    (order.OrderType == OrderType.Sell && currentPrice >= order.Price))
                {
                    // Execute order
                    bool orderCompleted = false;
                    _ = order.OrderType switch
                    {
                        OrderType.Buy => orderCompleted = InstantBuy(stock, order.Amount, false),
                        OrderType.Sell => orderCompleted = InstantSell(stock, order.Amount, false),
                        _ => throw new NotImplementedException($"OrderType \"{order.OrderType}\" doesn't have an implementation."),
                    };

                    // Set completed
                    order.Completed = orderCompleted;

                    // Send a notification out to the player
                    if (order.Completed && NotificationsEnabled)
                        Lib.GameMessage.Broadcast($"{order.OrderType} order \"({stock.Symbol}) '{order.Amount}' for target price € {order.Price}\" completed.", icon: InterfaceControls.Icon.money);
                }
            }
            _playerTradeOrders.RemoveAll(a => a.Completed);

            // Remove history that is more than a week old
            var currentDateTime = Lib.Time.CurrentDateTime;
            int entries = 0;
            entries += _tradeHistory.RemoveAll(a => (currentDateTime - a.DateTime).TotalDays >= 7);

            // Remove history that is more than a month old
            entries += _historicalPortfolio.RemoveAll(a => (currentDateTime - a.Date).TotalDays >= 32);

            if (Plugin.Instance.Config.IsDebugEnabled && entries > 0)
                Plugin.Log.LogInfo($"Deleted {entries} trade history entries.");
        }

        private bool IsValidOrder(OrderType orderType, Stock stock, decimal amount)
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
