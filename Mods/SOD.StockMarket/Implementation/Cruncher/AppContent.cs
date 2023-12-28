using System.Linq;
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
            if (Content.CurrentContent != null && !Content.CurrentContent.Equals(this))
            {
                Content.PreviousContents.Add(Content.CurrentContent);
                Content.CurrentContent.Hide();
            }
            Content.CurrentContent = this;
            Container.SetActive(true);
        }

        public void Back()
        {
            // Show previous content seen
            var last = Content.PreviousContents.LastOrDefault();
            if (last != null)
            {
                Container.SetActive(false);
                Content.CurrentContent = last;
                Content.PreviousContents.Remove(last);
                Content.CurrentContent.Show();
            }
        }

        public void Hide()
        {
            Container.SetActive(false);
        }
    }
}
