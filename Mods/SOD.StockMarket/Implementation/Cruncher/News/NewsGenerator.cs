﻿using SOD.Common;
using SOD.StockMarket.Implementation.Stocks;
using System.Collections.Generic;
using System.Linq;

namespace SOD.StockMarket.Implementation.Cruncher.News
{
    internal static class NewsGenerator
    {
        private static readonly List<Article> _articles = new();
        internal static IReadOnlyList<Article> AllArticles => _articles;
        internal static Article[] UnreleasedArticles => _articles.Where(a => a.MinutesLeft > 0).ToArray();
        internal static Article[] ReleasedArticles => _articles.Where(a => a.MinutesLeft <= 0).ToArray();

        /// <summary>
        /// Generates an article for a specific stock.
        /// </summary>
        /// <param name="stock"></param>
        internal static void GenerateArticle(Stock stock, StockTrend? trend = null, bool murder = false)
        {
            var currentTime = Lib.Time.CurrentDateTime;

            // For murders the articles cannot release instantly
            // So we have a base of 10 steps, the total length can be up to 30 steps for murder or 40 steps for regular stock article
            var timeUntilRelease = trend != null ? MathHelper.Random.Next(murder ? 10 : 0, trend.Value.Steps / 100 * (murder ? 30 : 40)) : 0;

            Article article;
            // Decide which starter and headline
            if (murder)
            {
                var headline = NewsElements.MurderHeadlines[MathHelper.Random.Next(NewsElements.Headlines.Length - 1)];
                article = new Article(currentTime, string.Format(headline, $"{stock.Name} ({stock.Symbol})"), timeUntilRelease);
            }
            else
            {
                var starter = NewsElements.VagueHeadlineStarts[MathHelper.Random.Next(NewsElements.VagueHeadlineStarts.Length - 1)];
                var headline = NewsElements.Headlines[MathHelper.Random.Next(NewsElements.Headlines.Length - 1)];
                article = new Article(currentTime, $"{starter}: {stock.Name} ({stock.Symbol}) {headline}.", timeUntilRelease);
            }

            _articles.Add(article);

            if (Plugin.Instance.Config.IsDebugEnabled)
                Plugin.Log.LogInfo("Queued new unreleased article. MinutesTillRelease(" + article.MinutesLeft + "): " + article.Title);
        }

        internal static void TickToBeReleased()
        {
            foreach (var article in _articles.Where(a => a.MinutesLeft > 0))
            {
                article.MinutesLeft--;
                if (article.MinutesLeft <= 0)
                {
                    if (Plugin.Instance.Config.IsDebugEnabled)
                        Plugin.Log.LogInfo("Released article: " + article.Title);
                }
            }
        }

        /// <summary>
        /// Removes all articles that are atleast 5 days old.
        /// </summary>
        internal static void RemoveOutdatedArticles()
        {
            var currentDateTime = Lib.Time.CurrentDateTime;
            var entries = _articles.RemoveAll(a => (currentDateTime - a.DateTime).TotalDays >= 5);
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
            if (articles != null)
            {
                foreach (var article in articles)
                    _articles.Add(article);
            }
        }
    }
}
