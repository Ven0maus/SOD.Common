using System;
using System.Linq;
using TMPro;
using UnityEngine;

namespace SOD.StockMarket.Implementation.Cruncher.Content
{
    internal class AppMenu : AppContent
    {
        private TextMeshProUGUI _hours;

        public AppMenu(StockMarketAppContent content) : base(content)
        { }

        public override GameObject Container => Content.gameObject.transform.Find("Menu").gameObject;

        public override void OnSetup()
        {
            // Set hours text field
            _hours = Container.transform.Find("Hours").GetComponent<TextMeshProUGUI>();
            var closedDays = string.Join(", ", Plugin.Instance.Config.DaysClosed.Split(new[] { ',' },
                StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(a => Enum.Parse<SessionData.WeekDay>(a.ToLower()))
                .Select(a => a.ToString()[..3]));
            var open = Plugin.Instance.Config.OpeningHour;
            var close = Plugin.Instance.Config.ClosingHour;
            _hours.text = $"Open: {open}:00 | Close: {close}:00 | Closed: {closedDays}";

            var buttonContainers = Container.transform.Find("Scrollrect").Find("Panel");

            // Set button listeners
            MapButton("Introduction", Content.AppIntroduction.Show, buttonContainers);
            MapButton("News", Content.AppNews.Show, buttonContainers);
            MapButton("StockListings", Content.AppStocks.Show, buttonContainers);
            MapButton("Portfolio", Content.AppPortfolio.Show, buttonContainers);
            MapButton("TradeHistory", Content.AppTradeHistory.Show, buttonContainers);
            MapButton("Exit", Content.controller.OnAppExit, Content.gameObject.transform);
        }
    }
}
