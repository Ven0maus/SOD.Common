using SOD.Common;
using SOD.Common.Helpers;
using SOD.StockMarket.Implementation.Cruncher.Content;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
        internal AppContent CurrentContent { get; private set; }

        private readonly List<AppContent> _previousContents = new();
        internal IReadOnlyList<AppContent> PreviousContents => _previousContents;

        private bool _isSetup = false;

        private TextMeshProUGUI _openCloseText, _currentDate;

        public override void OnSetup()
        {
            var titleDateHeader = gameObject.transform.Find("TitleDateHeader");
            _openCloseText = titleDateHeader.Find("OpenClosed").GetComponent<TextMeshProUGUI>();
            _currentDate = titleDateHeader.Find("Date").GetComponent<TextMeshProUGUI>();

            SetTitleDateHeader(this, null);

            // Hook it also
            Lib.Time.OnMinuteChanged += SetTitleDateHeader;

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
            var parts = new AppContent[]
            {
                AppMenu, AppStocks, AppPortfolio, AppStock,
                AppFundsInterface, AppInstantBuyInterface, AppInstantSellInterface,
                AppNews, AppTradeHistory, AppIntroduction, AppBuyLimitInterface,
                AppSellLimitInterface, AppLimitOrdersOverview
            };
            foreach (var part in parts)
                part.OnSetup();

            name = "stockmarketpreset";

            // Load by default the menu
            AppMenu.Show();

            // Finish setup by marking this so update can run
            _isSetup = true;
        }
 
        private void SetTitleDateHeader(object sender, TimeChangedArgs e)
        {
            if (_openCloseText == null || _openCloseText.gameObject == null || _currentDate == null || _currentDate.gameObject == null)
            {
                // Became invalid because we left the app
                Lib.Time.OnMinuteChanged -= SetTitleDateHeader;
                return;
            }

            var marketOpen = Plugin.Instance.Market.IsOpen();
            _openCloseText.text = marketOpen ? "open" : "closed";
            _openCloseText.color = marketOpen ? Color.green : Color.red;
            _currentDate.text = $"Date: {GetCurrentDateTimeReadable()}";
        }

        private static string GetCurrentDateTimeReadable()
        {
            // Seems to be possible somehow, better run empty in this case.
            if (!Lib.Time.IsInitialized)
                return string.Empty;

            var date = Lib.Time.CurrentDateTime;
            var day = date.DayEnum.ToString()[..3];
            var month = date.MonthEnum.ToString()[..3];
            return $"{day}, {month} {date.Day}, {date.Year} {date.Hour}:{date.Minute}";
        }

#pragma warning disable IDE0051 // Remove unused private members
        private void Update()
#pragma warning restore IDE0051 // Remove unused private members
        {
            if (!SessionData.Instance.play || !controller.playerControlled || !_isSetup) return;
            AppBuyLimitInterface.Update();
            AppSellLimitInterface.Update();
        }

        internal void AddPreviousContent(AppContent content)
        {
            _previousContents.Add(content);
        }

        internal void RemovePreviousContent(AppContent content)
        {
            _previousContents.Remove(content);
        }

        internal void SetCurrentContent(AppContent content)
        {
            CurrentContent = content;
        }
    }
}
