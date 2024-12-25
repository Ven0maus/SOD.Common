using SOD.Common;
using SOD.QoL.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace SOD.QoL.Patches
{
    internal class LostAndFoundPatches
    {
        private const string _expirationSaveFile = "LostAndFoundSaveData_{0}.json";
        private static ExpirationSaveData _expirationSaveData;

        internal static void ExpireTimedOutLafs()
        {
            if (!Lib.Time.IsInitialized || _expirationSaveData == null) return;

            var cityBuildings = HighlanderSingleton<CityBuildings>.Instance;
            if (cityBuildings == null || cityBuildings.buildingDirectory == null) return;

            var currentTime = Lib.Time.CurrentDateTime;
            foreach (var building in cityBuildings.buildingDirectory)
            {
                if (building.lostAndFound == null) continue;

                foreach (var laf in building.lostAndFound)
                {
                    var uid = GetUniqueId(laf);
                    if (_expirationSaveData.Expirations.TryGetValue(uid, out var lafExpireTime))
                    {
                        if (lafExpireTime <= currentTime)
                        {
                            // LaF is expired remove it
                            ExpireLostAndFound(building, laf);

                            // Remove also from tracking
                            _expirationSaveData.Expirations.Remove(uid);
                        }
                    }
                    else
                    {
                        // If it doesn't exist yet we'll just add it here
                        var expireHours = Plugin.Instance.Config.RandomizeExpireTime ?
                            Plugin.Random.Next(Plugin.Instance.Config.ExpireTimeMin, Plugin.Instance.Config.ExpireTimeMax + 1) :
                            Plugin.Instance.Config.ExpireTimeMax;
                        _expirationSaveData.Expirations[uid] = Lib.Time.CurrentDateTime.AddHours(expireHours);
                    }
                }
            }

            foreach (var kvp in _toBeDeleted)
            {
                kvp.Item1.lostAndFound.Remove(kvp.Item2);
                
                // Immediately create a new lost and found to replace the old one
                int num = kvp.Item1.preset.maxLostAndFound - kvp.Item1.lostAndFound.Count;
                if (num > 0)
                    kvp.Item1.TriggerNewLostAndFound();
            }
            _toBeDeleted.Clear();
        }

        internal static void InitializeExpireTimes(string saveFilePath)
        {
            var filePath = new Lazy<string>(() =>
            {
                var fileName = string.Format(_expirationSaveFile, Lib.SaveGame.GetUniqueString(saveFilePath));
                return Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), fileName);
            });

            if (saveFilePath == null || !File.Exists(filePath.Value))
            {
                _expirationSaveData = new()
                {
                    Expirations = new()
                };
                return;
            }

            try
            {
                var options = new JsonSerializerOptions
                {
                    Converters = { new TimeDataJsonConverter() },
                    WriteIndented = true
                };

                var json = File.ReadAllText(filePath.Value);
                _expirationSaveData = ExpirationSaveData.Deserialize(json);
                Plugin.Log.LogInfo("Loaded LostAndFoundSaveData from file.");
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Unable to read LostAndFoundSaveData file (corrupted?): {e.Message}");
                _expirationSaveData = new()
                {
                    Expirations = new()
                };
            }
        }

        internal static void SaveExpireTimes(string saveFilePath)
        {
            if (_expirationSaveData != null && _expirationSaveData.Expirations.Count > 0)
            {
                var fileName = string.Format(_expirationSaveFile, Lib.SaveGame.GetUniqueString(saveFilePath));
                var filePath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), fileName);

                try
                {
                    var json = _expirationSaveData.Serialize();
                    File.WriteAllText(filePath, json);
                    Plugin.Log.LogInfo("Saved LostAndFoundSaveData to file.");
                }
                catch (Exception e)
                {
                    Plugin.Log.LogError($"Unable to save LostAndFoundSaveData to file: {e.Message}");
                }
            }
        }

        internal static void DeleteSaveData(string saveFilePath)
        {
            var fileName = string.Format(_expirationSaveFile, Lib.SaveGame.GetUniqueString(saveFilePath));
            var filePath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), fileName);

            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    Plugin.Log.LogInfo("Deleted LostAndFoundSaveData file");
                }
                catch (Exception e)
                {
                    Plugin.Log.LogInfo($"Unable to delete LostAndFoundSaveData file: {e.Message}");
                }
            }
        }

        internal static string GetUniqueId(GameplayController.LostAndFound laf)
        {
            var data = laf.preset + "|" + laf.buildingID + "|" + laf.ownerID + "|" + laf.spawnedItem + "|" + laf.spawnedNote;
            return Lib.SaveGame.GetUniqueString(data);
        }

        private static readonly List<(NewBuilding, GameplayController.LostAndFound)> _toBeDeleted = new();
        internal static void ExpireLostAndFound(NewBuilding building, GameplayController.LostAndFound laf)
        {
            if (CityData.Instance.savableInteractableDictionary.TryGetValue(laf.spawnedNote, out var interactable))
            {
                interactable.MarkAsTrash(true, false, 0f);
                interactable.SafeDelete(false);
            }
            if (CityData.Instance.savableInteractableDictionary.TryGetValue(laf.spawnedItem, out interactable))
            {
                interactable.MarkAsTrash(true, false, 0f);
            }
            _toBeDeleted.Add((building, laf));
        }
    }
}
