using SOD.Common;
using SOD.StockMarket.Implementation.Stocks;
using SOD.StockMarket.Implementation.Trade;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UniverseLib;

namespace SOD.StockMarket.Implementation.Cruncher.Content
{
    internal class AppLimitOrdersOverview : AppContent
    {
        private TradeOrderPagination _tradeOrderPagination;
        private OrderEntry[] _slots;

        public AppLimitOrdersOverview(StockMarketAppContent content) : base(content)
        { }

        public override GameObject Container => Content.gameObject.transform.Find("LimitOrdersOverview").gameObject;

        public override void OnSetup()
        {
            // Setup main slots
            _slots = Container.transform.Find("Scrollrect").Find("Panel").GetComponentsInChildren<RectTransform>()
                .Where(a => a.name.StartsWith("OrderEntry"))
                .OrderBy(a => ExtractNumber(a.name))
                .Select(a => new OrderEntry(this, a.gameObject))
                .ToArray();

            // Setup pagination
            _tradeOrderPagination = new TradeOrderPagination(Plugin.Instance.Market.TradeController, _slots.Length);

            // Map buttons
            MapButton("Next", Next);
            MapButton("Previous", Previous);
            MapButton("Back", Back);

            // Set current
            SetSlots(_tradeOrderPagination.Current);

            // Update the current set slots
            Lib.Time.OnMinuteChanged += UpdateOrders;
        }

        internal void SetStock(Stock stock)
        {
            // If stock is null, then all orders from all stocks are shown
            _tradeOrderPagination.SetForStockOnly(stock);
            SetSlots(_tradeOrderPagination.Current);
        }

        private void UpdateOrders(object sender, Common.Helpers.TimeChangedArgs e)
        {
            if (Content.controller == null || !Content.controller.appLoaded || !ContentActive) return;

            // Update the stock visuals
            SetSlots(_tradeOrderPagination.Current);
        }

        internal void Next()
        {
            var stocks = _tradeOrderPagination.Next();
            SetSlots(stocks);
        }

        internal void Previous()
        {
            var stocks = _tradeOrderPagination.Previous();
            SetSlots(stocks);
        }

        private void SetSlots(StockOrder[] stocks)
        {
            // Initial init
            for (int i = 0; i < stocks.Length; i++)
                _slots[i].SetOrder(stocks[i]);

            // When stocks become invalid, (app is closed this instance is no longer valid)
            if (_slots.Any(a => a.Invalid))
            {
                Lib.Time.OnMinuteChanged -= UpdateOrders;
                _slots = null;
                _tradeOrderPagination = null;
            }
        }

        private static int ExtractNumber(string name)
        {
            Regex regex = new(@"\d+");
            Match match = regex.Match(name);
            if (match.Success && int.TryParse(match.Value, out int number))
                return number;
            return default;
        }

        private class OrderEntry
        {
            internal bool Invalid { get; private set; } = false;
            private readonly GameObject _container;
            private readonly TextMeshProUGUI _symbol, _orderType, _price, _amount, _total;
            private readonly UnityEngine.UI.Button _cancelButton;
            private readonly AppLimitOrdersOverview _appContent;

            internal OrderEntry(AppLimitOrdersOverview appContent, GameObject slot)
            {
                _appContent = appContent;
                _container = slot;
                _cancelButton = slot.transform.Find("Cancel").GetComponent<UnityEngine.UI.Button>();
                _symbol = slot.transform.Find("Name").GetComponentInChildren<TextMeshProUGUI>();
                _orderType = slot.transform.Find("OrderType").GetComponentInChildren<TextMeshProUGUI>();
                _price = slot.transform.Find("Price").GetComponentInChildren<TextMeshProUGUI>();
                _amount = slot.transform.Find("Amount").GetComponentInChildren<TextMeshProUGUI>();
                _total = slot.transform.Find("Total").GetComponentInChildren<TextMeshProUGUI>();
            }

            internal void SetOrder(StockOrder stockOrder)
            {
                if (_container == null)
                {
                    Invalid = true;
                    return;
                }

                if (stockOrder == null || stockOrder.TradeOrder.Completed)
                {
                    _container.SetActive(false);
                    return;
                }
                _container.SetActive(true);

                // Set symbol and main price
                _symbol.text = stockOrder.Stock.Symbol;
                _orderType.text = stockOrder.TradeOrder.OrderType.ToString();
                _price.text = stockOrder.TradeOrder.Price.ToString(CultureInfo.InvariantCulture);
                _amount.text = stockOrder.TradeOrder.Amount.ToString(CultureInfo.InvariantCulture);
                _total.text = Math.Round(stockOrder.TradeOrder.Price * stockOrder.TradeOrder.Amount, 2).ToString(CultureInfo.InvariantCulture);

                // Add listener to the cancel button of this order
                _cancelButton.onClick.RemoveAllListeners();
                _cancelButton.onClick.AddListener(() =>
                {
                    // Cancel the order & update portfolio
                    Plugin.Instance.Market.TradeController.CancelOrder(stockOrder.TradeOrder);
                    _appContent.Content.AppPortfolio.UpdatePortfolio();
                    _container.SetActive(false);
                });
            }
        }
    }
}
