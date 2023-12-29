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

        private TextMeshProUGUI _bankBalance, _availableFunds, _investedInStocks, _ongoingLimitOrders, _daily, _weekly, _monthly;

        public AppPortfolio(StockMarketAppContent content) : base(content)
        { }

        public override GameObject Container => Content.gameObject.transform.Find("Portfolio").gameObject;

        public override void OnSetup()
        {
            // Setup the portfolio info references
            _bankBalance = Container.transform.Find("BankBalance").Find("Text").GetComponentInChildren<TextMeshProUGUI>();
            _availableFunds = Container.transform.Find("FreeCapital").Find("Text").GetComponentInChildren<TextMeshProUGUI>();
            _investedInStocks = Container.transform.Find("InvestedInStocks").Find("Text").GetComponentInChildren<TextMeshProUGUI>();
            _ongoingLimitOrders = Container.transform.Find("OngoingLimitOrders").Find("Text").GetComponentInChildren<TextMeshProUGUI>();
            _daily = Container.transform.Find("DailyPerformance").Find("Text").GetComponentInChildren<TextMeshProUGUI>();
            _weekly = Container.transform.Find("WeeklyPerformance").Find("Text").GetComponentInChildren<TextMeshProUGUI>();
            _monthly = Container.transform.Find("MonthlyPerformance").Find("Text").GetComponentInChildren<TextMeshProUGUI>();

            // Setup main slots
            _ownedStockSlots = Container.transform.Find("InvestedStocksOverview").Find("Panel").GetComponentsInChildren<RectTransform>()
                .Where(a => a.name.StartsWith("PortfolioEntry"))
                .OrderBy(a => ExtractNumber(a.name))
                .Select(a => new StockEntry(this, a.gameObject))
                .ToArray();

            // Setup pagination
            _ownedStocksPagination = new StockPagination(Plugin.Instance.Market.TradeController, _ownedStockSlots.Length);

            // Set next button listener
            MapButton("Next", Next);
            MapButton("Previous", Previous);
            MapButton("Back", Back);
            MapButton("WithdrawDepositFunds", Content.AppFundsInterface.Show);
            MapButton("LimitOrders", LimitOrders);

            // Set current
            SetSlots(_ownedStocksPagination.Current);

            // Update the portfolio
            UpdatePortfolio();

            // Update the current set slots
            Lib.Time.OnMinuteChanged += UpdateStocks;
        }

        private void LimitOrders()
        {
            Content.AppLimitOrdersOverview.SetStock(null);
            Content.AppLimitOrdersOverview.Show();
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
            _ongoingLimitOrders.text = $"€ {Math.Round(tradeController.TradeOrders.Select(a => a.Price * a.Amount).Sum(), 2).ToString(CultureInfo.InvariantCulture)}";

            // Set percentages
            var daily = tradeController.GetPortfolioPercentageChange(1);
            _daily.text = $"{(daily == null ? "/" : daily.Value.ToLimitedString(8))} %";
            _daily.color = daily == null || daily.Value == 0 ? Color.white : daily.Value > 0 ? Color.green : Color.red;
            var weekly = tradeController.GetPortfolioPercentageChange(7);
            _weekly.text = $"{(weekly == null ? "/" : weekly.Value.ToLimitedString(8))} %";
            _weekly.color = weekly == null || weekly.Value == 0 ? Color.white : weekly.Value > 0 ? Color.green : Color.red;
            var monthly = tradeController.GetPortfolioPercentageChange(30);
            _monthly.text = $"{(monthly == null ? "/" : monthly.Value.ToLimitedString(8))} %";
            _monthly.color = monthly == null || monthly.Value == 0 ? Color.white : monthly.Value > 0 ? Color.green : Color.red;

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
            private readonly AppPortfolio _appContent;
            private readonly GameObject _container;
            private readonly TextMeshProUGUI _symbol;
            private readonly UnityEngine.UI.Button _button;

            internal StockEntry(AppPortfolio appContent, GameObject slot)
            {
                _appContent = appContent;
                _container = slot;
                _button = slot.GetComponent<UnityEngine.UI.Button>();
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

                // Add listener to this specific stock
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(() =>
                {
                    _appContent.Content.AppStock.SetStock(stock);
                    _appContent.Content.AppStock.Show();
                });
            }
        }
    }
}
