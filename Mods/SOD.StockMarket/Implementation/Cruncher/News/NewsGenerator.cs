using SOD.Common;
using SOD.StockMarket.Implementation.Stocks;
using System.Collections.Generic;

namespace SOD.StockMarket.Implementation.Cruncher.News
{
    internal static class NewsGenerator
    {
        private static readonly List<Article> _articles = new();
        internal static IReadOnlyList<Article> Articles => _articles;

        // Some ideas:
        // Have stocks have other competitor stocks
        // This means when a news article negatively impacts stock A
        // Stocks B, C.. might get a positive trend because of this

        // There could also be generic sectors for stocks such as, an article that covers a whole sector (eg industrial)
        // Then some related industrial stocks could have a trend

        /// <summary>
        /// Generates an article for a specific stock.
        /// </summary>
        /// <param name="stock"></param>
        internal static void GenerateArticle(Stock stock)
        {
            var currentTime = Lib.Time.CurrentDateTime;
            var starter = NewsElements.VagueHeadlineStarts[MathHelper.Random.Next(NewsElements.VagueHeadlineStarts.Length)];
            var headline = NewsElements.Headlines[MathHelper.Random.Next(NewsElements.Headlines.Length)];
            _articles.Add(new Article(currentTime, $"{starter}: {stock.Name} ({stock.Symbol}) {headline}."));
        }

        /// <summary>
        /// Removes all articles that are atleast a week old.
        /// </summary>
        internal static void RemoveOutdatedArticles()
        {
            var currentDateTime = Lib.Time.CurrentDateTime;
            var entries = _articles.RemoveAll(a => (currentDateTime - a.DateTime).TotalDays >= 7);
            if (entries > 0 && Plugin.Instance.Config.IsDebugEnabled)
                Plugin.Log.LogInfo($"Removed {entries} outdated articles");
        }

        /// <summary>
        /// Clears all articles, use when starting a new game
        /// </summary>
        internal static void Clear()
        {
            _articles.Clear();
        }

        /// <summary>
        /// Imports articles from an existing save game
        /// </summary>
        /// <param name="articles"></param>
        internal static void Import(List<Article> articles)
        {
            Clear();
            foreach (var article in articles)
                _articles.Add(article);
        }
    }
}
