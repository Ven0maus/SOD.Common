using System.Collections.Generic;

namespace SOD.ZeroOverhead.Framework
{
    internal static class SimpleQueuePool<T>
    {
        private static readonly Stack<Queue<T>> _pool = new();

        public static Queue<T> Get()
        {
            return _pool.Count > 0 ? _pool.Pop() : new Queue<T>();
        }

        public static void Release(Queue<T> queue)
        {
            queue.Clear();
            _pool.Push(queue);
        }
    }
}
