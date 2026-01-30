using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace SOD.ZeroOverhead.Framework.Profiling
{
    /// <summary>
    /// A helper to easily profile runs.
    /// </summary>
    public static class Profiler
    {
        public static bool Enabled { get; set; }

        private static readonly ConcurrentDictionary<(Type, string), Queue<Stopwatch>> _runningTimers = new();
        private static readonly ConcurrentDictionary<(Type, string), ProfileStats> _stats = new();
        private static readonly AsyncLocal<Stack<ProfileLogBuffer>> _wrapStack = new();
        private static readonly ConcurrentDictionary<(Type, string), FrameStats> _frameStats = new();
        private static readonly AsyncLocal<Dictionary<(Type, string), double>> _frameAccumulated = new();

        /// <summary>FileName
        /// Wrap a block of code, profiling nested calls and outputting a single combined log at the end.
        /// </summary>
        public static void Wrap(string name, Action action)
        {
            if (!Enabled)
            {
                action();
                return;
            }

            // Create new buffer for this wrap scope
            var buffer = new ProfileLogBuffer(name);
            _wrapStack.Value ??= new Stack<ProfileLogBuffer>();
            _wrapStack.Value.Push(buffer);

            buffer.Start();

            try
            {
                action();
            }
            finally
            {
                buffer.Stop();
                _wrapStack.Value.Pop();

                // If there’s a parent wrap, append this log to it
                if (_wrapStack.Value.Count > 0)
                    _wrapStack.Value.Peek().AddChild(buffer);
                else
                    Plugin.LogDebug(buffer.Format());
            }
        }

        private static void WriteBufferedLog(string log, int depth)
        {
            if (_wrapStack.Value != null && _wrapStack.Value.Count > 0)
            {
                _wrapStack.Value.Peek().AddEntry(depth, log);
            }
            else
            {
                Plugin.LogDebug(log); // fallback: immediate log
            }
        }

        public static void BeginFrame()
        {
            if (!Enabled) return;
            _frameAccumulated.Value = new Dictionary<(Type, string), double>();
        }

        public static void EndFrame(int logEveryNFrames = 60)
        {
            if (!Enabled || _frameAccumulated.Value == null)
                return;

            foreach (var kv in _frameAccumulated.Value)
            {
                var stats = _frameStats.GetOrAdd(kv.Key, _ => new FrameStats(logEveryNFrames));
                stats.Add(kv.Value);
            }

            _frameAccumulated.Value = null;
        }

        public static void LogFrameAverages()
        {
            foreach (var kv in _frameStats)
            {
                var stats = kv.Value;

                string color = stats.Average < 1 ? "green"
                             : stats.Average < 5 ? "yellow" : "red";

                Plugin.LogDebug(
                    $"<color={color}>[FRAME AVG] {kv.Key.Item1.Name}.{kv.Key.Item2} " +
                    $"avg {stats.Average:F3} ms (min {stats.Min:F3}, max {stats.Max:F3})</color>");
            }
        }

        public static void StartProfileMethod(Type classType, string methodName)
        {
            if (!Enabled) return;

            var key = (classType, methodName);
            var queue = _runningTimers.GetOrAdd(key, _ => new Queue<Stopwatch>());
            queue.Enqueue(Stopwatch.StartNew());
        }

        public static void ConcludeMethodProfile(Type classType, string methodName, bool debugLog = true)
        {
            if (!Enabled) return;

            var key = (classType, methodName);

            if (!_runningTimers.TryGetValue(key, out var queue) || queue.Count == 0)
                return;

            var sw = queue.Dequeue();
            sw.Stop();

            if (queue.Count == 0)
                _runningTimers.TryRemove(key, out _);

            var stats = _stats.GetOrAdd(key, _ => new ProfileStats());
            stats.Add(sw.Elapsed.TotalMilliseconds);

            if (debugLog)
            {
                string color = sw.Elapsed.TotalMilliseconds < 1 ? "green"
                             : sw.Elapsed.TotalMilliseconds < 10 ? "yellow" : "red";

                Plugin.LogDebug($"<color={color}>[{classType.Name}.{methodName}] {sw.Elapsed.TotalMilliseconds:F3} ms (Run {stats.Count}, avg {stats.Average:F3} ms, min {stats.Min:F3} ms, max {stats.Max:F3} ms)</color>");
            }

            if (_frameAccumulated.Value != null)
            {
                _frameAccumulated.Value.TryGetValue(key, out var current);
                _frameAccumulated.Value[key] = current + sw.Elapsed.TotalMilliseconds;
            }
        }

        public static void Profile(string name, Action action)
        {
            if (!Enabled) { action(); return; }
            using var seg = new ProfileSegment(name);
            action();
        }

        public static T Profile<T>(string name, Func<T> action)
        {
            if (!Enabled) return action();
            using var seg = new ProfileSegment(name);
            return action();
        }

        private sealed class ProfileSegment : IDisposable
        {
            private static readonly AsyncLocal<int> _depth = new(); // thread-local depth
            private readonly Stopwatch _sw;
            private readonly string _name;
            private readonly Type _callerType;

            internal ProfileSegment(string name)
            {
                _name = name;
                _callerType = new StackTrace().GetFrame(1).GetMethod().DeclaringType;

                _depth.Value++; // enter new depth
                _sw = Stopwatch.StartNew();
            }

            public void Dispose()
            {
                _sw.Stop();

                // Create indentation based on depth
                string indent = new(' ', (_depth.Value - 1) * 4);

                // Conclude and log
                double elapsedMs = _sw.Elapsed.TotalMilliseconds;
                string color = elapsedMs < 1 ? "green" : elapsedMs < 10 ? "yellow" : "red";
                string log = $"{indent}<color={color}>[{_callerType.Name}.{_name}] {elapsedMs:F3} ms</color>";

                // Write to wrap buffer if active
                WriteBufferedLog(log, _depth.Value - 1);

                _depth.Value--; // exit depth
            }
        }

        private class ProfileStats
        {
            public int Count;
            public double Total;
            public double Min;
            public double Max;
            public double Average => Count > 0 ? Total / Count : 0;

            public void Add(double ms)
            {
                Count++;
                Total += ms;
                Min = Count == 1 ? ms : Math.Min(Min, ms);
                Max = Count == 1 ? ms : Math.Max(Max, ms);
            }
        }
    }
}
