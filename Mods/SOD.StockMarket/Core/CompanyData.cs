using SOD.Common.Extensions;
using System.Linq;

namespace SOD.StockMarket.Core
{
    internal class CompanyData
    {
        internal string Name { get; private set; }
        internal string Symbol { get; private set; }
        internal Company Company { get; }
        internal decimal AverageSales => GetAverageSales();
        internal decimal MinSalary => GetMinSalary();
        internal decimal TopSalary => GetTopSalary();

        internal CompanyData(string name, string symbol)
        {
            Name = name;
            Symbol = symbol.ToUpper();
        }

        internal CompanyData(Company company)
        { 
            Company = company;
        }

        internal void UpdateInfo()
        {
            if (Company != null)
            {
                Name = Company.name;
                Symbol = string.Join("", Name.Split(' ').Select(a => a.Trim()[0])).ToUpper();
            }
        }

        private decimal? _averageSales;
        private decimal GetAverageSales()
        {
            if (Company != null && Company.sales.Count > 0)
            {
                return (decimal)Company.sales
                    .Select(a => a.cost)
                    .DefaultIfEmpty()
                    .Average();
            }
            return _averageSales ??= Helpers.Random.Next(5, 100);
        }

        private decimal? _minSalary;
        private decimal GetMinSalary()
        {
            if (Company != null && Company.minimumSalary > 0)
                return (decimal)Company.minimumSalary;
            return _minSalary ??= Helpers.Random.Next(350, 750);
        }

        private decimal? _topSalary;
        private decimal GetTopSalary()
        {
            if (Company != null && Company.topSalary > 0)
                return (decimal)Company.topSalary;
            return _topSalary ??= Helpers.Random.Next((int)GetMinSalary() + 1, 2000);
        }
    }
}
