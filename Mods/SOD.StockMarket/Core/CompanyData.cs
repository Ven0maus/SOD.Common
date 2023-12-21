using SOD.Common.Extensions;
using System;
using System.Linq;

namespace SOD.StockMarket.Core
{
    internal class CompanyData
    {
        internal string Name { get; private set; }
        internal string Symbol { get; private set; }
        internal Company Company { get; private set; }
        internal decimal AverageSales => GetAverageSales();
        internal decimal MinSalary => GetMinSalary();
        internal decimal TopSalary => GetTopSalary();
        internal double Volatility { get; private set; }

        internal CompanyData(string name, string symbol, double volatility)
        {
            Name = name;
            Symbol = symbol.ToUpper();
            Volatility = volatility;
        }

        internal CompanyData(Company company)
        { 
            Company = company;
        }

        /// <summary>
        /// Called later in the flow when Company object is initialized
        /// </summary>
        internal void UpdateInfo()
        {
            if (Company != null)
            {
                Name = Company.name;
                Symbol = string.Join("", Name.Split(' ').Select(a => a.Trim()[0])).ToUpper();
                Volatility = Math.Round(MathHelper.Random.NextDouble(0.15d, 0.85d), 2);
            }

            // Check if we need to keep company in memory for something later?
            Company = null;
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
            return _averageSales ??= MathHelper.Random.Next(5, 100);
        }

        private decimal? _minSalary;
        private decimal GetMinSalary()
        {
            if (Company != null && Company.minimumSalary > 0)
                return (decimal)Company.minimumSalary;
            return _minSalary ??= MathHelper.Random.Next(350, 750);
        }

        private decimal? _topSalary;
        private decimal GetTopSalary()
        {
            if (Company != null && Company.topSalary > 0)
                return (decimal)Company.topSalary;
            return _topSalary ??= MathHelper.Random.Next((int)GetMinSalary() + 1, 2000);
        }
    }
}
