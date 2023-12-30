using SOD.Common;
using SOD.StockMarket.Implementation.Trade;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace SOD.StockMarket.Implementation.Cruncher.Content
{
    internal class AppTradeHistory : AppContent
    {
        private TradeHistoryEntry[] _slots;
        private TradeHistoryPagination _tradeHistoryPagination;

        public AppTradeHistory(StockMarketAppContent content) : base(content)
        { }

        public override GameObject Container => Content.gameObject.transform.Find("TradeHistory").gameObject;

        public override void OnSetup()
        {
            // Setup slot entries
            var panel = Container.transform.Find("Scrollrect").Find("Panel");
            _slots = panel.GetComponentsInChildren<RectTransform>()
                .Where(a => a.name.StartsWith("TradeHistoryEntry"))
                .OrderBy(a => ExtractNumber(a.name))
                .Select(a => new TradeHistoryEntry(a.gameObject))
                .ToArray();

            // Setup pagination controller and sort by default descending on datetime
            _tradeHistoryPagination = new TradeHistoryPagination(Plugin.Instance.Market.TradeController, _slots.Length);
            _tradeHistoryPagination.SortBy(a => a.DateTime, false);

            // Map buttons
            MapButton("Next", Next);
            MapButton("Previous", Previous);
            MapButton("Back", Back);

            // Map header buttons
            var header = panel.Find("Header");
            MapButton("Date", () => { _tradeHistoryPagination.SortBy(a => a.DateTime); SetSlots(_tradeHistoryPagination.Current); }, header);
            MapButton("Name", () => { _tradeHistoryPagination.SortBy(a => a.StockSymbol); SetSlots(_tradeHistoryPagination.Current); }, header);
            MapButton("Price", () => { _tradeHistoryPagination.SortBy(a => a.Price); SetSlots(_tradeHistoryPagination.Current); }, header);
            MapButton("Amount", () => { _tradeHistoryPagination.SortBy(a => a.Amount); SetSlots(_tradeHistoryPagination.Current); }, header);
            MapButton("Total", () => { _tradeHistoryPagination.SortBy(a => a.Total); SetSlots(_tradeHistoryPagination.Current); }, header);
            MapButton("OrderType", () => { _tradeHistoryPagination.SortBy(a => a.OrderType); SetSlots(_tradeHistoryPagination.Current); }, header);

            // Set current
            SetSlots(_tradeHistoryPagination.Current);

            // Update the current set slots
            Lib.Time.OnMinuteChanged += UpdateHistories;
        }

        public override void Show()
        {
            base.Show();
            SetSlots(_tradeHistoryPagination.Current);
        }

        private void UpdateHistories(object sender, Common.Helpers.TimeChangedArgs e)
        {
            if (Content.controller == null || !Content.controller.appLoaded || !ContentActive) return;

            // Update the stock visuals
            SetSlots(_tradeHistoryPagination.Current);
        }

        internal void Next()
        {
            var stocks = _tradeHistoryPagination.Next();
            SetSlots(stocks);
        }

        internal void Previous()
        {
            var stocks = _tradeHistoryPagination.Previous();
            SetSlots(stocks);
        }

        private void SetSlots(TradeHistory[] histories)
        {
            // Initial init
            for (int i = 0; i < histories.Length; i++)
                _slots[i].SetHistory(histories[i]);

            // When histories become invalid, (app is closed this instance is no longer valid)
            if (_slots.Any(a => a.Invalid))
            {
                Lib.Time.OnMinuteChanged -= UpdateHistories;
                _slots = null;
                _tradeHistoryPagination = null;
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

        private class TradeHistoryEntry
        {
            internal bool Invalid = false;
            private readonly GameObject _container;
            private readonly TextMeshProUGUI _date, _symbol, _price, _amount, _total, _orderType;

            internal TradeHistoryEntry(GameObject slot)
            {
                _container = slot;
                _date = slot.transform.Find("Date").GetComponentInChildren<TextMeshProUGUI>();
                _symbol = slot.transform.Find("Name").GetComponentInChildren<TextMeshProUGUI>();
                _price = slot.transform.Find("Price").GetComponentInChildren<TextMeshProUGUI>();
                _amount = slot.transform.Find("Amount").GetComponentInChildren<TextMeshProUGUI>();
                _total = slot.transform.Find("Total").GetComponentInChildren<TextMeshProUGUI>();
                _orderType = slot.transform.Find("OrderType").GetComponentInChildren<TextMeshProUGUI>();
            }

            internal void SetHistory(TradeHistory history)
            {
                if (_container == null)
                {
                    Invalid = true;
                    return;
                }

                if (history == null)
                {
                    _container.SetActive(false);
                    return;
                }
                _container.SetActive(true);

                // Set text properties
                _date.text = history.DateTime.ToString();
                _symbol.text = history.StockSymbol;
                _price.text = history.Price.ToString(CultureInfo.InvariantCulture);
                _amount.text = history.Amount.ToString(CultureInfo.InvariantCulture);
                _total.text = history.Total.ToString(CultureInfo.InvariantCulture);
                _orderType.text = history.OrderType.ToString();
            }
        }
    }
}
