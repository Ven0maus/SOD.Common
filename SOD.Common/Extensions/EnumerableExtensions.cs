using System;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// Convert to an IL2CPP list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static Il2CppSystem.Collections.Generic.List<T> ToListIL2Cpp<T>(IEnumerable<T> enumerable)
        {
            var list = new Il2CppSystem.Collections.Generic.List<T>();
            foreach (var value in enumerable)
                list.Add(value);
            return list;
        }

        /// <summary>
        /// Convert to a regular list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<T> ToList<T>(this Il2CppSystem.Collections.Generic.List<T> list)
        {
            return list.Select(a => a).ToList();
        }
    }
}
