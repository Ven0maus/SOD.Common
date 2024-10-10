using SOD.Narcotics.AddictionCore.Addictions;
using System.Collections.Generic;
using System.Linq;

namespace SOD.Narcotics.AddictionCore
{
    public static class AddictionManager
    {
        private readonly static Dictionary<int, List<Addiction>> _addictions = new();
        private readonly static Dictionary<int, Dictionary<AddictionType, int>> _consumptionCounters = new();
        private readonly static Dictionary<int, float> _susceptibilityModifiers = new();
        private readonly static int baseAddictionThreshold = 10; // Default threshold before addiction

        /// <summary>
        /// Assigns a new addiction.
        /// </summary>
        /// <param name="addictionType"></param>
        /// <param name="stage"></param>
        public static void Assign(int humanId, AddictionType addictionType)
        {
            if (!_addictions.TryGetValue(humanId, out var addictions))
            {
                _addictions[humanId] = new List<Addiction>();
            }

            if (!addictions.Any(a => a.AddictionType == addictionType))
            {
                // Get a new addiction class
                Addiction addiction = AddictionFactory.Get(addictionType, humanId);

                // Add to collection
                addictions.Add(addiction);
            }
        }

        /// <summary>
        /// Cures the given addiction.
        /// </summary>
        /// <param name="addictionType"></param>
        public static void Cure(int humanId, AddictionType addictionType)
        {
            var addiction = GetAddiction(humanId, addictionType);
            if (addiction != null)
            {
                _addictions[humanId].Remove(addiction);
                addiction.Cure();
            }
        }

        /// <summary>
        /// This method is called when an item related to an addiction is consumed.
        /// It worsens the addiction by progressing its stage.
        /// </summary>
        public static void OnItemConsumed(int humanId, AddictionType addictionType, float itemPotency = 1.0f)
        {
            // Ensure the consumption counter is initialized for the human
            if (!_consumptionCounters.ContainsKey(humanId))
            {
                _consumptionCounters[humanId] = new Dictionary<AddictionType, int>();
            }

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
            {
                _consumptionCounters[humanId][addictionType] = 0;
            }

            // Define the susceptibility of the human
            if (!_susceptibilityModifiers.TryGetValue(humanId, out var susceptibility))
                susceptibility = _susceptibilityModifiers[humanId] = UnityEngine.Random.Range(0.5f, 1.5f);

            // Increment consumption count for this substance
            _consumptionCounters[humanId][addictionType]++;
            int currentConsumption = _consumptionCounters[humanId][addictionType];
            int addictionThreshold = (int)(baseAddictionThreshold * susceptibility / itemPotency);

            if (Plugin.Instance.Config.DebugMode)
                Plugin.Log.LogInfo($"[Human: {humanId}] consumed {addictionType.ToString().ToLower()}: {currentConsumption}/{addictionThreshold}");

            // Check if consumption exceeds the threshold, triggering addiction
            if (currentConsumption >= addictionThreshold)
            {
                Assign(humanId, addictionType);
            }
        }

        /// <summary>
        /// This method is called when an action that helps recovery is performed.
        /// It improves the addiction by reducing its stage.
        /// </summary>
        public static void OnRecoveryAction(int humanId, AddictionType addictionType, float recoveryAmount)
        {
            var addiction = GetAddiction(humanId, addictionType);
            if (addiction != null)
            {
                addiction.AdjustProgress(-recoveryAmount);
            }
        }

        /// <summary>
        /// Call this periodically (e.g., once per hour) to allow natural recovery.
        /// </summary>
        public static void NaturalRecovery(AddictionType addictionType, float recoveryAmount)
        {
            // Make sure the clone the collection into a new array, because addictions can cure during recovery and be removed from _addictions
            var addictions = _addictions.Values
                .SelectMany(a => a)
                .Where(a => a.AddictionType == addictionType)
                .ToArray();
            foreach (var addiction in addictions)
            {
                if (addiction.Recovering)
                    addiction.AdjustProgress(-recoveryAmount);
            }
        }

        /// <summary>
        /// Returns the full addiction item
        /// </summary>
        /// <param name="humanId"></param>
        /// <param name="addictionType"></param>
        /// <returns></returns>
        private static Addiction GetAddiction(int humanId, AddictionType addictionType)
        {
            if (_addictions.TryGetValue(humanId, out var addictions))
                return addictions.FirstOrDefault(a => a.AddictionType == addictionType);
            return null;
        }
    }
}
