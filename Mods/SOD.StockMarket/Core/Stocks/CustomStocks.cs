namespace SOD.StockMarket.Core.Stocks
{
    /// <summary>
    /// All the custom stock configurations that are added to the market.
    /// </summary>
    internal static class CustomStocks
    {
        internal readonly static (CompanyData data, decimal? basePrice)[] Stocks = new (CompanyData data, decimal? basePrice)[]
        {
            // Mega-corporations
            (new CompanyData("Starch Kola", "STK", 0.4d), (decimal)MathHelper.Random.NextDouble(5000f, 10000f)),
            (new CompanyData("Kaizen", "KAI", 0.3d), (decimal)MathHelper.Random.NextDouble(2000f, 5000f)),

            // Currencies
            (new CompanyData("Crow", "CRO", 0.05d), (decimal)MathHelper.Random.NextDouble(0.975f, 1.025f))
        };
    }
}
