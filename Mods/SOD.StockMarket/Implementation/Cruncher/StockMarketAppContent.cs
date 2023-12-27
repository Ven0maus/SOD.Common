using UniverseLib;

namespace SOD.StockMarket.Implementation.Cruncher
{
    internal class StockMarketAppContent : CruncherAppContent
    {
        public override void OnSetup()
        {
            // Set listener properly
            var exitButton = gameObject.transform.FindChild("Exit");
            var button = exitButton.GetComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(() =>
            {
                controller.OnAppExit();
            });
        }
    }
}
