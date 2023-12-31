using System;
using System.Linq;

namespace SOD.StockMarket.Implementation.Cruncher.News
{
    internal sealed class NewsPagination
    {
        internal int CurrentPage { get; private set; } = 0;

        private readonly int _maxArticlesPerPage;

        internal NewsPagination(int maxArticlesPerPage)
        {
            _maxArticlesPerPage = maxArticlesPerPage;
        }

        /// <summary>
        /// Returns the current selection
        /// </summary>
        internal Article[] Current
        {
            get
            {
                var articles = new Article[_maxArticlesPerPage];
                SetArticles(articles);
                return articles;
            }
        }

        /// <summary>
        /// Returns the next 7 stock slots.
        /// </summary>
        /// <returns></returns>
        internal Article[] Next()
        {
            var articles = new Article[_maxArticlesPerPage];
            var maxPages = (int)Math.Ceiling((double)NewsGenerator.Articles.Count / _maxArticlesPerPage);
            if (CurrentPage < maxPages - 1)
            {
                CurrentPage++;
                SetArticles(articles);
            }
            else if (CurrentPage == maxPages - 1)
            {
                CurrentPage = 0;
                SetArticles(articles);
            }
            return articles;
        }

        /// <summary>
        /// Returns the previous stock slots.
        /// </summary>
        /// <returns></returns>
        internal Article[] Previous()
        {
            var articles = new Article[_maxArticlesPerPage];
            if (CurrentPage > 0)
            {
                CurrentPage--;
                SetArticles(articles);
            }
            else if (CurrentPage == 0)
            {
                var maxPages = (int)Math.Ceiling((double)NewsGenerator.Articles.Count / _maxArticlesPerPage);
                CurrentPage = maxPages - 1;
                SetArticles(articles);
            }
            return articles;
        }

        internal Article[] Reset()
        {
            var articles = new Article[_maxArticlesPerPage];
            CurrentPage = 0;
            SetArticles(articles);
            return articles;
        }

        private void SetArticles(Article[] slots)
        {
            if (NewsGenerator.Articles.Count == 0) return;

            // Sort by latest date first
            var sortedCollection = NewsGenerator.Articles
                .OrderByDescending(a => a.DateTime)
                .ToList();

            var startIndex = CurrentPage * _maxArticlesPerPage;
            for (int i = 0; i < _maxArticlesPerPage; i++)
            {
                var article = sortedCollection.Count > (startIndex + i) ? sortedCollection[startIndex + i] : null;
                if (article == null)
                {
                    slots[i] = null;
                    continue;
                }
                slots[i] = article;
            }
        }
    }
}
