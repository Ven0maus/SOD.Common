using SOD.Common;
using SOD.StockMarket.Implementation.Stocks;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

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
            _slots = Container.transform.Find("Scrollrect").Find("Panel").GetComponentsInChildren<RectTransform>()
                .Where(a => a.name.StartsWith("StockEntry"))
                .OrderBy(a => ExtractNumber(a.name))
                .Select(a => new StockEntry(a.gameObject))
                .ToArray();
            if (_slots.Length != 7)
                throw new Exception($"Something is wrong in the asset bundle, missing slots for stocks. {_slots.Length}/7");

            // Setup pagination
            _stocksPagination = new StockPagination(Plugin.Instance.Market, 7);

            // Map buttons
            MapButton("Next", Next);
            MapButton("Previous", Previous);
            MapButton("Back", Back);

            // Set current
            SetSlots(_stocksPagination.Current);

            // Update the current set slots
            Lib.Time.OnMinuteChanged += UpdateStocks;
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
            private readonly GameObject _container;
            private readonly TextMeshProUGUI _symbol, _price, _today, _daily, _weekly, _monthly;

            internal StockEntry(GameObject slot)
            {
                _container = slot;
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

                // Set symbol and main price
                _symbol.text = stock.Symbol;
                _price.text = stock.Price.ToString(CultureInfo.InvariantCulture);

                // Set price diff of today
                var priceDiffToday = Math.Round(stock.Price - stock.OpeningPrice, 2);
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
                var dailyPercentage = GetPercentage(stock.Price, stock.OpeningPrice);
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
                var currentDate = Lib.Time.CurrentDate;
                var weekHistorical = stock.HistoricalData
                    .OrderByDescending(a => a.Date)
                    .FirstOrDefault(a => (currentDate - a.Date).TotalDays >= 7);
                if (weekHistorical == null)
                {
                    _weekly.text = "/";
                    _weekly.color = Color.white;
                    return;
                }

                var weeklyPercentage = GetPercentage(stock.Price, weekHistorical.Open);
                if (weeklyPercentage == 0)
                    _weekly.color = Color.white;
                else if (weeklyPercentage > 0)
                    _weekly.color = Color.green;
                else
                    _weekly.color = Color.red;
                _weekly.text = weeklyPercentage.ToString(CultureInfo.InvariantCulture) + " %";
            }

            private void SetMonthlyPercentage(Stock stock)
            {
                var currentDate = Lib.Time.CurrentDate;
                var monthHistorical = stock.HistoricalData
                    .OrderByDescending(a => a.Date)
                    .FirstOrDefault(a => (currentDate - a.Date).TotalDays >= 30);
                if (monthHistorical == null)
                {
                    _monthly.text = "/";
                    _monthly.color = Color.white;
                    return;
                }

                var monthlyPercentage = GetPercentage(stock.Price, monthHistorical.Open);
                if (monthlyPercentage == 0)
                    _monthly.color = Color.white;
                else if (monthlyPercentage > 0)
                    _monthly.color = Color.green;
                else
                    _monthly.color = Color.red;
                _monthly.text = monthlyPercentage.ToString(CultureInfo.InvariantCulture) + " %";
            }

            private static decimal GetPercentage(decimal currentPrice, decimal openingPrice)
            {
                double percentageChange;
                if (openingPrice != 0)
                {
                    percentageChange = (double)((currentPrice - openingPrice) / openingPrice * 100);
                }
                else
                {
                    // Handle the case when openingPrice is zero
                    if (currentPrice > 0)
                    {
                        // If currentPrice is positive, consider percentage change as infinite
                        percentageChange = double.PositiveInfinity;
                    }
                    else if (currentPrice < 0)
                    {
                        // If currentPrice is negative, consider percentage change as negative infinite
                        percentageChange = double.NegativeInfinity;
                    }
                    else
                    {
                        // If currentPrice is also zero, consider percentage change as zero
                        percentageChange = 0;
                    }
                }
                return Math.Round((decimal)percentageChange, 2);
            }
        }
    }
}
