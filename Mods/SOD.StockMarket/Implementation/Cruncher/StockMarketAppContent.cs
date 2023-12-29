using SOD.StockMarket.Implementation.Cruncher.Content;
using System.Collections.Generic;

namespace SOD.StockMarket.Implementation.Cruncher
{
    internal class StockMarketAppContent : CruncherAppContent
    {
        internal AppMenu AppMenu { get; private set; }
        internal AppStocks AppStocks { get; private set; }
        internal AppPortfolio AppPortfolio { get; private set; }
        internal AppStock AppStock { get; private set; }
        internal AppFundsInterface AppFundsInterface { get; private set; }
        internal AppInstantBuyInterface AppInstantBuyInterface { get; private set; }
        internal AppInstantSellInterface AppInstantSellInterface { get; private set; }
        internal AppNews AppNews { get; private set; }
        internal AppTradeHistory AppTradeHistory { get; private set; }
        internal AppIntroduction AppIntroduction { get; private set; }
        internal AppBuyLimitInterface AppBuyLimitInterface { get; private set; }
        internal AppSellLimitInterface AppSellLimitInterface { get; private set; }
        internal AppLimitOrdersOverview AppLimitOrdersOverview { get; private set; }
        internal IAppContent CurrentContent { get; private set; }

        private readonly List<IAppContent> _previousContents = new();
        internal IReadOnlyList<IAppContent> PreviousContents => _previousContents;

        private bool _isSetup = false;

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
            AppBuyLimitInterface = new AppBuyLimitInterface(this);
            AppSellLimitInterface = new AppSellLimitInterface(this);
            AppLimitOrdersOverview = new AppLimitOrdersOverview(this);

            // Invoke setup on each content
            var parts = new IAppContent[]
            {
                AppMenu, AppStocks, AppPortfolio, AppStock,
                AppFundsInterface, AppInstantBuyInterface, AppInstantSellInterface,
                AppNews, AppTradeHistory, AppIntroduction, AppBuyLimitInterface,
                AppSellLimitInterface, AppLimitOrdersOverview
            };
            foreach (var part in parts)
                part.OnSetup();

            // Load by default the menu
            AppMenu.Show();

            // Finish setup by marking this so update can run
            _isSetup = true;
        }

        private void Update()
        {
            if (!SessionData.Instance.play || !controller.playerControlled || !_isSetup) return;
            AppBuyLimitInterface.Update();
            AppSellLimitInterface.Update();
        }

        internal void AddPreviousContent(IAppContent content)
        {
            _previousContents.Add(content);
        }

        internal void RemovePreviousContent(IAppContent content)
        {
            _previousContents.Remove(content);
        }

        internal void SetCurrentContent(IAppContent content)
        {
            CurrentContent = content;
        }
    }
}
