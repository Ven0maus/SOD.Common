using System;
using System.Globalization;

namespace SOD.StockMarket.Implementation.Trade
{
    internal static class Extensions
    {
        internal static string ToLimitedString(this decimal value, int max, CultureInfo culture = null)
        {
            return value.ToString(culture ?? CultureInfo.InvariantCulture)[..Math.Min(max, value.ToString(culture ?? CultureInfo.InvariantCulture).Length)];
        }
    }
}
