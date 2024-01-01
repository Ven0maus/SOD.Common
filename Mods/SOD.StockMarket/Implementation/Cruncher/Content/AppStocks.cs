using Bogus.DataSets;
using SOD.Common;
using SOD.StockMarket.Implementation.Stocks;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UniverseLib;

namespace SOD.StockMarket.Implementation.Cruncher.Content
{
    internal class AppStocks : AppContent
    {
        private StockPagination _stocksPagination;
        private StockEntry[] _largeSlots, _compactSlots;
        private bool _currentViewLarge;
        public override GameObject Container => Content.gameObject.transform.Find("Stocks").gameObject;

        private Transform _largeView, _compactView;
        private TextMeshProUGUI _viewButtonText;

        public AppStocks(StockMarketAppContent content) : base(content)
        { }

        public override void OnSetup()
        {
            // Setup main slots
            _largeView = Container.transform.Find("Scrollrect").Find("LargeView");
            _compactView = Container.transform.Find("Scrollrect").Find("CompactView");

            _largeSlots = _largeView.GetComponentsInChildren<RectTransform>()
                .Where(a => a.name.StartsWith("StockEntry"))
                .OrderBy(a => ExtractNumber(a.name))
                .Select(a => new StockEntry(this, a.gameObject))
                .ToArray();
            _compactSlots = _compactView.GetComponentsInChildren<RectTransform>()
                .Where(a => a.name.StartsWith("StockEntry"))
                .OrderBy(a => ExtractNumber(a.name))
                .Select(a => new StockEntry(this, a.gameObject))
                .ToArray();

            // Setup pagination and sort by default ascending on name
            _stocksPagination = new StockPagination(Plugin.Instance.Market, () => GetSlots().Length);
            _stocksPagination.SortBy(a => a.Name, true);

            // Map buttons
            MapButton("Next", Next);
            MapButton("Previous", Previous);
            MapButton("ChangeView", ChangeView);
            MapButton("Back", Back);

            // Get button text
            _viewButtonText = Container.transform.Find("ChangeView").GetComponentInChildren<TextMeshProUGUI>();

            // Map header buttons for both views
            var header = _largeView.Find("Header");
            MapButton("Name", () => { _stocksPagination.SortBy(a => a.Name); SetSlots(_stocksPagination.Current); }, header);
            MapButton("Price", () => { _stocksPagination.SortBy(a => a.Price); SetSlots(_stocksPagination.Current); }, header);
            MapButton("Today", () => { _stocksPagination.SortBy(a => a.TodayDiff); SetSlots(_stocksPagination.Current); }, header);
            MapButton("Daily", () => { _stocksPagination.SortBy(a => a.DailyPercentage); SetSlots(_stocksPagination.Current); }, header);
            MapButton("Weekly", () => { _stocksPagination.SortBy(a => a.WeeklyPercentage); SetSlots(_stocksPagination.Current); }, header);
            MapButton("Monthly", () => { _stocksPagination.SortBy(a => a.MonthlyPercentage); SetSlots(_stocksPagination.Current); }, header);

            header = _compactView.Find("Header");
            MapButton("Name", () => { _stocksPagination.SortBy(a => a.Name); SetSlots(_stocksPagination.Current); }, header);
            MapButton("Price", () => { _stocksPagination.SortBy(a => a.Price); SetSlots(_stocksPagination.Current); }, header);
            MapButton("Today", () => { _stocksPagination.SortBy(a => a.TodayDiff); SetSlots(_stocksPagination.Current); }, header);
            MapButton("Daily", () => { _stocksPagination.SortBy(a => a.DailyPercentage); SetSlots(_stocksPagination.Current); }, header);
            MapButton("Weekly", () => { _stocksPagination.SortBy(a => a.WeeklyPercentage); SetSlots(_stocksPagination.Current); }, header);
            MapButton("Monthly", () => { _stocksPagination.SortBy(a => a.MonthlyPercentage); SetSlots(_stocksPagination.Current); }, header);

            // Set current view (to true, change view handles it)
            _currentViewLarge = false;
            ChangeView();

            // Update the current set slots
            Lib.Time.OnMinuteChanged += UpdateStocks;
        }

        private void ChangeView()
        {
            _currentViewLarge = !_currentViewLarge;
            if (_currentViewLarge)
            {
                _compactView.gameObject.SetActive(false);
                _largeView.gameObject.SetActive(true);
                _viewButtonText.text = "Show compact view";
            }
            else
            {
                _compactView.gameObject.SetActive(true);
                _largeView.gameObject.SetActive(false);
                _viewButtonText.text = "Show normal view";
            }
            
            SetSlots(_stocksPagination.Current);
        }

        public override void Show()
        {
            base.Show();
            SetSlots(_stocksPagination.Current);
        }

        private void UpdateStocks(object sender, Common.Helpers.TimeChangedArgs e)
        {
            if (Content.controller == null || !Content.controller.appLoaded || !ContentActive) return;

            // Update the stock visuals
            SetSlots(_stocksPagination.Current);
        }

