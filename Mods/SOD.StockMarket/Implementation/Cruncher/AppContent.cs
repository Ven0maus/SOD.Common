using System;
using System.Linq;
using UnityEngine;

namespace SOD.StockMarket.Implementation.Cruncher
{
    internal abstract class AppContent
    {
        protected bool ContentActive { get { return Content != null && Content.CurrentContent != null && Content.CurrentContent.Equals(this); } }
        protected readonly StockMarketAppContent Content;
        internal AppContent(StockMarketAppContent content)
        {
            Content = content;
        }

        public abstract void OnSetup();
        public abstract GameObject Container { get; }

        public virtual void Show()
        {
            if (Content.CurrentContent != null && !Content.CurrentContent.Equals(this))
            {
                Content.AddPreviousContent(Content.CurrentContent);
                Content.CurrentContent.Hide();
            }
            Content.SetCurrentContent(this);
            Container.SetActive(true);
        }

        public virtual void Back()
        {
            // Show previous content seen
            var last = Content.PreviousContents.LastOrDefault();
            if (last != null)
            {
                Container.SetActive(false);
                Content.SetCurrentContent(last);
                Content.RemovePreviousContent(last);
                Content.CurrentContent.Show();
            }
        }

        public void Hide()
        {
            Container.SetActive(false);
        }

        protected void MapButton(string name, Action action, Transform customPath = null)
        {
            try
            {
                var backButton = customPath?.Find(name) ?? Container.transform.Find(name);
                var button = backButton.GetComponent<UnityEngine.UI.Button>();
                button.onClick.AddListener(action);
            }
            catch (NullReferenceException)
            {
                Plugin.Log.LogInfo("Button name: " + name);
                throw;
            }
        }
    }
}
