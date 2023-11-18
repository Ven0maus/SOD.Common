using System.Collections.Generic;

namespace SOD.Common.Harmony.Common
{
    public static class HookHelper
    {
        private static readonly HashSet<string> _runOnlyOnce = new HashSet<string>();

        /// <summary>
        /// Stores the provided method name on first call and returns false.
        /// <br>Returns true if this is called again with this method name.</br>
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static bool RunOnlyOnce(string method)
        {
            if (_runOnlyOnce.Contains(method)) return true;
            _runOnlyOnce.Add(method);
            return false;
        }
    }
}
