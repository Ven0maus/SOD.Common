using AssetBundleLoader;
using HarmonyLib;
using SOD.Common.Extensions;
using SOD.StockMarket.Implementation.Cruncher;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SOD.StockMarket.Patches
{
    internal class MainMenuControllerPatches
    {
        [HarmonyPatch(typeof(MainMenuController), nameof(MainMenuController.Start))]
        internal class MainMenuController_Start
        {
            private static bool _init = false;

            [HarmonyPostfix]
            internal static void Postfix()
            {
                if (_init) return;
                _init = true;

                // This loads the stock market content and add's it to the crunchers
                LoadStockMarketBundle();
            }

            private static void LoadStockMarketBundle()
            {
                // Get bundle information
                var bundleName = "stockmarketbundle";
                var bundleDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), bundleName);
                var assetBundle = BundleLoader.LoadBundle(bundleDirectory, stable: true);

                // Load cruncher app preset
                var newCruncherApp = assetBundle.LoadAsset<CruncherAppPreset>("StockMarketPreset");
                _ = newCruncherApp.appContent[0].AddComponent<StockMarketAppContent>();

                // Add to crunchers
                InsertAppToCrunchers(newCruncherApp);
            }

            private static void InsertAppToCrunchers(CruncherAppPreset preset)
            {
                foreach (var cruncher in Resources.FindObjectsOfTypeAll<InteractablePreset>()
                    .Where(preset => preset.name.Contains("Cruncher")))
                {
                    cruncher.additionalApps.Insert(cruncher.additionalApps.Count - 2, preset);
                }
            }
        }
    }
}
