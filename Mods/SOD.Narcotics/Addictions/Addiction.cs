using System;
using System.Collections.Generic;

namespace SOD.Narcotics.Addictions
{
    public class Addiction
    {
        public bool IsActive { get; set; }
        public string AddictionName => $"{AddictionType} Addiction";
        public AddictionType AddictionType { get; private set; }
        public AddictionStage Stage { get; set; }
        public Dictionary<AddictionStage, Action<bool>> StageActions { get; private set; }
        public float StageProgress { get; set; }
        public TimeSpan RelapseTime { get; set; }
        public bool Recovering { get; set; }

        public Addiction(AddictionType addictionType)
        {
            AddictionType = addictionType;
            StageActions = StageActionFactory.GetActions(addictionType);
        }

        private void ApplyStageEffects()
        {
            if (StageActions.TryGetValue(Stage, out Action<bool> action))
                action?.Invoke(true);
        }

        private void RemoveStageEffects()
        {
            if (StageActions.TryGetValue(Stage, out Action<bool> action))
                action?.Invoke(false);
        }

        public void Cure()
        {
            IsActive = false;
            StageProgress = 0f;
            Recovering = false;

            // Remove all effects
            foreach (var stage in new[] { AddictionStage.Extreme, AddictionStage.Severe, AddictionStage.Mild })
            {
                Stage = stage;
                RemoveStageEffects();
            }

            Plugin.Log.LogInfo($"[{AddictionName}] [Status]: Cured");
        }

        public void Update()
        {
            if (!IsActive) return;

            ApplyStageEffects();
            Plugin.Log.LogInfo($"[{AddictionName}] [Status]: {Stage}");
        }
    }

    public enum AddictionType
    {
        Alcohol,
        Nicotine,
        Opioid
    }

    public enum AddictionStage
    {
        Mild,
        Severe,
        Extreme
    }
}
