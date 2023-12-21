using SOD.StockMarket.Core.DataConversion.Converters;
using System;
using System.Collections.Generic;
using System.IO;

namespace SOD.StockMarket.Core.DataConversion
{
    internal static class ConverterFactory
    {
        private static readonly Dictionary<string, Func<IDataConverter>> _converters = new(StringComparer.OrdinalIgnoreCase)
        {
            { ".csv", () => CsvConverter.Create() },
            { ".bin", () => BinaryConverter.Create() },
        };

        public static IDataConverter Get(string filePath)
        {
            var ext = Path.GetExtension(filePath);
            if (_converters.TryGetValue(ext, out var converterFactory))
                return converterFactory.Invoke();
            throw new NotSupportedException($"Data type \"{ext}\" is not supported.");
        }
    }
}
