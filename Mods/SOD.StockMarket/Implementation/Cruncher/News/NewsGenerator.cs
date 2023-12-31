using SOD.StockMarket.Implementation.Stocks;
using System.Collections.Generic;

namespace SOD.StockMarket.Implementation.Cruncher.News
{
    internal static class NewsGenerator
    {
        public static List<Article> Articles = new();

        // Some ideas:
        // Have stocks have other competitor stocks
        // This means when a news article negatively impacts stock A
        // Stocks B, C.. might get a positive trend because of this

        // There could also be generic sectors for stocks such as, an article that covers a whole sector (eg industrial)
        // Then some related industrial stocks could have a trend

        public static Article GenerateArticle(Stock stock)
        {
            return null;
        }
    }
}
