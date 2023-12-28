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
        internal AppBuyInterface AppBuyInterface;
        internal AppSellInterface AppSellInterface;
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
            AppBuyInterface = new AppBuyInterface(this);
            AppSellInterface = new AppSellInterface(this);
            AppNews = new AppNews(this);
            AppTradeHistory = new AppTradeHistory(this);
            AppIntroduction = new AppIntroduction(this);

            // Invoke setup on each content
            var parts = new IAppContent[]
            {
                AppMenu, AppStocks, AppPortfolio, AppStock,
                AppFundsInterface, AppBuyInterface, AppSellInterface,
                AppNews, AppTradeHistory, AppIntroduction
            };
            foreach (var part in parts)
                part.OnSetup();

            // Load by default the menu
            AppMenu.Show();
        }
    }
}
