using UnityEngine;
using UniverseLib;

namespace SOD.StockMarket.Implementation.Cruncher.Content
{
    internal class AppFundsInterface : AppContent
    {
        public AppFundsInterface(StockMarketAppContent content) : base(content)
        { }

        public override GameObject Container => Content.gameObject.transform.Find("PortfolioFundsInterface").gameObject;

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
