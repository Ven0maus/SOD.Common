using SOD.StockMarket.Implementation.Cruncher.Content;
using System.Collections.Generic;

namespace SOD.StockMarket.Implementation.Cruncher
{
    internal class StockMarketAppContent : CruncherAppContent
    {
        internal AppMenu AppMenu;
        internal AppStocks AppStocks;
        internal AppPortfolio AppPortfolio;
        internal AppStock AppStock;
        internal AppFundsInterface AppFundsInterface;
        internal AppInstantBuyInterface AppInstantBuyInterface;
        internal AppInstantSellInterface AppInstantSellInterface;
        internal AppNews AppNews;
        internal AppTradeHistory AppTradeHistory;
        internal AppIntroduction AppIntroduction;
        internal IAppContent CurrentContent;

        internal readonly List<IAppContent> PreviousContents = new();

        public override void OnSetup()
        {
            // Create all app contents
            AppMenu = new AppMenu(this);
            AppStocks = new AppStocks(this);
            AppPortfolio = new AppPortfolio(this);
            AppStock = new AppStock(this);
            AppFundsInterface = new AppFundsInterface(this);
            AppInstantBuyInterface = new AppInstantBuyInterface(this);
            AppInstantSellInterface = new AppInstantSellInterface(this);
            AppNews = new AppNews(this);
            AppTradeHistory = new AppTradeHistory(this);
            AppIntroduction = new AppIntroduction(this);

            // Invoke setup on each content
            var parts = new IAppContent[]
            {
                AppMenu, AppStocks, AppPortfolio, AppStock,
                AppFundsInterface, AppInstantBuyInterface, AppInstantSellInterface,
                AppNews, AppTradeHistory, AppIntroduction
            };
            foreach (var part in parts)
                part.OnSetup();

            // Load by default the menu
            AppMenu.Show();
        }
    }
}
