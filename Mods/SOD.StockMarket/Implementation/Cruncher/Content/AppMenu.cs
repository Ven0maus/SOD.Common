using UnityEngine;
using UniverseLib;

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
            var buttonContainer = buttonContainers.Find("Introduction");
            var button = buttonContainer.GetComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(() =>
            {
                Content.AppIntroduction.Show();
            });

            buttonContainer = buttonContainers.Find("News");
            button = buttonContainer.GetComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(() =>
            {
                Content.AppNews.Show();
            });

            buttonContainer = buttonContainers.Find("StockListings");
            button = buttonContainer.GetComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(() =>
            {
                Plugin.Log.LogInfo("Click stocks");
                Content.AppStocks.Show();
            });

            buttonContainer = buttonContainers.Find("Portfolio");
            button = buttonContainer.GetComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(() =>
            {
                Plugin.Log.LogInfo("Click stocks");
                Content.AppPortfolio.Show();
            });

            buttonContainer = buttonContainers.Find("TradeHistory");
            button = buttonContainer.GetComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(() =>
            {
                Content.AppTradeHistory.Show();
            });

            // Exit button
            buttonContainer = Content.gameObject.transform.Find("Exit");
            button = buttonContainer.GetComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(() =>
            {
                Content.controller.OnAppExit();
            });
        }
    }
}
