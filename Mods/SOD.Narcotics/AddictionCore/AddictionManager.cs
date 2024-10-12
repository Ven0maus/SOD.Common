using SOD.Common;
using SOD.Common.Custom;
using SOD.Narcotics.AddictionCore.Addictions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace SOD.Narcotics.AddictionCore
{
    public static class AddictionManager
    {
        // Dynamic data
        private readonly static Dictionary<AddictionType, Addiction> _addictions = new();
        private readonly static Dictionary<AddictionType, float> _addictionMeters = new();
        private readonly static Dictionary<AddictionType, float> _susceptibilityFactors = new();

        // Config loaded on plugin start
        private readonly static Dictionary<AddictionType, float> _addictionPotentials = new();
        private readonly static Dictionary<AddictionType, bool> _enabledAddictions = new();
        private static float _hourlyAddictionRecoveryRate = 0.1f;

        private static MersenneTwister _random;
        public static MersenneTwister Random
        {
            get
            {
                return _random ??= new MersenneTwister((int)Lib.SaveGame.GetUniqueNumber(CityData.Instance.seed));
            }
        }

        /// <summary>
        /// Assigns a new addiction.
        /// </summary>
        /// <param name="addictionType"></param>
        /// <param name="stage"></param>
        public static void Assign(AddictionType addictionType)
        {
            if (!_enabledAddictions.ContainsKey(addictionType) || _addictions.ContainsKey(addictionType)) return;

            // Get a new addiction class
            Addiction addiction = AddictionFactory.Get(addictionType);
            addiction.Initialize();

            // Add to collection
            _addictions[addictionType] = addiction;

            if (Plugin.Instance.Config.DebugMode)
                Plugin.Log.LogInfo($"You have become addicted to \"{addictionType.ToString().ToLower()}\".");
        }

        /// <summary>
        /// Cures the given addiction.
        /// </summary>
        /// <param name="addictionType"></param>
        public static void Cure(AddictionType addictionType)
        {
            if (!_enabledAddictions.ContainsKey(addictionType)) return;
            var addiction = GetAddiction(addictionType);
            if (addiction != null)
            {
                _addictions.Remove(addictionType);
                addiction.Cure();
            }
        }

        /// <summary>
        /// This method is called when an item related to an addiction is consumed.
        /// It worsens the addiction by progressing its stage.
        /// </summary>
        public static void OnItemConsumed(AddictionType addictionType, float consumptionPercentage, float? itemPotency = null)
        {
            if (!_enabledAddictions.ContainsKey(addictionType)) return;

            // Add all entries if missing
            if (!_addictionMeters.ContainsKey(addictionType))
                _addictionMeters[addictionType] = 0;

            // Define the susceptibility of the player
            if (!_susceptibilityFactors.ContainsKey(addictionType))
            {
                _susceptibilityFactors[addictionType] = Random.NextFloat(
                    Plugin.Instance.Config.MinimumSusceptibility, 
                    Plugin.Instance.Config.MaximumSusceptibility);
            }

            // Calculate how much of the item's potency is consumed based on the consumption percentage
            // consumptionPercentage should be a value between 0 and 1, representing the proportion of the item consumed.
            float effectivePotency = (itemPotency ?? 1.0f) * consumptionPercentage;

            // Calculate addiction increment
            float addictionIncrement = effectivePotency * _susceptibilityFactors[addictionType] * (_addictionPotentials[addictionType] * 100) * UnityEngine.Time.deltaTime;

            // Update addiction meter
            _addictionMeters[addictionType] += addictionIncrement;

            if (Plugin.Instance.Config.DebugMode)
            {
                Plugin.Log.LogInfo($"Consumption percentage: {consumptionPercentage}, Potency: {effectivePotency}, Addiction Increment: {addictionIncrement}");
                Plugin.Log.LogInfo($"You consumed {addictionType.ToString().ToLower()}: {_addictionMeters[addictionType]}/1");
            }

            // Check if consumption exceeds the threshold, triggering addiction
            if (_addictionMeters[addictionType] >= 1f)
            {
                _addictionMeters[addictionType] = 0f;

                var addiction = GetAddiction(addictionType);
                if (addiction == null)
                {
                    Assign(addictionType);
                }
                else
                {
                    // Remain at 100 if we're already at extreme
                    if (addiction.Stage == AddictionStage.Extreme)
                        _addictionMeters[addictionType] = 1f;
                    else
                        addiction.MoveToNextStage();
                }
            }
        }

        /// <summary>
        /// Call this periodically (e.g., once per hour) to allow natural recovery.
        /// </summary>
        public static void NaturalRecovery()
        {
            // This method is called once every in-game hour
            foreach (var addictionMeter in _addictionMeters.ToArray()) 
            {
                // Exponential decay over-time
                var currentValue = _addictionMeters[addictionMeter.Key];
                _addictionMeters[addictionMeter.Key] = ApplyExponentialDecay(currentValue);

                if (Plugin.Instance.Config.DebugMode)
                    Plugin.Log.LogInfo($"\"{addictionMeter.Key} addiction\" naturally decayed from \"{currentValue}\" to \"{_addictionMeters[addictionMeter.Key]}\".");

                // Ensure addiction meter doesn't go below zero
                if (_addictionMeters[addictionMeter.Key] <= 0f)
                {
                    var addiction = GetAddiction(addictionMeter.Key);
                    if (addiction != null)
                    {
                        // Reset, and move to the previous stage
                        _addictionMeters[addictionMeter.Key] = 0.99f;
                        addiction.MoveToPreviousStage();
                    }
                    else
                    {
                        // If we're trying to go under or equal to 0, and no addiction then remove.
                        _addictionMeters.Remove(addictionMeter.Key);
                    }
                }
            }
        }

        private static float ApplyExponentialDecay(float addictionValue)
        {
            // Exponential decay: addictionValue * e^(-decayRate * timeElapsed)
            // Since timeElapsed is 1 per in-game hour, we can simplify the formula
            float newAddictionValue = addictionValue * (float)Math.Exp(-_hourlyAddictionRecoveryRate);

            // Clamp to avoid negative values, just in case
            return Math.Max(newAddictionValue, 0f);
        }

        /// <summary>
        /// Inits configuration values so the manager can properly work.
        /// </summary>
        public static void InitConfigValues()
        {
            if (_enabledAddictions.Count != 0 || _addictionPotentials.Count != 0) return;

            // Enabled or not
            _enabledAddictions.Add(AddictionType.Alcohol, Plugin.Instance.Config.EnableAlcoholAddiction);
            _enabledAddictions.Add(AddictionType.Nicotine, Plugin.Instance.Config.EnableNicotineAddiction);
            _enabledAddictions.Add(AddictionType.Opioid, Plugin.Instance.Config.EnableOpioidAddiction);
            _enabledAddictions.Add(AddictionType.Sugar, Plugin.Instance.Config.EnableSugarAddiction);
            _enabledAddictions.Add(AddictionType.Caffeine, Plugin.Instance.Config.EnableCaffeineAddiction);

            // Potential for addiction
            _addictionPotentials.Add(AddictionType.Alcohol, Plugin.Instance.Config.AlcoholAddictionPotential);
            _addictionPotentials.Add(AddictionType.Nicotine, Plugin.Instance.Config.NicotineAddictionPotential);
            _addictionPotentials.Add(AddictionType.Opioid, Plugin.Instance.Config.OpioidAddictionPotential);
            _addictionPotentials.Add(AddictionType.Sugar, Plugin.Instance.Config.SugarAddictionPotential);
            _addictionPotentials.Add(AddictionType.Caffeine, Plugin.Instance.Config.CaffeineAddictionPotential);

            _hourlyAddictionRecoveryRate = Plugin.Instance.Config.AddictionHourlyRecoveryRate;
        }

        /// <summary>
        /// Saves addictions information to file.
        /// </summary>
        public static void Save(string filePath)
        {
            if (_addictions.Count == 0 && _susceptibilityFactors.Count == 0 && _addictionMeters.Count == 0 && _random == null)
                return;

            var saveData = AddictionsSaveData.Create(
                _addictions,
                _susceptibilityFactors,
                _addictionMeters, 
                _random);
            var jsonData = JsonSerializer.Serialize(saveData, new JsonSerializerOptions { WriteIndented = false });

            var seed = Lib.SaveGame.GetUniqueString(filePath);
            var path = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"addictions_{seed}.json");

            File.WriteAllText(path, jsonData);

            Plugin.Log.LogInfo("Saved addictions data.");
        }

        /// <summary>
        /// Loads addictions information from file.
        /// </summary>
        public static void Load(string filePath)
        {
            // When we are loading, they should be cleared anyway
            ClearExistingData();

            var seed = Lib.SaveGame.GetUniqueString(filePath);
            var path = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"addictions_{seed}.json");

            if (!File.Exists(path)) return;

            var jsonData = File.ReadAllText(path);

            AddictionsSaveData saveData;
            try
            {
                saveData = JsonSerializer.Deserialize<AddictionsSaveData>(jsonData);
            }
            catch (Exception)
            {
                Plugin.Log.LogWarning("Corrupted or outdated addictions savedata found, loading addictions skipped.");
                return;
            }

            foreach (var entry in saveData.SusceptibilityFactors)
                _susceptibilityFactors[entry.Key] = entry.Value;

            foreach (var entry in saveData.AddictionMeters)
                _addictionMeters[entry.Key] = entry.Value;

            if (saveData.Mt != null)
                _random = new MersenneTwister((saveData.Index, saveData.Mt));

            foreach (var entry in saveData.Addictions)
            {
                _addictions[entry.Key] = AddictionFactory.Get(entry.Value.AddictionType);
                _addictions[entry.Key].AppliedStageEffects = entry.Value.AppliedStageEffects.ToHashSet();
                _addictions[entry.Key].Stage = entry.Value.Stage;
                _addictions[entry.Key].Initialize();
            }

            Plugin.Log.LogInfo("Addictions save data has been loaded.");
        }

        public static void ClearExistingData()
        {
            _addictions.Clear();
            _addictionMeters.Clear();
            _susceptibilityFactors.Clear();
            _random = null;
        }

        private static Addiction GetAddiction(AddictionType addictionType)
        {
            return _addictions.TryGetValue(addictionType, out var addiction) ? addiction : null;
        }
    }
}
