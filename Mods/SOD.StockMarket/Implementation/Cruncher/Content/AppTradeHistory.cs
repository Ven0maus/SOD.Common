using System;
using UnityEngine;
using UniverseLib;

namespace SOD.StockMarket.Implementation.Cruncher.Content
{
    internal class AppTradeHistory : AppContent
    {
        public AppTradeHistory(StockMarketAppContent content) : base(content)
        { }

        public override GameObject Container => Content.gameObject.transform.Find("TradeHistory").gameObject;

        public override void OnSetup()
        {
            // Set back button listener
            var backButton = Container.transform.Find("Back");
            var button = backButton.GetComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(() =>
            {
                Back();
            });
        }
    }
}
