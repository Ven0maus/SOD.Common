using HarmonyLib;
using SOD.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace SOD.LifeAndLiving.Patches.EconomyRebalancePatches
{
    internal class NewGameLocationPatches
    {
        [HarmonyPatch(typeof(NewGameLocation), nameof(NewGameLocation.GetPrice))]
        internal class NewGameLocation_GetPrice
        {
            private static readonly Random _valueRandom = new();
            internal static readonly Dictionary<string, int> ApartementPriceCache = new();

            [HarmonyPostfix]
            internal static void Postfix(NewGameLocation __instance, ref int __result)
            {
                if (Plugin.Instance.Config.DisableEconomyRebalance) return;
                var key = __instance.building.buildingID + "_" + __instance.residenceNumber;
                if (ApartementPriceCache.TryGetValue(key, out int newValue))
                {
                    __result = newValue;
                    return;
                }

                // Initial calculation, some values may pass (high values that are over the minimum)
                __result += __result / 100 * Plugin.Instance.Config.ApartementCostPercentage;

                // Minimum value should be respected
                if (__result < Plugin.Instance.Config.MinimumApartementCost)
                {
                    var half = Plugin.Instance.Config.MinimumApartementCost / 2;
                    var smallPercentage = half / 100 * 25;
                    __result = RoundToNearestInterval(_valueRandom.Next(half, half + smallPercentage) * 2, 50, 100);
                }

                ApartementPriceCache.Add(key, __result);
            }

            private static int RoundToNearestInterval(int number, int lowerInterval, int higherInterval)
            {
                int roundedToLowerInterval = (int)Math.Round(number / (double)lowerInterval) * lowerInterval;
                int roundedToHigherInterval = (int)Math.Round(number / (double)higherInterval) * higherInterval;

                int difference1 = Math.Abs(number - roundedToLowerInterval);
                int difference2 = Math.Abs(number - roundedToHigherInterval);

                if (difference1 < difference2)
                {
                    return roundedToLowerInterval;
                }
                else
                {
                    return roundedToHigherInterval;
                }
            }

            internal static void Load(string hash)
            {
                ApartementPriceCache.Clear();

                var apartmentPath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"apartmentpricecache_{hash}.json");
                if (File.Exists(apartmentPath))
                {
                    var json = File.ReadAllText(apartmentPath);
                    var jsonContent = JsonSerializer.Deserialize<Dictionary<string, int>>(json);
                    foreach (var value in jsonContent)
                        ApartementPriceCache.Add(value.Key, value.Value);
                    Plugin.Log.LogInfo("Loaded apartment price cache.");
                }
            }

            internal static void Save(string hash)
            {
                if (ApartementPriceCache.Count > 0)
                {
                    var apartmentFilePath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"apartmentpricecache_{hash}.json");
                    var json = JsonSerializer.Serialize(ApartementPriceCache, new JsonSerializerOptions { WriteIndented = false });
                    File.WriteAllText(apartmentFilePath, json);
                    Plugin.Log.LogInfo("Saved apartment price cache.");
                }
            }

            internal static void Delete(string hash)
            {
                var apartmentFilePath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"apartmentpricecache_{hash}.json");
                if (File.Exists(apartmentFilePath))
                {
                    File.Delete(apartmentFilePath);
                    Plugin.Log.LogInfo("Deleted apartment price cache.");
                }
            }
        }
    }
}
