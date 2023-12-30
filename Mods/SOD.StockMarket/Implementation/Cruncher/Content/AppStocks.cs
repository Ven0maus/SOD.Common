using SOD.Common;
using SOD.StockMarket.Implementation.Stocks;
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
        private StockEntry[] _slots;

        public override GameObject Container => Content.gameObject.transform.Find("Stocks").gameObject;

        public AppStocks(StockMarketAppContent content) : base(content)
        { }

        public override void OnSetup()
        {
            // Setup main slots
            var panel = Container.transform.Find("Scrollrect").Find("Panel");
            _slots = panel.GetComponentsInChildren<RectTransform>()
                .Where(a => a.name.StartsWith("StockEntry"))
                .OrderBy(a => ExtractNumber(a.name))
                .Select(a => new StockEntry(this, a.gameObject))
                .ToArray();

            // Setup pagination
            _stocksPagination = new StockPagination(Plugin.Instance.Market, _slots.Length);

            // Map buttons
            MapButton("Next", Next);
            MapButton("Previous", Previous);
            MapButton("Back", Back);

            // Map header buttons
            var header = panel.Find("Header");
            MapButton("Name", () => { _stocksPagination.SortBy(a => a.Name); SetSlots(_stocksPagination.Current); }, header);
            MapButton("Price", () => { _stocksPagination.SortBy(a => a.Price); SetSlots(_stocksPagination.Current); }, header);
            MapButton("Today", () => { _stocksPagination.SortBy(a => a.TodayDiff); SetSlots(_stocksPagination.Current); }, header);
            MapButton("Daily", () => { _stocksPagination.SortBy(a => a.DailyPercentage); SetSlots(_stocksPagination.Current); }, header);
            MapButton("Weekly", () => { _stocksPagination.SortBy(a => a.WeeklyPercentage); SetSlots(_stocksPagination.Current); }, header);
            MapButton("Monthly", () => { _stocksPagination.SortBy(a => a.MonthlyPercentage); SetSlots(_stocksPagination.Current); }, header);

            // Set current
            SetSlots(_stocksPagination.Current);

            // Update the current set slots
            Lib.Time.OnMinuteChanged += UpdateStocks;
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
            for (int i = 0; i < stocks.Length; i++)
                _slots[i].SetStock(stocks[i]);

            // When stocks become invalid, (app is closed this instance is no longer valid)
            if (_slots.Any(a => a.Invalid))
            {
                Lib.Time.OnMinuteChanged -= UpdateStocks;
                _slots = null;
                _stocksPagination = null;
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
