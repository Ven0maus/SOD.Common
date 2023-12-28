using System;
using UnityEngine;
using UniverseLib;

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
            var backButton = Container.transform.Find("Back");
            var button = backButton.GetComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(() =>
            {
                Back();
            });
        }
    }
}
