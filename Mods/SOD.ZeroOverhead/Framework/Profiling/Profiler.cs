using SOD.ZeroOverhead.Framework.Pooling;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace SOD.ZeroOverhead.Framework.Profiling
{
    /// <summary>
    /// A helper to easily profile runs.
    /// </summary>
    public static class Profiler
    {
        private static readonly ConcurrentDictionary<(Type, string), Queue<Stopwatch>> _profileRuns = new();

        /// <summary>
        /// Start a new profile for a specific action, concludes automatically after the action is finished.
        /// </summary>
        /// <param name="profileName"></param>
        /// <param name="action"></param>
        public static void Profile(Action action, string profileName = null)
        {
            using var segment = new ProfileSegment(profileName ?? new StackTrace().GetFrame(1).GetMethod().Nam;);
            action();
        }

        /// <summary>
        /// Start a new profile for a specific action, concludes automatically after the action is finished.
        /// </summary>
        /// <param name="profileName"></param>
        /// <param name="action"></param>
        public static T Profile<T>(Func<T> action, string profileName = null)
        {
            using var segment = new ProfileSegment(profileName ?? new StackTrace().GetFrame(1).GetMethod().Name);
            return action();
        }

        /// <summary>
        /// Start a new profile run for a given method.
        /// </summary>
        /// <param name="classType"></param>
        /// <param name="methodName"></param>
        public static void StartProfileMethod(Type classType, string methodName)
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
        public static void ConcludeMethodProfile(Type classType, string methodName, double minMilliseconds = 0)
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

            if (sw.Elapsed.TotalMilliseconds >= minMilliseconds)
                Plugin.LogDebug($"<color={ConsoleColor.Cyan}>[{classType.Name}.{methodName}] took {sw.Elapsed.TotalMilliseconds.ToString("F3", System.Globalization.CultureInfo.InvariantCulture)} ms</color>");
        }
    }
}
