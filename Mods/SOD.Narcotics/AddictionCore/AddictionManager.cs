using SOD.Common;
using SOD.Common.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace SOD.Narcotics.AddictionCore
{
    public static class AddictionManager
    {
        private readonly static Dictionary<int, List<Addiction>> _addictions = new();
        private readonly static Dictionary<int, Dictionary<AddictionType, int>> _consumptionCounters = new();
        private readonly static Dictionary<int, float> _susceptibilityModifiers = new();
        private readonly static Dictionary<AddictionType, float> _addictionPotentials = new();
        private readonly static Dictionary<AddictionType, bool> _enabledAddictions = new();

        /// <summary>
        /// Assigns a new addiction.
        /// </summary>
        /// <param name="addictionType"></param>
        /// <param name="stage"></param>
        public static void Assign(int humanId, AddictionType addictionType)
        {
            if (!_enabledAddictions.ContainsKey(addictionType)) return;
            if (!_addictions.TryGetValue(humanId, out var addictions))
                _addictions[humanId] = new List<Addiction>();

            if (!addictions.Any(a => a.AddictionType == addictionType))
            {
                // Get a new addiction class
                Addiction addiction = AddictionFactory.Get(humanId, addictionType);
                addiction.Initialize();

                // Add to collection
                addictions.Add(addiction);

                if (Plugin.Instance.Config.DebugMode)
                    Plugin.Log.LogInfo($"[Human: {humanId}] has become addicted to \"{addictionType.ToString().ToLower()}\".");
            }
        }

        /// <summary>
        /// Cures the given addiction.
        /// </summary>
        /// <param name="addictionType"></param>
        public static void Cure(int humanId, AddictionType addictionType)
        {
            if (!_enabledAddictions.ContainsKey(addictionType)) return;
            var addiction = GetAddiction(humanId, addictionType);
            if (addiction != null)
            {
                _addictions[humanId].Remove(addiction);
                if (_addictions[humanId].Count == 0)
                    _addictions.Remove(humanId);
                addiction.Cure();
            }
        }

        /// <summary>
        /// This method is called when an item related to an addiction is consumed.
        /// It worsens the addiction by progressing its stage.
        /// </summary>
        public static void OnItemConsumed(int humanId, AddictionType addictionType, float itemPotency = 1.0f)
        {
            if (!_enabledAddictions.ContainsKey(addictionType)) return;
            // Ensure the consumption counter is initialized for the human
            if (!_consumptionCounters.ContainsKey(humanId))
                _consumptionCounters[humanId] = new Dictionary<AddictionType, int>();

            // Check if the human already has this addiction
            var addiction = GetAddiction(humanId, addictionType);
            if (addiction != null)
            {
                // If already addicted, add stage progression for continued use
                addiction.AdjustProgress(0.15f);
                return;
            }

            // If not addicted, track the consumption count
            if (!_consumptionCounters[humanId].ContainsKey(addictionType))
                _consumptionCounters[humanId][addictionType] = 0;

            // Define the susceptibility of the human
            if (!_susceptibilityModifiers.TryGetValue(humanId, out var susceptibility))
            {
                susceptibility = _susceptibilityModifiers[humanId] = UnityEngine.Random.Range(
                    Plugin.Instance.Config.MinimumSusceptibility, 
                    Plugin.Instance.Config.MaximumSusceptibility);
            }

            // Increment consumption count for this substance
            _consumptionCounters[humanId][addictionType]++;
            int currentConsumption = _consumptionCounters[humanId][addictionType];
            int addictionThreshold = (int)(Plugin.Instance.Config.BaseAddictionThreshold * susceptibility / (itemPotency * _addictionPotentials[addictionType]));

            if (Plugin.Instance.Config.DebugMode)
                Plugin.Log.LogInfo($"[Human: {humanId}] consumed {addictionType.ToString().ToLower()}: {currentConsumption}/{addictionThreshold}");

            // Check if consumption exceeds the threshold, triggering addiction
            if (currentConsumption >= addictionThreshold)
                Assign(humanId, addictionType);
        }

        /// <summary>
        /// This method is called when an action that helps recovery is performed.
        /// It improves the addiction by reducing its stage.
        /// </summary>
        public static void OnRecoveryAction(int humanId, AddictionType addictionType, float recoveryAmount)
        {
            if (!_enabledAddictions.ContainsKey(addictionType)) return;
            var addiction = GetAddiction(humanId, addictionType);
            if (addiction != null)
            {
                addiction.AdjustProgress(-recoveryAmount);
            }
        }

        /// <summary>
        /// Call this periodically (e.g., once per hour) to allow natural recovery.
        /// </summary>
        public static void NaturalRecovery()
        {
            // Make sure the clone the collection into a new array, because addictions can cure during recovery and be removed from _addictions
            var addictions = _addictions.Values
                .SelectMany(a => a)
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
            if (_addictions.Count == 0 && _consumptionCounters.Count == 0 && _susceptibilityModifiers.Count == 0)
                return;

            var saveData = AddictionsSaveData.Create(_addictions, _consumptionCounters, _susceptibilityModifiers);
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
            _susceptibilityModifiers.Clear();

            var seed = Lib.SaveGame.GetUniqueString(filePath);
            var path = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"addictions_{seed}.json");

            if (!File.Exists(path)) return;

            var jsonData = File.ReadAllText(path);
            var saveData = JsonSerializer.Deserialize<AddictionsSaveData>(jsonData);

            foreach (var entry in saveData.ConsumptionCounters)
                _consumptionCounters[entry.Key] = entry.Value;

            foreach (var entry in saveData.SusceptibilityModifiers)
                _susceptibilityModifiers[entry.Key] = entry.Value;

            foreach (var entry in saveData.Addictions) 
            {
                _addictions[entry.Key] = entry.Value.Select(a =>
                {
                    var addiction = AddictionFactory.Get(a.HumanId, a.AddictionType);
                    addiction.AppliedStageEffects = a.AppliedStageEffects.ToHashSet();
                    addiction.Progression = a.Progression;
                    addiction.TimeSinceLastWorsening = Time.TimeData.Deserialize(a.TimeSinceLastWorsening);
                    addiction.Stage = a.Stage;
                    addiction.Initialize();
                    return addiction;
                }).ToList();
            }

            Plugin.Log.LogInfo("Addictions information loaded.");
        }

        private static Addiction GetAddiction(int humanId, AddictionType addictionType)
        {
            if (_addictions.TryGetValue(humanId, out var addictions))
                return addictions.FirstOrDefault(a => a.AddictionType == addictionType);
            return null;
        }
    }
}
