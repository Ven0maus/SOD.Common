using SOD.Common;
using SOD.StockMarket.Implementation.Stocks;
using System;
using System.Globalization;
using TMPro;
using UnityEngine;

namespace SOD.StockMarket.Implementation.Cruncher.Content
{
    internal class AppInstantSellInterface : AppContent
    {
        public AppInstantSellInterface(StockMarketAppContent content) : base(content)
        { }

        public override GameObject Container => Content.gameObject.transform.Find("InstantSellStockInterface").gameObject;

        private Stock _stock;
        private TextMeshProUGUI _stockSymbol, _stockPrice, _owned, _payment, _amount;

        public override void OnSetup()
        {
            // Setup info references
            _stockSymbol = Container.transform.Find("Stock").Find("Text").GetComponent<TextMeshProUGUI>();
            _stockPrice = Container.transform.Find("Price").Find("Text").GetComponent<TextMeshProUGUI>();
            _owned = Container.transform.Find("Owned").Find("Text").GetComponent<TextMeshProUGUI>();
            _payment = Container.transform.Find("Payment").Find("Text").GetComponent<TextMeshProUGUI>();
            _amount = Container.transform.Find("Amount").Find("Text").GetComponent<TextMeshProUGUI>();

            // Set button listeners
            MapButton("Back", Back);
            MapButton("+1", () => IncreaseAmount(1));
            MapButton("+10", () => IncreaseAmount(10));
            MapButton("+100", () => IncreaseAmount(100));
            MapButton("+1000", () => IncreaseAmount(1000));
            MapButton("Max", () => IncreaseAmount(int.MaxValue));
            MapButton("Reset", ResetAmount);
            MapButton("Sell", InstantSell);

            // Do initial info update
            UpdateInfo(null);

            Lib.Time.OnMinuteChanged += UpdateInfo;
        }

        private int _currentAmount = 0;
        private decimal _currentPayment = 0;
        private void IncreaseAmount(int amount)
        {
            BalanceAmountAndCostBasedOnCurrentPrices(amount);

            // Update new info
            UpdateInfo(null);
        }

        private void ResetAmount()
        {
            _currentAmount = 0;
            _currentPayment = 0;

            // Update new info
            UpdateInfo(null);
        }

        private void InstantSell()
        {
            if (_currentAmount == 0) return;

            var tradeController = Plugin.Instance.Market.TradeController;
            tradeController.InstantSell(_stock, _currentAmount);
            Content.AppPortfolio.UpdatePortfolio();
            Content.AppStock.UpdateInfo();
            ResetAmount();
        }

        private void BalanceAmountAndCostBasedOnCurrentPrices(int amount)
        {
            var tradeController = Plugin.Instance.Market.TradeController;
            var maxAffordableAmount = tradeController.GetInvestedVolume(_stock);

            var newAmount = amount == int.MaxValue ? maxAffordableAmount : (_currentAmount + amount);
            if (newAmount > maxAffordableAmount)
                newAmount = maxAffordableAmount;

            var totalPayment = Math.Round(newAmount * _stock.Price, 2);

            _currentAmount = newAmount;
            _currentPayment = totalPayment;
        }

        private void UpdateInfo(object sender, Common.Helpers.TimeChangedArgs e)
        {
            if (_stockSymbol == null || _stockSymbol.gameObject == null)
            {
                Lib.Time.OnMinuteChanged -= UpdateInfo;
                return;
            }
            if (_stock == null || !ContentActive) return;

            UpdateInfo(e);
        }

        private void UpdateInfo(Common.Helpers.TimeChangedArgs e)
        {
            if (_stock == null) return;

            _stockSymbol.text = _stock.Symbol;
            _stockPrice.text = _stock.Price.ToString(CultureInfo.InvariantCulture);

            var tradeController = Plugin.Instance.Market.TradeController;
            _owned.text = tradeController.AvailableFunds.ToString(CultureInfo.InvariantCulture);

            // Make sure to balance the amount / cost on update
            if (e != null)
                BalanceAmountAndCostBasedOnCurrentPrices(0);

            _amount.text = _currentAmount.ToString(CultureInfo.InvariantCulture);
            _payment.text = _currentPayment.ToString(CultureInfo.InvariantCulture);
        }

        internal void SetStock(Stock stock)
        {
            // Also set initial info
            _stock = stock;
            UpdateInfo(null);
        }
    }
}