        internal void Next()
        {
            var stocks = _stocksPagination.Next();
            SetSlots(stocks);
        }

        internal void Previous()
        {
            var stocks = _stocksPagination.Previous();
            SetSlots(stocks);
        }

        private void SetSlots(Stock[] stocks)
        {
            // Initial init
            var slots = GetSlots();
            for (int i = 0; i < stocks.Length; i++)
                slots[i].SetStock(stocks[i]);

            // When stocks become invalid, (app is closed this instance is no longer valid)
            if (slots.Any(a => a.Invalid))
            {
                Lib.Time.OnMinuteChanged -= UpdateStocks;
                _largeSlots = null;
                _compactSlots = null;
                _stocksPagination = null;
            }
        }

        private StockEntry[] GetSlots()
        {
            return _currentViewLarge ? _largeSlots : _compactSlots;
        }

        private static int ExtractNumber(string name)
        {
            Regex regex = new(@"\d+");
            Match match = regex.Match(name);
            if (match.Success && int.TryParse(match.Value, out int number))
                return number;
            return default;
        }

        private class StockEntry
        {
            internal bool Invalid = false;
            private readonly AppStocks _appStocks;
            private readonly GameObject _container;
            private readonly TextMeshProUGUI _symbol, _price, _today, _daily, _weekly, _monthly;
            private readonly UnityEngine.UI.Button _button;

            internal StockEntry(AppStocks appStocks, GameObject slot)
            {
                _appStocks = appStocks;
                _container = slot;
                _button = slot.transform.Find("Selector").GetComponent<UnityEngine.UI.Button>();
                _symbol = slot.transform.Find("Name").GetComponentInChildren<TextMeshProUGUI>();
                _price = slot.transform.Find("Price").GetComponentInChildren<TextMeshProUGUI>();
                _today = slot.transform.Find("Today").GetComponentInChildren<TextMeshProUGUI>();
                _daily = slot.transform.Find("Daily").GetComponentInChildren<TextMeshProUGUI>();
                _weekly = slot.transform.Find("Weekly").GetComponentInChildren<TextMeshProUGUI>();
                _monthly = slot.transform.Find("Monthly").GetComponentInChildren<TextMeshProUGUI>();
            }

            internal void SetStock(Stock stock)
            {
                if (_container == null)
                {
                    Invalid = true;
                    return;
                }

                if (stock == null)
                {
                    _container.SetActive(false);
                    return;
                }
                _container.SetActive(true);

                // Add listener to this specific stock
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(() =>
                {
                    _appStocks.Content.AppStock.SetStock(stock);
                    _appStocks.Content.AppStock.Show();
                });

                // Set symbol and main price
                _symbol.text = stock.Symbol;
                _price.text = stock.Price.ToString(CultureInfo.InvariantCulture);

                // Set price diff of today
                var priceDiffToday = stock.TodayDiff;
                if (priceDiffToday == 0)
                    _today.color = Color.white;
                else if (priceDiffToday > 0)
                    _today.color = Color.green;
                else
                    _today.color = Color.red;
                _today.text = priceDiffToday.ToString(CultureInfo.InvariantCulture);

                // Set percentages
                SetDailyPercentageText(stock);
                SetWeeklyPercentage(stock);
                SetMonthlyPercentage(stock);
            }

            private void SetDailyPercentageText(Stock stock)
            {
                var dailyPercentage = stock.DailyPercentage;
                if (dailyPercentage == 0)
                    _daily.color = Color.white;
                else if (dailyPercentage > 0)
                    _daily.color = Color.green;
                else
                    _daily.color = Color.red;
                _daily.text = dailyPercentage.ToString(CultureInfo.InvariantCulture) + " %";
            }

            private void SetWeeklyPercentage(Stock stock)
            {
                var weeklyPercentage = stock.WeeklyPercentage;
                if (weeklyPercentage == null)
                {
                    _weekly.text = "/";
                    _weekly.color = Color.white;
                    return;
                }

                var value = weeklyPercentage.Value;
                if (value == 0)
                    _weekly.color = Color.white;
                else if (value > 0)
                    _weekly.color = Color.green;
                else
                    _weekly.color = Color.red;
                _weekly.text = value.ToString(CultureInfo.InvariantCulture) + " %";
            }

            private void SetMonthlyPercentage(Stock stock)
            {
                var monthlyPercentage = stock.MonthlyPercentage;
                if (monthlyPercentage == null)
                {
                    _monthly.text = "/";
                    _monthly.color = Color.white;
                    return;
                }

                var value = monthlyPercentage.Value;
                if (value == 0)
                    _monthly.color = Color.white;
                else if (value > 0)
                    _monthly.color = Color.green;
                else
                    _monthly.color = Color.red;
                _monthly.text = value.ToString(CultureInfo.InvariantCulture) + " %";
            }
        }
    }
}
