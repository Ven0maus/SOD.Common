using UnityEngine;

namespace SOD.StockMarket.Implementation.Cruncher.Content
{
    internal class AppMenu : AppContent
    {
        public AppMenu(StockMarketAppContent content) : base(content)
        { }

        public override GameObject Container => Content.gameObject.transform.Find("Menu").gameObject;

        public override void OnSetup()
        {
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
