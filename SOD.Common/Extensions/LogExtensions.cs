using BepInEx.Logging;
using System;

namespace SOD.Common.Extensions
{
    public static class LogExtensions
    {
        /// <summary>
        /// Use as follows: LogMethodEntry(typeof(Class), nameof(Class.Method));
        /// <br>Will log following message: "Method entry \"{className}_{methodName}\"".</br>
        /// </summary>
        /// <param name="manualLogSource"></param>
        /// <param name="classType"></param>
        /// <param name="methodName"></param>
        public static void LogMethodEntry(this ManualLogSource manualLogSource, Type classType, string methodName)
        {
            manualLogSource.LogInfo($"Method entry \"{classType.Name}_{methodName}\".");
        }

        /// <summary>
        /// Use as follows: LogMethodExit(typeof(Class), nameof(Class.Method));
        /// <br>Will log following message: "Method exit \"{className}_{methodName}\"".</br>
        /// </summary>
        /// <param name="manualLogSource"></param>
        /// <param name="classType"></param>
        /// <param name="methodName"></param>
        public static void LogMethodExit(this ManualLogSource manualLogSource, Type classType, string methodName)
        {
            manualLogSource.LogInfo($"Method exit \"{classType.Name}_{methodName}\".");
        }
    }
}
