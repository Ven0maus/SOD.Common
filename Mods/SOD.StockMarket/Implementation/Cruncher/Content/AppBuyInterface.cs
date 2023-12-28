using UnityEngine;
using UniverseLib;

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
            var backButton = Container.transform.Find("Back");
            var button = backButton.GetComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(() =>
            {
                Back();
            });
        }
    }
}
