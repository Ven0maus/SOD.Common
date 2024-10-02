using SOD.Narcotics.AddictionCore.Addictions;
using System;

namespace SOD.Narcotics.AddictionCore
{
    public abstract class Addiction : IAddiction
    {
        public bool IsActive { get; set; }
        public string AddictionName => $"{AddictionType} Addiction";
        public AddictionType AddictionType { get; private set; }
        public AddictionStage Stage { get; set; }
        public float StageProgress { get; set; }
        public TimeSpan RelapseTime { get; set; }
        public bool Recovering { get; set; }

        public Addiction(AddictionType addictionType, TimeSpan? relapseTime = null)
        {
            AddictionType = addictionType;
            RelapseTime = relapseTime ?? TimeSpan.FromSeconds(5);
        }

        private void ApplyStageEffects()
        {
            GetStageAction()?.Invoke(true);
            Plugin.Log.LogInfo($"[{AddictionName}] Applied effects of stage \"{Stage}\".");
        }

        private void RemoveStageEffects()
        {
            GetStageAction()?.Invoke(false);
            Plugin.Log.LogInfo($"[{AddictionName}] Removed effects of stage \"{Stage}\".");
        }

        private Action<bool> GetStageAction()
        {
            switch (Stage)
            {
                case AddictionStage.Mild:
                    return MildStageAction();
                case AddictionStage.Severe:
                    return SevereStageAction();
                case AddictionStage.Extreme:
                    return ExtremeStageAction();
            }

            throw new NotSupportedException($"Stage \"{Stage}\" is not supported.");
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

        public abstract Action<bool> MildStageAction();
        public abstract Action<bool> SevereStageAction();
        public abstract Action<bool> ExtremeStageAction();
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
