using SOD.Common.Extensions;
using SOD.StockMarket.Core.Stocks;
using System;
using System.Linq;

namespace SOD.StockMarket.Core
{
    /// <summary>
    /// Info container regarding company details.
    /// </summary>
    internal class CompanyData
    {
        /// <summary>
        /// The name of the stock.
        /// </summary>
        internal string Name { get; private set; }
        /// <summary>
        /// The symbol of the stock.
        /// </summary>
        internal string Symbol { get; private set; }
        /// <summary>
        /// Used to determine stock starting price.
        /// </summary>
        internal decimal AverageSales => GetAverageSales();
        /// <summary>
        /// Used to determine stock starting price.
        /// </summary>
        internal decimal MinSalary => GetMinSalary();
        /// <summary>
        /// Used to determine stock starting price.
        /// </summary>
        internal decimal TopSalary => GetTopSalary();
        /// <summary>
        /// Used to determine stock price movement volatility.
        /// </summary>
        internal double Volatility { get; private set; }

        /// <summary>
        /// Reference to the in-game company, this is removed after initialize.
        /// </summary>
        private Company _company;

        /// <summary>
        /// Use this to insert exported stock information, or custom stocks.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="volatility"></param>
        /// <param name="symbol"></param>
        internal CompanyData(string name, double volatility, string symbol = null)
        {
            Name = name;
            Symbol = symbol ?? StockSymbolGenerator.Generate(Name);
            Volatility = volatility;
        }

        /// <summary>
        /// Use this to insert a new stock from an in-game company.
        /// </summary>
        /// <param name="company"></param>
        internal CompanyData(Company company)
        {
            _company = company;
        }

        /// <summary>
        /// Called later in the flow when Company object is initialized
        /// </summary>
        internal void Initialize()
        {
            if (_company != null)
            {
                Name = _company.name;
                Symbol = StockSymbolGenerator.Generate(Name);
                Volatility = Math.Round(MathHelper.Random.NextDouble(0.15d, 0.85d), 2);
            }

            // Check if we need to keep company in memory for something later?
            _company = null;
        }

        private decimal? _averageSales;
        private decimal GetAverageSales()
        {
            if (_company != null && _company.sales.Count > 0)
            {
                return (decimal)_company.sales
                    .Select(a => a.cost)
                    .DefaultIfEmpty()
                    .Average();
            }
            return _averageSales ??= MathHelper.Random.Next(5, 100, true);
        }

        private decimal? _minSalary;
        private decimal GetMinSalary()
        {
            if (_company != null && _company.minimumSalary > 0)
                return (decimal)_company.minimumSalary;
            return _minSalary ??= MathHelper.Random.Next(350, 750, true);
        }

        private decimal? _topSalary;
        private decimal GetTopSalary()
        {
            if (_company != null && _company.topSalary > 0)
                return (decimal)_company.topSalary;
            return _topSalary ??= MathHelper.Random.Next((int)GetMinSalary() + 1, 2000);
        }
    }
}
