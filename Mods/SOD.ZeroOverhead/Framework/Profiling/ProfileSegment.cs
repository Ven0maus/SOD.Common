using System;
using System.Diagnostics;

namespace SOD.ZeroOverhead.Framework.Profiling
{
    public sealed class ProfileSegment : IDisposable
    {
        private readonly Stopwatch _stopwatch;
        private readonly string _segmentName;
        public TimeSpan Elapsed => _stopwatch.Elapsed;

        public ProfileSegment(string segmentName)
        {
            _segmentName = segmentName;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            Plugin.LogDebug($"<color={ConsoleColor.Cyan}>[{_segmentName}] took {Elapsed.TotalMilliseconds.ToString("F3", System.Globalization.CultureInfo.InvariantCulture)} ms</color>");
        }
    }
}
