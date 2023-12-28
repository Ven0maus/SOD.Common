using SOD.Common;
using SOD.StockMarket.Implementation.Stocks;
using System;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;

namespace SOD.StockMarket.Implementation.Cruncher.Content
{
    internal class AppStock : AppContent
    {
        private Stock _stock;
        private TextMeshProUGUI _stockName, _stockSymbol, _price, _lastWeek, _lastMonth, _volumeOwned, _totalOwnedWorth;

        public AppStock(StockMarketAppContent content) : base(content)
        { }

        public override GameObject Container => Content.gameObject.transform.Find("Stock").gameObject;

        public override void OnSetup()
        {
            // Setup main info references
            _stockName = Container.transform.Find("StockName").Find("Text").GetComponent<TextMeshProUGUI>();
            _stockSymbol = Container.transform.Find("StockSymbol").Find("Text").GetComponent<TextMeshProUGUI>();
            _price = Container.transform.Find("Price").Find("Text").GetComponent<TextMeshProUGUI>();
            _lastWeek = Container.transform.Find("LastWeek").Find("Text").GetComponent<TextMeshProUGUI>();
            _lastMonth = Container.transform.Find("LastMonth").Find("Text").GetComponent<TextMeshProUGUI>();
            _volumeOwned = Container.transform.Find("VolumeOwned").Find("Text").GetComponent<TextMeshProUGUI>();
            _totalOwnedWorth = Container.transform.Find("OwnedTotalWorth").Find("Text").GetComponent<TextMeshProUGUI>();

            // Setup button listeners
            MapButton("Back", Back);
            MapButton("InstantBuy", InstantBuy);
            MapButton("InstantSell", InstantSell);

            // Update the info
            UpdateInfo();

            Lib.Time.OnMinuteChanged += UpdateInfo;
        }

        private void UpdateInfo(object sender, Common.Helpers.TimeChangedArgs e)
        {
            if (_stock == null || !ContentActive) return;

            UpdateInfo();
        }

        private void UpdateInfo()
        {
            // Set stock info
            _stockName.text = _stock.Name;
            _stockSymbol.text = _stock.Symbol;
            _price.text = $"€ {_stock.Price.ToString(CultureInfo.InvariantCulture)}";

            var lastWeek = GetHistoricalStockData(_stock, 7);
            _lastWeek.text = $"€ {(lastWeek != null ? lastWeek.Open.ToString(CultureInfo.InvariantCulture) : "/")}";

            var lastMonth = GetHistoricalStockData(_stock, 30);
            _lastMonth.text = $"€ {(lastMonth != null ? lastMonth.Open.ToString(CultureInfo.InvariantCulture) : "/")}";

            // Set trade info
            var tradeController = Plugin.Instance.Market.TradeController;
            var investedVolume = tradeController.GetInvestedVolume(_stock);
            _volumeOwned.text = investedVolume.ToString(CultureInfo.InvariantCulture);
            _totalOwnedWorth.text = $"€ {Math.Round(investedVolume * _stock.Price, 2).ToString(CultureInfo.InvariantCulture)}";
        }

        private static StockData GetHistoricalStockData(Stock stock, int days)
        {
            var currentDate = Lib.Time.CurrentDate;
            return stock.HistoricalData
                .OrderByDescending(a => a.Date)
                .FirstOrDefault(a => (currentDate - a.Date).TotalDays >= days);
        }

        internal void SetStock(Stock stock)
        {
            // Also update the stock info directly
            _stock = stock;
            UpdateInfo();
        }

        private void InstantBuy()
        {
            Content.AppInstantBuyInterface.SetStock(_stock);
            Content.AppInstantBuyInterface.Show();
        }

        private void InstantSell()
        {
            Content.AppInstantSellInterface.SetStock(_stock);
            Content.AppInstantSellInterface.Show();
        }
    }
}
