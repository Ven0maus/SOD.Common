using BepInEx;
using SOD.Common;
using SOD.Common.BepInEx;
using SOD.ZeroOverhead.Framework.Profiling;
using System;
using System.Reflection;

namespace SOD.ZeroOverhead
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(Common.Plugin.PLUGIN_GUID)]
    public class Plugin : PluginController<Plugin, IPluginBindings>
    {
        public const string PLUGIN_GUID = "Venomaus.SOD.ZeroOverhead";
        public const string PLUGIN_NAME = "ZeroOverhead";
        public const string PLUGIN_VERSION = "1.0.0";

        public override void Load()
        {
            // Profiler is only enabled when debug mode is true
            Profiler.Enabled = Config.EnableDebugMode;

            Lib.Time.OnMinuteChanged += Time_OnMinuteChanged;

            Harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.LogInfo("Plugin is patched.");
        }

        private void Time_OnMinuteChanged(object sender, Common.Helpers.TimeChangedArgs e)
        {
            Profiler.LogFrameAverages();
        }

        public static void LogDebug(string message)
        {
            if (!Instance.Config.EnableDebugMode) return;
            Log.LogMessage($"<color={ConsoleColor.DarkCyan}>[DEBUG]:</color> {message}");
        }
    }
}
