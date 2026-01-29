using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SOD.ZeroOverhead.Framework
{
    /// <summary>
    /// A helper to easily profile runs.
    /// </summary>
    internal static class ProfilingHelper
    {
        private static readonly Dictionary<(Type, string), Queue<Stopwatch>> _profileRuns = new();

        /// <summary>
        /// Start a new profile run for a given method.
        /// </summary>
        /// <param name="classType"></param>
        /// <param name="methodName"></param>
        public static void Profile(Type classType, string methodName)
        {
            var key = (classType, methodName);
            if (!_profileRuns.TryGetValue(key, out var queue))
            {
                _profileRuns[key] = queue = SimpleQueuePool<Stopwatch>.Get();
            }
            var sw = Stopwatch.StartNew();
            queue.Enqueue(sw);
        }

        /// <summary>
        /// Conclude a profile run for a given method.
        /// </summary>
        /// <param name="classType"></param>
        /// <param name="methodName"></param>
        public static void Conclude(Type classType, string methodName)
        {
            var key = (classType, methodName);
            if (!_profileRuns.TryGetValue(key, out var queue))
            {
                return;
            }

            var sw = queue.Dequeue();
            sw.Stop();

            if (queue.Count == 0)
            {
                _profileRuns.Remove(key);
                SimpleQueuePool<Stopwatch>.Release(queue);
            }

            Plugin.LogDebug($"<color={ConsoleColor.Cyan}>[{classType.Name}.{methodName}] took {sw.Elapsed.TotalMilliseconds.ToString("F3", System.Globalization.CultureInfo.InvariantCulture)} ms</color>");
        }
    }
}
