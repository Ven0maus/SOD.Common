using SOD.Common;
using SOD.StockMarket.Implementation.Cruncher.News;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace SOD.StockMarket.Implementation.Cruncher.Content
{
    internal class AppNews : AppContent
    {
        private ArticleEntry[] _slots;
        private NewsPagination _newsPagination;

        public AppNews(StockMarketAppContent content) : base(content)
        { }

        public override GameObject Container => Content.gameObject.transform.Find("News").gameObject;

        public override void OnSetup()
        {
            // Setup slot entries
            _slots = Container.transform.Find("Scrollrect").Find("Panel").GetComponentsInChildren<RectTransform>()
                .Where(a => a.name.StartsWith("NewsArticle"))
                .OrderBy(a => ExtractNumber(a.name))
                .Select(a => new ArticleEntry(a.gameObject))
                .ToArray();

            // Setup pagination controller and sort by default descending on datetime
            _newsPagination = new NewsPagination(_slots.Length);

            // Map buttons
            MapButton("Next", Next);
            MapButton("Previous", Previous);
            MapButton("Back", Back);

            // Set current
            SetSlots(_newsPagination.Current);

            // Update the current set slots
            Lib.Time.OnMinuteChanged += UpdateArticles;
        }

        public override void Show()
        {
            base.Show();
            SetSlots(_newsPagination.Current);
        }

        private void UpdateArticles(object sender, Common.Helpers.TimeChangedArgs e)
        {
            if (Content.controller == null || !Content.controller.appLoaded || !ContentActive) return;

            // Update the article visuals
            SetSlots(_newsPagination.Current);
        }

        internal void Next()
        {
            var articles = _newsPagination.Next();
            SetSlots(articles);
        }

        internal void Previous()
        {
            var articles = _newsPagination.Previous();
            SetSlots(articles);
        }

        private void SetSlots(Article[] histories)
        {
            // Initial init
            for (int i = 0; i < histories.Length; i++)
                _slots[i].SetHistory(histories[i]);

            // When articles become invalid, (app is closed this instance is no longer valid)
            if (_slots.Any(a => a.Invalid))
            {
                Lib.Time.OnMinuteChanged -= UpdateArticles;
                _slots = null;
                _newsPagination = null;
            }
        }

        private static int ExtractNumber(string name)
        {
            Regex regex = new(@"\d+");
            Match match = regex.Match(name);
            if (match.Success && int.TryParse(match.Value, out int number))
                return number;
            return default;
        }

        private class ArticleEntry
        {
            internal bool Invalid = false;
            private readonly GameObject _container;
            private readonly TextMeshProUGUI _date, _headLine;

            internal ArticleEntry(GameObject slot)
            {
                _container = slot;
                _date = slot.transform.Find("Date").GetComponentInChildren<TextMeshProUGUI>();
                _headLine = slot.transform.Find("Headline").GetComponentInChildren<TextMeshProUGUI>();
            }

            internal void SetHistory(Article article)
            {
                if (_container == null)
                {
                    Invalid = true;
                    return;
                }

                if (article == null)
                {
                    _container.SetActive(false);
                    return;
                }
                _container.SetActive(true);

                // Set text properties
                _date.text = article.DateTime.ToString();
                _headLine.text = article.Title;
            }
        }
    }
}
