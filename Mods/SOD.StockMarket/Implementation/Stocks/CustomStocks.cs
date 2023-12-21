namespace SOD.StockMarket.Implementation.Stocks
{
    /// <summary>
    /// All the custom stock configurations that are added to the market.
    /// </summary>
    internal static class CustomStocks
    {
        internal readonly static (CompanyStockData data, decimal? basePrice)[] Stocks = new (CompanyStockData data, decimal? basePrice)[]
        {
            // Mega-corporations
            (new CompanyStockData("Starch Kola", 0.4d), (decimal)MathHelper.Random.NextDouble(5000f, 10000f)),
            (new CompanyStockData("Kaizen", 0.3d), (decimal)MathHelper.Random.NextDouble(2000f, 5000f)),

            // Currencies
            (new CompanyStockData("Crow", 0.05d), (decimal)MathHelper.Random.NextDouble(0.975f, 1.025f))
        };
    }
}
