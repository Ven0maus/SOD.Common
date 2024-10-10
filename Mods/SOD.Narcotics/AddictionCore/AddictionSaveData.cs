using System.Collections.Generic;
using System.Linq;

namespace SOD.Narcotics.AddictionCore
{
    public class AddictionsSaveData
    {
        public Dictionary<int, List<Addiction>> Addictions { get; set; } = new();
        public Dictionary<int, Dictionary<AddictionType, int>> ConsumptionCounters { get; set; } = new();
        public Dictionary<int, float> SusceptibilityModifiers { get; set; } = new();

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
            Dictionary<int, List<AddictionCore.Addiction>> addictionDatas,
            Dictionary<int, Dictionary<AddictionType, int>> consumptionCounters,
            Dictionary<int, float> susceptibilityModifiers)
        {
            var saveData = new AddictionsSaveData();
            foreach (var entry in addictionDatas)
            {
                // Convert
                saveData.Addictions[entry.Key] = addictionDatas[entry.Key]
                    .Select(a => new Addiction
                {
                    AddictionType = a.AddictionType,
                    Stage = a.Stage,
                    AppliedStageEffects = a.AppliedStageEffects.ToArray(),
                    HumanId = a.HumanId,
                    Progression = a.Progression,
                    TimeSinceLastWorsening = a.TimeSinceLastWorsening.Serialize()
                }).ToList();
            }
            saveData.ConsumptionCounters = consumptionCounters;
            saveData.SusceptibilityModifiers = susceptibilityModifiers;
            return saveData;
        }
    }
}
