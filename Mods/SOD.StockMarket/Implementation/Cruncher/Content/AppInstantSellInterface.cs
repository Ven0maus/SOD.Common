using SOD.StockMarket.Implementation.Stocks;
using UnityEngine;

namespace SOD.StockMarket.Implementation.Cruncher.Content
{
    internal class AppInstantSellInterface : AppContent
    {
        public AppInstantSellInterface(StockMarketAppContent content) : base(content)
        { }

        public override GameObject Container => Content.gameObject.transform.Find("InstantSellStockInterface").gameObject;

        private Stock _stock;

        public override void OnSetup()
        {
            // Set back button listener
            MapButton("Back", Back);
        }

        internal void SetStock(Stock stock)
        {
            _stock = stock;
        }
    }
}
