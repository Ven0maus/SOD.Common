using UnityEngine;

namespace SOD.StockMarket.Implementation.Cruncher.Content
{
    internal class AppSellInterface : AppContent
    {
        public AppSellInterface(StockMarketAppContent content) : base(content)
        { }

        public override GameObject Container => Content.gameObject.transform.Find("SellStockInterface").gameObject;

        public override void OnSetup()
        {
            // Set back button listener
            MapButton("Back", Back);
        }
    }
}
