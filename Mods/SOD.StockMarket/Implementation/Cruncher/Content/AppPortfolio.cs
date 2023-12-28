using SOD.Common;
using SOD.StockMarket.Implementation.Stocks;
using System;
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

        public AppPortfolio(StockMarketAppContent content) : base(content)
        { }

        public override GameObject Container => Content.gameObject.transform.Find("Portfolio").gameObject;

        public override void OnSetup()
        {
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
            var nextButton = Container.transform.Find("Next");
            var button = nextButton.GetComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(() =>
            {
                Next();
            });

            // Set previous button listener
            var previousButton = Container.transform.Find("Previous");
            button = previousButton.GetComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(() =>
            {
                Previous();
            });

            // Set back button listener
            var backButton = Container.transform.Find("Back");
            button = backButton.GetComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(() =>
            {
                Back();
            });

            // Set back button listener
            var withdrawDepositButton = Container.transform.Find("WithdrawDepositFunds");
            button = withdrawDepositButton.GetComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(() =>
            {
                Content.AppFundsInterface.Show();
            });

            // Set current
            SetSlots(_ownedStocksPagination.Current);

            // Update the current set slots
            Lib.Time.OnMinuteChanged += UpdateStocks;
        }

        private void UpdateStocks(object sender, Common.Helpers.TimeChangedArgs e)
        {
            if (Content.controller == null || !Content.controller.appLoaded || !ContentActive) return;

            // Update the stock visuals
            SetSlots(_ownedStocksPagination.Current);

            // Update also the portfolio info
            UpdatePortfolio();
        }

        private void UpdatePortfolio()
        {
            // TODO
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
                _symbol.text = $"{stock.Symbol} ({stock.Price})";
            }
        }
    }
}
