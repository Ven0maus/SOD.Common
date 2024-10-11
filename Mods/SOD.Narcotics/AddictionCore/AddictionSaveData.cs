using SOD.Common.Custom;
using SOD.Common.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace SOD.Narcotics.AddictionCore
{
    public class AddictionsSaveData
    {
        public Dictionary<AddictionType, Addiction> Addictions { get; set; } = new();
        public Dictionary<AddictionType, int> ConsumptionCounters { get; set; } = new();
        public Dictionary<AddictionType, string> LastTimeSinceConsumption { get; set; } = new();
        public float SusceptibilityModifier { get; set; } = new();
        public int Index { get; set; }
        public uint[] Mt { get; set; }

        public class Addiction
        {
            public AddictionType AddictionType { get; set; }
            public AddictionStage Stage { get; set; }
            public AddictionStage[] AppliedStageEffects { get; set; }
            public float Progression { get; set; }
            public int HumanId { get; set; }
            public string TimeSinceLastWorsening { get; set; }
        }

        public static AddictionsSaveData Create(
            Dictionary<AddictionType, AddictionCore.Addiction> addictionDatas,
            Dictionary<AddictionType, int> consumptionCounters,
            float susceptibilityModifier,
            Dictionary<AddictionType, Time.TimeData> lastTimeSinceConsumption,
            MersenneTwister random)
        {
            var saveData = new AddictionsSaveData();
            foreach (var entry in addictionDatas)
            {
                // Convert
                saveData.Addictions[entry.Key] = new Addiction
                {
                    AddictionType = addictionDatas[entry.Key].AddictionType,
                    Stage = addictionDatas[entry.Key].Stage,
                    AppliedStageEffects = addictionDatas[entry.Key].AppliedStageEffects.ToArray(),
                    Progression = addictionDatas[entry.Key].Progression,
                    TimeSinceLastWorsening = addictionDatas[entry.Key].TimeSinceLastWorsening.Serialize()
                };
            }
            if (random != null)
            {
                var state = random.SaveState();
                saveData.Index = state.index;
                saveData.Mt = state.mt;
            }
            saveData.LastTimeSinceConsumption = lastTimeSinceConsumption
                .ToDictionary(a => a.Key, a => a.Value.Serialize());
            saveData.ConsumptionCounters = consumptionCounters;
            saveData.SusceptibilityModifier = susceptibilityModifier;
            return saveData;
        }
    }
}
