using UnityEngine;

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
            MapButton("Back", Back);
        }
    }
}
