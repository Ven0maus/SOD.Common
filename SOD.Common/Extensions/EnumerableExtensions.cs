using System;
using System.Collections.Generic;

namespace SOD.Common.Extensions
{
    internal static class EnumerableExtensions
    {
        internal static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var value in enumerable)
                action(value);
        }
    }
}
