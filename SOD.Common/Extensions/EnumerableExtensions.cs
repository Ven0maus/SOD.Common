using System;
using System.Collections.Generic;

namespace SOD.Common.Extensions
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var value in enumerable)
                action(value);
        }

        public static IEnumerable<TResult> Select<TSource, TResult>(
            this Il2CppSystem.Collections.Generic.List<TSource> source, Func<TSource, TResult> selector)
        {
            foreach (var value in source)
                yield return selector.Invoke(value);
        }

        public static IEnumerable<T> Where<T>(
            this Il2CppSystem.Collections.Generic.List<T> enumerable, Func<T, bool> criteria)
        {
            foreach (var value in enumerable)
            {
                if (criteria.Invoke(value))
                    yield return value;
            }
        }

        public static Il2CppSystem.Collections.Generic.List<T> ToListIL2Cpp<T>(IEnumerable<T> enumerable)
        {
            var list = new Il2CppSystem.Collections.Generic.List<T>();
            foreach (var value in enumerable)
                list.Add(value);
            return list;
        }
    }
}
