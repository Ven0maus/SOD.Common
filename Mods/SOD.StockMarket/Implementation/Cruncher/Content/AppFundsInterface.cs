using SOD.Common;
using SOD.StockMarket.Implementation.Trade;
using System.Globalization;
using TMPro;
using UnityEngine;

namespace SOD.StockMarket.Implementation.Cruncher.Content
{
    internal class AppFundsInterface : AppContent
    {
        public AppFundsInterface(StockMarketAppContent content) : base(content)
        { }

        public override GameObject Container => Content.gameObject.transform.Find("PortfolioFundsInterface").gameObject;

        private TextMeshProUGUI _bankBalance, _availableFunds;

        public override void OnSetup()
        {
            _bankBalance = Container.transform.Find("BankBalance").GetComponent<TextMeshProUGUI>();
            _availableFunds = Container.transform.Find("AvailableFunds").GetComponent<TextMeshProUGUI>();

            // Set back button listener
            MapButton("Back", Back);

            // Deposit buttons
            MapButton("Deposit100", () => Deposit(100));
            MapButton("Deposit500", () => Deposit(500));
            MapButton("Deposit1000", () => Deposit(1000));
            MapButton("Deposit5000", () => Deposit(5000));
            MapButton("DepositAll", () => Deposit(int.MaxValue));

            // Withdraw buttons
            MapButton("Withdraw100", () => Withdraw(100));
            MapButton("Withdraw500", () => Withdraw(500));
            MapButton("Withdraw1000", () => Withdraw(1000));
            MapButton("Withdraw5000", () => Withdraw(5000));
            MapButton("WithdrawAll", () => Withdraw(int.MaxValue));

            // Set the balance values
            UpdateInfo(this, null);

            // Keep the balance values updated if something changes
            Lib.Time.OnMinuteChanged += UpdateInfo;
        }

        private void UpdateInfo(object sender, Common.Helpers.TimeChangedArgs e)
        {
            // This event can become invalid when exiting the app
            if (_bankBalance == null || _bankBalance.gameObject == null ||
                _availableFunds == null || _availableFunds.gameObject == null)
            {
                Lib.Time.OnMinuteChanged -= UpdateInfo;
                return;
            }

            var tradeController = Plugin.Instance.Market.TradeController;
            _bankBalance.text = $"Bank Balance: € {TradeController.Money.ToString(CultureInfo.InvariantCulture)}";
            _availableFunds.text = $"Available Funds: € {tradeController.AvailableFunds.ToString(CultureInfo.InvariantCulture)}";
        }

        private void Deposit(int amount)
        {
            var tradeController = Plugin.Instance.Market.TradeController;
            var playerMoney = TradeController.Money;
            if (playerMoney < amount)
                amount = playerMoney;

            // Update the balance info
            if (amount != 0)
            {
                tradeController.DepositFunds(amount);
                UpdateInfo(this, null);
                Content.AppPortfolio.UpdatePortfolio();
            }
        }

        private void Withdraw(int amount)
        {
            var tradeController = Plugin.Instance.Market.TradeController;
            if (tradeController.AvailableFunds < amount)
                amount = tradeController.AvailableFunds;

            // Update the balance info
            if (amount != 0)
            {
                tradeController.WithdrawFunds(amount);
                UpdateInfo(this, null);
                Content.AppPortfolio.UpdatePortfolio();
            }
        }
    }
}
