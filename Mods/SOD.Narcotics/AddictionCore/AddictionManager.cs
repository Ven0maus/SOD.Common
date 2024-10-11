using SOD.Common;
using SOD.Common.Custom;
using SOD.Common.Helpers;
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
        private readonly static Dictionary<AddictionType, Addiction> _addictions = new();
        private readonly static Dictionary<AddictionType, int> _consumptionCounters = new();
        private static float? _susceptibilityModifier;
        private readonly static Dictionary<AddictionType, Time.TimeData> _lastTimeSinceConsumption = new();
        private readonly static Dictionary<AddictionType, float> _addictionPotentials = new();
        private readonly static Dictionary<AddictionType, float> _consumptionRate = new();
        private readonly static Dictionary<AddictionType, bool> _enabledAddictions = new();

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

            // Remove this addiction type from the counter
            _consumptionCounters.Remove(addictionType);
            _lastTimeSinceConsumption.Remove(addictionType);

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
        public static void OnItemConsumed(AddictionType addictionType, float? itemPotency = null)
        {
            if (!_enabledAddictions.ContainsKey(addictionType)) return;

            // Check if the human already has this addiction
            var addiction = GetAddiction(addictionType);
            if (addiction != null)
            {
                // If already addicted, add stage progression for continued use
                addiction.AdjustProgress(0.15f);
                return;
            }

            // Set current consumption time
            if (!_lastTimeSinceConsumption.ContainsKey(addictionType))
                _lastTimeSinceConsumption[addictionType] = Lib.Time.CurrentDateTime;

            // If not addicted, track the consumption count
            if (!_consumptionCounters.ContainsKey(addictionType))
                _consumptionCounters[addictionType] = 0;

            // Define the susceptibility of the human
            if (_susceptibilityModifier == null)
            {
                _susceptibilityModifier = Random.NextFloat(
                    Plugin.Instance.Config.MinimumSusceptibility, 
                    Plugin.Instance.Config.MaximumSusceptibility);
            }

            // Increment consumption count for this substance
            _consumptionCounters[addictionType]++;
            int currentConsumption = _consumptionCounters[addictionType];
            int addictionThreshold = (int)(Plugin.Instance.Config.BaseAddictionThreshold * _susceptibilityModifier.Value / ((itemPotency ?? 1.0f) * _addictionPotentials[addictionType]));

            if (Plugin.Instance.Config.DebugMode)
                Plugin.Log.LogInfo($"You consumed {addictionType.ToString().ToLower()}: {currentConsumption}/{addictionThreshold}");

            // Check if consumption exceeds the threshold, triggering addiction
            if (currentConsumption >= addictionThreshold)
                Assign(addictionType);
        }

        /// <summary>
        /// Called every tick when consuming a narcotic
        /// </summary>
        /// <param name="addictionType"></param>
        /// <param name="rate"></param>
        /// <param name="potency"></param>
        public static void AddConsumptionRate(AddictionType addictionType, float rate, float potency)
        {
            if (!_consumptionRate.ContainsKey(addictionType))
                _consumptionRate.Add(addictionType, 0);

            _consumptionRate[addictionType] += rate;

            // Every 1f of consuming (1 second)
            if (_consumptionRate[addictionType] >= 1f)
            {
                _consumptionRate[addictionType] = 1f - _consumptionRate[addictionType];

                // We apply a lower potency consumption for this addiction type
                OnItemConsumed(addictionType, potency);

                if (Plugin.Instance.Config.DebugMode)
                    Plugin.Log.LogInfo($"Consumed \"{addictionType}\" with potency \"{potency}\".");
            }
        }

        /// <summary>
        /// This method is called when an action that helps recovery is performed.
        /// It improves the addiction by reducing its stage.
        /// </summary>
        public static void OnRecoveryAction(AddictionType addictionType, float recoveryAmount)
        {
            if (!_enabledAddictions.ContainsKey(addictionType)) return;
            var addiction = GetAddiction(addictionType);
            if (addiction != null)
            {
                addiction.AdjustProgress(-recoveryAmount);
            }
        }

        public static void ForgetConsumptionCounters()
        {
            var cdt = Lib.Time.CurrentDateTime;
            var toBeRemoved = new List<AddictionType>();
            var hours = Plugin.Instance.Config.ResetConsumptionCounterAfterHours;
            foreach (var consumptionCounter in _lastTimeSinceConsumption)
            {
                // After a half day of not consuming, it will do a full reset of the counter
                if (consumptionCounter.Value.AddHours(hours) <= cdt)
                {
                    if (_consumptionCounters.ContainsKey(consumptionCounter.Key))
                        _consumptionCounters.Remove(consumptionCounter.Key);
                    if (_consumptionRate.ContainsKey(consumptionCounter.Key))
                        _consumptionRate.Remove(consumptionCounter.Key);

                    if (Plugin.Instance.Config.DebugMode)
                        Plugin.Log.LogInfo($"Forgot consumption counters for addiction \"{consumptionCounter.Key}\".");

                    toBeRemoved.Add(consumptionCounter.Key);
                }
            }

            if (toBeRemoved.Count > 0)
            {
                foreach (var tbr in toBeRemoved)
                    _lastTimeSinceConsumption.Remove(tbr);
                toBeRemoved.Clear();
            }
        }

        /// <summary>
        /// Call this periodically (e.g., once per hour) to allow natural recovery.
        /// </summary>
        public static void NaturalRecovery()
        {
            ForgetConsumptionCounters();

            // Make sure to clone the collection into a new array, because addictions can cure during recovery and be removed from _addictions
            var addictions = _addictions.Values
                .Where(a => a.Recovering)
                .ToArray();
            foreach (var addiction in addictions)
                addiction.AdjustProgress(-0.03f);
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
        }

        /// <summary>
        /// Saves addictions information to file.
        /// </summary>
        public static void Save(string filePath)
        {
            if (_addictions.Count == 0 && _consumptionCounters.Count == 0 && _susceptibilityModifier == null && _random == null)
                return;

            var saveData = AddictionsSaveData.Create(_addictions, _consumptionCounters, _susceptibilityModifier.Value, _lastTimeSinceConsumption, _random);
            var jsonData = JsonSerializer.Serialize(saveData, new JsonSerializerOptions { WriteIndented = false });

            var seed = Lib.SaveGame.GetUniqueString(filePath);
            var path = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"addictions_{seed}.json");

            File.WriteAllText(path, jsonData);

            Plugin.Log.LogInfo("Saved addictions information.");
        }

        /// <summary>
        /// Loads addictions information from file.
        /// </summary>
        public static void Load(string filePath)
        {
            // When we are loading, they should be cleared anyway
            _addictions.Clear();
            _consumptionCounters.Clear();
            _susceptibilityModifier = null;
            _lastTimeSinceConsumption.Clear();

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

            _susceptibilityModifier = saveData.SusceptibilityModifier;

            foreach (var entry in saveData.ConsumptionCounters)
                _consumptionCounters[entry.Key] = entry.Value;

            foreach (var entry in saveData.LastTimeSinceConsumption)
                _lastTimeSinceConsumption[entry.Key] = Time.TimeData.Deserialize(entry.Value);

            if (saveData.Mt != null)
                _random = new MersenneTwister((saveData.Index, saveData.Mt));

            foreach (var entry in saveData.Addictions)
            {
                _addictions[entry.Key] = AddictionFactory.Get(entry.Value.AddictionType);
                _addictions[entry.Key].AppliedStageEffects = entry.Value.AppliedStageEffects.ToHashSet();
                _addictions[entry.Key].Progression = entry.Value.Progression;
                _addictions[entry.Key].TimeSinceLastWorsening = Time.TimeData.Deserialize(entry.Value.TimeSinceLastWorsening);
                _addictions[entry.Key].Stage = entry.Value.Stage;
                _addictions[entry.Key].Initialize();
            }

            Plugin.Log.LogInfo("Addictions information loaded.");
        }

        public static (AddictionType addictionType, float? potency)? GetAddictionTypeAndPotency(Interactable interactable)
        {
            var ri = interactable.preset.retailItem;
            if (ri.drunk > 0)
            {
                return (AddictionType.Alcohol, ri.drunk);
            }
            else if (ri.numb > 0 || ri.desireCategory == CompanyPreset.CompanyCategory.medical)
            {
                if (interactable.preset.name != "Bandage" && interactable.preset.name != "Splint" && interactable.preset.name != "HeatPack")
                    return (AddictionType.Opioid, null);
            }
            else if (interactable.preset.name == "ChocolateBar" || interactable.preset.name == "Donut" || interactable.preset.name == "Eclair")
            {
                return (AddictionType.Sugar, null);
            }
            else if (ri.desireCategory == CompanyPreset.CompanyCategory.caffeine)
            {
                if (interactable.preset.name == "MugCoffee" || interactable.preset.name == "TakeawayCoffee")
                    return (AddictionType.Caffeine, null);
            }

            return null;
        }

        private static Addiction GetAddiction(AddictionType addictionType)
        {
            return _addictions.TryGetValue(addictionType, out var addiction) ? addiction : null;
        }
    }
}
