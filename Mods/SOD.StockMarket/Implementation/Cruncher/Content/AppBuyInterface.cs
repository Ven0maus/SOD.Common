using SOD.StockMarket.Implementation.Stocks;
using System;
using UnityEngine;

namespace SOD.StockMarket.Implementation.Cruncher.Content
{
    internal class AppBuyInterface : AppContent
    {
        public AppBuyInterface(StockMarketAppContent content) : base(content)
        { }

        public override GameObject Container => Content.gameObject.transform.Find("BuyStockInterface").gameObject;

        public override void OnSetup()
        {
            // Set back button listener
            MapButton("Back", Back);
        }

        internal void SetStock(Stock stock)
        {
            
        }
    }
}
