using System.Collections.Generic;

namespace SOD.ZeroOverhead.Framework
{
    internal static class SimpleListPool<T>
    {
        private static readonly Stack<List<T>> _pool = new();

        public static List<T> Get()
        {
            return _pool.Count > 0 ? _pool.Pop() : new List<T>();
        }

        public static void Release(List<T> list)
        {
            list.Clear();
            _pool.Push(list);
        }
    }
}
