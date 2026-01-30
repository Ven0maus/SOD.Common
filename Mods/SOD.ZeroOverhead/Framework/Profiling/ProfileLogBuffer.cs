using System.Collections.Generic;
using System.Diagnostics;

namespace SOD.ZeroOverhead.Framework.Profiling
{
    /// <summary>
    /// Buffered log for a wrap scope.
    /// </summary>
    internal class ProfileLogBuffer
    {
        private readonly List<(int depth, string log)> _entries = new();
        private readonly List<ProfileLogBuffer> _children = new();
        private readonly string _name;
        private Stopwatch _sw;

        public ProfileLogBuffer(string name)
        {
            _name = name;
        }

        public void Start()
        {
            _sw = Stopwatch.StartNew();
        }

        public void Stop()
        {
            _sw.Stop();
        }

        public void AddChild(ProfileLogBuffer child)
        {
            _children.Add(child);
        }

        public void AddEntry(int depth, string log)
        {
            _entries.Add((depth, log));
        }

        public string Format(int parentDepth = 0)
        {
            string indent = new string(' ', parentDepth * 4);
            string displayTime = _sw.Elapsed.TotalMilliseconds < 1
                ? $"{_sw.Elapsed.TotalMilliseconds * 1000:F3} µs"
                : $"{_sw.Elapsed.TotalMilliseconds:F3} ms";

            string result = $"{indent}[{_name}] {displayTime}\n";

            foreach (var entry in _entries)
            {
                string eIndent = new string(' ', (parentDepth + entry.depth) * 4);
                result += $"{eIndent}{entry.log}\n";
            }

            foreach (var child in _children)
            {
                result += child.Format(parentDepth + 1);
            }

            return result.TrimEnd();
        }
    }

}
