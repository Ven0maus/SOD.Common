using BepInEx;
using Il2CppInterop.Runtime.Injection;
using SOD.Common.BepInEx;
using SOD.StockMarket.Implementation;
using SOD.StockMarket.Implementation.Cruncher;
using SOD.StockMarket.Implementation.DataConversion;
using System;
using System.Reflection;

namespace SOD.StockMarket
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(Common.Plugin.PLUGIN_GUID)]
    public class Plugin : PluginController<Plugin, IPluginBindings>
    {
        public const string PLUGIN_GUID = "Venomaus.SOD.StockMarket";
        public const string PLUGIN_NAME = "StockMarket";
        public const string PLUGIN_VERSION = "1.0.0";

        /// <summary>
        /// The stockmarket running during the game, used to do all calculations
        /// </summary>
        internal Market Market { get; private set; }

        public override void Load()
        {
            // Register app content
            ClassInjector.RegisterTypeInIl2Cpp<StockMarketAppContent>();

            Harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.LogInfo("Plugin is patched.");

            // Initialize market object
            Market = new Market();
            Log.LogInfo("Initialized stock market hooks.");
        }

        public override void OnConfigureBindings()
        {
            base.OnConfigureBindings();
            ValidateBindingValues();
        }

        /// <summary>
        /// Validates if all bindings are valid and updates invalid ones.
        /// </summary>
        private void ValidateBindingValues()
        {
            var maxTrendSteps = Config.MaxHoursTrendsCanPersist;
            if (maxTrendSteps < 1)
                Config.MaxHoursTrendsCanPersist = Constants.MaxHoursTrendsCanPersist;

            var minTrendSteps = Config.MinHoursTrendsMustPersist;
            if (minTrendSteps < 1)
                Config.MinHoursTrendsMustPersist = Constants.MinHoursTrendsMustPersist;

            var trendChancePerStock = Config.StockTrendChancePercentage;
            if (trendChancePerStock < 0 || trendChancePerStock > 100)
                Config.StockTrendChancePercentage = Constants.StockTrendChancePercentage;

            // -1 is infinite, no point in going lower
            var maxTrends = Config.MaxTrends;
            if (maxTrends < -1)
                Config.MaxTrends = Constants.MaxTrends;

            var pastHistoricalDataVolatility = Config.PastHistoricalDataVolatility;
            if (pastHistoricalDataVolatility < 1.0)
                Config.PastHistoricalDataVolatility = Constants.PastHistoricalDataVolatility;

            // Fallback to default format
            var stockDataSaveFormat = Config.StockDataSaveFormat;
            if (!Enum.TryParse<DataSaveFormat>(stockDataSaveFormat.Trim(), true, out _))
            {
                Config.StockDataSaveFormat = Constants.StockDataSaveFormat;
            }
        }
    }
}