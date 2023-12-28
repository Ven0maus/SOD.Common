using UnityEngine;

namespace SOD.StockMarket.Implementation.Cruncher
{
    internal abstract class AppContent : IAppContent
    {
        protected bool ContentActive { get { return Content != null && Content.CurrentContent != null && Content.CurrentContent.Equals(this); } }
        protected readonly StockMarketAppContent Content;
        internal AppContent(StockMarketAppContent content)
        {
            Content = content;
        }

        public abstract void OnSetup();
        public abstract GameObject Container { get; }

        public void Show()
        {
            Container.SetActive(true);
        }

        public void Hide()
        {
            Container.SetActive(false);
        }
    }
}
