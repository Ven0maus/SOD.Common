using Bogus;

namespace SOD.StockMarket.Implementation.Stocks
{
    internal static class StockNameGenerator
    {
        private static readonly Faker faker = new();

        internal static string GenerateStockName()
        {
            return faker.Company.CompanyName();
        }
    }
}
