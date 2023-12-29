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
    internal class AppPortfolio : AppContent
    {
        private StockPagination _ownedStocksPagination;
        private StockEntry[] _ownedStockSlots;

        private TextMeshProUGUI _bankBalance, _availableFunds, _investedInStocks, _totalPortfolio, _daily, _weekly, _monthly;

        public AppPortfolio(StockMarketAppContent content) : base(content)
        { }

        public override GameObject Container => Content.gameObject.transform.Find("Portfolio").gameObject;

        public override void OnSetup()
        {
            // Setup the portfolio info references
            _bankBalance = Container.transform.Find("BankBalance").Find("Text").GetComponentInChildren<TextMeshProUGUI>();
            _availableFunds = Container.transform.Find("FreeCapital").Find("Text").GetComponentInChildren<TextMeshProUGUI>();
            _investedInStocks = Container.transform.Find("InvestedInStocks").Find("Text").GetComponentInChildren<TextMeshProUGUI>();
            _totalPortfolio = Container.transform.Find("TotalPortfolio").Find("Text").GetComponentInChildren<TextMeshProUGUI>();
            _daily = Container.transform.Find("DailyPerformance").Find("Text").GetComponentInChildren<TextMeshProUGUI>();
            _weekly = Container.transform.Find("WeeklyPerformance").Find("Text").GetComponentInChildren<TextMeshProUGUI>();
            _monthly = Container.transform.Find("MonthlyPerformance").Find("Text").GetComponentInChildren<TextMeshProUGUI>();

            // Setup main slots
            _ownedStockSlots = Container.transform.Find("InvestedStocksOverview").Find("Panel").GetComponentsInChildren<RectTransform>()
                .Where(a => a.name.StartsWith("PortfolioEntry"))
                .OrderBy(a => ExtractNumber(a.name))
                .Select(a => new StockEntry(a.gameObject))
                .ToArray();
            if (_ownedStockSlots.Length != 8)
                throw new Exception($"Something is wrong in the asset bundle, missing slots for portfolio slots. {_ownedStockSlots.Length}/8");

            // Setup pagination
            _ownedStocksPagination = new StockPagination(Plugin.Instance.Market.TradeController, 8);

            // Set next button listener
            MapButton("Next", Next);
            MapButton("Previous", Previous);
            MapButton("Back", Back);
            MapButton("WithdrawDepositFunds", Content.AppFundsInterface.Show);

            // Set current
            SetSlots(_ownedStocksPagination.Current);

            // Update the portfolio
            UpdatePortfolio();

            // Update the current set slots
            Lib.Time.OnMinuteChanged += UpdateStocks;
        }

        private void UpdateStocks(object sender, Common.Helpers.TimeChangedArgs e)
        {
            if (Content.controller == null || !Content.controller.appLoaded || !ContentActive) return;

            // Update also the portfolio info
            UpdatePortfolio();
        }

        internal void UpdatePortfolio()
        {
            // Set funds information
            var tradeController = Plugin.Instance.Market.TradeController;
            _bankBalance.text = $"€ {TradeController.Money.ToString(CultureInfo.InvariantCulture)}";
            _availableFunds.text = $"€ {tradeController.AvailableFunds.ToString(CultureInfo.InvariantCulture)}";
            var totalInvested = tradeController.TotalInvestedInStocks;
            _investedInStocks.text = $"€ {totalInvested.ToString(CultureInfo.InvariantCulture)}";
            _totalPortfolio.text = $"€ {Math.Round(tradeController.AvailableFunds + totalInvested, 2).ToString(CultureInfo.InvariantCulture)}";

            // Set percentages
            _daily.text = $"{tradeController.GetPercentageChangeDaysAgoToNow(1).ToString(CultureInfo.InvariantCulture)} %";
            _weekly.text = $"{tradeController.GetPercentageChangeDaysAgoToNow(7).ToString(CultureInfo.InvariantCulture)} %";
            _monthly.text = $"{tradeController.GetPercentageChangeDaysAgoToNow(30).ToString(CultureInfo.InvariantCulture)} %";

            // Update stocks
            SetSlots(_ownedStocksPagination.Current);
        }

        internal void Next()
        {
            var stocks = _ownedStocksPagination.Next();
            SetSlots(stocks);
        }

        internal void Previous()
        {
            var stocks = _ownedStocksPagination.Previous();
            SetSlots(stocks);
        }

        private void SetSlots(Stock[] stocks)
        {
            // Initial init
            for (int i = 0; i < stocks.Length; i++)
                _ownedStockSlots[i].SetStock(stocks[i]);

            // When stocks become invalid, (app is closed this instance is no longer valid)
            if (_ownedStockSlots.Any(a => a.Invalid))
            {
                Lib.Time.OnMinuteChanged -= UpdateStocks;
                _ownedStockSlots = null;
                _ownedStocksPagination = null;
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

        class StockEntry
        {
            internal bool Invalid = false;
            private readonly GameObject _container;
            private readonly TextMeshProUGUI _symbol;

            internal StockEntry(GameObject slot)
            {
                _container = slot;
                _symbol = slot.GetComponentInChildren<TextMeshProUGUI>();
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

                // Set name
                var tradeController = Plugin.Instance.Market.TradeController;
                var investedVolume = tradeController.GetInvestedVolume(stock);
                _symbol.text = $"{stock.Symbol} ({Math.Round(stock.Price * investedVolume, 2).ToString(CultureInfo.InvariantCulture)})";
            }
        }
    }
}
