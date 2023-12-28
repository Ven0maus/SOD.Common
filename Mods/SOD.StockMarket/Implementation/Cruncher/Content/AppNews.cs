using UnityEngine;

namespace SOD.StockMarket.Implementation.Cruncher.Content
{
    internal class AppNews : AppContent
    {
        public AppNews(StockMarketAppContent content) : base(content)
        { }

        public override GameObject Container => Content.gameObject.transform.Find("News").gameObject;

        public override void OnSetup()
        {
            // Set back button listener
            MapButton("Back", Back);
        }
    }
}
