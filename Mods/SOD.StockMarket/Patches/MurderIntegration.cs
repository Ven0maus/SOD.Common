using SOD.Common;
using SOD.StockMarket.Implementation;
using SOD.StockMarket.Implementation.Cruncher.News;
using SOD.StockMarket.Implementation.Stocks;
using System.Linq;

namespace SOD.StockMarket.Patches
{
    internal class MurderIntegration
    {
        internal static void Initialize()
        {
            Lib.Detective.OnVictimReported += Detective_OnVictimReported;
        }

        private static void Detective_OnVictimReported(object sender, Common.Helpers.DetectiveObjects.VictimArgs e)
        {
            var victim = e.Victim;
            var reporter = e.Reporter;
            var reportType = e.ReportType;

            if (Plugin.Instance.Config.IsDebugEnabled)
                Plugin.Log.LogInfo($"Victim \"{victim.GetCitizenName()}\" was reported by \"{reporter.GetCitizenName()}\" | Report Type: \"{reportType}\".");

            if (!Plugin.Instance.Market.Initialized) return;

            var job = victim.job;
            if (job == null || job.employer == null) return;

            var company = job.employer;

            // Find stock related to the company
            var stock = Plugin.Instance.Market.Stocks.FirstOrDefault(a => a.CompanyId == company.companyID);
            if (stock == null)
            {
                if (Plugin.Instance.Config.IsDebugEnabled)
                    Plugin.Log.LogInfo($"Victim was part of company \"{company.name}\", but no stock was found. Self employed or illegal companies don't have stocks.");
                return;
            }

            if (Plugin.Instance.Config.IsDebugEnabled)
                Plugin.Log.LogInfo($"Victim was part of company \"{company.name}\", created a negative \"{stock.Symbol}\" stock impact.");

            // Remove existing trend if it exists
            if (stock.Trend != null)
                stock.RemoveTrend();

            var minTrendPercentage = Plugin.Instance.Config.MinimumMurderTrendPercentage;
            var maxTrendPercentage = Plugin.Instance.Config.MaximumMurderTrendPercentage;

            // Generate a new negative trend
            var stockTrend = new StockTrend(MathHelper.Random.Next(minTrendPercentage, maxTrendPercentage), stock.Price, Market.CalculateRandomSteps());
            stock.SetTrend(stockTrend);

            if (Plugin.Instance.Config.IsDebugEnabled)
                Plugin.Log.LogInfo($"Created trend: {stockTrend.Percentage}% | Source Price: {stockTrend.StartPrice} | Target Price: {stockTrend.EndPrice} | Steps: {stockTrend.Steps}.");

            if (!Plugin.Instance.Market.Simulation)
                NewsGenerator.GenerateArticle(stock, stockTrend, murder: true);
        }
    }
}
